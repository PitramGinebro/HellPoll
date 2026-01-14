using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{   
    public GameObject optionsMenu;
    public GameObject mainMenu;
    
    public void OpenMainMenuPanel()
    {
        mainMenu.SetActive(true);
        optionsMenu.SetActive(false);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void playgame()
    {
        // Cargamos la escena directamente
        SceneManager.LoadScene("level1");
    }
}