using UnityEngine;
using ThreeDPool.Managers;
using ThreeDPool.Controllers;

public class ManoManager : MonoBehaviour
{
    public CartaData cartaSeleccionada;

    void Update()
    {
        // Solo intentamos disparar el rayo si tenemos una carta en la mano
        if (Input.GetMouseButtonDown(0) && cartaSeleccionada != null)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                // Intentamos obtener el script de la bola que tocamos
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
        // Aplicamos el efecto de la carta a la bola
        bola.AplicarCarta(cartaSeleccionada);
        Debug.Log("Equipo: ¡Carta aplicada con éxito!");

        // IMPORTANTE: Al usar la carta, la quitamos de la mano (así la UI desaparece)
        cartaSeleccionada = null; 
    }
}