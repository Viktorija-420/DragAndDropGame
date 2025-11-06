using System;
using System.Collections; // Add this for IEnumerator
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Advertisements;

public class InterstitialAds : MonoBehaviour, IUnityAdsLoadListener, IUnityAdsShowListener
{
    [SerializeField] string _androidAdUnitId = "Interstitial_Android";
    private string _adUnitId;

    public event Action onInterstitialAdReady;
    public bool isReady = false;

    [SerializeField] private Button _interstitialAdButton;

    private void Awake()
    {
        _adUnitId = _androidAdUnitId;

        if (_interstitialAdButton != null)
            _interstitialAdButton.onClick.AddListener(ShowInterstitial);
    }

    private void Start()
    {
        LoadAd();
    }

    private void Update()
    {
        if (_interstitialAdButton != null)
            _interstitialAdButton.interactable = isReady;
    }

    public void LoadAd()
    {
        if (!Advertisement.isInitialized)
        {
            Debug.LogWarning("❌ Unity Ads is not initialized. Cannot load interstitial ad.");
            return;
        }

        Debug.Log("🟡 Loading interstitial ad...");
        Advertisement.Load(_adUnitId, this);
    }

    public void showAd()
    {
        if (isReady)
        {
            // hide banner ............
            Advertisement.Show(_adUnitId, this); // Fixed capitalization
            isReady = false;
        }
        else
        {
            Debug.LogWarning("⚠️ Interstitial ad not ready yet.");
            LoadAd();
        }
    }

    public void ShowInterstitial()
    {
        if (AdManager.Instance != null && AdManager.Instance.interstitialAd != null)
        {
            Debug.Log("Showing interstitial ad manually.");
            showAd();
        }
        else
        {
            Debug.LogWarning("Interstitial ad not ready yet, loading again.");
            LoadAd();
        }
    }
    
    public void OnUnityAdsAdLoaded(string placementId)
    {
        Debug.Log("Interstitial ad loaded!");
        if (_interstitialAdButton != null)
            _interstitialAdButton.interactable = true;
        isReady = true;
        onInterstitialAdReady?.Invoke();
    }

    public void OnUnityAdsFailedToLoad(string placementId, UnityAdsLoadError error, string message)
    {
        Debug.LogWarning("Failed to load interstitial ad");
        LoadAd();
    }

    public void OnUnityAdsShowClick(string placementId)
    {
        Debug.Log("User clicked on interstitial");
    }

    public void OnUnityAdsShowComplete(string placementId, UnityAdsShowCompletionState showCompletionState)
    {
        if (showCompletionState == UnityAdsShowCompletionState.COMPLETED)
        {
            Debug.Log("Interstitial ad completed successfully.");
            StartCoroutine(SlowDownTimeTemporarily(30f));
            LoadAd();
        }
        else
        {
            Debug.Log("Interstitial ad skipped or status is unknown!");
            LoadAd();
        }
    }
    
    private IEnumerator SlowDownTimeTemporarily(float seconds)
    {
        Time.timeScale = 0.4f;
        Debug.Log("Time slowed to 0.4x for "+seconds+" sec.");

        yield return new WaitForSeconds(seconds);

        Time.timeScale = 1.0f;
        Debug.Log("Time restored to normal.");
    }

    public void OnUnityAdsShowFailure(string placementId, UnityAdsShowError error, string message)
    {
        Debug.Log("Error showing interstitial ad!");
        LoadAd();
    }

    public void OnUnityAdsShowStart(string placementId)
    {
        Debug.Log("Showing interstitial ad at this moment!");
        Time.timeScale = 0f;
    }
    
    public void SetButton(Button button)
    {
        if (button == null)
            return;

        button.onClick.RemoveAllListeners(); // Fixed method name
        button.onClick.AddListener(ShowInterstitial); // Use existing method
        _interstitialAdButton = button;
        _interstitialAdButton.interactable = false;
    }
}