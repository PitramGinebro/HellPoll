using System;
using System.Collections.Generic;
using ThreeDPool.EventHandlers;
using UnityEngine;

namespace ThreeDPool.Managers
{
    /// <summary>
    /// La responsabilitat d'aquesta classe és registrar esdeveniments i despatxar-los.
    /// Per ara és un gestor d'esdeveniments bàsic.
    /// Això ajuda a assegurar que hem registrat i eliminat tots els esdeveniments correctament
    /// i que no queden referències penjant.
    /// </summary>
    public static class EventManager
    {
        // Diccionari per emmagatzemar els subscriptors de cada esdeveniment (identificats per una cadena de text).
        private static Dictionary<string, Action<object, IGameEvent>> _subscribers = new Dictionary<string, Action<object, IGameEvent>>();

        // Permet que un objecte es subscrigui a un esdeveniment concret per rebre notificacions.
        public static void Subscribe(string eventID, Action<object, IGameEvent> callback)
        {
            // Si l'esdeveniment ja existeix al diccionari, afegim el nou mètode de resposta (callback).
            if (_subscribers.ContainsKey(eventID))
                _subscribers[eventID] += callback;
            // Si és el primer subscriptor, creem la nova entrada al diccionari.
            else
                _subscribers.Add(eventID, callback);
        }

        // Permet que un objecte es des-subscrigui per deixar de rebre notificacions i alliberar memòria.
        public static void Unsubscribe(string eventID, Action<object, IGameEvent> callback)
        {
            // Si l'esdeveniment existeix, restem el callback del delegat d'accions.
            if (_subscribers.ContainsKey(eventID))
                _subscribers[eventID] -= callback;
        }

        // Envia una notificació a tots els subscriptors d'un esdeveniment específic.
        public static void Notify(string eventID, object sender, IGameEvent gameEvent)
        {
            // Només enviem la notificació si hi ha algú escoltant (si l'ID existeix al diccionari).
            if (_subscribers.ContainsKey(eventID))
            {
                // Deixem que això pugui llançar una excepció per detectar errors durant el desenvolupament.
                Action<object, IGameEvent> selectedCallback = _subscribers[eventID];

                // Comprovem que el callback no sigui nul mitjançant una asseveració.
                Debug.Assert(selectedCallback != null, "No hi ha esdeveniments subscrits per a: " + eventID);

                // Executem tots els mètodes subscrits passant l'emissor i les dades de l'esdeveniment.
                selectedCallback(sender, gameEvent);
            }
        }
    }
}