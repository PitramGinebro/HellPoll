using UnityEngine;
using ThreeDPool.EventHandlers;
using ThreeDPool.Managers;

namespace ThreeDPool.Controllers
{
    // Aquesta classe centralitza totes les entrades (teclat i ratolí) de l'usuari.
    class InputController : MonoBehaviour
    {
        private void Update()
        {
            // Si l'usuari prem la tecla Escapament (Escape), s'envia l'esdeveniment de pausa.
            if (Input.GetKey(KeyCode.Escape))
            {
                // El joc es pausa.
                EventManager.Notify(typeof(GameInputEvent).Name, this, new GameInputEvent() { State = GameInputEvent.States.Paused });
            }

            // No enviem cap més entrada si el joc està en pausa o en l'estat de preparació (GetSet).
            if (GameManager.Instance.CurrGameState == GameManager.GameState.GetSet ||
                GameManager.Instance.CurrGameState == GameManager.GameState.Pause)
                return;

            float x = 0.0f;
            float y = 0f;

            // Si es manté premut el botó esquerre del ratolí (LMB).
            if (Input.GetMouseButton(0))
            {
                // Es calcula el moviment horitzontal combinant el ratolí (Mouse X) i les tecles (Horizontal: A/D).
                x = Input.GetAxis("Mouse X") - Input.GetAxis("Horizontal");
                // Es calcula el moviment vertical del ratolí per gestionar la força.
                y = Input.GetAxis("Mouse Y");
            }
            // Si es deixa anar el botó esquerre del ratolí.
            else if (Input.GetMouseButtonUp(0))
            {
                // Es notifica l'acció de "Release" per realitzar el tir amb el taco.
                EventManager.Notify(typeof(GameInputEvent).Name, this, new GameInputEvent() { State = GameInputEvent.States.Release });
            }
            else
            {
                // Altres estats d'entrada si fos necessari.
            }

            // Si hi ha hagut moviment en l'eix X, es notifica l'esdeveniment de moviment horitzontal.
            if (x != 0.0f)
                EventManager.Notify(typeof(GameInputEvent).Name, this, new GameInputEvent() { State = GameInputEvent.States.HorizontalAxisMovement, axisOffset = x });

            // Si hi ha hagut moviment en l'eix Y, es notifica l'esdeveniment de moviment vertical.
            if (y != 0.0f)
                EventManager.Notify(typeof(GameInputEvent).Name, this, new GameInputEvent() { State = GameInputEvent.States.VerticalAxisMovement, axisOffset = y });
        }
    }
}