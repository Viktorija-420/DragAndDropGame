using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class EndScreenManager : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject endScreenPanel;
    public Image[] starImages;
    public Text timeText;
    public Text resultText;
    public Button restartButton;
    public Button mainMenuButton;
    
    [Header("Star Settings")]
    public Sprite starFilled;
    public Sprite starEmpty;
    
    [Header("Time Requirements (seconds)")]
    public float threeStarTime = 60f;    // 1 minute
    public float twoStarTime = 90f;      // 1.5 minutes
    // Anything above 90s gets 1 star
    
    private void Start()
    {
        // Hide end screen at start
        if (endScreenPanel != null)
            endScreenPanel.SetActive(false);
        
        // Setup button listeners
        if (restartButton != null)
            restartButton.onClick.AddListener(RestartGame);
            
        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(GoToMainMenu);
    }
    
    public void ShowEndScreen(bool success, float completionTime)
    {
        if (endScreenPanel != null)
        {
            endScreenPanel.SetActive(true);
            UpdateEndScreen(success, completionTime);
        }
    }
    
    private void UpdateEndScreen(bool success, float completionTime)
    {
        if (success)
        {
            // Calculate stars based on completion time
            int stars = CalculateStars(completionTime);
            
            // Update star display
            UpdateStars(stars);
            
            // Update time text
            if (timeText != null)
            {
                timeText.text = "Time: " + FormatTime(completionTime);
            }
            
            // Update result text
            if (resultText != null)
            {
                resultText.text = "Level Complete!\n" + GetStarMessage(stars);
                resultText.color = Color.green;
            }
        }
        else
        {
            // Player failed
            if (resultText != null)
            {
                resultText.text = "Try Again!";
                resultText.color = Color.red;
            }
            
            if (timeText != null)
            {
                timeText.text = "Time: " + FormatTime(completionTime);
            }
            
            // Show 0 stars for failure
            UpdateStars(0);
        }
    }
    
    private int CalculateStars(float completionTime)
    {
        if (completionTime <= threeStarTime)
        {
            return 3; // 3 stars for completing in 1 minute or less
        }
        else if (completionTime <= twoStarTime)
        {
            return 2; // 2 stars for completing in 1-1.5 minutes
        }
        else
        {
            return 1; // 1 star for completing in more than 1.5 minutes
        }
    }
    
    private string GetStarMessage(int stars)
    {
        switch (stars)
        {
            case 3: return "Perfect! 3 Stars!";
            case 2: return "Great! 2 Stars!";
            case 1: return "Good! 1 Star!";
            default: return "Complete!";
        }
    }
    
    private void UpdateStars(int starsCount)
    {
        for (int i = 0; i < starImages.Length; i++)
        {
            if (starImages[i] != null)
            {
                if (i < starsCount)
                {
                    starImages[i].sprite = starFilled;
                    starImages[i].color = Color.yellow;
                }
                else
                {
                    starImages[i].sprite = starEmpty;
                    starImages[i].color = Color.gray;
                }
            }
        }
    }
    
    private string FormatTime(float timeInSeconds)
    {
        int minutes = Mathf.FloorToInt(timeInSeconds / 60f);
        int seconds = Mathf.FloorToInt(timeInSeconds % 60f);
        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }
    
    private void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    
    private void GoToMainMenu()
    {
        // Replace "MainMenu" with your actual main menu scene name
        SceneManager.LoadScene("MainMenu");
    }
}