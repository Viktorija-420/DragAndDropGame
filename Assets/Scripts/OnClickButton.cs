using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    // Method to load a new scene
    public void LoadCityMenu()
    {
        SceneManager.LoadScene("CityScene");
        Time.timeScale = 1f;
        ObjectScript.carsLeft = 12;
        ObjectScript.carsCorrectlyPlaced = 0;
        ObjectScript.carsDestroyed = 0;
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
        Time.timeScale = 1f;
        ObjectScript.carsLeft = 12;
        ObjectScript.carsCorrectlyPlaced = 0;
        ObjectScript.carsDestroyed = 0;
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    
    }
}
