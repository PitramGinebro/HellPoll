using ThreeDPool.EventHandlers;
using ThreeDPool.Managers;

namespace ThreeDPool
{
    public class Player
    {
        // Propietats públiques amb setter privat per emmagatzemar el nom i la puntuació.
        public string Name { private set; get; }
        public int Score { private set; get; }

        // Indica si el jugador ha colpejat la bola blanca en el torn actual.
        public bool HasStrikedBall { private set; get; }

        // Variable interna per saber si és el torn actiu d'aquest jugador.
        private bool _isPlaying;

        // Constructor de la classe Player.
        public Player(string name)
        {
            // Inicialització dels camps de text i puntuació.
            Name = name;
            Score = 0;

            // Subscripció a l'esdeveniment de colpeig de la bola blanca.
            EventManager.Subscribe(typeof(CueBallActionEvent).Name, OnCueBallStriked);
        }

        // Mètode que s'executa quan rep la notificació que una bola ha estat colpejada.
        private void OnCueBallStriked(object sender, IGameEvent gameEvent)
        {
            CueBallActionEvent actionEvent = (CueBallActionEvent)gameEvent;

            // Si és el torn del jugador i l'estat de l'esdeveniment és "Striked", marquem que ha tirat.
            if (_isPlaying && actionEvent.State == CueBallActionEvent.States.Striked)
                HasStrikedBall = true;
        }

        // Defineix si el jugador passa a estar actiu o inactiu i reseteja l'estat del tir.
        public void SetPlayingState(bool isPlaying)
        {
            _isPlaying = isPlaying;
            HasStrikedBall = false;
        }

        // Calcula i actualitza la puntuació del jugador.
        public void CalculateScore(int score)
        {
            Score += score;

            // La puntuació mai serà negativa; si baixa de 0, es queda en 0.
            if (Score < 0)
                Score = 0;

            // Notifica al sistema (normalment a la UI) que la puntuació ha canviat.
            EventManager.Notify(typeof(ScoreUpdateEvent).Name, this, new ScoreUpdateEvent());
        }

        // Posa la puntuació a zero (útil per reiniciar la partida).
        public void ResetScore()
        {
            Score = 0;
        }
    }
}