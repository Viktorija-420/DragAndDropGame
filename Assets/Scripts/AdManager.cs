using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AdManager : MonoBehaviour
{
    public AdsInistializer adsInitializer;
    public InterstitialAd interstitialAd;
    [SerializeField] bool turnOffInterstitialAd = false;
    private bool firstAdShown = false;

    public RewardedAds rewardedAds;
    public RewardedAds2 rewardedAds2;
    
    [SerializeField] bool turnOffRewardedAds = false; 

    public BannerAd bannerAd;
    [SerializeField] bool turnOffBannerAd = false;

    public static AdManager Instance { get; private set; }

    private void Awake()
    {
        if(adsInitializer == null)
            adsInitializer = FindFirstObjectByType<AdsInistializer>();

        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        adsInitializer.OnAdsInitialized += HandleAdsInitialized;
    }

    private void HandleAdsInitialized()
    {
        if(!turnOffInterstitialAd)
        {
            interstitialAd.OnInterstitialAdReady += HandleInterstitialReady;
            interstitialAd.LoadAd();
        }

        if (!turnOffRewardedAds)
        {
            rewardedAds?.LoadAd();
            rewardedAds2?.LoadAd();
        }

        if (!turnOffBannerAd)
        {
            bannerAd.LoadAndShowBanner();
        }
    }

    private void HandleInterstitialReady()
    {
        if (!firstAdShown)
        {
            Debug.Log("Showing first time interstitial ad automatically!");
            interstitialAd.ShowAd();
            firstAdShown = true;
        }
        else
        {
            Debug.Log("Next interstitial ad is ready for manual show!");
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

    private bool firstSceneLoad = true;
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"Scene loaded: {scene.name}");

        // Find ad components if null
        if (interstitialAd == null)
            interstitialAd = FindFirstObjectByType<InterstitialAd>();

        if (rewardedAds == null)
            rewardedAds = FindFirstObjectByType<RewardedAds>();

        if (rewardedAds2 == null)
            rewardedAds2 = FindFirstObjectByType<RewardedAds2>();

        if (bannerAd == null)
            bannerAd = FindFirstObjectByType<BannerAd>();

        // Set up buttons
        SetupInterstitialButton();
        SetupRewardedButton();
        SetupBannerButton();

        // Handle interstitial ad on scene change
        if (!firstSceneLoad)
        {
            Debug.Log("Scene changed - preparing to show interstitial ad");
            StartCoroutine(ShowInterstitialAfterDelay(1.5f));
        }
        else
        {
            firstSceneLoad = false;
            Debug.Log("First scene load - skipping interstitial");
        }

        // Reload ads for new scene
        HandleAdsInitialized();
    }

    private void SetupInterstitialButton()
    {
        GameObject interstitialButtonObj = GameObject.FindGameObjectWithTag("InterstitialButton");
        Button interstitialButton = interstitialButtonObj != null ? interstitialButtonObj.GetComponent<Button>() : null;
        
        if (interstitialAd != null && interstitialButton != null)
        {
            interstitialAd.SetButton(interstitialButton);
            Debug.Log("Interstitial button set up successfully");
        }
    }

    private void SetupRewardedButton()
    {
        GameObject rewardedButtonObj = GameObject.FindGameObjectWithTag("RewardedButton");
        Button rewardedAdButton = rewardedButtonObj != null ? rewardedButtonObj.GetComponent<Button>() : null;
        
        if (rewardedAdButton != null)
        {
            if (rewardedAds2 != null)
            {
                rewardedAds2.SetButton(rewardedAdButton);
                Debug.Log("RewardedAds2 button set up successfully");
            }
            else if (rewardedAds != null)
            {
                rewardedAds.SetButton(rewardedAdButton);
                Debug.Log("Original RewardedAds button set up successfully");
            }
            else
            {
                Debug.LogWarning("No rewarded ads system found for button setup");
            }
        }
    }

    private void SetupBannerButton()
    {
        GameObject bannerButtonObj = GameObject.FindGameObjectWithTag("BannerButton");
        Button bannerButton = bannerButtonObj != null ? bannerButtonObj.GetComponent<Button>() : null;
        
        if(bannerAd != null && bannerButton != null)
        {
            bannerAd.SetButton(bannerButton);
            Debug.Log("Banner button set up successfully");
        }
    }

    private System.Collections.IEnumerator ShowInterstitialAfterDelay(float delay)
{
    yield return new WaitForSeconds(delay);
    
    if (!turnOffInterstitialAd && interstitialAd != null && interstitialAd.isReady)
    {
        Debug.Log("Showing interstitial ad on scene change");
        interstitialAd.ShowAd();
    }
    else if (!turnOffInterstitialAd && interstitialAd != null)
    {
        Debug.Log("Interstitial ad not ready, loading for next time");
        interstitialAd.LoadAd();
    }
    else if (!turnOffInterstitialAd && interstitialAd == null)
    {
        Debug.LogWarning("InterstitialAd reference lost, finding it again...");
        interstitialAd = FindFirstObjectByType<InterstitialAd>();
        if (interstitialAd != null)
        {
            interstitialAd.LoadAd();
        }
    }
}

    public MonoBehaviour GetActiveRewardedAdsSystem()
    {
        if (rewardedAds2 != null) return rewardedAds2;
        if (rewardedAds != null) return rewardedAds;
        return null;
    }

    public bool IsRewardedAdsAvailable()
    {
        return rewardedAds != null || rewardedAds2 != null;
    }
}