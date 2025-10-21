using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public GameObject endGameScreen;
    public Text timeText;
    public GameObject Stars; // Parent object containing 3 child star GameObjects
    public CameraScript cameraScript; // Pievienojam atsauci uz CameraScript
    public static bool GameEnded { get; private set; } = false;

    private bool gameEnded = false;

    void Start()
    {
        endGameScreen.SetActive(false);
        
        // Automātiski atrodam CameraScript, ja nav piešķirts Inspectorā
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
        GameEnded = true;
        gameEnded = true;
        // Time.timeScale = 0f;

        if (endGameScreen != null)
        {
            endGameScreen.SetActive(true);

            ShowStars(ObjectScript.carsCorrectlyPlaced);
            float time = Mathf.Round(Time.time); // assuming timeText.time is your timer value in seconds
            int roundedTime = Mathf.RoundToInt(time);

            System.TimeSpan timeSpan = System.TimeSpan.FromSeconds(roundedTime);
            timeText.text = string.Format("{0:00}:{1:00}:{2:00}", 
                timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);

            // Resetējam kameru
            ResetCamera();

        }
        else
        {
            Debug.LogWarning("EndGameScreen is not assigned in the inspector.");
        }

        // Time.timeScale = 0f;
        Debug.Log("Game Over: All cars placed.");
    }

    void ShowStars(int carsCorrectlyPlaced)
    {
        // Get all star children under Stars
        Transform[] stars = Stars.GetComponentsInChildren<Transform>(true);
        int starsToShow = 0;

        if (carsCorrectlyPlaced >= 10) starsToShow = 3;
        else if (carsCorrectlyPlaced >= 5) starsToShow = 2;
        else if (carsCorrectlyPlaced >= 2) starsToShow = 1;
        else starsToShow = 0;

        // Start at i = 1 because element 0 is the parent itself
        for (int i = 1; i < stars.Length; i++)
        {
            stars[i].gameObject.SetActive(i <= starsToShow);
        }
    }

    void ResetCamera()
    {
        // Izsaucam kameras resetēšanas metodi
        if (cameraScript != null)
        {
            cameraScript.ResetCamera();
        }
        else
        {
            Debug.LogWarning("CameraScript nav atrasts! Nevar resetēt kameru.");
        }
    }
}