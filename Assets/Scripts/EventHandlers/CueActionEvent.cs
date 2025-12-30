namespace ThreeDPool.EventHandlers
{
    // Aquest sistema d'esdeveniments serà útil per a la implementació de la interfície d'usuari (UI).
    // Defineix les dades que s'envien quan el taco realitza una acció.
    public struct CueActionEvent : IGameEvent
    {
        // Emmagatzema la quantitat de força que el jugador ha acumulat en estirar el taco.
        public float ForceGathered;
    }
}