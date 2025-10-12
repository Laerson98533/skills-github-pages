using System;
using System.Collections;
using System.Collections.Generic;
using QuizRush.Analytics;
using QuizRush.Services;
using QuizRush.UI;
using UnityEngine;

namespace QuizRush
{
    /// <summary>
    /// Core gameplay logic handling question flow, timing and score.
    /// </summary>
    public class QuizController : MonoBehaviour
    {
        [SerializeField] private QuestionRepository repository;
        [SerializeField] private QuizUI quizUI;
        [SerializeField] private AudioManager audioManager;
        [SerializeField] private AnalyticsHook analyticsHook;
        [SerializeField] private float questionDuration = 10f;

        private readonly ScoreSystem scoreSystem = new();
        private readonly HashSet<string> activeCategories = new();
        private Question currentQuestion;
        private float timer;
        private int questionsAsked;
        private int correctAnswers;
        private int bestStreak;
        private bool awaitingAnswer;
        private bool rewardUsedThisRound;
        private bool rewardOffered;

        public event Action<RoundResult> RoundFinished;
        public event Action RewardAdRequested;

        private void Awake()
        {
            repository ??= GetComponent<QuestionRepository>() ?? FindObjectOfType<QuestionRepository>();
            audioManager ??= FindObjectOfType<AudioManager>();
            analyticsHook ??= FindObjectOfType<AnalyticsHook>();
            BindUI(quizUI);
        }

        private void Update()
        {
            if (!awaitingAnswer)
            {
                return;
            }

            timer -= Time.deltaTime;
            quizUI?.UpdateTimer(timer);

            if (timer <= 3f)
            {
                audioManager?.PlayTick();
            }

            if (timer <= 0f)
            {
                HandleTimeout();
            }
        }

        /// <summary>
        /// Starts a new round using the provided category filters.
        /// </summary>
        public void StartRound(IEnumerable<string> categories)
        {
            activeCategories.Clear();
            if (categories != null)
            {
                foreach (var category in categories)
                {
                    activeCategories.Add(category);
                }
            }

            repository.BeginRound();
            scoreSystem.Reset();
            questionsAsked = 0;
            correctAnswers = 0;
            bestStreak = 0;
            rewardUsedThisRound = false;
            rewardOffered = false;
            analyticsHook?.Track("round_start", new Dictionary<string, object>
            {
                { "categories", string.Join(",", activeCategories) }
            });

            NextQuestion();
        }

        /// <summary>
        /// Grants the player an extra life after watching a rewarded ad.
        /// </summary>
        public void GrantExtraLife()
        {
            rewardUsedThisRound = true;
            rewardOffered = false;
            quizUI?.HideRewardPanel();
            NextQuestion();
            analyticsHook?.Track("ad_rewarded_earned");
        }

        private void NextQuestion()
        {
            if (questionsAsked >= 10)
            {
                FinishRound();
                return;
            }

            int difficulty = Mathf.Clamp(1 + questionsAsked / 2, 1, 5);
            currentQuestion = repository.GetNextQuestion(difficulty, activeCategories);
            if (currentQuestion == null)
            {
                FinishRound();
                return;
            }

            questionsAsked++;
            timer = questionDuration;
            awaitingAnswer = true;
            quizUI?.ShowQuestion(currentQuestion, scoreSystem.Score, scoreSystem.Streak);
            quizUI?.UpdateTimer(timer);
        }

        private void OnAnswerSelected(int index)
        {
            if (!awaitingAnswer)
            {
                return;
            }

            awaitingAnswer = false;
            bool isCorrect = index == currentQuestion.CorrectIndex;
            quizUI?.ShowAnswerResult(index, currentQuestion.CorrectIndex);

            if (isCorrect)
            {
                correctAnswers++;
                var points = scoreSystem.RegisterCorrect(timer);
                bestStreak = Mathf.Max(bestStreak, scoreSystem.Streak);
                quizUI?.UpdateScore(scoreSystem.Score, scoreSystem.Streak);
                audioManager?.PlayCorrect();
                StartCoroutine(ProceedAfterDelay());
            }
            else
            {
                scoreSystem.RegisterIncorrect();
                quizUI?.UpdateScore(scoreSystem.Score, scoreSystem.Streak);
                audioManager?.PlayIncorrect();
                OfferRewardOrEnd();
            }
        }

        private void HandleTimeout()
        {
            if (!awaitingAnswer)
            {
                return;
            }

            awaitingAnswer = false;
            quizUI?.ShowAnswerResult(-1, currentQuestion.CorrectIndex);
            scoreSystem.RegisterIncorrect();
            quizUI?.UpdateScore(scoreSystem.Score, scoreSystem.Streak);
            audioManager?.PlayIncorrect();
            OfferRewardOrEnd();
        }

        private void OfferRewardOrEnd()
        {
            if (!rewardUsedThisRound && !rewardOffered)
            {
                rewardOffered = true;
                quizUI?.ShowRewardPanel("Assista 1 anúncio para +1 vida e continuar!");
            }
            else
            {
                FinishRound();
            }
        }

        private void OnRewardRequested()
        {
            if (rewardUsedThisRound || !rewardOffered)
            {
                FinishRound();
                return;
            }

            analyticsHook?.Track("ad_rewarded_shown");
            RewardAdRequested?.Invoke();
        }

        private void OnRewardDeclined()
        {
            if (!rewardOffered)
            {
                return;
            }

            FinishRound();
        }

        private IEnumerator ProceedAfterDelay()
        {
            yield return new WaitForSeconds(1f);
            NextQuestion();
        }

        private void FinishRound()
        {
            awaitingAnswer = false;
            quizUI?.HideRewardPanel();
            analyticsHook?.Track("round_end", new Dictionary<string, object>
            {
                { "score", scoreSystem.Score },
                { "correct", correctAnswers },
                { "streak", bestStreak }
            });

            RoundFinished?.Invoke(new RoundResult
            {
                Score = scoreSystem.Score,
                CorrectAnswers = correctAnswers,
                BestStreak = bestStreak,
                CategoryMix = string.Join(",", activeCategories)
            });
        }

        private void OnDestroy()
        {
            BindUI(null);
        }

        /// <summary>
        /// Binds the runtime UI component, managing event subscriptions safely.
        /// </summary>
        public void BindUI(QuizUI ui)
        {
            if (quizUI != null)
            {
                quizUI.AnswerSelected -= OnAnswerSelected;
                quizUI.RewardRequested -= OnRewardRequested;
                quizUI.RewardDeclined -= OnRewardDeclined;
            }

            quizUI = ui;

            if (quizUI != null)
            {
                quizUI.AnswerSelected += OnAnswerSelected;
                quizUI.RewardRequested += OnRewardRequested;
                quizUI.RewardDeclined += OnRewardDeclined;
            }
        }

        /// <summary>
        /// Round summary information.
        /// </summary>
        public struct RoundResult
        {
            public int Score;
            public int CorrectAnswers;
            public int BestStreak;
            public string CategoryMix;
        }
    }
}
