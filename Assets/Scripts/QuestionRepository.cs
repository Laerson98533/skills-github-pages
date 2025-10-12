using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace QuizRush
{
    /// <summary>
    /// Provides access to the locally stored quiz questions, supports filtering by difficulty and
    /// ensures questions are not repeated within the same round until the pool is exhausted.
    /// </summary>
    public class QuestionRepository : MonoBehaviour
    {
        [SerializeField] private TextAsset jsonDataset;
        [SerializeField] private List<QuestionPack> additionalPacks = new();

        private readonly Dictionary<int, List<Question>> baseByDifficulty = new();
        private readonly Dictionary<int, Queue<Question>> roundQueues = new();
        private System.Random random;
        private readonly HashSet<string> categories = new();

        private void Awake()
        {
            random = new System.Random(DateTime.UtcNow.GetHashCode());
            LoadQuestions();
        }

        /// <summary>
        /// Loads and groups all questions, can be called again to refresh the repository.
        /// </summary>
        public void LoadQuestions()
        {
            baseByDifficulty.Clear();
            roundQueues.Clear();
            categories.Clear();

            jsonDataset ??= Resources.Load<TextAsset>("questions");

            var allQuestions = new List<Question>();
            if (jsonDataset != null)
            {
                try
                {
                    var wrapper = JsonUtility.FromJson<QuestionListWrapper>(jsonDataset.text);
                    if (wrapper?.questions != null)
                    {
                        allQuestions.AddRange(wrapper.questions);
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"Failed to parse questions dataset: {ex.Message}");
                }
            }

            foreach (var pack in additionalPacks.Where(p => p != null))
            {
                allQuestions.AddRange(pack.Questions);
            }

            if (allQuestions.Count == 0)
            {
                Debug.LogError("QuestionRepository: No questions available. Please supply a dataset.");
            }

            foreach (var group in allQuestions.GroupBy(q => Mathf.Clamp(q.Difficulty, 1, 5)))
            {
                baseByDifficulty[group.Key] = group.ToList();
                foreach (var question in group)
                {
                    if (!string.IsNullOrWhiteSpace(question.Category))
                    {
                        categories.Add(question.Category);
                    }
                }
            }
        }

        /// <summary>
        /// Returns all unique categories present in the dataset.
        /// </summary>
        public IReadOnlyCollection<string> GetCategories()
        {
            return categories;
        }

        /// <summary>
        /// Resets the per-round state so questions can be drawn without repetition for a new game.
        /// </summary>
        public void BeginRound()
        {
            roundQueues.Clear();
            foreach (var kvp in baseByDifficulty)
            {
                var shuffled = new List<Question>(kvp.Value);
                Shuffle(shuffled);
                roundQueues[kvp.Key] = new Queue<Question>(shuffled);
            }
        }

        /// <summary>
        /// Retrieves the next question for the requested difficulty band. The method will fall back
        /// to easier questions if the target difficulty pool is empty and harder questions otherwise.
        /// If all questions are exhausted the pools are automatically refreshed.
        /// </summary>
        public Question GetNextQuestion(int targetDifficulty, IReadOnlyCollection<string> allowedCategories = null)
        {
            targetDifficulty = Mathf.Clamp(targetDifficulty, 1, 5);

            if (roundQueues.Count == 0)
            {
                BeginRound();
            }

            Question question = TryDequeue(targetDifficulty, allowedCategories);
            if (question != null)
            {
                return question;
            }

            for (int diff = targetDifficulty - 1; diff >= 1; diff--)
            {
                question = TryDequeue(diff, allowedCategories);
                if (question != null)
                {
                    return question;
                }
            }

            for (int diff = targetDifficulty + 1; diff <= 5; diff++)
            {
                question = TryDequeue(diff, allowedCategories);
                if (question != null)
                {
                    return question;
                }
            }

            // All questions consumed - restart pools and try again.
            BeginRound();
            return TryDequeue(targetDifficulty, allowedCategories);
        }

        private Question TryDequeue(int difficulty, IReadOnlyCollection<string> allowedCategories)
        {
            if (!roundQueues.TryGetValue(difficulty, out var queue))
            {
                return null;
            }

            if (queue.Count == 0)
            {
                return null;
            }

            if (allowedCategories == null || allowedCategories.Count == 0)
            {
                return queue.Dequeue();
            }

            // Pull until we find a matching category, re-enqueue unmatched items.
            int attempts = queue.Count;
            while (attempts-- > 0)
            {
                var question = queue.Dequeue();
                if (allowedCategories.Contains(question.Category))
                {
                    return question;
                }

                queue.Enqueue(question);
            }

            return null;
        }

        private void Shuffle<T>(IList<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }

        [Serializable]
        private sealed class QuestionListWrapper
        {
            public List<Question> questions;
        }
    }
}
