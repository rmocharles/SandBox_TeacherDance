using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using GoogleMobileAds.Api;
using GoogleMobileAds.Common;

public class ADManager : MonoBehaviour
{
    private static ADManager instance;

    private RewardedAd rewardedAd;
    private InterstitialAd interstitialAd;

    public static ADManager GetInstance()
    {
        if (instance == null)
            return null;
        return instance;
    }

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        if (!instance) instance = this;
    }

    void Start()
    {
        MobileAds.SetiOSAppPauseOnBackground(true);

        List<String> deviceIds = new List<String>() { AdRequest.TestDeviceSimulator };

        // Add some test device IDs (replace with your own device IDs).
#if UNITY_IPHONE
        deviceIds.Add("96e23e80653bb28980d3f40beb58915c");
#elif UNITY_ANDROID
        deviceIds.Add("75EF8D155528C04DACBBA6F36F433035");
#endif

        // Configure TagForChildDirectedTreatment and test device IDs.
        RequestConfiguration requestConfiguration =
            new RequestConfiguration.Builder()
            .SetTagForChildDirectedTreatment(TagForChildDirectedTreatment.Unspecified)
            .SetTestDeviceIds(deviceIds).build();
        MobileAds.SetRequestConfiguration(requestConfiguration);

        // Initialize the Google Mobile Ads SDK.
        MobileAds.Initialize(HandleInitCompleteAction);


        
        LoadRewardAd();

        LoadFrontAd();
    }

    private void HandleInitCompleteAction(InitializationStatus initstatus)
    {
        Debug.Log("Initialization complete.");

        // Callbacks from GoogleMobileAds are not guaranteed to be called on
        // the main thread.
        // In this example we use MobileAdsEventExecutor to schedule these calls on
        // the next Update() loop.
        MobileAdsEventExecutor.ExecuteInUpdate(() =>
        {
            //statusText.text = "Initialization complete.";
            //RequestBannerAd();
        });
    }

    #region 보상형 광고
#if UNITY_EDITOR
    string rewardID = "unused";
#elif UNITY_ANDROID
        string rewardID = "ca-app-pub-3940256099942544/5224354917";

#elif UNITY_IPHONE
        string rewardID = "ca-app-pub-3940256099942544/1712485313";
#else
        string rewardID = "unexpected_platform";
#endif
    #endregion

    public void LoadRewardAd()
    {
        rewardedAd = new RewardedAd(rewardID);
        rewardedAd.LoadAd(CreateAdRequest());
    }

    public void RequestAndLoadRewardedAd(Action success, Action fail)
    {
        

        // Add Event Handlers
        rewardedAd.OnAdLoaded += (sender, args) =>
        {
            print("Reward ad loaded.");
            rewardedAd.Show();
        };
        rewardedAd.OnAdFailedToLoad += (sender, args) =>
        {
            print("Reward ad failed to load.");
        };
        rewardedAd.OnAdOpening += (sender, args) =>
        {
            print("Reward ad opening.");
        };
        rewardedAd.OnAdFailedToShow += (sender, args) =>
        {
            print("Reward ad failed to show with error: " + args.AdError.GetMessage());
        };
        rewardedAd.OnAdClosed += (sender, args) =>
        {
            print("Reward ad closed.");
            fail();
        };
        rewardedAd.OnUserEarnedReward += (sender, args) =>
        {
            print("User earned Reward ad reward: " + args.Amount);
            success();
        };
        rewardedAd.OnAdDidRecordImpression += (sender, args) =>
        {
            print("Reward ad recorded an impression.");
        };
        rewardedAd.OnPaidEvent += (sender, args) =>
        {
            string msg = string.Format("{0} (currency: {1}, value: {2}",
                                        "Rewarded ad received a paid event.",
                                        args.AdValue.CurrencyCode,
                                        args.AdValue.Value);
            print(msg);
        };

        rewardedAd.Show();

    }

    public void ShowRewardedAd(Action success, Action fail, Action loadFail)
    {
        if (rewardedAd.IsLoaded())
        {
            RequestAndLoadRewardedAd(() =>
            {
                success();
            },
        () =>
        {
            fail();
        });

            LoadRewardAd();
        }
        else
        {
            loadFail();
        }
        
    }

    #region 전면광고
#if UNITY_EDITOR
    string frontID = "ca-app-pub-3940256099942544/1033173712";
#elif UNITY_ANDROID
        string frontID = "ca-app-pub-3940256099942544/1033173712";
#elif UNITY_IPHONE
        string frontID = "ca-app-pub-3940256099942544/4411468910";
#else
        string frontID = "unexpected_platform";
#endif
    #endregion

    public void LoadFrontAd()
    {
        interstitialAd = new InterstitialAd(frontID);
        interstitialAd.LoadAd(CreateAdRequest());
    }

    public void RequestAndLoadInterstitialAd(Action success)
    {

        print("Requesting Interstitial ad.");
        // Add Event Handlers
        interstitialAd.OnAdLoaded += (sender, args) =>
        {
            print("Interstitial ad loaded.");
            interstitialAd.Show();
        };
        interstitialAd.OnAdFailedToLoad += (sender, args) =>
        {
            print("Interstitial ad failed to load with error: " + args.LoadAdError.GetMessage());
        };
        interstitialAd.OnAdOpening += (sender, args) =>
        {
            print("Interstitial ad opening.");
        };
        interstitialAd.OnAdClosed += (sender, args) =>
        {
            print("Interstitial ad closed.");
            success();
        };
        interstitialAd.OnAdDidRecordImpression += (sender, args) =>
        {
            print("Interstitial ad recorded an impression.");
        };
        interstitialAd.OnAdFailedToShow += (sender, args) =>
        {
            print("Interstitial ad failed to show.");
        };
        interstitialAd.OnPaidEvent += (sender, args) =>
        {
            string msg = string.Format("{0} (currency: {1}, value: {2}",
                                        "Interstitial ad received a paid event.",
                                        args.AdValue.CurrencyCode,
                                        args.AdValue.Value);
            print(msg);
        };

        interstitialAd.Show();
    }

    public void ShowAd(Action success)
    {
        RequestAndLoadInterstitialAd(() =>
        {
            success();
        });
        LoadFrontAd();
    }

    private AdRequest CreateAdRequest()
    {
        return new AdRequest.Builder().Build();
    }

}
