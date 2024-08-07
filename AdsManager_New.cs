using System.Collections;
using UnityEngine;
using GoogleMobileAds;
using GoogleMobileAds.Api;
using GoogleMobileAds.Common;
using System;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
//using Firebase;
//using Firebase.Analytics;
//using Firebase.RemoteConfig;

public class AdsManager_New : MonoBehaviour
{
    public static AdsManager_New Instance;
    //public bool testMode = false;
    public GameObject noInternetCanvas, interLoadingPanel,appOpenBg;

#if UNITY_ANDROID
    string simpleBannerId = "ca-app-pub-9585178868261997/1096013729";
    string bigBannerId = "ca-app-pub-3940256099942544/6300978111";
    string interId = "ca-app-pub-9585178868261997/9082276967";
    string rewardedId = "ca-app-pub-9585178868261997/4063004130";
    string rewardedInterstitialId = "ca-app-pub-9585178868261997/4063004130";
    string appOpenId = "ca-app-pub-9585178868261997/2516868615";
#elif UNITY_IOS

    string simpleBannerId = "ca-app-pub-9585178868261997/1096013729";
    string bigBannerId = "ca-app-pub-3940256099942544/6300978111";
    string interId = "ca-app-pub-9585178868261997/9082276967";
    string rewardedId = "ca-app-pub-9585178868261997/4063004130";
    string appOpenId = "ca-app-pub-9585178868261997/2516868615";
#endif
    BannerView simpleBannerView, bigBannerView;
    InterstitialAd interstitialAd;
    RewardedAd rewardedAd;
    RewardedInterstitialAd rewardedInterstitialAd;
    AppOpenAd appOpenAd;

    [Header("Turn Ads On/Off")]
    public bool simpleBannerAdToggler;
    public bool appOpenAdToggler;
    public bool interstitialAdToggler;
    public bool rewardedAdToggler;
    public bool bigBannerAdToggler;
    public bool rewardedInterstitialAdToggler;

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

