using System;
using UnityEngine;

#if GOOGLE_MOBILE_ADS
using GoogleMobileAds.Api;
#endif

namespace QuizRush.Ads
{
    /// <summary>
    /// Handles the rewarded ad placement used to continue a failed round.
    /// </summary>
    public class AdRewardedService : MonoBehaviour
    {
        [SerializeField] private AdConfig config;

#if GOOGLE_MOBILE_ADS
        private RewardedAd rewardedAd;
#endif

        private void Start()
        {
            EnsureConfig();
            Load();
        }

        /// <summary>
        /// Shows the rewarded ad granting an extra life when completed.
        /// </summary>
        public void ShowForExtraLife(Action onReward)
        {
#if GOOGLE_MOBILE_ADS
            EnsureConfig();
            if (config == null || !config.enableAds || !config.enableRewarded)
            {
                return;
            }

            if (rewardedAd != null && rewardedAd.CanShowAd())
            {
                rewardedAd.Show(reward =>
                {
                    onReward?.Invoke();
                    Load();
                });
            }
            else
            {
                Debug.LogWarning("AdRewardedService: Rewarded ad not ready.");
                onReward?.Invoke();
            }
#else
            Debug.Log("AdRewardedService: Show skipped (GOOGLE_MOBILE_ADS define missing).");
            onReward?.Invoke();
#endif
        }

        private void Load()
        {
#if GOOGLE_MOBILE_ADS
            EnsureConfig();
            if (config == null || !config.enableAds || !config.enableRewarded)
            {
                return;
            }

            RewardedAd.Load(config.rewardedAdUnitId, new AdRequest.Builder().Build(), (ad, error) =>
            {
                if (error != null || ad == null)
                {
                    Debug.LogWarning($"Rewarded load failed: {error}");
                    return;
                }

                rewardedAd = ad;
                rewardedAd.OnAdFullScreenContentClosed += (_, _) => Load();
            });
#else
            Debug.Log("AdRewardedService: Load skipped (GOOGLE_MOBILE_ADS define missing).");
#endif
        }

        private void EnsureConfig()
        {
            if (config == null)
            {
                config = Resources.Load<AdConfig>("AdConfig");
            }
        }
    }
}
