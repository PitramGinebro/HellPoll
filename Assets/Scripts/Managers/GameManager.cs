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
        [SerializeField] private GameType _gameType = GameType.SixBall;
        [SerializeField] private Transform _rackTransform;
        [SerializeField] private CueBallController _cueBall;
        [SerializeField] private GameUIScreen _gameUIScreen;

        private Queue<Player> _players = new Queue<Player>();
        private List<CueBallController> _ballsPocketed = new List<CueBallController>();
        private List<CueBallController> _ballsHitOut = new List<CueBallController>();
        private GameState _currGameState;
        private GameState _prevGameState;

        public string[] Winners;
        public int NumOfTimesPlayed { private set; get; }
        public int NumOfBallsStriked;

        public GameState CurrGameState { get { return _currGameState; } }
        public GameState PrevGameState { get { return _prevGameState; } }
        public Queue<Player> Players { get { return _players; } }

        protected override void Start()
        {
            base.Start();
            ChangeGameState(GameState.Practise);
            
            if (_playerNames != null && _players.Count == 0)
            {
                foreach (var playerName in _playerNames)
                    _players.Enqueue(new Player(playerName));
            }

            if (_gameUIScreen != null)
                _gameUIScreen.CreatePlayerUI();

            // Iniciamos la colocación de bolas
            OnPlay();
        }

        public void OnPlay()
        {
            if (_cueBall == null) _cueBall = Object.FindFirstObjectByType<CueBallController>();
            
            _ballsHitOut.Clear();
            _ballsPocketed.Clear();
            NumOfBallsStriked = 0;
            NumOfTimesPlayed++;

            foreach (var player in _players) player.ResetScore();

            ChangeGameState(GameState.Play);
            
            if (_cueBall != null)
            {
                _cueBall.gameObject.SetActive(true);
                _cueBall.PlaceBallInInitialPos();
            }

            PlaceBallBasedOnGameType();
        }

        private void PlaceBallBasedOnGameType()
        {
            if (_rackTransform == null) _rackTransform = GameObject.Find("RackTransform")?.transform;
            if (_rackTransform == null) return;

            // Limpiar bolas viejas antes de instanciar nuevas
            foreach (Transform child in _rackTransform) Destroy(child.gameObject);

            GameObject prefab = Resources.Load(_gameType.ToString() + "Rack", typeof(GameObject)) as GameObject;
            if (prefab != null)
            {
                Instantiate(prefab, _rackTransform.position, _rackTransform.rotation, _rackTransform);
            }
        }

        public void ReadyForNextRound()
        {
            // Reducimos el contador de bolas en movimiento
            if (NumOfBallsStriked > 0) NumOfBallsStriked--;

            // Si ya no hay bolas moviéndose, preparamos el siguiente tiro
            if (NumOfBallsStriked <= 0)
            {
                NumOfBallsStriked = 0;
                CalculateThePointAndNextTurn();
            }
        }

        private void CalculateThePointAndNextTurn()
        {
            // IMPORTANTE: Esto reactiva el Cue (palo) enviando el evento Stationary
            EventManager.Notify(typeof(CueBallActionEvent).Name, this, new CueBallActionEvent() { State = CueBallActionEvent.States.Stationary });
            
            // Lógica de cambio de turno o puntos (Hell Pool entrará aquí)
            if (_players.Count > 0)
            {
                Player currPlayer = _players.Peek();
                // Si metió bola, se queda el turno. Si no, cambia. (Lógica simplificada para test)
            }
        }

        public void OnPaused() { ChangeGameState(GameState.Pause); }
        public void OnContinue() { ChangeGameState(GameState.Play); }
        public void OnGetSet() { ChangeGameState(GameState.GetSet); }

        public void ChangeGameState(GameState newState) 
        { 
            _prevGameState = _currGameState;
            _currGameState = newState; 
        }

        public void AddToBallPocketedList(CueBallController ball) { if (!_ballsPocketed.Contains(ball)) _ballsPocketed.Add(ball); }
        public void AddToBallHitOutList(CueBallController ball) { if (!_ballsHitOut.Contains(ball)) _ballsHitOut.Add(ball); }
    }
}