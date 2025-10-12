using UnityEngine;

#if GOOGLE_MOBILE_ADS
using GoogleMobileAds.Api;
#endif

namespace QuizRush.Ads
{
    /// <summary>
    /// Initializes the Google Mobile Ads SDK on boot.
    /// </summary>
    public class AdMobInitializer : MonoBehaviour
    {
        [SerializeField] private AdConfig config;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            EnsureConfig();
#if GOOGLE_MOBILE_ADS
            if (config == null || !config.enableAds)
            {
                Debug.Log("AdMobInitializer: Ads disabled via config.");
                return;
            }

            MobileAds.Initialize(initStatus =>
            {
                Debug.Log("AdMob initialized: " + initStatus);
            });
#else
            Debug.Log("AdMobInitializer: GOOGLE_MOBILE_ADS define missing. Initialize skipped.");
#endif
        }

        private void EnsureConfig()
        {
            if (config != null)
            {
                return;
            }

            config = Resources.Load<AdConfig>("AdConfig");
        }
    }
}
