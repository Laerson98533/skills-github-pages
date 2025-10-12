using System;
using System.Collections.Generic;
using UnityEngine;

namespace QuizRush.Services
{
    /// <summary>
    /// Provides persistent storage for highscores and player settings using PlayerPrefs.
    /// </summary>
    public class PersistenceService : MonoBehaviour
    {
        private const string HighScoresKey = "QuizRush.HighScores";
        private const string BestCategoryPrefix = "QuizRush.Best.";
        private const string SoundKey = "QuizRush.Sound";
        private const string VibrationKey = "QuizRush.Vibration";

        /// <summary>
        /// Loads all stored high score entries.
        /// </summary>
        public List<ScoreEntry> LoadHighScores()
        {
            if (!PlayerPrefs.HasKey(HighScoresKey))
            {
                return new List<ScoreEntry>();
            }

            try
            {
                var json = PlayerPrefs.GetString(HighScoresKey);
                var wrapper = JsonUtility.FromJson<HighScoreWrapper>(json);
                return wrapper?.entries ?? new List<ScoreEntry>();
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Failed to load highscores: {ex.Message}");
                return new List<ScoreEntry>();
            }
        }

        /// <summary>
        /// Attempts to add a high score entry, keeping only the top 10 scores.
        /// </summary>
        public void AddHighScore(ScoreEntry entry)
        {
            var scores = LoadHighScores();
            scores.Add(entry);
            scores.Sort((a, b) => b.score.CompareTo(a.score));
            if (scores.Count > 10)
            {
                scores.RemoveRange(10, scores.Count - 10);
            }

            var wrapper = new HighScoreWrapper { entries = scores };
            var json = JsonUtility.ToJson(wrapper);
            PlayerPrefs.SetString(HighScoresKey, json);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Gets the best score recorded for a specific category.
        /// </summary>
        public int GetBestScoreForCategory(string category)
        {
            return PlayerPrefs.GetInt(BestCategoryPrefix + category, 0);
        }

        /// <summary>
        /// Stores the best score for a category if the new value is higher.
        /// </summary>
        public void UpdateBestScoreForCategory(string category, int score)
        {
            var key = BestCategoryPrefix + category;
            var current = PlayerPrefs.GetInt(key, 0);
            if (score > current)
            {
                PlayerPrefs.SetInt(key, score);
                PlayerPrefs.Save();
            }
        }

        /// <summary>
        /// Returns whether sound is enabled (default true).
        /// </summary>
        public bool IsSoundEnabled()
        {
            return PlayerPrefs.GetInt(SoundKey, 1) == 1;
        }

        /// <summary>
        /// Persists the sound preference flag.
        /// </summary>
        public void SetSoundEnabled(bool enabled)
        {
            PlayerPrefs.SetInt(SoundKey, enabled ? 1 : 0);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Returns whether vibration feedback is enabled (default true).
        /// </summary>
        public bool IsVibrationEnabled()
        {
            return PlayerPrefs.GetInt(VibrationKey, 1) == 1;
        }

        /// <summary>
        /// Stores the vibration preference flag.
        /// </summary>
        public void SetVibrationEnabled(bool enabled)
        {
            PlayerPrefs.SetInt(VibrationKey, enabled ? 1 : 0);
            PlayerPrefs.Save();
        }

        [Serializable]
        private sealed class HighScoreWrapper
        {
            public List<ScoreEntry> entries;
        }
    }

    /// <summary>
    /// Represents a single entry in the local leaderboard.
    /// </summary>
    [Serializable]
    public struct ScoreEntry
    {
        public int score;
        public int correctAnswers;
        public int maxStreak;
        public string categoryMix;
        public long timestamp;
    }
}
