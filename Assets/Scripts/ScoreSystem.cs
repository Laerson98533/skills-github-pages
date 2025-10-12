using UnityEngine;

namespace QuizRush
{
    /// <summary>
    /// Handles streak, multiplier and score calculation for each quiz round.
    /// </summary>
    public class ScoreSystem
    {
        private const int BasePoints = 100;
        private const float StreakIncrement = 0.2f;

        /// <summary>
        /// Current score of the running round.
        /// </summary>
        public int Score { get; private set; }

        /// <summary>
        /// Number of consecutive correct answers.
        /// </summary>
        public int Streak { get; private set; }

        /// <summary>
        /// Current multiplier applied to base points.
        /// </summary>
        public float Multiplier { get; private set; } = 1f;

        /// <summary>
        /// Resets the score system to its base state for a new round.
        /// </summary>
        public void Reset()
        {
            Score = 0;
            Streak = 0;
            Multiplier = 1f;
        }

        /// <summary>
        /// Registers a correct answer, updating streak and multiplier and returning the awarded points.
        /// </summary>
        /// <param name="timeRemaining">Seconds left on the timer, used for a small time bonus.</param>
        public int RegisterCorrect(float timeRemaining)
        {
            Streak++;
            Multiplier = 1f + (Streak - 1) * StreakIncrement;
            var points = Mathf.RoundToInt(BasePoints * Multiplier + Mathf.Max(0f, timeRemaining));
            Score += points;
            return points;
        }

        /// <summary>
        /// Registers an incorrect answer, resetting the streak and multiplier.
        /// </summary>
        public void RegisterIncorrect()
        {
            Streak = 0;
            Multiplier = 1f;
        }
    }
}
