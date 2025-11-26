using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AdManager : MonoBehaviour
{
    public AdsInitializer adsInitializer;
    public InterstitialAd interstitialAd;
    [SerializeField] bool turnOffInterstitialAd = false;
    private bool firstAdShown = false;

    public RewardedAds rewardedAds;
    public RewardedAds2 rewardedAds2; // NEW: Reference to the second rewarded ads script
    
    [SerializeField] bool turnOffRewardedAds = false; 

    public BannerAd bannerAd;
    [SerializeField] bool turnOffBannerAd = false;

    public static AdManager Instance { get; private set; }


    private void Awake()
    {
        if(adsInitializer == null)
            adsInitializer = FindFirstObjectByType<AdsInitializer>();

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
            // Load both rewarded ads systems if they exist
            rewardedAds?.LoadAd();
            rewardedAds2?.LoadAd(); // NEW: Load the second rewarded ads
        }

        if (!turnOffBannerAd)
        {
            bannerAd.LoadAndShowBanner(); // Changed from LoadBanner() to LoadAndShowBanner()
        }
    }

    private void HandleInterstitialReady()
    {
        if (!firstAdShown)
        {
            Debug.Log("Showing first time interstitial ad automatically!");
            interstitialAd.ShowAd();
            firstAdShown = true;

        } else
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

    private bool firstSceneLoad = false;
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (interstitialAd == null)
            interstitialAd = FindFirstObjectByType<InterstitialAd>();

        Button interstitialButton =
            GameObject.FindGameObjectWithTag("InterstitialButton").GetComponent<Button>();

        if (interstitialAd != null && interstitialButton != null)
        {
            interstitialAd.SetButton(interstitialButton);
        }


        if (rewardedAds == null)
            rewardedAds = FindFirstObjectByType<RewardedAds>();

        // NEW: Find the second rewarded ads script if not assigned
        if (rewardedAds2 == null)
            rewardedAds2 = FindFirstObjectByType<RewardedAds2>();

        if(bannerAd == null)
            bannerAd = FindFirstObjectByType<BannerAd>();

        Button rewardedAdButton =
            GameObject.FindGameObjectWithTag("RewardedButton").GetComponent<Button>();

        // Set up button for whichever rewarded ads system is available
        if (rewardedAdButton != null)
        {
            // Priority: Use RewardedAds2 if available, otherwise fall back to original RewardedAds
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


        Button bannerButton = GameObject.FindGameObjectWithTag("BannerButton").GetComponent<Button>();
        if(bannerAd != null && bannerButton != null)
        {
            bannerAd.SetButton(bannerButton);
        }

        if (!firstSceneLoad)
        {
            firstSceneLoad = true;
            Debug.Log("First time scene loaded!");
            return;
        }

        Debug.Log("Scene loaded!");
        HandleAdsInitialized();
    }

    // NEW: Helper method to get the active rewarded ads system
    public MonoBehaviour GetActiveRewardedAdsSystem()
    {
        if (rewardedAds2 != null) return rewardedAds2;
        if (rewardedAds != null) return rewardedAds;
        return null;
    }

    // NEW: Check if any rewarded ads system is available
    public bool IsRewardedAdsAvailable()
    {
        return rewardedAds != null || rewardedAds2 != null;
    }
}