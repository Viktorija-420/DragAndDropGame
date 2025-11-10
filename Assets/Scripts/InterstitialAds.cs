using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Advertisements;

public class InterstitialAds : MonoBehaviour, IUnityAdsLoadListener, IUnityAdsShowListener
{
    [SerializeField] string _androidAdUnitId = "Interstitial_Android";
    [SerializeField] private Button _interstitialAdButton;

    private string _adUnitId;
    public event Action onInterstitialAdReady;
    public bool isReady = false;

    private void Awake()
    {
        // Get the Ad Unit ID for the current platform
#if UNITY_ANDROID
        _adUnitId = _androidAdUnitId;
#else
        _adUnitId = "unexpected_platform";
#endif

        Debug.Log($"Initializing Interstitial Ads with ID: {_adUnitId}");
    }

    private void Start()
    {
        // Setup button if assigned in inspector
        if (_interstitialAdButton != null)
        {
            _interstitialAdButton.onClick.AddListener(ShowAd);
            _interstitialAdButton.interactable = isReady;
            Debug.Log("Interstitial button assigned in inspector");
        }
        
        // Load first ad
        if (Advertisement.isInitialized)
        {
            LoadAd();
        }
    }

    private void Update()
    {
        // Update button state if button exists
        if (_interstitialAdButton != null)
        {
            _interstitialAdButton.interactable = isReady;
        }
    }

    public void LoadAd()
    {
        if (!Advertisement.isInitialized)
        {
            Debug.LogWarning("Unity Ads not initialized yet");
            Invoke(nameof(LoadAd), 1f);
            return;
        }

        Debug.Log($"Loading Ad: {_adUnitId}");
        Advertisement.Load(_adUnitId, this);
    }

    public void ShowAd()
    {
        if (isReady)
        {
            Debug.Log("Showing interstitial ad");
            Time.timeScale = 0f;
            Advertisement.Show(_adUnitId, this);
            isReady = false;
        }
        else
        {
            Debug.LogWarning("Ad not ready - loading first");
            LoadAd();
            // Try to show after loading
            StartCoroutine(ShowAfterLoad());
        }
    }

    private IEnumerator ShowAfterLoad()
    {
        float timeout = 3f;
        float timer = 0f;
        
        while (!isReady && timer < timeout)
        {
            timer += Time.deltaTime;
            yield return null;
        }
        
        if (isReady)
        {
            Debug.Log("Ad loaded - showing now");
            Time.timeScale = 0f;
            Advertisement.Show(_adUnitId, this);
            isReady = false;
        }
    }

    // Load callbacks
    public void OnUnityAdsAdLoaded(string adUnitId)
    {
        Debug.Log($"Ad Loaded: {adUnitId}");
        isReady = true;
        onInterstitialAdReady?.Invoke();
    }

    public void OnUnityAdsFailedToLoad(string adUnitId, UnityAdsLoadError error, string message)
    {
        Debug.LogError($"Error loading Ad Unit {adUnitId}: {error} - {message}");
        isReady = false;
        // Retry after delay
        Invoke(nameof(LoadAd), 3f);
    }

    // Show callbacks
    public void OnUnityAdsShowFailure(string adUnitId, UnityAdsShowError error, string message)
    {
        Debug.LogError($"Error showing Ad Unit {adUnitId}: {error} - {message}");
        Time.timeScale = 1f;
        LoadAd();
    }

    public void OnUnityAdsShowStart(string adUnitId)
    {
        Debug.Log("Ad showing started");
        Time.timeScale = 0f;
    }

    public void OnUnityAdsShowClick(string adUnitId)
    {
        Debug.Log("Ad clicked");
    }

    public void OnUnityAdsShowComplete(string adUnitId, UnityAdsShowCompletionState showCompletionState)
    {
        Debug.Log($"Ad completed: {showCompletionState}");
        Time.timeScale = 1f;
        // Load next ad
        LoadAd();
    }

    public void SetButton(Button button)
    {
        if (button == null) 
        {
            Debug.Log("No button provided to SetButton");
            return;
        }

        _interstitialAdButton = button;
        _interstitialAdButton.onClick.RemoveAllListeners();
        _interstitialAdButton.onClick.AddListener(ShowAd);
        _interstitialAdButton.interactable = isReady;
        
        Debug.Log("Interstitial button configured successfully");
    }
}