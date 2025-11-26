using UnityEngine;
using UnityEngine.Advertisements;
using UnityEngine.UI;

public class BannerAd : MonoBehaviour
{
    [SerializeField] string _androidAdUnitId = "Banner_Android";
    string _adUnitId;

    [SerializeField] Button _bannerButton;
    public bool isBannerVisible = false;

    [SerializeField] BannerPosition _bannerPosition = BannerPosition.BOTTOM_CENTER;

    private void Start()
    {
        _adUnitId = _androidAdUnitId;
        Advertisement.Banner.SetPosition(_bannerPosition);
        
        // Load and show banner automatically when the game starts
        LoadAndShowBanner();
    }

    public void LoadAndShowBanner()
    {
        if(!Advertisement.isInitialized)
        {
            Debug.Log("Tried to load banner ad before Unity ads was initialized!");
            // Try again after a short delay
            Invoke(nameof(LoadAndShowBanner), 1f);
            return;
        }

        Debug.Log("Loading Banner ad!");
        BannerLoadOptions options = new BannerLoadOptions
        {
            loadCallback = OnBannerLoaded,
            errorCallback = OnBannerError
        };

        Advertisement.Banner.Load(_adUnitId, options);
    }

    void OnBannerLoaded()
    {
        Debug.Log("Banner ad loaded!");
        // Automatically show the banner when loaded
        ShowBannerAd();
        
        if (_bannerButton != null)
            _bannerButton.interactable = true;
    }

    void OnBannerError(string message)
    {
        Debug.LogWarning("Banner Error: "+message);
        // Retry loading after a short delay
        Invoke(nameof(LoadAndShowBanner), 2f);
    }

    public void ShowBannerAd()
    {
        if(isBannerVisible)
        {
            return; // Banner is already visible, do nothing
        }

        BannerOptions options = new BannerOptions
        {
            clickCallback = OnBannerClicked,
            hideCallback = OnBannerHidden,
            showCallback = OnBannerShown
        };

        Advertisement.Banner.Show(_adUnitId, options);
    }

    public void HideBannerAd()
    {
        // Remove this method or make it private to prevent hiding
        // Advertisement.Banner.Hide();
        Debug.Log("Banner hiding disabled - banner should always be visible");
    }

    void OnBannerClicked()
    {
        Debug.Log("User clicked on banner ad!");
    }

    void OnBannerHidden()
    {
        Debug.Log("Banner was hidden! Showing it again...");
        isBannerVisible = false;
        // Immediately show the banner again if it gets hidden
        ShowBannerAd();
    }

    void OnBannerShown()
    {
        Debug.Log("Banner ad is visible!");
        isBannerVisible = true;
    }

    public void SetButton(Button button)
    {
        if (button == null)
            return;

        button.onClick.RemoveAllListeners();
        // Remove the toggle functionality and just show the banner
        button.onClick.AddListener(ShowBannerAd);
        _bannerButton = button;
        _bannerButton.interactable = true;
    }

    // Optional: Handle when the app loses/gains focus
    private void OnApplicationPause(bool pauseStatus)
    {
        if (!pauseStatus && !isBannerVisible)
        {
            // If app resumes and banner isn't visible, show it again
            Invoke(nameof(ShowBannerAd), 0.5f);
        }
    }
}