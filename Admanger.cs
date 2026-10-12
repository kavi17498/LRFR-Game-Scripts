using System;
using UnityEngine;
using GoogleMobileAds.Api;

public class AdsManager : MonoBehaviour
{
    public static AdsManager Instance { get; private set; }

    [Header("AdMob IDs")]
#if UNITY_ANDROID
    [SerializeField] private string bannerAdId = "ca-app-pub-2191454323846211/9545023928";
    [SerializeField] private string interstitialAdId = "ca-app-pub-2191454323846211/1164236951";
    [SerializeField] private string rewardedAdId = "ca-app-pub-2191454323846211/3251078312";
#elif UNITY_IOS
    [SerializeField] private string bannerAdId = "ca-app-pub-3940256099942544/2934735716";
    [SerializeField] private string interstitialAdId = "ca-app-pub-3940256099942544/4411468910";
    [SerializeField] private string rewardedAdId = "ca-app-pub-3940256099942544/1712485313";
#else
    private string bannerAdId = "unused";
    private string interstitialAdId = "unused";
    private string rewardedAdId = "unused";
#endif

    private BannerView bannerView;
    private InterstitialAd interstitialAd;
    private RewardedAd rewardedAd;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // Keep this object when changing scenes
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        MobileAds.Initialize(initStatus =>
        {
            Debug.Log("AdMob initialized.");

            LoadBannerAd();
            LoadInterstitialAd();
            LoadRewardedAd();
        });
    }

    // =========================
    // BANNER
    // =========================

    public void LoadBannerAd()
    {
        if (bannerView != null)
        {
            bannerView.Destroy();
        }

        bannerView = new BannerView(
            bannerAdId,
            AdSize.Banner,
            AdPosition.Bottom
        );

        AdRequest request = new AdRequest();

        bannerView.LoadAd(request);

        Debug.Log("Banner ad loading...");
    }

    public void ShowBanner()
    {
        if (bannerView != null)
        {
            bannerView.Show();
        }
    }

    public void HideBanner()
    {
        if (bannerView != null)
        {
            bannerView.Hide();
        }
    }

    // =========================
    // INTERSTITIAL
    // =========================

    public void LoadInterstitialAd()
    {
        if (interstitialAd != null)
        {
            interstitialAd.Destroy();
            interstitialAd = null;
        }

        AdRequest request = new AdRequest();

        InterstitialAd.Load(
            interstitialAdId,
            request,
            (ad, error) =>
            {
                if (error != null)
                {
                    Debug.LogError(
                        "Interstitial failed to load: " + error
                    );

                    return;
                }

                interstitialAd = ad;

                Debug.Log("Interstitial loaded.");

                interstitialAd.OnAdFullScreenContentClosed += () =>
                {
                    Debug.Log("Interstitial closed.");

                    // Load another one for next time
                    LoadInterstitialAd();
                };
            }
        );
    }

    public void ShowInterstitial(Action onAdClosed = null)
    {
        if (interstitialAd != null && interstitialAd.CanShowAd())
        {
            interstitialAd.OnAdFullScreenContentClosed += () =>
            {
                Debug.Log("Interstitial closed.");

                onAdClosed?.Invoke();

                // Load the next interstitial
                LoadInterstitialAd();
            };

            interstitialAd.Show();
        }
        else
        {
            Debug.Log("Interstitial is not ready.");

            // Continue immediately if ad isn't available
            onAdClosed?.Invoke();

            // Try loading another ad
            LoadInterstitialAd();
        }
    }

    // =========================
    // REWARDED
    // =========================

    public void LoadRewardedAd()
    {
        if (rewardedAd != null)
        {
            rewardedAd.Destroy();
            rewardedAd = null;
        }

        AdRequest request = new AdRequest();

        RewardedAd.Load(
            rewardedAdId,
            request,
            (ad, error) =>
            {
                if (error != null)
                {
                    Debug.LogError(
                        "Rewarded failed to load: " + error
                    );

                    return;
                }

                rewardedAd = ad;

                Debug.Log("Rewarded ad loaded.");

                rewardedAd.OnAdFullScreenContentClosed += () =>
                {
                    Debug.Log("Rewarded closed.");

                    // Prepare another rewarded ad
                    LoadRewardedAd();
                };
            }
        );
    }

    public void ShowRewarded(Action rewardAction)
    {
        if (rewardedAd != null &&
            rewardedAd.CanShowAd())
        {
            rewardedAd.Show(reward =>
            {
                Debug.Log(
                    $"Reward received: {reward.Amount} {reward.Type}"
                );

                rewardAction?.Invoke();
            });
        }
        else
        {
            Debug.Log("Rewarded ad is not ready.");

            LoadRewardedAd();
        }
    }

    // =========================
    // CLEANUP
    // =========================

    private void OnDestroy()
    {
        if (bannerView != null)
        {
            bannerView.Destroy();
        }

        if (interstitialAd != null)
        {
            interstitialAd.Destroy();
        }

        if (rewardedAd != null)
        {
            rewardedAd.Destroy();
        }
    }
}