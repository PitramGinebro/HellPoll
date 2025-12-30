using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ThreeDPool.EventHandlers;
using ThreeDPool.Controllers;
using ThreeDPool.UIControllers;

namespace ThreeDPool.Managers
{
    /// <summary>
    /// Aquesta classe és el gestor de regles del joc de billar.
    /// Decideix quins jugadors juguen, calcula la puntuació i el destí de les boles.
    /// </summary>
    public class GameManager : Singleton<GameManager>
    {
        // Defineix quants boles d'objectiu hi haurà a la taula.
        public enum GameType
        {
            JustCue = 1,  // Només la blanca (per pràctica).
            ThreeBall = 3,
            SixBall = 6,
            SevenBall,
        }

        // Estats globals de la màquina de dades del joc.
        public enum GameState
        {
            Practise = 1, // Mode lliure.
            GetSet,       // Preparant la partida (UI).
            Play,         // Partida activa.
            Pause,        // Joc pausat.
            Complete      // Partida finalitzada.
        }

        [SerializeField] private string[] _playerNames; // Noms dels jugadors des de l'editor.
        [SerializeField] private GameType _gameType;    // Tipus de joc seleccionat.
        [SerializeField] private Transform _rackTransform; // On es col·locaran les boles (triàngle).
        [SerializeField] private CueBallController _cueBall; // Referència a la bola blanca.
        [SerializeField] private GameUIScreen _gameUIScreen; // Referència a la interfície.

        // Utilitzem una cua (Queue) per gestionar els torns de forma automàtica.
        private Queue<Player> _players = new Queue<Player>();

        private List<CueBallController> _ballsPocketed; // Boles que han entrat al forat.
        private List<CueBallController> _ballsHitOut;   // Boles que han sortit de la taula.
        private GameState _currGameState;
        private GameState _prevGameState;
        private bool _ballsInstantiated;

        public int NumOfBallsStriked; // Quantes boles s'estan movent actualment.

        public GameState CurrGameState { get { return _currGameState; } }
        public GameState PrevGameState { get { return _prevGameState; } }
        public Queue<Player> Players { get { return _players; } }
        public string[] Winners; // Noms dels guanyadors al final.
        public int NumOfTimesPlayed { private set; get; }

        protected override void Start()
        {
            base.Start();

            ChangeGameState(GameState.Practise); // Comencem sempre en mode pràctica.
            NumOfBallsStriked = 0;

            // Inicialitzem la llista de jugadors.
            if (_playerNames != null)
            {
                foreach (var playerName in _playerNames)
                {
                    var player = new Player(playerName);
                    _players.Enqueue(player); // Els fiquem a la cua de torns.
                }
            }

            // Calculem la mida de les llistes segons el tipus de joc + la bola blanca.
            int arraySize = (int)_gameType + 1;
            _ballsPocketed = new List<CueBallController>(arraySize);
            _ballsHitOut = new List<CueBallController>(arraySize);

            // Creem els elements visuals de la UI per a cada jugador.
            _gameUIScreen.CreatePlayerUI();
        }

        // Instancia el "Rack" (conjunt de boles) segons el mode triat.
        private void PlaceBallBasedOnGameType()
        {
            if (_gameType != GameType.JustCue)
            {
                string rackString = "Rack";
                // Carrega el prefab des de la carpeta Resources.
                Instantiate((Resources.Load(_gameType.ToString() + rackString, typeof(GameObject)) as GameObject), _rackTransform.position, _rackTransform.rotation);
            }
        }

        // Comprova si ja han entrat totes les boles d'objectiu.
        private bool IsGameComplete()
        {
            return _ballsPocketed.Count() == (int)_gameType;
        }

        // Corrutina que s'executa en acabar la partida per calcular guanyadors.
        private IEnumerator OnGameComplete()
        {
            yield return new WaitForEndOfFrame();

            int winningScore = 0;
            // Busca la puntuació més alta.
            foreach (var player in _players)
            {
                if (player.Score >= winningScore)
                    winningScore = player.Score;
            }

            // Selecciona tots els jugadors que tinguin aquesta puntuació (per si hi ha empat).
            Winners = _players.Where(p => p.Score == winningScore).Select(p => p.Name).ToArray();

            // Notifica a la resta del joc que hem acabat.
            EventManager.Notify(typeof(GameStateEvent).Name, this, new GameStateEvent() { GameState = GameStateEvent.State.Complete });
        }

