using System.Collections;
using UnityEngine;
using UnityEngine.Advertisements;
using UnityEngine.UI;

public class RewardedAds2 : MonoBehaviour, IUnityAdsLoadListener, IUnityAdsShowListener
{
    [SerializeField] string _androidAdUnitId = "Rewarded_Android";
    string _adUnitId;

    [SerializeField] Button _rewardedAdButton;

    private void Awake()
    {
        _adUnitId = _androidAdUnitId;
    }

    public void LoadAd()
    {
        if (!Advertisement.isInitialized)
        {
            Debug.LogWarning("Tried to load rewarded ad before Unity ads was initialized.");
            return;
        }

        Debug.Log("Loading rewarded ad.");
        Advertisement.Load(_adUnitId, this);
    }

    public void OnUnityAdsAdLoaded(string placementId)
    {
        Debug.Log("Rewarded ad loaded!");

        if(placementId.Equals(_adUnitId)) {
            _rewardedAdButton.interactable = true;
        }
    }

    public void OnUnityAdsFailedToLoad(string placementId, UnityAdsLoadError error, string message)
    {
        Debug.LogWarning("Failed to load rewarded ad!");
        StartCoroutine(WaitAndLoad(5f));
    }

    public IEnumerator WaitAndLoad(float delay)
    {
        yield return new WaitForSeconds(delay);
        LoadAd();
    }

    public void OnUnityAdsShowFailure(string placementId, UnityAdsShowError error, string message)
    {
        Debug.LogWarning("Failed to show rewarded ad!");
        StartCoroutine(WaitAndLoad(5f));
    }

    public void OnUnityAdsShowStart(string placementId)
    {
        Time.timeScale = 0f;
    }

    public void OnUnityAdsShowClick(string placementId)
    {
        Debug.Log("User clicked on rewarded ad");
    }

    public void OnUnityAdsShowComplete(string placementId, UnityAdsShowCompletionState showCompletionState)
    {
        Time.timeScale = 1f;

        if (placementId.Equals(_adUnitId) && showCompletionState == UnityAdsShowCompletionState.COMPLETED) 
        {
            Debug.Log("Rewarded ad completed!");
            
            // Reduce moves by 5 when ad is completely watched
            ReduceMovesBy5();
                
            _rewardedAdButton.interactable = false;
            StartCoroutine(WaitAndLoad(10f));
        }
        else
        {
            Debug.Log("Rewarded ad not completed properly");
            _rewardedAdButton.interactable = true;
        }
    }

    // Improved method: Reduce moves by 5 with better error handling
    private void ReduceMovesBy5()
    {
        Debug.Log("Attempting to reduce moves by 5...");
        
        // Method 1: Try to find UIManager in scene
        UIManager uiManager = FindObjectOfType<UIManager>();
        
        if (uiManager != null)
        {
            Debug.Log("UIManager found! Calling ReduceMoves(5)");
            uiManager.ReduceMoves(5);
        }
        else
        {
            Debug.LogError("UIManager not found in scene!");
            
            // Method 2: Try alternative approach - find GameManager and access moves
            GameManager2 gameManager = FindObjectOfType<GameManager2>();
            if (gameManager != null)
            {
                Debug.Log("GameManager2 found, but cannot access moves directly");
            }
            
            // Method 3: Try to find any object with move counting capability
            MonoBehaviour[] allObjects = FindObjectsOfType<MonoBehaviour>();
            foreach (MonoBehaviour obj in allObjects)
            {
                if (obj.GetType().Name.Contains("UI") || obj.GetType().Name.Contains("Manager"))
                {
                    Debug.Log($"Found potential manager: {obj.GetType().Name}");
                }
            }
        }
    }

    public void SetButton(Button button)
    {
        if (button == null)
        {
            return;
        }
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(ShowAd);
        _rewardedAdButton = button;
        _rewardedAdButton.interactable = false;
    }

    public void ShowAd()
    {
        Debug.Log("ShowAd called - button clicked");
        _rewardedAdButton.interactable = false;
        Advertisement.Show(_adUnitId, this);
    }

    // Add this for testing without ads
    public void TestReduceMoves()
    {
        Debug.Log("TEST: Manually reducing moves by 5");
        ReduceMovesBy5();
    }
}