using UnityEngine;
using ThreeDPool.Controllers;
using ThreeDPool.Managers;

namespace ThreeDPool
{
    // Aquesta classe detecta boles que toquen els límits exteriors o parets de la taula.
    public class TableWallCollider : MonoBehaviour
    {
        // Mètode que s'executa contínuament mentre un objecte roman dins del Trigger.
        private void OnTriggerStay(Collider collider)
        {
            // Intenta obtenir el component CueBallController de l'objecte detectat.
            CueBallController cueBallController = collider.gameObject.GetComponent<CueBallController>();

            // Si l'objecte és una bola i el seu Rigidbody s'ha aturat (està en mode "Sleeping").
            if (cueBallController != null && cueBallController.GetComponent<Rigidbody>().IsSleeping())
            {
                // Avisa al GameManager per afegir aquesta bola a la llista de boles fora de la taula.
                GameManager.Instance.AddToBallHitOutList(cueBallController);
            }
        }
    }
}