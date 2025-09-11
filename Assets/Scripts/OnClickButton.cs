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

    public void QuitGame()
    {
        Debug.Log("Byeeee! quiting game...");
        Application.Quit();
    }
}
