using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class AdManager : MonoBehaviour
{
    public Adsinitializer adsInitializer;
    public InterstitialAds interstitialAd;
    [SerializeField] bool turnoffInterstitialAds = false;

    public RewardedAds rewardedAd;
    [SerializeField] bool turnoffRewardedAds = false;

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

        if (!turnoffRewardedAds && rewardedAd != null)
        {
            rewardedAd.LoadAd();
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

        // Find interstitial button
        Button interstitialButton = FindInterstitialButton();
        if (interstitialButton != null && interstitialAd != null)
        {
            interstitialAd.SetButton(interstitialButton);
            Debug.Log("Interstitial button found and set up");
        }

        // SHOW AD ON EVERY SCENE LOAD
        if (!turnoffInterstitialAds)
        {
            StartCoroutine(ShowAdOnSceneLoad());
        }

        // Find rewarded ad if null
        if (rewardedAd == null)
            rewardedAd = FindFirstObjectByType<RewardedAds>();

        // Find rewarded button using reliable methods
        Button rewardedButton = FindRewardedButton();
        
        if (rewardedButton != null && rewardedAd != null)
        {
            rewardedAd.SetButton(rewardedButton);
            Debug.Log("Rewarded button found and set up");
            
            // Load the rewarded ad when scene loads
            if (!turnoffRewardedAds)
            {
                rewardedAd.LoadAd();
            }
        }
        else
        {
            Debug.Log("No rewarded button found in scene");
        }
    }

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
        Button[] allButtons = FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (Button button in allButtons)
        {
            if (button.name.Contains("Ad") || button.name.Contains("Interstitial"))
            {
                return button;
            }
        }

        return null;
    }

    private Button FindRewardedButton()
    {
        // Method 1: Find by specific names
        string[] possibleNames = { "RewardedButton", "RewardedAdButton", "BonusAdButton", "AdButton", "WatchAdButton" };
        
        foreach (string name in possibleNames)
        {
            GameObject buttonObj = GameObject.Find(name);
            if (buttonObj != null)
            {
                Button button = buttonObj.GetComponent<Button>();
                if (button != null) return button;
            }
        }

        // Method 2: Find by name containing keywords
        Button[] allButtons = FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (Button button in allButtons)
        {
            if (button.name.Contains("Rewarded") || button.name.Contains("Bonus") || 
                button.name.Contains("WatchAd") || button.name.Contains("Ad"))
            {
                return button;
            }
        }

        return null;
    }

    private IEnumerator ShowAdOnSceneLoad()
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
                interstitialAd.LoadAd();
                
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
            }
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