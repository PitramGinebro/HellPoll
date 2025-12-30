namespace ThreeDPool.EventHandlers
{
    // Aquesta classe defineix l'esdeveniment que s'activa quan cal actualitzar la puntuació.
    // Implementa IGameEvent per poder ser enviat a través de l'EventManager.
    public class ScoreUpdateEvent : IGameEvent
    {
        // Actualment no conté dades addicionals, ja que serveix com a senyal 
        // perquè els subscriptors (com la UI) consultin les noves puntuacions dels jugadors.
    }
}