using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("Game Settings")]
    public float threeStarTime = 60f; // 1 minute for 3 stars
    public float twoStarTime = 120f;  // 2 minutes for 2 stars
    
    [Header("UI References")]
    public GameObject endGamePanel;
    public Image[] stars;
    public Text timeText;
    public Text resultText;
    
    [Header("Vehicle Tracking")]
    public GameObject[] vehicles; // Assign all draggable vehicles in inspector
    
    private bool gameEnded = false;
    private float gameStartTime;
    private int vehiclesPlaced = 0;
    private int vehiclesDestroyed = 0;
    private int totalVehicles;
    private FlyingObjectSpawnScript spawner;
    
    // Singleton pattern for easy access
    public static GameManager Instance { get; private set; }
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        totalVehicles = vehicles.Length;
        gameStartTime = Time.time;
        
        // Find the spawner but don't stop it yet
        spawner = FindObjectOfType<FlyingObjectSpawnScript>();
        
        if (endGamePanel != null)
            endGamePanel.SetActive(false);
        
        // Initialize all stars as inactive
        if (stars != null)
        {
            foreach (Image star in stars)
            {
                if (star != null)
                    star.gameObject.SetActive(false);
            }
        }
        
        Debug.Log($"Game started with {totalVehicles} vehicles to place");
        Debug.Log($"Flying object spawner found: {spawner != null}");
    }
    
    void Update()
    {
        if (!gameEnded)
        {
            CheckGameCompletion();
        }
    }
    
    public void VehiclePlaced()
    {
        if (gameEnded) return;
        
        vehiclesPlaced++;
        Debug.Log($"Vehicle placed: {vehiclesPlaced}/{totalVehicles}");
        CheckGameCompletion();
    }
    
    public void VehicleDestroyed()
    {
        if (gameEnded) return;
        
        vehiclesDestroyed++;
        Debug.Log($"Vehicle destroyed: {vehiclesDestroyed}/{totalVehicles}");
        CheckGameCompletion();
    }
    
    private void CheckGameCompletion()
    {
        if (gameEnded) return;
        
        // All vehicles placed correctly
        if (vehiclesPlaced >= totalVehicles)
        {
            Debug.Log("All vehicles placed - ending game with stars");
            EndGameWithStars();
        }
        // All vehicles destroyed
        else if (vehiclesDestroyed >= totalVehicles)
        {
            Debug.Log("All vehicles destroyed - ending game with 0 stars");
            EndGameWithZeroStars();
        }
        // Some placed, some destroyed - check if all are accounted for
        else if ((vehiclesPlaced + vehiclesDestroyed) >= totalVehicles)
        {
            Debug.Log("Mixed completion - ending game with stars");
            EndGameWithStars();
        }
    }
    
    private void EndGameWithStars()
    {
        if (gameEnded) return;
        
        gameEnded = true;
        float gameTime = Time.time - gameStartTime;
        int starCount = CalculateStars(gameTime);
        
        StartCoroutine(ShowEndGameScreen(starCount, gameTime, "Mission Complete!"));
    }
    
    private void EndGameWithZeroStars()
    {
        if (gameEnded) return;
        
        gameEnded = true;
        float gameTime = Time.time - gameStartTime;
        
        StartCoroutine(ShowEndGameScreen(0, gameTime, "All Vehicles Destroyed!"));
    }
    
    private int CalculateStars(float gameTime)
    {
        if (gameTime <= threeStarTime)
            return 3;
        else if (gameTime <= twoStarTime)
            return 2;
        else
            return 1;
    }
    
    private IEnumerator ShowEndGameScreen(int starCount, float gameTime, string resultMessage)
    {
        // Wait a moment before showing end screen
        yield return new WaitForSeconds(1f);
        
        // Stop flying object spawning only when game ends
        if (spawner != null)
        {
            // Use the existing CancelInvoke calls that work with your script
            spawner.CancelInvoke("SpawnCloud");
            spawner.CancelInvoke("SpawnObject");
            Debug.Log("Stopped all flying object spawning");
        }
        
        // Set up the end game panel if references are set
        if (endGamePanel != null)
        {
            endGamePanel.SetActive(true);
            
            if (resultText != null)
                resultText.text = resultMessage;
            
            if (timeText != null)
            {
                // Format time as minutes:seconds
                int minutes = Mathf.FloorToInt(gameTime / 60f);
                int seconds = Mathf.FloorToInt(gameTime % 60f);
                timeText.text = string.Format("Time: {0:00}:{1:00}", minutes, seconds);
            }
            
            // Animate stars appearing if stars array is set
            if (stars != null && stars.Length >= 3)
            {
                for (int i = 0; i < starCount; i++)
                {
                    yield return new WaitForSeconds(0.5f);
                    if (stars[i] != null)
                    {
                        stars[i].gameObject.SetActive(true);
                        // Add a little animation
                        StartCoroutine(AnimateStar(stars[i].transform));
                    }
                }
            }
        }
        
        // Disable dragging after game ends
        ObjectScript.drag = false;
    }
    
    private IEnumerator AnimateStar(Transform starTransform)
    {
        Vector3 originalScale = starTransform.localScale;
        starTransform.localScale = Vector3.zero;
        
        float duration = 0.5f;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float scale = Mathf.Lerp(0f, 1f, elapsed / duration);
            starTransform.localScale = originalScale * scale;
            yield return null;
        }
        
        starTransform.localScale = originalScale;
    }
    
    // Method to restart the game
    public void RestartGame()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
    }
}