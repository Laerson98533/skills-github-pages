using UnityEngine;

namespace QuizRush.Ads
{
    /// <summary>
    /// Holds editor configurable toggles for enabling/disabling AdMob placements while testing.
    /// </summary>
    [CreateAssetMenu(fileName = "AdConfig", menuName = "QuizRush/Ad Config")]
    public class AdConfig : ScriptableObject
    {
        [Header("Runtime Toggles")]
        public bool enableAds = true;
        public bool enableBanner = true;
        public bool enableInterstitial = true;
        public bool enableRewarded = true;

        [Header("Ad Unit Ids")]
        public string bannerAdUnitId = "ca-app-pub-xxx/banner";
        public string interstitialAdUnitId = "ca-app-pub-xxx/interstitial";
        public string rewardedAdUnitId = "ca-app-pub-xxx/rewarded";
    }
}