        //if (testMode)
        //{
        //    simpleBannerId = "ca-app-pub-3940256099942544/6300978111";
        //    bigBannerId = "ca-app-pub-3940256099942544/6300978111";
        //    interId = "ca-app-pub-3940256099942544/1033173712";
        //    rewardedId = "ca-app-pub-3940256099942544/5224354917";
        //    appOpenId = "ca-app-pub-3940256099942544/3419835294";
        //    rewardedInterstitialId = "ca-app-pub-3940256099942544/5354046379";
        //}
    }

    private void Start()
    {
        Screen.sleepTimeout = 10000;
        //8.4.1
        //Invoke(nameof(InitializeFirebaseAndAds),0.1f);
        //RequestInterstitialAd();
        StartCoroutine(RequestAfterTime());
    }

    public void InitializeFirebaseAndAds()
    {
        MobileAds.Initialize(initStatus =>
        {
            print("Ads Initialised !!");
            Invoke(nameof(InitializeFBAfter), 0.5f);
            
        });
    }

    void InitializeFBAfter()
    {
        //FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        //{
        //    FirebaseManager.instance.remoteConfig = FirebaseRemoteConfig.DefaultInstance;
        //    FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);
        //    FirebaseManager.instance.remoteConfig.SetDefaultsAsync(FirebaseManager.instance.GetRemoteConfigDefaults());

        //    FirebaseManager.instance.FetchRemoteConfigValues();
        //    RequestAds();
        //});
    }

    IEnumerator RequestAfterTime()
    {
        yield return new WaitForSecondsRealtime(5f);
        RequestAds();
    }
    void RequestAds()
    {
        if (rewardedAdToggler && rewardedAd == null)
            RequestRewardedAd();

        if (rewardedInterstitialAdToggler && rewardedInterstitialAd == null)
            RequestRewardedInterstitialAd();

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
        simpleBannerView = new BannerView(simpleBannerId, AdSize.Banner, AdPosition.Bottom);
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
        simpleBannerShown = true;
        simpleBannerView?.Show();
    }

    [ContextMenu("Hide Banner")]
    public void HideSimpleBannerAd()
    {
        simpleBannerShown = false;
        simpleBannerView?.Hide();
    }
    #endregion
    bool resumeSimpleBannerAfterThisAd, resumeBigBannerAfterThisAd;
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
            HideBannerAdsIfOpen();
            StartCoroutine(ShowInterCo());
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
    IEnumerator ShowInterCo()
    {
        interLoadingPanel.SetActive(true);
        for(int i = 0; i < 4; i++)
        {
            yield return new WaitForSeconds(1.0f);
        }
        interLoadingPanel.SetActive(false);
        interstitialAd.Show();
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
            ShowRewardedInterstitialAd();
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
    #region RewardedInterstitial
    Coroutine rewardedInterstitialRecallCo;
    void RequestRewardedInterstitialAd()
    {
        if (rewardedAd != null)
        {
            Debug.Log("Already Loaded Rewarded Interstitial ad returning");
            return;
        }

        Debug.Log("Loading the rewarded Interstitial ad.");

        // create our request used to load the ad.
        var adRequest = new AdRequest();
        adRequest.Keywords.Add("unity-admob-sample");

        RewardedInterstitialAd.Load(rewardedInterstitialId, adRequest, (RewardedInterstitialAd ad, LoadAdError error) =>
        {
            if (error != null || ad == null)
            {
                print("Rewarded Interstitial failed to load" + error);
                if (rewardedInterstitialRecallCo == null)
                    rewardedInterstitialRecallCo = StartCoroutine(ReloadRewardedInterstitialAdCo());
                return;
            }
            else if (ad != null)
            {
                print("Rewarded Interstitial ad loaded !!");
                rewardedInterstitialAd = ad;
            }
            RewardedInterstitialAdEvents(rewardedInterstitialAd);
        });
    }
    public void ShowRewardedInterstitialAd(int re = 0)
    {
        if (rewardedInterstitialAd != null && rewardedInterstitialAd.CanShowAd())
        {
            ShownCheck();
            rewardedInterstitialAd.Show((Reward reward) =>
            {
                rewardedInterstitialAd = null;
                print("Give Reward to Player !!");
                rewardedAdEvent.Invoke();
                //Menu.instance.GetRewards(re);
            });
            HideBannerAdsIfOpen();
        }
        else
        {
            if (rewardedInterstitialRecallCo != null)
            {
                StopCoroutine(rewardedInterstitialRecallCo);
                rewardedInterstitialRecallCo = null;
            }
            rewardedInterstitialAd = null;
            RequestRewardedInterstitialAd();
            print("Rewarded ad not ready");
        }

    }
    public void RewardedInterstitialAdEvents(RewardedInterstitialAd ad)
    {
        // Raised when the ad is estimated to have earned money.
        ad.OnAdPaid += (AdValue adValue) =>
        {
            Debug.Log("Rewarded Interstitial ad paid {0} {1}." +
                adValue.Value +
                adValue.CurrencyCode);
        };
        // Raised when an impression is recorded for an ad.
        ad.OnAdImpressionRecorded += () =>
        {
            Debug.Log("Rewarded Interstitial ad recorded an impression.");
        };
        // Raised when a click is recorded for an ad.
        ad.OnAdClicked += () =>
        {
            Debug.Log("Rewarded Interstitial ad was clicked.");
        };
        // Raised when an ad opened full screen content.
        ad.OnAdFullScreenContentOpened += () =>
        {
            Debug.Log("Rewarded Interstitial ad full screen content opened.");
        };
        // Raised when the ad closed full screen content.
        ad.OnAdFullScreenContentClosed += () =>
        {
            Debug.Log("Rewarded Interstitial ad full screen content closed.");
            if (rewardedInterstitialRecallCo != null)
            {
                StopCoroutine(rewardedInterstitialRecallCo);
                rewardedInterstitialRecallCo = null;
            }

            rewardedInterstitialAd = null;
            RequestRewardedInterstitialAd();
            ShowBannerAdsIfOpen();
        };
        // Raised when the ad failed to open full screen content.
        ad.OnAdFullScreenContentFailed += (AdError error) =>
        {
            Debug.LogError("Rewarded Interstitial ad failed to open full screen content " +
                           "with error : " + error);
            if (rewardedInterstitialRecallCo != null)
            {
                StopCoroutine(rewardedInterstitialRecallCo);
                rewardedInterstitialRecallCo = null;
            }

            rewardedInterstitialAd = null;
            RequestRewardedInterstitialAd();
            ShowBannerAdsIfOpen();
        };
    }
    IEnumerator ReloadRewardedInterstitialAdCo()
    {
        yield return new WaitForSeconds(15f);
        if (rewardedInterstitialAd == null)
        {
            RequestRewardedInterstitialAd();

            if (rewardedInterstitialRecallCo != null)
            {
                StopCoroutine(rewardedInterstitialRecallCo);
                rewardedInterstitialRecallCo = null;
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

            if (appOpenFirst == true)// && FirebaseManager.instance.IsAppOpenStartEnabled())
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

        if (appOpenAd != null && appOpenAd.CanShowAd())// && FirebaseManager.instance.IsAppOpenResumeEnabled())
        {
            appOpenBg.SetActive(true);
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
            if(SceneManager.GetActiveScene().buildIndex == 0)
            {
                SceneManager.LoadScene(1);
            }
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
            appOpenBg.SetActive(false);
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
            appOpenBg.SetActive(false);
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
        if (wasPaused && !pauseStatus && adAlreadyShown == false)// && FirebaseManager.instance.IsAppOpenResumeEnabled())
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