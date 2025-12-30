using UnityEngine;
using ThreeDPool.Controllers;

namespace ThreeDPool
{
    // Aquesta classe gestiona la col·lisió de les boles amb els forats de la taula.
    class PocketsCollider : MonoBehaviour
    {
        // Mètode que s'executa automàticament quan un objecte entra dins del Trigger del forat.
        private void OnTriggerEnter(Collider collider)
        {
            // Intenta obtenir el component CueBallController de l'objecte que ha entrat al forat.
            CueBallController cueBall = collider.gameObject.GetComponent<CueBallController>();

            // Si l'objecte té el component (és a dir, és una bola), crida al mètode BallPocketed.
            if (cueBall != null)
                cueBall.BallPocketed();
        }
    }
}