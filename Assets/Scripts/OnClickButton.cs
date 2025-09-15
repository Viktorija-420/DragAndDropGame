using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    // Method to load a new scene
    public void LoadCityMenu()
    {
        SceneManager.LoadScene("CityScene");
    }

    public void LoadMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void LoadGame2()
    {
        SceneManager.LoadScene("Game2");
    }

    public void QuitGame()
    {
        Debug.Log("Byeeee! quiting game...");
        Application.Quit();
    }
}
