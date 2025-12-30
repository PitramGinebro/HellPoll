namespace ThreeDPool.EventHandlers
{
    // Aquesta és la interfície base per a tots els esdeveniments del joc.
    // Funciona com una "etiqueta" per identificar qualsevol classe o struct com un esdeveniment vàlid.
    public interface IGameEvent
    {
        // No conté mètodes ni propietats; s'utilitza per al polimorfisme en el sistema d'esdeveniments.
    }
}