namespace ThreeDPool.EventHandlers
{
    // Aquesta estructura defineix l'esdeveniment d'entrada (input) del joc.
    public struct GameInputEvent : IGameEvent
    {
        // Aquests estats tenen noms molt genèrics per evitar confusions 
        // a l'hora de portar el joc a altres plataformes.
        public enum States
        {
            Default,                // Estat per defecte.
            HorizontalAxisMovement, // Moviment en l'eix horitzontal (ex: girar el taco).
            VerticalAxisMovement,   // Moviment en l'eix vertical (ex: força del tir).
            Release,                // Acció de deixar anar el botó o tecla (executar tir).
            Paused                  // Acció de pausar el joc.
        }

        // Emmagatzema el valor del moviment dels eixos (de -1.0 a 1.0).
        public float axisOffset;

        // L'estat d'entrada actual que s'està enviant.
        public States State;
    }
}