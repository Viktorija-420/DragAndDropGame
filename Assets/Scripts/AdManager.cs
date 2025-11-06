using UnityEngine;
using UnityEngine.SceneManagement; // Add this for SceneManager
using UnityEngine.UI; // Add this for Button

public class AdManager : MonoBehaviour
{
    public Adsinitializer adsInitializer;
    public InterstitialAds interstitialAd; // Fixed class name
    [SerializeField] bool turnoffInterstitialAds = false;
    private bool firstAdShown = false;

    /// ////////////

    public static AdManager Instance { get; private set; } // Fixed property name

    private void Awake()
    {
        if (adsInitializer == null)
            adsInitializer = FindFirstObjectByType<Adsinitializer>();

        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (adsInitializer != null)
            adsInitializer.OnAdsInitialized += HandleAdsInitialized;
    }

    private void HandleAdsInitialized()
    {
        if (!turnoffInterstitialAds && interstitialAd != null)
        {
            interstitialAd.onInterstitialAdReady += HandleInitializedReady; // Fixed event name
            interstitialAd.LoadAd();
        }
    }

    private void HandleInitializedReady()
    {
        if (!firstAdShown && interstitialAd != null)
        {
            Debug.Log("Showing first interstitial ad automatically!");
            interstitialAd.showAd();
            firstAdShown = true;
        }
        else
        {
            Debug.Log("Next interstitial ad is ready for manual show!");
        }
    }

    private void OnEnable() // Fixed method name
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable() // Fixed method name
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    private bool firstSceneLoaded = false;
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (interstitialAd == null)
            interstitialAd = FindFirstObjectByType<InterstitialAds>(); // Fixed class name

        Button interstitialButton = GameObject.FindGameObjectWithTag("InterstitialButton")?.GetComponent<Button>();

        if (interstitialButton != null && interstitialAd != null) // Fixed variable name
        {
            interstitialAd.SetButton(interstitialButton);
        }

        if (!firstSceneLoaded) // Fixed: removed "= true" - just check the boolean value
        {
            firstSceneLoaded = true;
            Debug.Log("First scene loaded.");
            return;
        }
        Debug.Log("Scene loaded.");
        HandleAdsInitialized();
    }
}