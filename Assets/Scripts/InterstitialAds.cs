using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Advertisements;
using UnityEngine.UI;

public class InterstitialAd : MonoBehaviour, IUnityAdsLoadListener, IUnityAdsShowListener
{
    [SerializeField] string _androidAdUnitId = "Interstitial_Android";
    string _adUnitId;

    public event Action OnInterstitialAdReady;
    public bool isReady = false;
    [SerializeField] Button _interstitialAdButton;

    private Coroutine _slowDownCoroutine;

    void Awake()
    {
        _adUnitId = _androidAdUnitId;
        // Make sure this object persists across scenes if it's managed by AdManager
        if (transform.parent == null)
        {
            DontDestroyOnLoad(gameObject);
        }
    }

    private void Update()
    {
        if (_interstitialAdButton != null)
        {
            _interstitialAdButton.interactable = isReady;
        }
    }

    public void OnInterstitialAdButtonClicked()
    {
        Debug.Log("Interstitial ad button clicked!");
        ShowInterstitial();
    }

    public void LoadAd()
    {
        if (!Advertisement.isInitialized)
        {
            Debug.LogWarning("Tried to load interstitial ad before Unity ads was initialized!");
            StartCoroutine(RetryLoadAfterDelay(3f));
            return;
        }

        Debug.Log("Loading interstitial ad");
        Advertisement.Load(_adUnitId, this);
    }

    private IEnumerator RetryLoadAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        // Check if object still exists before loading
        if (this != null && gameObject != null)
        {
            LoadAd();
        }
    }

    public void ShowAd()
    {
        if(isReady && this != null)
        {
            Advertisement.Show(_adUnitId, this);
            isReady = false;
        }
        else
        {
            Debug.LogWarning("Interstitial ad is not ready yet or object destroyed!");
            if (this != null)
            {
                LoadAd();
            }
        }
    }

    public void ShowInterstitial()
    {
        if (isReady && this != null)
        {
            Debug.Log("Showing interstitial ad manually!");
            ShowAd();
        }
        else
        {
            Debug.Log("Interstitial ad not ready yet, loading again!");
            if (this != null)
            {
                LoadAd();
            }
        }
    }

    public void OnUnityAdsAdLoaded(string placementId)
    {
        if (this == null) return;
        
        Debug.Log("Interstitial ad loaded!");
        if (_interstitialAdButton != null)
        {
            _interstitialAdButton.interactable = true;
        }
        isReady = true;
        OnInterstitialAdReady?.Invoke();
    }

    public void OnUnityAdsFailedToLoad(string placementId, UnityAdsLoadError error, string message)
    {
        if (this == null) return;
        
        Debug.LogWarning($"Failed to load interstitial ad: {error} - {message}");
        StartCoroutine(RetryLoadAfterDelay(5f));
    }

    public void OnUnityAdsShowClick(string placementId)
    {
        Debug.Log("User clicked on interstitial ad!");
    }

    public void OnUnityAdsShowComplete(string placementId, UnityAdsShowCompletionState showCompletionState)
    {
        if (this == null) return;
        
        if (showCompletionState == UnityAdsShowCompletionState.COMPLETED)
        {
            Debug.Log("Interstitial ad watched completely!");
            // Use a safer coroutine that checks for object existence
            _slowDownCoroutine = StartCoroutine(SlowDownTimeTemporarily(30f));
        }
        else
        {
            Debug.Log("Interstitial ad skipped or status unknown!");
        }
        
        // Always reload after showing
        LoadAd();
    }

    private IEnumerator SlowDownTimeTemporarily(float seconds)
    {
        Time.timeScale = 0.4f;
        Debug.Log("Time slowed down to 0.4x for " + seconds + " sec");
        
        float elapsed = 0f;
        while (elapsed < seconds && this != null)
        {
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        // Only restore time if this object still exists
        if (this != null)
        {
            Time.timeScale = 1.0f;
            Debug.Log("Time restored to normal!");
        }
    }

    public void OnUnityAdsShowFailure(string placementId, UnityAdsShowError error, string message)
    {
        if (this == null) return;
        
        Debug.Log($"Error showing interstitial ad: {error} - {message}");
        LoadAd();
    }

    public void OnUnityAdsShowStart(string placementId)
    {
        if (this == null) return;
        
        Debug.Log("Showing interstitial ad at this moment!");
        Time.timeScale = 0f;
    }

    public void SetButton(Button button)
    {
        if (button == null || this == null)
            return;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnInterstitialAdButtonClicked);
        _interstitialAdButton = button;
        _interstitialAdButton.interactable = isReady;
    }

    private void OnDestroy()
    {
        // Stop any running coroutines when object is destroyed
        if (_slowDownCoroutine != null)
        {
            StopCoroutine(_slowDownCoroutine);
        }
        
        // Restore time scale if this object was controlling it
        if (Time.timeScale != 1.0f)
        {
            Time.timeScale = 1.0f;
        }
    }
}