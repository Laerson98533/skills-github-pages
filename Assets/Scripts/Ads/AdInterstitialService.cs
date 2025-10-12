using System;
using UnityEngine;
using QuizRush.Analytics;

#if GOOGLE_MOBILE_ADS
using GoogleMobileAds.Api;
#endif

namespace QuizRush.Ads
{
    /// <summary>
    /// Manages interstitial ad loading and frequency capping.
    /// </summary>
    public class AdInterstitialService : MonoBehaviour
    {
        [SerializeField] private AdConfig config;
        [SerializeField] private AnalyticsHook analyticsHook;
        [SerializeField] private float frequencyCapSeconds = 120f;

        private float lastShowTime = float.NegativeInfinity;
        private int roundsSinceLastShow;

#if GOOGLE_MOBILE_ADS
        private InterstitialAd interstitialAd;
#endif

        private void Start()
        {
            EnsureConfig();
            EnsureAnalytics();
            Load();
        }

        private void OnDestroy()
        {
#if GOOGLE_MOBILE_ADS
            interstitialAd?.Destroy();
#endif
        }

        /// <summary>
        /// Attempts to show an interstitial if the conditions are met.
        /// </summary>
        public void TryShowIfEligible()
        {
#if GOOGLE_MOBILE_ADS
            EnsureConfig();
            if (config == null || !config.enableAds || !config.enableInterstitial)
            {
                return;
            }

            if (Time.unscaledTime - lastShowTime < frequencyCapSeconds)
            {
                return;
            }

            if (roundsSinceLastShow < 2)
            {
                return;
            }

            if (interstitialAd != null && interstitialAd.CanShowAd())
            {
                analyticsHook?.Track("ad_interstitial_shown");
                interstitialAd.Show();
                lastShowTime = Time.unscaledTime;
                roundsSinceLastShow = 0;
            }
#else
            Debug.Log("AdInterstitialService: Interstitial show skipped (GOOGLE_MOBILE_ADS define missing).");
#endif
        }

        /// <summary>
        /// Should be called when a round ends to update eligibility state.
        /// </summary>
        public void NotifyRoundCompleted()
        {
            roundsSinceLastShow++;
            if (roundsSinceLastShow >= 2)
            {
                TryShowIfEligible();
            }
        }

        private void Load()
        {
#if GOOGLE_MOBILE_ADS
            EnsureConfig();
            if (config == null || !config.enableAds || !config.enableInterstitial)
            {
                return;
            }

            if (interstitialAd != null)
            {
                interstitialAd.Destroy();
                interstitialAd = null;
            }

            interstitialAd = new InterstitialAd(config.interstitialAdUnitId);
            interstitialAd.OnAdFullScreenContentClosed += (sender, args) =>
            {
                Load();
            };
            interstitialAd.OnAdFailedToLoad += (sender, args) => Debug.LogWarning($"Interstitial failed: {args.LoadAdError}");
            interstitialAd.LoadAd(new AdRequest.Builder().Build());
#else
            Debug.Log("AdInterstitialService: Load skipped (GOOGLE_MOBILE_ADS define missing).");
#endif
        }

        private void EnsureConfig()
        {
            if (config == null)
            {
                config = Resources.Load<AdConfig>("AdConfig");
            }
        }

        private void EnsureAnalytics()
        {
            if (analyticsHook == null)
            {
                analyticsHook = FindObjectOfType<AnalyticsHook>();
            }
        }
    }
}
