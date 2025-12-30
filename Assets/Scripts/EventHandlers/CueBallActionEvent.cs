namespace ThreeDPool.EventHandlers
{
    // Aquesta estructura defineix l'esdeveniment que gestiona les accions i estats de la bola.
    public struct CueBallActionEvent : IGameEvent
    {
        // Enumeració de tots els estats possibles de la bola durant el joc.
        public enum States
        {
            Default,    // Estat inicial o per defecte.
            Placing,    // La bola s'està posicionant a la taula (reposicionament).
            Striked,    // La bola acaba de rebre l'impacte del taco.
            InMotion,   // La bola es troba actualment en moviment per la taula.
            Stationary, // La bola s'ha aturat completament després de moure's.
        }

        // Variable que emmagatzema l'estat actual de la bola en aquest esdeveniment.
        public States State;
    }
}