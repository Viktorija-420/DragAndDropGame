using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EndScreenManager : MonoBehaviour
{
    public GameObject endGameScreen;
    public Text statsText;
    public GameObject Stars;
    public CameraScript cameraScript; // Reference to the camera script

    private bool gameEnded = false;

    void Start()
    {
        endGameScreen.SetActive(false);

        // If cameraScript is not assigned, try to find it
        if (cameraScript == null)
        {
            cameraScript = FindObjectOfType<CameraScript>();
        }
    }

    void Update()
    {
        if (!gameEnded && ObjectScript.carsLeft <= 0)
        {
            EndGame();
        }
    }

    void EndGame()
    {
        gameEnded = true;

        // Reset the camera when game ends
        if (cameraScript != null)
        {
            cameraScript.ResetCamera();
        }
        else
        {
            Debug.LogWarning("CameraScript not found - cannot reset camera");
        }

        if (endGameScreen != null)
        {
            endGameScreen.SetActive(true);
            statsText.text =
                "Time taken: " + FormatTime(Time.time) + "\n" +
                "Cars correctly placed: " + ObjectScript.carsCorrectlyPlaced + "\n" +
                "Cars destroyed: " + ObjectScript.carsDestroyed;

            ShowStars(ObjectScript.carsCorrectlyPlaced);
        }
        else
        {
            Debug.LogWarning("EndGameScreen is not assigned in the inspector.");
        }

        Time.timeScale = 0f;
        Debug.Log("Game Over: All cars placed.");
    }

    void ShowStars(int carsCorrectlyPlaced)
    {
        // Get all star children under Stars
        Transform[] stars = Stars.GetComponentsInChildren<Transform>(true);
        int starsToShow = 0;

        if (carsCorrectlyPlaced >= 11) starsToShow = 3;
        else if (carsCorrectlyPlaced >= 6) starsToShow = 2;
        else if (carsCorrectlyPlaced >= 3) starsToShow = 1;
        else starsToShow = 0;

        // Start at i = 1 because element 0 is the parent itself
        for (int i = 1; i < stars.Length; i++)
        {
            stars[i].gameObject.SetActive(i <= starsToShow);
        }
    }

    // Helper method to format time as mm:ss:mm
    private string FormatTime(float timeInSeconds)
    {
        int hours = Mathf.FloorToInt(timeInSeconds / 3600);
        int minutes = Mathf.FloorToInt((timeInSeconds % 3600) / 60);
        int seconds = Mathf.FloorToInt(timeInSeconds % 60);

        return string.Format("{0:00}:{1:00}:{2:00}", hours, minutes, seconds);
    }

    // Public method to manually reset camera (can be called from UI button)
    public void ResetCameraManually()
    {
        if (cameraScript != null)
        {
            cameraScript.ResetCamera();
        }
    }
}