using QuizRush.Ads;
using QuizRush.Analytics;
using QuizRush.Services;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace QuizRush
{
    /// <summary>
    /// Ensures critical runtime systems exist regardless of scene setup.
    /// </summary>
    public static class RuntimeBootstrapper
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void OnSceneLoaded()
        {
            var sceneName = SceneManager.GetActiveScene().name;
            if (sceneName == "Boot")
            {
                CreateBootObjects();
            }
            else if (sceneName == "Main")
            {
                CreateMainSystems();
            }
        }

        private static void CreateBootObjects()
        {
            if (Object.FindObjectOfType<BootLoader>() == null)
            {
                new GameObject("BootLoader").AddComponent<BootLoader>();
            }

            if (Object.FindObjectOfType<AdMobInitializer>() == null)
            {
                var adGo = new GameObject("AdMobInitializer");
                adGo.AddComponent<AdMobInitializer>();
                Object.DontDestroyOnLoad(adGo);
            }
        }

        private static void CreateMainSystems()
        {
            if (Object.FindObjectOfType<AudioManager>() == null)
            {
                var audioGo = new GameObject("AudioManager");
                var music = audioGo.AddComponent<AudioSource>();
                music.playOnAwake = false;
                music.loop = true;
                var sfx = audioGo.AddComponent<AudioSource>();
                sfx.playOnAwake = false;
                audioGo.AddComponent<AudioManager>();
                Object.DontDestroyOnLoad(audioGo);
            }

            if (Object.FindObjectOfType<GameManager>() != null)
            {
                return;
            }

            var systems = new GameObject("GameSystems");
            systems.AddComponent<QuestionRepository>();
            systems.AddComponent<PersistenceService>();
            systems.AddComponent<QuizController>();
            systems.AddComponent<AdBannerService>();
            systems.AddComponent<AdInterstitialService>();
            systems.AddComponent<AdRewardedService>();
            systems.AddComponent<AnalyticsHook>();
            systems.AddComponent<GameManager>();
            Object.DontDestroyOnLoad(systems);
        }
    }
}
