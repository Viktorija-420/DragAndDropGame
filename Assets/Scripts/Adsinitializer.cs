using System;
using UnityEngine;
using UnityEngine.Advertisements;

public class Adsinitializer : MonoBehaviour, IUnityAdsInitializationListener
{
    [SerializeField] string _androidGameId;
    [SerializeField] bool _testMode = true;
    private string _gameId;
    public event Action OnAdsInitialized;

    private void Awake()
    {
        InitializeAds(); // Fixed method name
    }

    public void InitializeAds() // Fixed method name
    {
#if UNITY_ANDROID
        _gameId = _androidGameId;
#elif UNITY_IOS
        // Add iOS game ID if needed
        // _gameId = _iOSGameId;
#else
        _gameId = "unexpected_platform";
#endif
        
        if (!Advertisement.isInitialized && Advertisement.isSupported) // Fixed property name
        {
            Debug.Log($"Initializing Unity Ads with Game ID: {_gameId}, Test Mode: {_testMode}");
            Advertisement.Initialize(_gameId, _testMode, this);
        }
        else if (!Advertisement.isSupported)
        {
            Debug.LogWarning("Unity Ads is not supported on this platform.");
        }
    }

    public void OnInitializationComplete()
    {
        Debug.Log("Unity Ads initialization complete!");
        OnAdsInitialized?.Invoke();
    }
    
    public void OnInitializationFailed(UnityAdsInitializationError error, string message)
    {
        Debug.Log($"Unity Ads Initialization Failed: {error.ToString()} - {message}");
    }
}