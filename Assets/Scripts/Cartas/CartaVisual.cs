using UnityEngine;
using UnityEngine.UI;
using TMPro; // Asegúrate de tener instalado TextMeshPro

public class CartaVisual : MonoBehaviour
{
    [Header("Referencias del Sistema")]
    public ManoManager manoManager; // Arrastra el objeto que tiene el script ManoManager

    [Header("Componentes de la Interfaz (UI)")]
    public TextMeshProUGUI textoNombre; // El objeto de texto del nombre
    public Image iconoCarta;           // El objeto de imagen del icono
    public GameObject contenedorVisual; // El objeto 'Padre' que quieres ocultar/mostrar (ej: el Panel)

    void Update()
    {
        // 1. COMPROBACIÓN DE SEGURIDAD
        // Si no hay manager o no hay carta seleccionada...
        if (manoManager == null || manoManager.cartaSeleccionada == null)
        {
            // Ocultamos el visual para que no aparezca un cuadro vacío
            if (contenedorVisual != null) 
                contenedorVisual.SetActive(false);
            
            return; // Salimos de la función para no ejecutar el código de abajo
        }

        // 2. ACTIVAR VISUAL
        // Si llegamos aquí, significa que SÍ hay una carta en la mano
        if (contenedorVisual != null && !contenedorVisual.activeSelf)
        {
            contenedorVisual.SetActive(true);
        }

        // 3. ACTUALIZAR DATOS DE LA CARTA
        CartaData cartaActual = manoManager.cartaSeleccionada;

        // Escribimos el nombre si el hueco no está vacío
        if (textoNombre != null)
        {
            textoNombre.text = cartaActual.nombreCarta;
        }

        // Ponemos el icono si el hueco no está vacío y la carta tiene imagen
        if (iconoCarta != null && cartaActual.imagenIcono != null)
        {
            iconoCarta.sprite = cartaActual.imagenIcono;
        }
    }
}