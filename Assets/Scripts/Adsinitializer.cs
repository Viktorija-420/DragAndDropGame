using System;
using UnityEngine;
using UnityEngine.Advertisements;

public class Adsinitializer : MonoBehaviour, IUnityAdsInitializationListener
{
    [SerializeField] string _androidGameId;
    [SerializeField] string _iOSGameId;
    [SerializeField] bool _testMode = true;
    
    private string _gameId;
    public event Action OnAdsInitialized;

    void Awake()
    {
        InitializeAds();
    }

    public void InitializeAds()
    {
#if UNITY_ANDROID
        _gameId = _androidGameId;
#elif UNITY_IOS
        _gameId = _iOSGameId;
#else
        _gameId = "unexpected_platform";
#endif

        if (!Advertisement.isInitialized && Advertisement.isSupported)
        {
            Debug.Log($"Initializing Unity Ads with Game ID: {_gameId}");
            Advertisement.Initialize(_gameId, _testMode, this);
        }
    }

    public void OnInitializationComplete()
    {
        Debug.Log("Unity Ads initialization complete.");
        OnAdsInitialized?.Invoke();
    }

    public void OnInitializationFailed(UnityAdsInitializationError error, string message)
    {
        Debug.Log($"Unity Ads Initialization Failed: {error} - {message}");
    }
}