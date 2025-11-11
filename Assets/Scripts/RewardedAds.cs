using UnityEngine;
using UnityEngine.Advertisements;
using UnityEngine.UI;
using System.Collections;

public class RewardedAds : MonoBehaviour, IUnityAdsLoadListener, IUnityAdsShowListener
{
    [SerializeField] string _androidAdUnitId = "Rewarded_Android";
    string _adUnitId;

    [SerializeField] Button _rewardedAdButton;
    public FlyingObjectManager flyingObjectManager;

    private bool isOnCooldown = false;
    private float cooldownTime = 10f; // 10 seconds cooldown
    private Image buttonImage;
    private Text buttonText;

    public void Awake()
    {
        _adUnitId = _androidAdUnitId;

        if (flyingObjectManager == null)
            flyingObjectManager = FindFirstObjectByType<FlyingObjectManager>();

        // Get button components for visual effects
        if (_rewardedAdButton != null)
        {
            buttonImage = _rewardedAdButton.GetComponent<Image>();
            buttonText = _rewardedAdButton.GetComponentInChildren<Text>();
        }

        Debug.Log($"RewardedAds Awake - FlyingObjectManager: {flyingObjectManager != null}");
    }

    public void LoadAd()
    {
        if (!Advertisement.isInitialized)
        {
            Debug.Log("Tried to load ad before initialization.");
            return;
        }

        Debug.Log($"Loading rewarded ad with ID: {_adUnitId}");
        Advertisement.Load(_adUnitId, this);
    }

    public void OnUnityAdsAdLoaded(string placementId)
    {
        Debug.Log($"Rewarded ad loaded: {placementId}");

        if (placementId.Equals(_adUnitId) && _rewardedAdButton != null && !isOnCooldown)
        {
            SetButtonActive(true);
            Debug.Log("Rewarded button set to interactable");
        }
    }

    public void OnUnityAdsFailedToLoad(string placementId, UnityAdsLoadError error, string message)
    {
        Debug.LogWarning($"Error loading rewarded ad: {error} - {message}");
        StartCoroutine(WaitAndLoad(5f));
    }

    public IEnumerator WaitAndLoad(float delay)
    {
        yield return new WaitForSeconds(delay);
        LoadAd();
    }

    public void OnUnityAdsShowFailure(string placementId, UnityAdsShowError error, string message)
    {
        Debug.LogWarning($"Failed to show rewarded ad: {error} - {message}");
        StartCoroutine(WaitAndLoad(5f));
    }

    public void OnUnityAdsShowStart(string placementId)
    {
        Debug.Log("Rewarded ad show start");
        Time.timeScale = 0f;
    }

    public void OnUnityAdsShowClick(string placementId)
    {
        Debug.Log("User clicked on rewarded ad.");
    }

    public void OnUnityAdsShowComplete(string placementId, UnityAdsShowCompletionState showCompletionState)
    {
        Debug.Log($"Rewarded ad show complete: {placementId}, State: {showCompletionState}");

        if (placementId.Equals(_adUnitId) && showCompletionState.Equals(UnityAdsShowCompletionState.COMPLETED))
        {
            Debug.Log("Rewarded ad completed successfully!");
            
            if (flyingObjectManager != null)
            {
                Debug.Log("Calling DestroyAllFlyingObjects...");
                flyingObjectManager.DestroyAllFlyingObjects();
            }
            else
            {
                Debug.LogError("FlyingObjectManager is null!");
            }

            // Start cooldown
            StartCoroutine(StartCooldown());
        }
        else
        {
            Debug.LogWarning($"Ad not completed: {showCompletionState}");
        }

        Time.timeScale = 1f;
    }

    private IEnumerator StartCooldown()
    {
        isOnCooldown = true;
        SetButtonActive(false);
        
        float timer = cooldownTime;
        
        while (timer > 0f)
        {
            // Update button text with countdown
            if (buttonText != null)
            {
                buttonText.text = Mathf.CeilToInt(timer).ToString();
            }
            
            timer -= Time.deltaTime;
            yield return null;
        }
        
        // Cooldown finished
        isOnCooldown = false;
        
        // Reset button text
        if (buttonText != null)
        {
            buttonText.text = " ";
        }
        
        // Reload ad for next use
        LoadAd();
    }

    private void SetButtonActive(bool active)
    {
        if (_rewardedAdButton != null)
        {
            _rewardedAdButton.interactable = active;
            
            // Make button see-through during cooldown
            if (buttonImage != null)
            {
                Color color = buttonImage.color;
                color.a = active ? 1f : 0.3f; // Full opacity when active, transparent when on cooldown
                buttonImage.color = color;
            }
            
            if (buttonText != null)
            {
                Color textColor = buttonText.color;
                textColor.a = active ? 1f : 0.5f; // Make text slightly transparent too
                buttonText.color = textColor;
            }
        }
    }

    public void SetButton(Button button)
    {
        if (button == null)
        {
            Debug.LogWarning("Tried to set null button");
            return;
        }

        _rewardedAdButton = button;
        buttonImage = button.GetComponent<Image>();
        buttonText = button.GetComponentInChildren<Text>();
        
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(ShowAd);
        SetButtonActive(false);
        Debug.Log("Rewarded button set up successfully");
    }
    
    public void ShowAd()
    {
        if (isOnCooldown)
        {
            Debug.Log("Button is on cooldown, cannot show ad");
            return;
        }

        Debug.Log("ShowAd called");

        SetButtonActive(false);

        if (Advertisement.isInitialized)
        {
            Debug.Log($"Showing rewarded ad: {_adUnitId}");
            Advertisement.Show(_adUnitId, this);
        }
        else
        {
            Debug.LogWarning("Ads not initialized");
        }
    }
}