using UnityEngine;
using ThreeDPool.Managers;
using ThreeDPool.Controllers;

public class ManoManager : MonoBehaviour
{
    // Aquí arrastra una carta desde tu carpeta de Assets para probar
    public CartaData cartaSeleccionada;

    void Update()
    {
        // Si haces clic izquierdo
        if (Input.GetMouseButtonDown(0) && cartaSeleccionada != null)
        {
            // Lanzamos un rayo desde la cámara hacia donde está el ratón
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                // Si tocamos algo que tiene el script EstadoBola
                EstadoBola estado = hit.collider.GetComponent<EstadoBola>();
                
                if (estado != null)
                {
                    AplicarCartaABola(estado);
                }
            }
        }
    }

    void AplicarCartaABola(EstadoBola bola)
    {
        if (cartaSeleccionada.tipoCarta == TipoCarta.Pasiva_Roja)
        {
            bola.AplicarCarta(cartaSeleccionada);
            Debug.Log("¡Carta " + cartaSeleccionada.nombreCarta + " aplicada a la bola!");
            
            // Opcional: Quitar la carta de la mano tras usarla
            // cartaSeleccionada = null; 
        }
    }
}