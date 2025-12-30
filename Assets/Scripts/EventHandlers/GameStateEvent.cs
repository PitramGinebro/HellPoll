namespace ThreeDPool.EventHandlers
{
    // Aquesta classe defineix l'esdeveniment que comunica els canvis en l'estat global del joc.
    public class GameStateEvent : IGameEvent
    {
        // Enumeració dels diferents modes o estats en què es pot trobar la partida.
        public enum State
        {
            Practise, // Mode pràctica (sense regles de competició).
            Play,     // Mode de joc actiu (partida normal).
            Complete  // El joc ha finalitzat.
        }

        // Emmagatzema l'estat actual del joc.
        public State GameState;

        // Emmagatzema el nom del jugador que està jugant actualment.
        public string CurrPlayer;
    }
}