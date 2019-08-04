using GoogleMobileAds.Api;
using GoogleMobileAds.Api.Mediation.AppLovin;
using GoogleMobileAds.Api.Mediation.UnityAds;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interstitial : MonoBehaviour
{
    private InterstitialAd interstitial;
    private int AdsOnLevel = 3;

    private static Interstitial intestitialAd;
    public static Interstitial Instance()
    {
        if (intestitialAd == null)
        {
            if (!Application.isPlaying)
            {
                GameObject exists = GameObject.Find("Interstitial");
                if (exists)
                {
                    Destroy(exists);
                }
            }
            intestitialAd = Instantiate(Resources.Load<Interstitial>("Interstitial"));
            intestitialAd.name = "Interstitial";
        }
        return intestitialAd;
    }

    void Start()
    {
#if UNITY_ANDROID
        string appId = "ca-app-pub-8954387591640940~6201948368";
#elif UNITY_IPHONE
        string appId = "ca-app-pub-8954387591640940~2103755403";
#else
        string appId = "";
#endif

        MobileAds.Initialize(appId);
        AppLovin.Initialize();
        UnityAds.SetGDPRConsentMetaData(true);
    }


    public void ShowIntestitial(int level)
    {
        if (level > AdsOnLevel)
        {
            RequestIntestitial();
            AdsOnLevel += 3;
        }
    }

    public void ShowIntestitial()
    {
        RequestIntestitial();
    }

    private void RequestIntestitial()
    {
#if UNITY_ANDROID
        string interstitialUnitId = "ca-app-pub-8954387591640940/2809498268";
#elif UNITY_IPHONE
        string interstitialUnitId = "ca-app-pub-8954387591640940/6436740329";
#else
        string interstitialUnitId = "";
#endif
        if (interstitial != null)
        {
            interstitial.Destroy();
        }
        this.interstitial = new InterstitialAd(interstitialUnitId);

        // Called when an ad request has successfully loaded.
        this.interstitial.OnAdLoaded += HandleOnAdLoaded;
        // Called when an ad request failed to load.
        this.interstitial.OnAdFailedToLoad += HandleOnAdFailedToLoad;
        // Called when an ad is shown.
        this.interstitial.OnAdOpening += HandleOnAdOpened;
        // Called when the ad is closed.
        this.interstitial.OnAdClosed += HandleOnAdClosed;
        // Called when the ad click caused the user to leave the application.
        this.interstitial.OnAdLeavingApplication += HandleOnAdLeavingApplication;

        interstitial.LoadAd(GetAdRequest());

        ShowAd();
    }

    public void ShowAd()
    {
        if (this.interstitial.IsLoaded())
            this.interstitial.Show();
    }

    private AdRequest GetAdRequest()
    {
        return new AdRequest.Builder().Build();
    }

    public void HandleOnAdLoaded(object sender, EventArgs args)
    {
    }
    public void HandleOnAdFailedToLoad(object sender, AdFailedToLoadEventArgs args)
    {
        MonoBehaviour.print("HandleFailedToReceiveAd event received with message: "
                            + args.Message);
    }

    public void HandleOnAdOpened(object sender, EventArgs args)
    {
        MonoBehaviour.print("HandleAdOpened event received");
    }

    public void HandleOnAdClosed(object sender, EventArgs args)
    {
        MonoBehaviour.print("HandleAdClosed event received");
    }

    public void HandleOnAdLeavingApplication(object sender, EventArgs args)
    {
        MonoBehaviour.print("HandleAdLeavingApplication event received");
    }
}
