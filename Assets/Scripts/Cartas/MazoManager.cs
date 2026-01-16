using UnityEngine;
using System.Collections.Generic;

public class MazoManager : MonoBehaviour
{
    [Header("Base de Datos de Cartas")]
    public List<CartaData> todasLasCartasDisponibles; // Arrastra aquí tus archivos de cartas (.asset)

    [Header("Referencias")]
    public ManoManager manoManager; // Arrastra aquí tu objeto Mano_Manager

    void Update()
    {
        // Al pulsar la tecla R, robamos una carta para probar
        if (Input.GetKeyDown(KeyCode.R))
        {
            RobarCartaAleatoria();
        }
    }

    public void RobarCartaAleatoria()
    {
        if (todasLasCartasDisponibles.Count > 0)
        {
            int indice = Random.Range(0, todasLasCartasDisponibles.Count);
            CartaData nuevaCarta = todasLasCartasDisponibles[indice];
            
            // Enviamos la carta a la mano
            if (manoManager != null)
            {
                manoManager.cartaSeleccionada = nuevaCarta;
                Debug.Log("Equipo: Has robado la carta: " + nuevaCarta.nombreCarta);
            }
        }
    }
}