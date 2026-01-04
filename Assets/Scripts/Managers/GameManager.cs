using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ThreeDPool.EventHandlers;
using ThreeDPool.Controllers;
using ThreeDPool.UIControllers;

namespace ThreeDPool.Managers
{
    public class GameManager : Singleton<GameManager>
    {
        public enum GameType { JustCue = 1, ThreeBall = 3, SixBall = 6, SevenBall }
        public enum GameState { Practise = 1, GetSet, Play, Pause, Complete }

        [SerializeField] private string[] _playerNames;
        [SerializeField] private GameType _gameType;
        [SerializeField] private Transform _rackTransform;
        [SerializeField] private CueBallController _cueBall;
        [SerializeField] private GameUIScreen _gameUIScreen;

        private Queue<Player> _players = new Queue<Player>();
        private List<CueBallController> _ballsPocketed;
        private List<CueBallController> _ballsHitOut;
        private GameState _currGameState;
        private GameState _prevGameState;
        private bool _ballsInstantiated;

        public int NumOfBallsStriked;
        public GameState CurrGameState { get { return _currGameState; } }
        public GameState PrevGameState { get { return _prevGameState; } }
        public Queue<Player> Players { get { return _players; } }
        public string[] Winners;
        public int NumOfTimesPlayed { private set; get; }

        protected override void Start()
        {
            base.Start();
            ChangeGameState(GameState.Practise);
            NumOfBallsStriked = 0;

            if (_playerNames != null && _players.Count == 0)
            {
                foreach (var playerName in _playerNames)
                    _players.Enqueue(new Player(playerName));
            }

            int arraySize = (int)_gameType + 1;
            _ballsPocketed = new List<CueBallController>(arraySize);
            _ballsHitOut = new List<CueBallController>(arraySize);

            if (_gameUIScreen != null)
                _gameUIScreen.CreatePlayerUI();
        }

        private void PlaceBallBasedOnGameType()
        {
            // Búsqueda de emergencia si el transform se perdió al cambiar de escena
            if (_rackTransform == null)
            {
                GameObject found = GameObject.Find("RackTransform");
                if (found != null) _rackTransform = found.transform;
            }

            if (_rackTransform != null && _gameType != GameType.JustCue)
            {
                string rackString = "Rack";
                GameObject prefab = Resources.Load(_gameType.ToString() + rackString, typeof(GameObject)) as GameObject;
                if (prefab != null)
                {
                    Instantiate(prefab, _rackTransform.position, _rackTransform.rotation);
                    _ballsInstantiated = true; 
                }
            }
        }

        public void OnPlay()
        {
            // RE-VINCULACIÓN AUTOMÁTICA AL CAMBIAR DE ESCENA
            if (_cueBall == null) _cueBall = Object.FindFirstObjectByType<CueBallController>();
            if (_rackTransform == null) {
                GameObject found = GameObject.Find("RackTransform");
                if (found != null) _rackTransform = found.transform;
            }

            _ballsHitOut.Clear();
            _ballsPocketed.Clear();
            NumOfBallsStriked = 0;
            NumOfTimesPlayed++;

            foreach (var player in _players) player.ResetScore();

            ChangeGameState(GameState.Play);
            
            if (_cueBall != null)
                _cueBall.PlaceBallInInitialPos();

            // Si no hay boles en la mesa (o es nueva escena), los creamos
            if (!_ballsInstantiated || GameObject.FindObjectsByType<CueBallController>(FindObjectsSortMode.None).Length <= 1)
            {
                PlaceBallBasedOnGameType();
            }
        }

        public void OnPaused() { ChangeGameState(GameState.Pause); }
        public void OnContinue() { ChangeGameState(GameState.Play); }
        public void OnGetSet() { ChangeGameState(GameState.GetSet); }

        public void ReadyForNextRound()
        {
            if (CurrGameState == GameState.Practise)
            {
                if (_cueBall != null) _cueBall.PlaceBallInPosWhilePractise();
            }
            else if (CurrGameState == GameState.Play || CurrGameState == GameState.Pause)
            {
                NumOfBallsStriked--;
                if (NumOfBallsStriked <= 0)
                {
                    NumOfBallsStriked = 0;
                    CalculateThePointAndNextTurn();
                }
            }
        }

        private void CalculateThePointAndNextTurn()
        {
            if (_players.Count == 0) return;
            Player currPlayer = _players.Peek();

            if (currPlayer.HasStrikedBall)
            {
                CueBallController whiteBall = _ballsPocketed.FirstOrDefault(b => b.BallType == CueBallController.CueBallType.White);

                if (whiteBall != null)
                {
                    currPlayer.CalculateScore(-1);
                    _ballsPocketed.Remove(whiteBall);
                    _ballsPocketed.ForEach(b => b.IsPocketedInPrevTurn = true);
                    whiteBall.PlaceBallInInitialPos();
                    SetNewPlayerTurn();
                }
                else
                {
                    var ballsCurrentlyPocketed = _ballsPocketed.Where(b => b.IsPocketedInPrevTurn == false);
                    if (ballsCurrentlyPocketed.Any())
                    {
                        currPlayer.CalculateScore(ballsCurrentlyPocketed.Count());
                        _ballsPocketed.ForEach(b => b.IsPocketedInPrevTurn = true);
                    }
                    else
                    {
                        SetNewPlayerTurn();
                    }
                }
                foreach (var ballHitOut in _ballsHitOut) ballHitOut.PlaceBallInInitialPos();
            }

            _ballsHitOut.Clear();
            foreach (var player in _players) player.SetPlayingState((player == _players.Peek()));

            if (IsGameComplete())
                StartCoroutine(OnGameComplete());
            else
                EventManager.Notify(typeof(CueBallActionEvent).Name, this, new CueBallActionEvent() { State = CueBallActionEvent.States.Stationary });
        }

        private bool IsGameComplete()
        {
            return _ballsPocketed.Count(b => b.BallType != CueBallController.CueBallType.White) == (int)_gameType;
        }

        private IEnumerator OnGameComplete()
        {
            yield return new WaitForEndOfFrame();
            int winningScore = _players.Max(p => p.Score);
            Winners = _players.Where(p => p.Score == winningScore).Select(p => p.Name).ToArray();
            EventManager.Notify(typeof(GameStateEvent).Name, this, new GameStateEvent() { GameState = GameStateEvent.State.Complete });
        }

        private void SetNewPlayerTurn()
        {
            if (_players.Count == 0) return;
            Player player = _players.Dequeue();
            _players.Enqueue(player);
            Player newPlayer = _players.Peek();
            EventManager.Notify(typeof(GameStateEvent).Name, this, new GameStateEvent() { CurrPlayer = newPlayer.Name });
        }

        public void ChangeGameState(GameState newGameState)
        {
            if (newGameState != _currGameState)
            {
                _prevGameState = _currGameState;
                _currGameState = newGameState;
            }
        }

        public void AddToBallPocketedList(CueBallController ball)
        {
            if (!_ballsPocketed.Contains(ball)) _ballsPocketed.Add(ball);
        }

        public void AddToBallHitOutList(CueBallController ball)
        {
            if (!_ballsHitOut.Contains(ball) && !_ballsPocketed.Contains(ball)) _ballsHitOut.Add(ball);
        }
    }
}