using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public GameObject endGameScreen;
    public Text statsText;
    public GameObject Stars;
    public CameraScript cameraScript; // Reference to the camera script

    private bool gameEnded = false;
    private float startTime;

    void Start()
    {
        endGameScreen.SetActive(false);
        startTime = Time.time;

        // If cameraScript is not assigned, try to find it
        if (cameraScript == null)
        {
            cameraScript = FindObjectOfType<CameraScript>();
        }
    }

    void Update()
    {
        // End game if at least one car is destroyed
        if (!gameEnded && ObjectScript.carsDestroyed > 0)
        {
            EndGame(0); // 0 stars
        }

        // End game if all cars placed
        else if (!gameEnded && ObjectScript.carsLeft <= 0)
        {
            float totalTime = Time.time - startTime;
            int stars = CalculateStars(totalTime);
            EndGame(stars);
        }
    }

    void EndGame(int starsToShow)
    {
        gameEnded = true;

        // Reset camera
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

            float totalTime = Time.time - startTime;
            statsText.text = FormatTime(totalTime);

            ShowStars(starsToShow);
        }
        else
        {
            Debug.LogWarning("EndGameScreen is not assigned in the inspector.");
        }

        Time.timeScale = 0f;
        Debug.Log("Game Over");
    }

    int CalculateStars(float time)
    {
        if (time < 120f) // less than 2 minutes
            return 3;
        else if (time < 180f) // less than 3 minutes
            return 2;
        else if (ObjectScript.carsLeft <= 0)
            return 1;
        else
            return 0;
    }

    void ShowStars(int starsToShow)
    {
        Transform[] stars = Stars.GetComponentsInChildren<Transform>(true);

        // Start at i = 1 because element 0 is the parent itself
        for (int i = 1; i < stars.Length; i++)
        {
            stars[i].gameObject.SetActive(i <= starsToShow);
        }
    }

    // Helper method to format time as mm:ss
    private string FormatTime(float timeInSeconds)
    {
        int hours = Mathf.FloorToInt(timeInSeconds / 3600);
        int minutes = Mathf.FloorToInt(timeInSeconds % 3600 / 60);
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
