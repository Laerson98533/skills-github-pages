using System;
using UnityEngine;

#if GOOGLE_MOBILE_ADS
using GoogleMobileAds.Api;
#endif

namespace QuizRush.Ads
{
    /// <summary>
    /// Controls the banner ad lifecycle restricted to the menu screen.
    /// </summary>
    public class AdBannerService : MonoBehaviour
    {
        [SerializeField] private AdConfig config;

#if GOOGLE_MOBILE_ADS
        private BannerView bannerView;
#endif

        /// <summary>
        /// Loads and shows the banner ad when enabled.
        /// </summary>
        public void Show()
        {
#if GOOGLE_MOBILE_ADS
            EnsureConfig();
            if (config == null || !config.enableAds || !config.enableBanner)
            {
                return;
            }

            if (bannerView == null)
            {
                bannerView = new BannerView(config.bannerAdUnitId, AdSize.Banner, AdPosition.Bottom);
                bannerView.OnAdFailedToLoad += (sender, args) => Debug.LogWarning($"Banner failed: {args.LoadAdError}");
            }

            bannerView.LoadAd(CreateRequest());
#else
            Debug.Log("AdBannerService: Banner request skipped (GOOGLE_MOBILE_ADS define missing).");
#endif
        }

        /// <summary>
        /// Hides the banner when leaving the menu hub.
        /// </summary>
        public void Hide()
        {
#if GOOGLE_MOBILE_ADS
            bannerView?.Hide();
#endif
        }

        /// <summary>
        /// Destroys the banner to free resources.
        /// </summary>
        public void DestroyBanner()
        {
#if GOOGLE_MOBILE_ADS
            bannerView?.Destroy();
            bannerView = null;
#endif
        }

        private void OnDestroy()
        {
            DestroyBanner();
        }

#if GOOGLE_MOBILE_ADS
        private void EnsureConfig()
        {
            if (config == null)
            {
                config = Resources.Load<AdConfig>("AdConfig");
            }
        }

        private AdRequest CreateRequest()
        {
            return new AdRequest.Builder().Build();
        }
#endif
    }
}
