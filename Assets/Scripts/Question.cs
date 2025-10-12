using System;
using UnityEngine;

namespace QuizRush
{
    /// <summary>
    /// Represents a trivia question with metadata used to drive quiz progression.
    /// </summary>
    [Serializable]
    public class Question
    {
        [SerializeField] private string category;
        [SerializeField, TextArea] private string text;
        [SerializeField] private string[] options = Array.Empty<string>();
        [SerializeField, Range(0, 3)] private int correctIndex;
        [SerializeField, Range(1, 5)] private int difficulty = 1;

        /// <summary>
        /// Question category such as Cinema or Ciência.
        /// </summary>
        public string Category => category;

        /// <summary>
        /// Localized question text.
        /// </summary>
        public string Text => text;

        /// <summary>
        /// Array of answer options (exactly four entries expected).
        /// </summary>
        public string[] Options => options;

        /// <summary>
        /// Index of the correct answer within <see cref="Options"/>.
        /// </summary>
        public int CorrectIndex => correctIndex;

        /// <summary>
        /// Difficulty scalar between 1 (easy) and 5 (hard).
        /// </summary>
        public int Difficulty => difficulty;

        /// <summary>
        /// Creates a new <see cref="Question"/> instance.
        /// </summary>
        public Question(string category, string text, string[] options, int correctIndex, int difficulty)
        {
            this.category = category;
            this.text = text;
            this.options = options;
            this.correctIndex = correctIndex;
            this.difficulty = difficulty;
        }
    }
}
