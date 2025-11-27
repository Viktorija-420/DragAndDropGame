using System.Collections;
using UnityEngine;
using UnityEngine.Advertisements;
using UnityEngine.UI;

public class RewardedAds : MonoBehaviour, IUnityAdsLoadListener, IUnityAdsShowListener
{
    [SerializeField] string _androidAdUnitId = "Rewarded_Android";
    string _adUnitId;

    [SerializeField] Button _rewardedAdButton;
    public FlyingObjectManager flyingObjectManager;

    private void Awake()
    {
        _adUnitId = _androidAdUnitId;

        if(flyingObjectManager == null)
            flyingObjectManager = FindFirstObjectByType<FlyingObjectManager>();
    }

    public void LoadAd()
    {
        if (!Advertisement.isInitialized)
        {
            Debug.LogWarning("Tried to load rewarded ad before Unity ads was initialized.");
            StartCoroutine(WaitAndLoad(3f));
            return;
        }

        Debug.Log("Loading rewarded ad.");
        Advertisement.Load(_adUnitId, this);
    }

    public void OnUnityAdsAdLoaded(string placementId)
    {
        Debug.Log("Rewarded ad loaded!");

        if(placementId.Equals(_adUnitId)) {
            if (_rewardedAdButton != null)
            {
                _rewardedAdButton.interactable = true;
            }
        }
    }

    public void OnUnityAdsFailedToLoad(string placementId, UnityAdsLoadError error, string message)
    {
        Debug.LogWarning($"Failed to load rewarded ad: {error} - {message}");
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
            Debug.Log("Rewarded ad completed - destroying flying objects!");
            
            if (flyingObjectManager != null)
            {
                flyingObjectManager.DestroyAllFlyingObjects();
                Debug.Log("All flying objects destroyed successfully!");
            }
            else
            {
                Debug.LogWarning("FlyingObjectManager not found! Trying to find it...");
                flyingObjectManager = FindFirstObjectByType<FlyingObjectManager>();
                if (flyingObjectManager != null)
                {
                    flyingObjectManager.DestroyAllFlyingObjects();
                    Debug.Log("Found and destroyed flying objects!");
                }
                else
                {
                    Debug.LogError("Could not find FlyingObjectManager in scene!");
                }
            }
            
            if (_rewardedAdButton != null)
            {
                _rewardedAdButton.interactable = false;
            }
            StartCoroutine(WaitAndLoad(10f));
        }
        else
        {
            Debug.Log("Rewarded ad not completed properly");
            if (_rewardedAdButton != null)
            {
                _rewardedAdButton.interactable = true;
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
        if (_rewardedAdButton != null)
        {
            _rewardedAdButton.interactable = false;
        }
        Advertisement.Show(_adUnitId, this);
    }
}