        // Fa passar el torn al següent jugador de la cua.
        private void SetNewPlayerTurn()
        {
            Player player = _players.Dequeue(); // Treu el jugador actual.
            _players.Enqueue(player);           // El posa al final de la cua.

            Player newPlayer = _players.Peek(); // Mira qui toca ara sense treure'l.
            EventManager.Notify(typeof(GameStateEvent).Name, this, new GameStateEvent() { CurrPlayer = newPlayer.Name });
        }

        // La lògica principal després de cada tir: calcula punts i penalitzacions.
        private void CalculateThePointAndNextTurn()
        {
            Player currPlayer = _players.Peek();

            if (currPlayer.HasStrikedBall)
            {
                // Mirem si la bola blanca ha caigut al forat (falta).
                CueBallController whiteBall = _ballsPocketed.FirstOrDefault(b => b.BallType == CueBallController.CueBallType.White);

                if (whiteBall != null)
                {
                    currPlayer.CalculateScore(-1); // Penalització.
                    _ballsPocketed.Remove(whiteBall);
                    // Marquem la resta de boles com a "ja pocketed" per no tornar-les a puntuar.
                    _ballsPocketed.ForEach(b => b.IsPocketedInPrevTurn = true);
                    whiteBall.PlaceBallInInitialPos(); // Torna la blanca a la taula.
                    SetNewPlayerTurn(); // Passa el torn per haver comès falta.
                }
                else
                {
                    // Si han entrat boles noves en aquest torn.
                    var ballsCurrentlyPocketed = _ballsPocketed.Where(b => b.IsPocketedInPrevTurn == false);
                    if (ballsCurrentlyPocketed.Count() > 0)
                    {
                        currPlayer.CalculateScore(ballsCurrentlyPocketed.Count()); // Suma punts.
                        _ballsPocketed.ForEach(b => b.IsPocketedInPrevTurn = true);
                        // El jugador continua jugant (no cridem a SetNewPlayerTurn).
                    }
                    else
                    {
                        SetNewPlayerTurn(); // Si no ha entrat res, canvi de torn.
                    }
                }

                // Si alguna bola ha sortit volant de la taula, la tornem a posar.
                foreach (var ballHitOut in _ballsHitOut)
                    ballHitOut.PlaceBallInInitialPos();
            }

            _ballsHitOut.Clear();

            // Actualitzem qui està "jugant" per a la UI.
            foreach (var player in _players)
            {
                player.SetPlayingState((player == _players.Peek()));
            }

            if (IsGameComplete())
                StartCoroutine(OnGameComplete());
            else
                EventManager.Notify(typeof(CueBallActionEvent).Name, this, new CueBallActionEvent() { State = CueBallActionEvent.States.Stationary });
        }

        // Canvia l'estat del joc guardant l'anterior.
        public void ChangeGameState(GameState newGameState)
        {
            if (newGameState != _currGameState)
            {
                _prevGameState = _currGameState;
                _currGameState = newGameState;
            }
        }

        // Preparatius abans de començar.
        public void OnGetSet()
        {
            ChangeGameState(GameState.GetSet);
        }

        // Inicia una partida real des de zero.
        public void OnPlay()
        {
            _ballsHitOut.Clear();
            _ballsPocketed.Clear();
            NumOfBallsStriked = 0;
            NumOfTimesPlayed++;

            foreach (var player in _players)
                player.ResetScore();

            ChangeGameState(GameState.Play);
            _cueBall.PlaceBallInInitialPos();

            if (!_ballsInstantiated)
            {
                PlaceBallBasedOnGameType();
                _ballsInstantiated = true;
            }
        }

        public void OnPaused() { ChangeGameState(GameState.Pause); }
        public void OnContinue() { ChangeGameState(GameState.Play); }

        // Es crida des de les boles quan s'aturen. Si totes estan quietes, processem el torn.
        public void ReadyForNextRound()
        {
            if (CurrGameState == GameState.Practise)
            {
                _cueBall.PlaceBallInPosWhilePractise();
            }
            else if (CurrGameState == GameState.Play || CurrGameState == GameState.Pause)
            {
                NumOfBallsStriked--;
                if (NumOfBallsStriked <= 0) // Si ja no hi ha boles movent-se.
                {
                    NumOfBallsStriked = 0;
                    CalculateThePointAndNextTurn();
                }
            }
        }

        // Afegeix boles a la llista de forat.
        public void AddToBallPocketedList(CueBallController ball)
        {
            if (!_ballsPocketed.Contains(ball))
                _ballsPocketed.Add(ball);
        }

        // Afegeix boles a la llista de fora de la taula.
        public void AddToBallHitOutList(CueBallController ball)
        {
            if (!_ballsHitOut.Contains(ball) && !_ballsPocketed.Contains(ball))
                _ballsHitOut.Add(ball);
        }
    }
}