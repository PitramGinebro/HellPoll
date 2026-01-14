using UnityEngine;
using ThreeDPool.Controllers;

public class PocketsCollider : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        CueBallController ball = other.GetComponent<CueBallController>();
        if (ball != null)
        {
            // Ahora 'BallType' ya no dará error porque es público en el otro script
            ball.BallPocketed();
            Debug.Log("Bola metida: " + ball.BallType);
        }
    }
}