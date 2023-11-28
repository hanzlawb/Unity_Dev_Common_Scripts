using System.Collections;
using UnityEngine;
using GoogleMobileAds;
using GoogleMobileAds.Api;
using GoogleMobileAds.Common;
using System;
using UnityEngine.Events;

public class AdsManager_New : MonoBehaviour
{
    public static AdsManager_New Instance;
    public bool testMode = false;
    public GameObject noInternetCanvas;

    public string simpleBannerId = "ca-app-pub-3940256099942544/6300978111";
    public string bigBannerId = "ca-app-pub-3940256099942544/6300978111";
    public string interId = "ca-app-pub-3940256099942544/1033173712";
    public string rewardedId = "ca-app-pub-3940256099942544/5224354917";
    public string appOpenId = "ca-app-pub-3940256099942544/3419835294";

    BannerView simpleBannerView,bigBannerView;
    InterstitialAd interstitialAd;
    RewardedAd rewardedAd;
    AppOpenAd appOpenAd;

    [Header("Turn Ads On/Off")]
    public bool simpleBannerAdToggler;
    public bool appOpenAdToggler;
    public bool interstitialAdToggler;
    public bool rewardedAdToggler;
    public bool bigBannerAdToggler;

    [HideInInspector]
    public UnityEvent rewardedAdEvent;
    public float customBigBannerX = 0f;
    public float customBigBannerY = 0.1f;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }
        else
        {
            Destroy(gameObject);
        }

        if (testMode)
        {
            simpleBannerId = "ca-app-pub-3940256099942544/6300978111";
            bigBannerId = "ca-app-pub-3940256099942544/6300978111";
            interId = "ca-app-pub-3940256099942544/1033173712";
            rewardedId = "ca-app-pub-3940256099942544/5224354917";
            appOpenId = "ca-app-pub-3940256099942544/3419835294";
        }
    }

    private void Start()
    {
        //8.4.1
        MobileAds.Initialize(initStatus =>
        {
            print("Ads Initialised !!");
            RequestAds();
            //if (appOpenAdToggler)
            //    AppStateEventNotifier.AppStateChanged += OnAppStateChanged;
        });
    }

    void RequestAds()
    {
        if (PlayerPrefs.GetInt("RemoveAds") == 1)
        {
            return;
        }

        if (appOpenAdToggler && appOpenAd == null)
            RequestAppOpenAd();

        if (simpleBannerAdToggler && simpleBannerView == null)
            LoadSimpleBannerAd();

        if (bigBannerAdToggler && bigBannerView == null)
            LoadBigBannerAd();

        if (interstitialAdToggler && interstitialAd == null)
            RequestInterstitialAd();

        if (rewardedAdToggler && rewardedAd == null)
            RequestRewardedAd();
    }

    bool noInternet, functionCalled;
    private void Update()
    {
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            noInternet = true;
            functionCalled = false;
            noInternetCanvas?.SetActive(true);
            //Debug.Log("Not Reachable, Show Pop Up");
        }
        else
        {
            //Debug.Log(noInternet + "" + functionCalled);
            if (noInternet == true && functionCalled == false)
            {
                //RequestAds();
                Debug.Log("Request Again");
                noInternetCanvas?.SetActive(false);
                functionCalled = true;
                noInternet = false;
            }
        }

        //Debug.LogError(simpleBannerShown);
    }

    #region SimpleBannerAd
    void CreateBannerView()
    {
        if (!simpleBannerAdToggler)
            return;

        if (PlayerPrefs.GetInt("RemoveAds") == 1)
        {
            return;
        }

        Debug.Log("Creating banner view");
        // If we already have a banner, destroy the old one.
        if (simpleBannerView != null)
        {
            simpleBannerView.Destroy();
            simpleBannerView = null;
        }
        // Create a 320x50 banner at top of the screen
        simpleBannerView = new BannerView(simpleBannerId, AdSize.Banner, AdPosition.Top);
    }

    public void LoadSimpleBannerAd()
    {
        if (!simpleBannerAdToggler)
            return;
        // create an instance of a banner view first.
        if (simpleBannerView == null)
        {
            CreateBannerView();
        }
        // create our request used to load the ad.
        var adRequest = new AdRequest();
        adRequest.Keywords.Add("unity-admob-sample");
        // send the request to load the ad.
        Debug.Log("Loading banner ad.");
        simpleBannerView.LoadAd(adRequest);
        HideSimpleBannerAd();
    }

    bool simpleBannerShown = false;

    [ContextMenu("Show Banner")]
    public void ShowSimpleBannerAd()
    {
        simpleBannerShown= true;
        simpleBannerView?.Show();
    }

    [ContextMenu("Hide Banner")]
    public void HideSimpleBannerAd()
    {
        simpleBannerShown = false;
        simpleBannerView?.Hide();
    }
    #endregion
    bool resumeSimpleBannerAfterThisAd,resumeBigBannerAfterThisAd;
    void HideBannerAdsIfOpen()
    {
        if (simpleBannerShown)
        {
            resumeSimpleBannerAfterThisAd = true;
            //Debug.LogError("ss");
        }
        else
        {
            resumeSimpleBannerAfterThisAd = false;
        }

        if (resumeSimpleBannerAfterThisAd)
        {
            HideSimpleBannerAd();
        }

        ///Big
        if (bigBannerShown)
        {
            resumeBigBannerAfterThisAd = true;
            //Debug.LogError("ss");
        }
        else
        {
            resumeBigBannerAfterThisAd = false;
        }

        if (resumeBigBannerAfterThisAd)
        {
            HideBigBannerAd();
        }
    }
    void ShowBannerAdsIfOpen()
    {
        if (resumeSimpleBannerAfterThisAd)
        {
            //Debug.LogError("After");
            ShowSimpleBannerAd();
        }

        if (resumeBigBannerAfterThisAd)
        {
            ShowBigBannerAd();
        }
    }

    #region BigBannerAd

    void CreateBig_BannerView()
    {
        if (!bigBannerAdToggler)
            return;

        if (PlayerPrefs.GetInt("RemoveAds") == 1)
        {
            return;
        }

        //Debug.LogError("Creating banner view");
        // If we already have a banner, destroy the old one.
        if (bigBannerView != null)
        {
            bigBannerView.Destroy();
            bigBannerView = null;
        }
        // Create a 320x50 banner at top of the screen

        float screenWidth = Screen.width;
        float screenHeight = Screen.height;

        int positionX = Mathf.RoundToInt(screenWidth * customBigBannerX);
        int positionY = Mathf.RoundToInt(screenHeight * customBigBannerY);
        bigBannerView = new BannerView(bigBannerId, AdSize.MediumRectangle, positionX, positionY);//valueX, valueY); //y450   x60

    }

    public void LoadBigBannerAd()
    {
        if (!bigBannerAdToggler)
            return;
        // create an instance of a banner view first.
        if (bigBannerView == null)
        {
            CreateBig_BannerView();
        }
        // create our request used to load the ad.
        var adRequest = new AdRequest();
        adRequest.Keywords.Add("unity-admob-sample");
        // send the request to load the ad.
        Debug.Log("Loading Big banner ad.");
        bigBannerView.LoadAd(adRequest);
        HideBigBannerAd();
    }

    bool bigBannerShown = false;

    [ContextMenu("Show Big Banner")]
    public void ShowBigBannerAd()
    {
        //Debug.LogError("Show Big");
        bigBannerShown = true;
        bigBannerView?.Show();
    }

    [ContextMenu("Hide Big Banner")]
    public void HideBigBannerAd()
    {
        bigBannerShown = false;
        bigBannerView?.Hide();
    }
    #endregion

    #region Interstitial

    Coroutine interRecallCo;
    void RequestInterstitialAd()
    {
        if (!interstitialAdToggler)
            return;

        if (PlayerPrefs.GetInt("RemoveAds") == 1)
        {
            return;
        }
        if (interstitialAd != null)
        {
            Debug.Log("Already Loaded interstitial ad. returning");
            return;
        }

        Debug.Log("Loading the interstitial ad.");

        // create our request used to load the ad.
        var adRequest = new AdRequest();
        adRequest.Keywords.Add("unity-admob-sample");

        InterstitialAd.Load(interId, adRequest, (InterstitialAd ad, LoadAdError error) =>
        {
            if (error != null || ad == null)
            {
                print("Interstitial ad failed to load" + error);
                if (interRecallCo == null)
                    interRecallCo = StartCoroutine(ReloadInterstitialAdCo());
                return;
            }
            else if (ad != null)
            {
                print("Interstitial ad loaded !!" + ad.GetResponseInfo());
                interstitialAd = ad;
            }
            InterstitialAdEvent(interstitialAd);
        });
    }

    IEnumerator ReloadInterstitialAdCo()
    {
        yield return new WaitForSeconds(15f);
        if (interstitialAd == null)
        {
            RequestInterstitialAd();
            interRecallCo = null;
        }
    }

    [ContextMenu("Show Inter")]
    public void ShowInterstitialAd()
    {
        if (!interstitialAdToggler)
            return;
        if (interstitialAd != null && interstitialAd.CanShowAd())
        {
            ShownCheck();
            interstitialAd.Show();
            HideBannerAdsIfOpen();
        }
        else
        {
            print("Intersititial ad not ready!!");
            if (interRecallCo != null)
            {
                StopCoroutine(interRecallCo);
                interRecallCo = null;
            }

            interstitialAd = null;
            RequestInterstitialAd();
        }
        
    }
    public void InterstitialAdEvent(InterstitialAd ad)
    {
        // Raised when the ad is estimated to have earned money.
        ad.OnAdPaid += (AdValue adValue) =>
        {
            Debug.Log("Interstitial ad paid {0} {1}." +
                adValue.Value +
                adValue.CurrencyCode);
        };
        // Raised when an impression is recorded for an ad.
        ad.OnAdImpressionRecorded += () =>
        {
            Debug.Log("Interstitial ad recorded an impression.");
        };
        // Raised when a click is recorded for an ad.
        ad.OnAdClicked += () =>
        {
            Debug.Log("Interstitial ad was clicked.");
        };
        // Raised when an ad opened full screen content.
        ad.OnAdFullScreenContentOpened += () =>
        {
            Debug.Log("Interstitial ad full screen content opened.");
        };
        // Raised when the ad closed full screen content.
        ad.OnAdFullScreenContentClosed += () =>
        {
            Debug.Log("Interstitial ad full screen content closed.");
            if (interRecallCo != null)
            {
                StopCoroutine(interRecallCo);
                interRecallCo = null;
            }

            interstitialAd = null;
            RequestInterstitialAd();
            ShowBannerAdsIfOpen();
        };
        // Raised when the ad failed to open full screen content.
        ad.OnAdFullScreenContentFailed += (AdError error) =>
        {
            Debug.LogError("Interstitial ad failed to open full screen content " +
                           "with error : " + error);
            if (interRecallCo != null)
            {
                StopCoroutine(interRecallCo);
                interRecallCo = null;
            }
            interstitialAd = null;
            RequestInterstitialAd();
            ShowBannerAdsIfOpen();
        };
    }

    #endregion

    #region Rewarded
    Coroutine rewardedRecallCo;
    void RequestRewardedAd()
    {
        if (rewardedAd != null)
        {
            Debug.Log("Already Loaded Rewarded adrr returning");
            return;
        }

        Debug.Log("Loading the rewarded ad.");

        // create our request used to load the ad.
        var adRequest = new AdRequest();
        adRequest.Keywords.Add("unity-admob-sample");

        RewardedAd.Load(rewardedId, adRequest, (RewardedAd ad, LoadAdError error) =>
        {
            if (error != null || ad == null)
            {
                print("Rewarded failed to load" + error);
                if (rewardedRecallCo == null)
                    rewardedRecallCo = StartCoroutine(ReloadRewardedAdCo());
                return;
            }
            else if (ad != null)
            {
                print("Rewarded ad loaded !!");
                rewardedAd = ad;
            }
            RewardedAdEvents(rewardedAd);
        });
    }
    public void ShowRewardedAd(int re = 0)
    {
        if (rewardedAd != null && rewardedAd.CanShowAd())
        {
            ShownCheck();
            rewardedAd.Show((Reward reward) =>
            {
                rewardedAd = null;
                print("Give Reward to Player !!");
                rewardedAdEvent.Invoke();
                //Menu.instance.GetRewards(re);
            });
            HideBannerAdsIfOpen();
        }
        else
        {
            if (rewardedRecallCo != null)
            {
                StopCoroutine(rewardedRecallCo);
                rewardedRecallCo = null;
            }
            rewardedAd = null;
            RequestRewardedAd();
            print("Rewarded ad not ready");
        }
        
    }
    public void RewardedAdEvents(RewardedAd ad)
    {
        // Raised when the ad is estimated to have earned money.
        ad.OnAdPaid += (AdValue adValue) =>
        {
            Debug.Log("Rewarded ad paid {0} {1}." +
                adValue.Value +
                adValue.CurrencyCode);
        };
        // Raised when an impression is recorded for an ad.
        ad.OnAdImpressionRecorded += () =>
        {
            Debug.Log("Rewarded ad recorded an impression.");
        };
        // Raised when a click is recorded for an ad.
        ad.OnAdClicked += () =>
        {
            Debug.Log("Rewarded ad was clicked.");
        };
        // Raised when an ad opened full screen content.
        ad.OnAdFullScreenContentOpened += () =>
        {
            Debug.Log("Rewarded ad full screen content opened.");
        };
        // Raised when the ad closed full screen content.
        ad.OnAdFullScreenContentClosed += () =>
        {
            Debug.Log("Rewarded ad full screen content closed.");
            if (rewardedRecallCo != null)
            {
                StopCoroutine(rewardedRecallCo);
                rewardedRecallCo = null;
            }

            rewardedAd = null;
            RequestRewardedAd();
            ShowBannerAdsIfOpen();
        };
        // Raised when the ad failed to open full screen content.
        ad.OnAdFullScreenContentFailed += (AdError error) =>
        {
            Debug.LogError("Rewarded ad failed to open full screen content " +
                           "with error : " + error);
            if (rewardedRecallCo != null)
            {
                StopCoroutine(rewardedRecallCo);
                rewardedRecallCo = null;
            }

            rewardedAd = null;
            RequestRewardedAd();
            ShowBannerAdsIfOpen();
        };
    }
    IEnumerator ReloadRewardedAdCo()
    {
        yield return new WaitForSeconds(15f);
        if (rewardedAd == null)
        {
            RequestRewardedAd();

            if (rewardedRecallCo != null)
            {
                StopCoroutine(rewardedRecallCo);
                rewardedRecallCo = null;
            }
        }
    }

    #endregion

    #region AppOpenAd
    private void OnAppStateChanged(AppState state)
    {
        Debug.Log("App State changed to : " + state);

        // If the app is Foregrounded and the ad is available, show it.
        if (state == AppState.Foreground)
        {
            ShowAppOpenAd();
        }
    }

    bool appOpenFirst = true;
    void RequestAppOpenAd()
    {
        if (!appOpenAdToggler)
            return;

        if (PlayerPrefs.GetInt("RemoveAds") == 1)
        {
            return;
        }
        if (appOpenAd != null)
        {
            Debug.Log("Already Loaded Rewarded adrr returning");
            return;
        }
        Debug.Log("Loading app open ad.");
        // Create our request used to load the ad.
        var adRequest = new AdRequest();

        // Send the request to load the ad.
        AppOpenAd.Load(appOpenId, adRequest, (AppOpenAd ad, LoadAdError error) =>
        {
            // If the operation failed with a reason.
            if (error != null || ad == null)
            {
                Debug.LogError("App open ad failed to load an ad with error : "
                                + error);

                if (appOpenRecallCo == null)
                    appOpenRecallCo = StartCoroutine(ReLoadAppOpenAdCo());
                return;
            }
            Debug.Log("App open ad loaded with response : " + ad.GetResponseInfo());
            appOpenAd = ad;
            // Register to ad events to extend functionality.
            RegisterAppOpenEventHandlers(ad);

            if (appOpenFirst == true)
            {
                appOpenFirst = false;
                ShowAppOpenAd();
            }
        });
    }
    public void ShowAppOpenAd()
    {
        if (!appOpenAdToggler)
            return;

        if (appOpenAd != null && appOpenAd.CanShowAd())
        {
            ShownCheck();
            Debug.Log("Showing app open ad.");
            appOpenAd.Show();
            HideBannerAdsIfOpen();
        }
        else
        {
            if (appOpenRecallCo != null)
            {
                StopCoroutine(appOpenRecallCo);
                appOpenRecallCo = null;
            }
            appOpenAd = null;
            RequestAppOpenAd();
        }
    }
    private void RegisterAppOpenEventHandlers(AppOpenAd ad)
    {
        // Raised when the ad closed full screen content.
        ad.OnAdFullScreenContentClosed += () =>
        {
            //Uncomment this as per_ need.
            //if (Menu.instance != null)
            //{
            //    Menu.instance.LoadingScreen.SetActive(false);
            //}
            Debug.Log("App open ad full screen content closed.");

            if (appOpenRecallCo != null)
            {
                StopCoroutine(appOpenRecallCo);
                appOpenRecallCo = null;
            }
            appOpenAd = null;
            RequestAppOpenAd();
            ShowBannerAdsIfOpen();
        };
        // Raised when the ad failed to open full screen content.
        ad.OnAdFullScreenContentFailed += (AdError error) =>
        {
            if (appOpenRecallCo != null)
            {
                StopCoroutine(appOpenRecallCo);
                appOpenRecallCo = null;
            }
            appOpenAd = null;
            RequestAppOpenAd();
            ShowBannerAdsIfOpen();
        };
    }
    Coroutine appOpenRecallCo;
    IEnumerator ReLoadAppOpenAdCo()
    {
        yield return new WaitForSeconds(15f);
        if (appOpenAd == null)
        {
            RequestAppOpenAd();
            if (appOpenRecallCo != null)
            {
                StopCoroutine(appOpenRecallCo);
                appOpenRecallCo = null;
            }
        }
    }

    private bool wasPaused = false; // Initial state is considered paused

    bool adAlreadyShown;
    private void OnApplicationPause(bool pauseStatus)
    {
        if (wasPaused && !pauseStatus && adAlreadyShown == false)
        {
            //Debug.LogError("AAA");
            StartCoroutine(ShowAppOpenAdWithDelay());
        }
        wasPaused = pauseStatus;
    }

    IEnumerator ShowAppOpenAdWithDelay()
    {
        yield return new WaitForSeconds(0.1f);
        ShowAppOpenAd();
    }

    Coroutine waitCo;
    public void ShownCheck()
    {
        if (waitCo != null)
        {
            StopCoroutine(waitCo);
            waitCo = null;
        }
        waitCo = StartCoroutine(JustShownCheck());
    }
    IEnumerator JustShownCheck()
    {
        adAlreadyShown = true;
        yield return new WaitForSeconds(10.0f);
        adAlreadyShown = false;
    }
    #endregion
}