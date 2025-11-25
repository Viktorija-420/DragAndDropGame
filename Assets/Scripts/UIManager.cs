using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public Text movesText;
    public GameObject winPanel;
    public Button restartButton;
    
    private int moveCount = 0;

    private void Start()
    {
        // Subscribe to events
        if (GameManager2.Instance != null)
        {
            GameManager2.Instance.OnDiskMoved += OnDiskMoved;
            GameManager2.Instance.OnWin += OnWin;
        }
        
        UpdateMovesText();
        winPanel.SetActive(false);

        // Setup restart button
        if (restartButton != null)
        {
            restartButton.onClick.AddListener(RestartGame);
        }
    }

    private void OnDiskMoved(DiskDrag disk, int fromTower, int toTower)
    {
        moveCount++;
        UpdateMovesText();
    }

    private void OnWin()
    {
        winPanel.SetActive(true);
    }

    private void UpdateMovesText()
    {
        if (movesText != null)
            movesText.text = "Moves: " + moveCount;
    }

    public void RestartGame()
    {
        moveCount = 0;
        winPanel.SetActive(false);
        if (GameManager2.Instance != null)
        {
            GameManager2.Instance.InitializeGame();
        }
        UpdateMovesText();
    }
}