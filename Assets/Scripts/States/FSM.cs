using System.Collections.Generic;
using UnityEngine;

namespace ThreeDPool.States
{
    /// <summary>
    /// Aquesta classe implementa una Màquina d'Estats Finits (FSM).
    /// Permet gestionar diferents estats de joc o de boles de manera organitzada.
    /// </summary>
    public class FSM : MonoBehaviour
    {
        // Llista per emmagatzemar tots els estats disponibles en aquesta màquina.
        private List<IState> _states;

        // Referència a l'estat que s'està executant actualment.
        private IState _currentState;

        /// <summary>
        /// Afegeix un nou estat a la llista si no ha estat afegit prèviament.
        /// </summary>
        public void AddState(IState state)
        {
            // Només l'afegim si no n'hi ha cap altre del mateix tipus ja registrat.
            if (_states.Find(s => s.GetType() == state.GetType()) == null)
                _states.Add(state);
        }

        /// <summary>
        /// Gestiona la transició d'un estat a un altre.
        /// </summary>
        public void ChangeStateTo(IState newState)
        {
            // Si ja estem en aquest estat, no fem res.
            if (newState == _currentState)
                return;

            // Si hi ha un estat actual actiu, executem la seva lògica de sortida (OnExit).
            if (_currentState != null)
                _currentState.OnExit();

            // Assignem el nou estat i executem la seva lògica d'entrada (OnEnter).
            if (newState != null)
            {
                _currentState = newState;
                _currentState.OnEnter();
            }
        }

        /// <summary>
        /// Crida el mètode d'actualització de l'estat actual en cada frame de Unity.
        /// </summary>
        public void Update()
        {
            // Si tenim un estat actiu, l'actualitzem.
            if (_currentState != null)
                _currentState.OnUpdate();
        }
    }
}