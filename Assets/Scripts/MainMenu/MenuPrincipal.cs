using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuPrincipal : MonoBehaviour
{
    [Header("Configuración de Paneles")]
    public GameObject panelMenu;    // El panel que queremos cerrar
    public GameObject playerPanel;  // El panel de controles/HUD que queremos abrir

    // Esta función se activará cuando pulses el botón PLAY
    public void Jugar()
    {
        // 1. Desactivamos el menú principal
        if (panelMenu != null)
        {
            panelMenu.SetActive(false);
            Debug.Log("Menú desactivado");
        }

        // 2. Activamos la interfaz del jugador (HUD/Controles)
        if (playerPanel != null)
        {
            playerPanel.SetActive(true);
            Debug.Log("HUD activado");
        }
        
        // Opcional: Si el juego empieza pausado, lo reanudamos
        Time.timeScale = 1f;
    }

    public void Salir()
    {
        Debug.Log("Saliendo del juego...");
        Application.Quit();
    }
}