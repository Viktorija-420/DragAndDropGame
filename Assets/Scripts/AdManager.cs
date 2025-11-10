using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AdManager : MonoBehaviour
{
    public Adsinitializer adsInitializer;
    public InterstitialAds interstitialAd;
    [SerializeField] bool turnoffInterstitialAds = false;

    public static AdManager Instance { get; private set; }

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
        Debug.Log("Ads initialized - loading first ad");
        if (!turnoffInterstitialAds && interstitialAd != null)
        {
            interstitialAd.LoadAd();
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"Scene loaded: {scene.name} - preparing to show ad");
        
        // Find interstitial ad if null
        if (interstitialAd == null)
        {
            interstitialAd = FindFirstObjectByType<InterstitialAds>();
        }

        // FIXED: Remove tag dependency - find button by name instead
        Button interstitialButton = FindInterstitialButton();
        if (interstitialButton != null && interstitialAd != null)
        {
            interstitialAd.SetButton(interstitialButton);
            Debug.Log("Interstitial button found and set up");
        }
        else
        {
            Debug.Log("No interstitial button found in scene - ads will still show automatically");
        }

        // SHOW AD ON EVERY SCENE LOAD
        if (!turnoffInterstitialAds)
        {
            StartCoroutine(ShowAdOnSceneLoad());
        }
    }

    // FIXED: Find button by name instead of tag
    private Button FindInterstitialButton()
    {
        // Method 1: Find by name
        GameObject buttonObj = GameObject.Find("InterstitialButton");
        if (buttonObj != null)
        {
            Button button = buttonObj.GetComponent<Button>();
            if (button != null) return button;
        }

        // Method 2: Find any button with "Ad" in the name
        Button[] allButtons = FindObjectsByType<Button>(FindObjectsSortMode.None);
        foreach (Button button in allButtons)
        {
            if (button.name.Contains("Ad") || button.name.Contains("Interstitial"))
            {
                return button;
            }
        }

        return null;
    }

    private System.Collections.IEnumerator ShowAdOnSceneLoad()
    {
        // Wait for 1 second to let everything initialize
        yield return new WaitForSecondsRealtime(1f);
        
        if (interstitialAd != null)
        {
            // If ad is ready, show it immediately
            if (interstitialAd.isReady)
            {
                Debug.Log("Ad is ready - showing immediately");
                interstitialAd.ShowAd();
            }
            else
            {
                Debug.Log("Ad not ready yet - loading and waiting");
                // Load ad and wait for it to be ready
                interstitialAd.LoadAd();
                
                // Wait up to 5 seconds for ad to load
                float timeout = 5f;
                float timer = 0f;
                
                while (!interstitialAd.isReady && timer < timeout)
                {
                    timer += Time.deltaTime;
                    yield return null;
                }
                
                if (interstitialAd.isReady)
                {
                    Debug.Log("Ad loaded after waiting - showing now");
                    interstitialAd.ShowAd();
                }
                else
                {
                    Debug.LogWarning("Ad loading timeout - continuing without ad");
                }
            }
        }
        else
        {
            Debug.LogWarning("InterstitialAd component not found");
        }
    }

    public void ShowInterstitialAd()
    {
        if (!turnoffInterstitialAds && interstitialAd != null)
        {
            interstitialAd.ShowAd();
        }
    }

    public bool IsInterstitialReady()
    {
        return interstitialAd != null && interstitialAd.isReady;
    }
}