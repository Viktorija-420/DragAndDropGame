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
    public void RestartGame()
    {
        Time.timeScale = 1f; // Reset time scale in case it was changed
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }
}
