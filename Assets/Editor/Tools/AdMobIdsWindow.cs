#if UNITY_EDITOR
using QuizRush.Ads;
using UnityEditor;
using UnityEngine;

namespace QuizRush.EditorTools
{
    /// <summary>
    /// Simple editor window to author AdMob ids inside the Unity editor.
    /// </summary>
    public class AdMobIdsWindow : EditorWindow
    {
        private AdConfig config;

        [MenuItem("Tools/Quiz Rush/AdMob Ids")]
        public static void ShowWindow()
        {
            var window = GetWindow<AdMobIdsWindow>();
            window.titleContent = new GUIContent("AdMob IDs");
            window.Show();
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("AdMob Configuration", EditorStyles.boldLabel);
            config = (AdConfig)EditorGUILayout.ObjectField("Config", config, typeof(AdConfig), false);

            if (config == null)
            {
                EditorGUILayout.HelpBox("Assign an AdConfig asset to edit.", MessageType.Info);
                return;
            }

            EditorGUI.BeginChangeCheck();
            config.enableAds = EditorGUILayout.Toggle("Enable Ads", config.enableAds);
            config.enableBanner = EditorGUILayout.Toggle("Enable Banner", config.enableBanner);
            config.enableInterstitial = EditorGUILayout.Toggle("Enable Interstitial", config.enableInterstitial);
            config.enableRewarded = EditorGUILayout.Toggle("Enable Rewarded", config.enableRewarded);

            EditorGUILayout.Space();
            config.bannerAdUnitId = EditorGUILayout.TextField("Banner ID", config.bannerAdUnitId);
            config.interstitialAdUnitId = EditorGUILayout.TextField("Interstitial ID", config.interstitialAdUnitId);
            config.rewardedAdUnitId = EditorGUILayout.TextField("Rewarded ID", config.rewardedAdUnitId);

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(config);
                AssetDatabase.SaveAssets();
            }
        }
    }
}
#endif
