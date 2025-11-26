using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class UIManager : MonoBehaviour
{
    [Header("UI References")]
    public Text movesText;
    public Text timerText;
    public GameObject winPanel;
    public Button restartButton;
    public Button mainMenuButton;
    
    [Header("Win Panel Display")]
    public Text finalTimeText;
    public Text finalMovesText;
    
    [Header("Star Rating")]
    public GameObject[] starIcons;
    public GameObject[] starEmptySlots;
    
    [Header("Time Limits for Stars (in seconds)")]
    public float threeStarTime = 150f;
    public float twoStarTime = 270f;
    
    [Header("Scene Names")]
    public string mainMenuSceneName = "MainMenu";

    private int moveCount = 0;
    private float gameTime = 0f;
    private bool isPlaying = true;
    private Coroutine timerCoroutine;
    private int starsEarned = 0;

    private void Start()
    {
        InitializeUI();
        SubscribeToEvents();
        StartTimer();
    }

    private void InitializeUI()
    {
        if (winPanel != null)
            winPanel.SetActive(false);
        
        UpdateMovesText();
        UpdateTimerText();
        InitializeStars();

        if (restartButton != null)
            restartButton.onClick.AddListener(RestartGame);

        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(GoToMainMenu);
        else
            Debug.LogWarning("Main Menu Button not assigned in UIManager!");
    }

    public void GoToMainMenu()
    {
        Debug.Log("Loading main menu...");
        
        if (timerCoroutine != null)
            StopCoroutine(timerCoroutine);
        
        if (!string.IsNullOrEmpty(mainMenuSceneName))
            SceneManager.LoadScene(mainMenuSceneName);
        else
            SceneManager.LoadScene("MainMenu");
    }

    // ADD THIS MISSING METHOD
    private void InitializeStars()
    {
        // Hide all filled stars at start
        if (starIcons != null)
        {
            foreach (var star in starIcons)
            {
                if (star != null)
                    star.SetActive(false);
            }
        }
        
        // Show all empty star slots
        if (starEmptySlots != null)
        {
            foreach (var emptyStar in starEmptySlots)
            {
                if (emptyStar != null)
                    emptyStar.SetActive(true);
            }
        }
    }

    private void SubscribeToEvents()
    {
        if (GameManager2.Instance != null)
        {
            GameManager2.Instance.OnDiskMoved += OnDiskMoved;
            GameManager2.Instance.OnWin += OnWin;
        }
        else
        {
            Debug.LogWarning("GameManager2 instance not found!");
        }
    }

    private void StartTimer()
    {
        isPlaying = true;
        gameTime = 0f;
        starsEarned = 0;
        
        if (timerCoroutine != null)
            StopCoroutine(timerCoroutine);
            
        timerCoroutine = StartCoroutine(TimerRoutine());
    }

    private IEnumerator TimerRoutine()
    {
        while (isPlaying)
        {
            if (timerText != null)
            {
                gameTime += Time.deltaTime;
                UpdateTimerText();
            }
            yield return null;
        }
    }

    private void OnDiskMoved(DiskDrag disk, int fromTower, int toTower)
    {
        moveCount++;
        UpdateMovesText();
    }

    private void OnWin()
    {
        isPlaying = false;
        CalculateStars();
        ShowStars();
        UpdateFinalStats();
        
        if (winPanel != null)
            winPanel.SetActive(true);
    }

    private void CalculateStars()
    {
        if (gameTime <= threeStarTime)
        {
            starsEarned = 3;
        }
        else if (gameTime <= twoStarTime)
        {
            starsEarned = 2;
        }
        else
        {
            starsEarned = 1;
        }
        
        Debug.Log($"Completed in {FormatTime(gameTime)} - Earned {starsEarned} stars!");
    }

    private void ShowStars()
    {
        if (starIcons != null)
        {
            for (int i = 0; i < starIcons.Length; i++)
            {
                if (starIcons[i] != null)
                {
                    starIcons[i].SetActive(i < starsEarned);
                }
            }
        }
        
        if (starEmptySlots != null)
        {
            for (int i = 0; i < starEmptySlots.Length; i++)
            {
                if (starEmptySlots[i] != null)
                {
                    starEmptySlots[i].SetActive(i >= starsEarned);
                }
            }
        }
    }

    private void UpdateFinalStats()
    {
        // Update the final time display on win panel
        if (finalTimeText != null)
        {
            finalTimeText.text = "Time: " + FormatTime(gameTime);
        }
        
        // Update final moves display on win panel (optional)
        if (finalMovesText != null)
        {
            finalMovesText.text = "Moves: " + moveCount;
        }
        
        // Keep the gameplay UI updated too
        if (movesText != null)
            movesText.text = "Moves: " + moveCount;
            
        if (timerText != null)
            timerText.text = "Time: " + FormatTime(gameTime);
    }

    private void UpdateMovesText()
    {
        if (movesText != null && isPlaying)
            movesText.text = "Moves: " + moveCount;
    }

    private void UpdateTimerText()
    {
        if (timerText != null && isPlaying)
            timerText.text = "Time: " + FormatTime(gameTime);
    }

    private string FormatTime(float timeInSeconds)
    {
        int minutes = Mathf.FloorToInt(timeInSeconds / 60f);
        int seconds = Mathf.FloorToInt(timeInSeconds % 60f);
        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    public void RestartGame()
    {
        moveCount = 0;
        gameTime = 0f;
        starsEarned = 0;
        
        if (winPanel != null)
            winPanel.SetActive(false);
        
        InitializeStars(); // This should work now
        
        if (GameManager2.Instance != null)
            GameManager2.Instance.InitializeGame();
        
        UpdateMovesText();
        UpdateTimerText();
        StartTimer();
    }

    private void OnDestroy()
    {
        if (GameManager2.Instance != null)
        {
            GameManager2.Instance.OnDiskMoved -= OnDiskMoved;
            GameManager2.Instance.OnWin -= OnWin;
        }
        
        if (timerCoroutine != null)
            StopCoroutine(timerCoroutine);
    }
}