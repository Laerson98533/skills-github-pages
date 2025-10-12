using System.Collections.Generic;
using System.Linq;
using QuizRush.Ads;
using QuizRush.Analytics;
using QuizRush.Services;
using QuizRush.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace QuizRush
{
    /// <summary>
    /// Central game state machine controlling scene transitions and persistent data.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private QuestionRepository repository;
        [SerializeField] private QuizController quizController;
        [SerializeField] private PersistenceService persistenceService;
        [SerializeField] private AudioManager audioManager;
        [SerializeField] private AnalyticsHook analyticsHook;
        [SerializeField] private AdBannerService bannerService;
        [SerializeField] private AdInterstitialService interstitialService;
        [SerializeField] private AdRewardedService rewardedService;
        [SerializeField] private Transform uiRoot;
        [SerializeField] private MenuUI menuPrefab;
        [SerializeField] private QuizUI quizPrefab;
        [SerializeField] private ResultUI resultPrefab;

        private MenuUI menuUI;
        private QuizUI quizUI;
        private ResultUI resultUI;

        private readonly HashSet<string> selectedCategories = new();
        private bool roundRewardClaimed;
        private Canvas canvas;

        private void Awake()
        {
            repository ??= GetComponent<QuestionRepository>() ?? FindObjectOfType<QuestionRepository>();
            quizController ??= GetComponent<QuizController>() ?? FindObjectOfType<QuizController>();
            persistenceService ??= GetComponent<PersistenceService>() ?? FindObjectOfType<PersistenceService>();
            audioManager ??= FindObjectOfType<AudioManager>();
            analyticsHook ??= FindObjectOfType<AnalyticsHook>();
            bannerService ??= GetComponent<AdBannerService>() ?? FindObjectOfType<AdBannerService>();
            interstitialService ??= GetComponent<AdInterstitialService>() ?? FindObjectOfType<AdInterstitialService>();
            rewardedService ??= GetComponent<AdRewardedService>() ?? FindObjectOfType<AdRewardedService>();

            if (uiRoot == null)
            {
                CreateCanvas();
            }

            EnsurePrefabs();
        }

        private void Start()
        {
            InstantiateUI();
            if (menuUI == null || quizUI == null || resultUI == null)
            {
                Debug.LogError("GameManager: UI instantiation failed.");
                return;
            }

            analyticsHook?.TrackSessionStart();
            ShowMenu();
        }

        private void InstantiateUI()
        {
            if (menuPrefab == null || quizPrefab == null || resultPrefab == null)
            {
                Debug.LogError("GameManager: UI prefabs missing. Ensure assets exist under Resources/Prefabs.");
                return;
            }

            menuUI = Instantiate(menuPrefab, uiRoot);
            quizUI = Instantiate(quizPrefab, uiRoot);
            resultUI = Instantiate(resultPrefab, uiRoot);

            quizUI.gameObject.SetActive(false);
            resultUI.gameObject.SetActive(false);

            quizController.BindUI(quizUI);
            quizController.RoundFinished += OnRoundFinished;
            quizController.RewardAdRequested += OnRewardAdRequested;

            menuUI.PlayRequested += OnPlayRequested;
            menuUI.CategoryToggled += OnCategoryToggled;
            menuUI.SoundToggled += OnSoundToggled;
            menuUI.VibrationToggled += OnVibrationToggled;
            menuUI.StoreRequested += () => Debug.Log("Store coming soon");

            resultUI.PlayAgainRequested += OnPlayRequested;
            resultUI.MenuRequested += ShowMenu;

            var categories = repository.GetCategories();
            if (categories.Count == 0)
            {
                categories = new HashSet<string>(new[] { "Cultura Geral", "Geografia", "Cinema", "Desporto", "Ciência", "Emoji-quiz" });
            }

            foreach (var category in categories)
            {
                selectedCategories.Add(category);
            }

            menuUI.SetupCategories(categories);
            bool soundEnabled = persistenceService == null || persistenceService.IsSoundEnabled();
            bool vibrationEnabled = persistenceService == null || persistenceService.IsVibrationEnabled();
            menuUI.ApplySettings(soundEnabled, vibrationEnabled);
            menuUI.ShowHighScores(persistenceService != null ? persistenceService.LoadHighScores() : new List<ScoreEntry>());

            audioManager?.SetMuted(!soundEnabled);
        }

        private void EnsurePrefabs()
        {
            if (menuPrefab == null)
            {
                menuPrefab = Resources.Load<MenuUI>("Prefabs/MenuUI");
                if (menuPrefab == null)
                {
                    var go = Resources.Load<GameObject>("Prefabs/MenuUI");
                    menuPrefab = go != null ? go.GetComponent<MenuUI>() : null;
                }
            }

            if (quizPrefab == null)
            {
                quizPrefab = Resources.Load<QuizUI>("Prefabs/QuizUI");
                if (quizPrefab == null)
                {
                    var go = Resources.Load<GameObject>("Prefabs/QuizUI");
                    quizPrefab = go != null ? go.GetComponent<QuizUI>() : null;
                }
            }

            if (resultPrefab == null)
            {
                resultPrefab = Resources.Load<ResultUI>("Prefabs/ResultUI");
                if (resultPrefab == null)
                {
                    var go = Resources.Load<GameObject>("Prefabs/ResultUI");
                    resultPrefab = go != null ? go.GetComponent<ResultUI>() : null;
                }
            }
        }

        private void CreateCanvas()
        {
            var canvasGo = new GameObject("UIRoot");
            canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.AddComponent<UnityEngine.UI.CanvasScaler>();
            scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.5f;
            canvasGo.AddComponent<UnityEngine.UI.GraphicRaycaster>();

            if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                var eventSystemGo = new GameObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem), typeof(UnityEngine.EventSystems.StandaloneInputModule));
                DontDestroyOnLoad(eventSystemGo);
            }

            uiRoot = canvasGo.transform;
            DontDestroyOnLoad(canvasGo);
        }

        private void OnDestroy()
        {
            if (quizController != null)
            {
                quizController.RoundFinished -= OnRoundFinished;
                quizController.RewardAdRequested -= OnRewardAdRequested;
            }
        }

        private void ShowMenu()
        {
            menuUI.gameObject.SetActive(true);
            quizUI.gameObject.SetActive(false);
            resultUI.gameObject.SetActive(false);
            bannerService?.Show();
            menuUI.ShowHighScores(persistenceService != null ? persistenceService.LoadHighScores() : new List<ScoreEntry>());
            roundRewardClaimed = false;
        }

        private void OnPlayRequested()
        {
            if (selectedCategories.Count == 0)
            {
                var fallback = repository.GetCategories();
                foreach (var category in fallback)
                {
                    selectedCategories.Add(category);
                }
            }

            bannerService?.Hide();
            menuUI.gameObject.SetActive(false);
            resultUI.gameObject.SetActive(false);
            quizUI.gameObject.SetActive(true);
            roundRewardClaimed = false;
            quizController.StartRound(selectedCategories);
        }

        private void OnRoundFinished(QuizController.RoundResult result)
        {
            quizUI.gameObject.SetActive(false);
            resultUI.gameObject.SetActive(true);
            resultUI.ShowResults(result.Score, result.CorrectAnswers, result.BestStreak);
            if (persistenceService != null)
            {
                persistenceService.AddHighScore(new ScoreEntry
                {
                    score = result.Score,
                    correctAnswers = result.CorrectAnswers,
                    maxStreak = result.BestStreak,
                    categoryMix = result.CategoryMix,
                    timestamp = System.DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                });

                if (!string.IsNullOrEmpty(result.CategoryMix))
                {
                    foreach (var category in result.CategoryMix.Split(',').Where(s => !string.IsNullOrWhiteSpace(s)))
                    {
                        persistenceService.UpdateBestScoreForCategory(category, result.Score);
                    }
                }
                else
                {
                    foreach (var category in selectedCategories)
                    {
                        persistenceService.UpdateBestScoreForCategory(category, result.Score);
                    }
                }
            }

            interstitialService?.NotifyRoundCompleted();
            menuUI.ShowHighScores(persistenceService != null ? persistenceService.LoadHighScores() : new List<ScoreEntry>());
            bannerService?.Hide();
        }

        private void OnCategoryToggled(string category, bool enabled)
        {
            if (enabled)
            {
                selectedCategories.Add(category);
            }
            else
            {
                selectedCategories.Remove(category);
            }
        }

        private void OnSoundToggled(bool enabled)
        {
            persistenceService?.SetSoundEnabled(enabled);
            audioManager?.SetMuted(!enabled);
        }

        private void OnVibrationToggled(bool enabled)
        {
            persistenceService?.SetVibrationEnabled(enabled);
            // Implementation hook for haptics; can be integrated with platform-specific API later.
        }

        private void OnRewardAdRequested()
        {
            if (roundRewardClaimed)
            {
                return;
            }

            roundRewardClaimed = true;
            if (rewardedService != null)
            {
                rewardedService.ShowForExtraLife(() => quizController.GrantExtraLife());
            }
            else
            {
                quizController.GrantExtraLife();
            }
        }
    }
}
