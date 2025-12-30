namespace ThreeDPool.States
{
    /// <summary>
    /// Interfície que defineix l'estructura obligatòria per a qualsevol estat del joc.
    /// Gràcies a aquesta interfície, la FSM pot gestionar qualsevol estat sense saber què fa exactament.
    /// </summary>
    public interface IState
    {
        /// <summary>
        /// S'executa una sola vegada quan la FSM canvia a aquest estat.
        /// S'utilitza per a la configuració inicial (activar objectes, carregar dades).
        /// </summary>
        void OnEnter();

        /// <summary>
        /// S'executa a cada frame (fotograma) mentre l'estat estigui actiu.
        /// Aquí va la lògica principal, com el moviment o el processament d'entrades de l'usuari.
        /// </summary>
        void OnUpdate();

        /// <summary>
        /// S'executa una sola vegada just abans de sortir d'aquest estat cap a un altre.
        /// S'utilitza per a la neteja (desactivar objectes, alliberar memòria).
        /// </summary>
        void OnExit();
    }
}