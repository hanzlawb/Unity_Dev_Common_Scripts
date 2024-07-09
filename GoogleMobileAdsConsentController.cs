using GoogleMobileAds.Ump.Api;
using System;
using UnityEngine;
using UnityEngine.UI;
using GoogleMobileAds.Api;
using System.Collections.Generic;
using GoogleMobileAds.Common;

/// <summary>
/// Helper class that implements consent using the Google User Messaging Platform (UMP) SDK.
/// </summary>
public class GoogleMobileAdsConsentController : MonoBehaviour
{
    /// <summary>
    /// If true, it is safe to call MobileAds.Initialize() and load Ads.
    /// </summary>
    public bool CanRequestAds =>
        ConsentInformation.ConsentStatus == ConsentStatus.Obtained ||
        ConsentInformation.ConsentStatus == ConsentStatus.NotRequired;

    public void Start()
    {
#if UNITY_ANDROID
        Invoke(nameof(AdInitializer), 1f);
#endif
    }

    #region "Consent And Ads Initialization"
    public void AdInitializer()
    {
        //if(internetAvailable)
        InitializeConsentBasedAds();
    }
    private void InitializeConsentBasedAds()
    {
#if UNITY_IOS
        if (FirebaseInitialize.Instance)
        {
            if (FirebaseInitialize.Instance.IsAdmobConsentShowing)
            {
                if (_consentController.CanRequestAds)
                {
                    InitAdManager();
                }
                // Ensures that privacy and consent information is up to date.
                InitializeGoogleMobileAdsConsent();
            }
            else
            {
                InitAdManager();
            }
        }
#endif

#if UNITY_ANDROID
        if (CanRequestAds)
        {
            InitAdManager();
        }
        // Ensures that privacy and consent information is up to date.
        InitializeGoogleMobileAdsConsent();

#endif
    }
    public void InitAdManager()
    {
#if UNITY_IOS
        MobileAds.SetiOSAppPauseOnBackground(true);
#endif
        // https://developers.google.com/admob/unity/quick-start#raise_ad_events_on_the_unity_main_thread
        MobileAds.RaiseAdEventsOnUnityMainThread = true;
        List<String> deviceIds = new List<String>() { AdRequest.TestDeviceSimulator };

#if UNITY_ANDROID
        MobileAds.RaiseAdEventsOnUnityMainThread = true;
#endif
        MobileAds.Initialize(HandleInitCompleteAction);

        //AppStateEventNotifier.AppStateChanged += OnAppStateChanged;
    }

    private void HandleInitCompleteAction(InitializationStatus initstatus)
    {
        Debug.Log("Initialization complete.");

        MobileAdsEventExecutor.ExecuteInUpdate(() =>
        {
            //Debug.Log("Mobile Ads Initialized From Consent");
            //AdsManager_New.Instance.InitializeFirebaseAndAds();


        });
    }
    #endregion

    /// <summary>
    /// Startup method for the Google User Messaging Platform (UMP) SDK
    /// which will run all startup logic including loading any required
    /// updates and displaying any required forms.
    /// </summary>
    internal static List<string> TestDeviceIds = new List<string>()
        {
            AdRequest.TestDeviceSimulator,
#if UNITY_IPHONE
            "96e23e80653bb28980d3f40beb58915c",
#elif UNITY_ANDROID
            "99f15fd987c9498087f6ed0add26e05d",
#endif
        };

    private void InitializeGoogleMobileAdsConsent()
    {
        Debug.Log("Google Mobile Ads gathering consent.");
        GatherConsent((string error) =>
        {
            if (error != null)
            {
                Debug.LogError("Failed to gather consent with error: " +
                    error);

                //InitAdManager();
                PlayerPrefs.SetInt("Consent", 0);
            }
            else
            {
                Debug.Log("Google Mobile Ads consent updated.");
            }

            if (CanRequestAds)
            {
                InitAdManager();
            }

            Debug.Log("Consent Called");
        });
    }
    public void GatherConsent(Action<string> onComplete)
    {
        Debug.Log("Gathering consent.");

        var requestParameters = new ConsentRequestParameters
        {
            // False means users are not under age.
            TagForUnderAgeOfConsent = false,
            ConsentDebugSettings = new ConsentDebugSettings
            {
                // For debugging consent settings by geography.
                DebugGeography = DebugGeography.Disabled,
                // https://developers.google.com/admob/unity/test-ads
                TestDeviceHashedIds = TestDeviceIds,
            }
        };

        // Combine the callback with an error popup handler.
        onComplete = (onComplete == null)
            ? UpdateErrorPopup
            : onComplete + UpdateErrorPopup;

        // The Google Mobile Ads SDK provides the User Messaging Platform (Google's
        // IAB Certified consent management platform) as one solution to capture
        // consent for users in GDPR impacted countries. This is an example and
        // you can choose another consent management platform to capture consent.
        ConsentInformation.Update(requestParameters, (FormError updateError) =>
        {
            // Enable the change privacy settings button.
            //UpdatePrivacyButton();

            if (updateError != null)
            {
                onComplete(updateError.Message);
                return;
            }

            // Determine the consent-related action to take based on the ConsentStatus.
            if (CanRequestAds)
            {
                // Consent has already been gathered or not required.
                // Return control back to the user.
                onComplete(null);
                return;
            }

            // Consent not obtained and is required.
            // Load the initial consent request form for the user.
            ConsentForm.LoadAndShowConsentFormIfRequired((FormError showError) =>
            {
                //UpdatePrivacyButton();
                if (showError != null)
                {
                    // Form showing failed.
                    if (onComplete != null)
                    {
                        onComplete(showError.Message);
                    }
                }
                // Form showing succeeded.
                else if (onComplete != null)
                {
                    onComplete(null);
                }
            });
        });
    }

    /// <summary>
    /// Shows the privacy options form to the user.
    /// </summary>
    /// <remarks>
    /// Your app needs to allow the user to change their consent status at any time.
    /// Load another form and store it to allow the user to change their consent status
    /// </remarks>
    public void ShowPrivacyOptionsForm(Action<string> onComplete)
    {
        Debug.Log("Showing privacy options form.");

        // combine the callback with an error popup handler.
        onComplete = (onComplete == null)
            ? UpdateErrorPopup
            : onComplete + UpdateErrorPopup;

        ConsentForm.LoadAndShowConsentFormIfRequired((FormError showError) =>
        {
            //UpdatePrivacyButton();
            if (showError != null)
            {
                // Form showing failed.
                if (onComplete != null)
                {
                    onComplete(showError.Message);
                }
            }
            // Form showing succeeded.
            else if (onComplete != null)
            {
                onComplete(null);
            }
        });
    }

    /// <summary>
    /// Reset ConsentInformation for the user.
    /// </summary>
    public void ResetConsentInformation()
    {
        ConsentInformation.Reset();
    }
    void UpdateErrorPopup(string message)
    {
        if (string.IsNullOrEmpty(message))
        {
            return;
        }
    }
}