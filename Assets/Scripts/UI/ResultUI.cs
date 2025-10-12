using System;
using UnityEngine;
using UnityEngine.UI;

namespace QuizRush.UI
{
    /// <summary>
    /// Displays round results and navigation options.
    /// </summary>
    public class ResultUI : MonoBehaviour
    {
        [SerializeField] private Text scoreText;
        [SerializeField] private Text correctText;
        [SerializeField] private Text streakText;
        [SerializeField] private Button playAgainButton;
        [SerializeField] private Button menuButton;

        public event Action PlayAgainRequested;
        public event Action MenuRequested;

        private void Awake()
        {
            if (scoreText == null)
            {
                BuildDefaultUI();
            }

            playAgainButton?.onClick.AddListener(() => PlayAgainRequested?.Invoke());
            menuButton?.onClick.AddListener(() => MenuRequested?.Invoke());
        }

        /// <summary>
        /// Populates the result labels.
        /// </summary>
        public void ShowResults(int score, int correctAnswers, int bestStreak)
        {
            if (scoreText != null)
            {
                scoreText.text = $"Pontuação: {score}";
            }

            if (correctText != null)
            {
                correctText.text = $"Acertos: {correctAnswers}";
            }

            if (streakText != null)
            {
                streakText.text = $"Maior Streak: {bestStreak}";
            }
        }

        private void BuildDefaultUI()
        {
            var rect = GetComponent<RectTransform>() ?? gameObject.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 0);
            rect.anchorMax = new Vector2(1, 1);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var layout = gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(60, 60, 120, 60);
            layout.spacing = 20f;
            layout.childAlignment = TextAnchor.MiddleCenter;

            scoreText = CreateText("Pontuação: 0", 28);
            correctText = CreateText("Acertos: 0", 24);
            streakText = CreateText("Maior Streak: 0", 24);

            playAgainButton = CreateButton("Jogar de novo");
            menuButton = CreateButton("Menu");
        }

        private Text CreateText(string content, int size)
        {
            var go = new GameObject("Text", typeof(RectTransform));
            go.transform.SetParent(transform, false);
            var text = go.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.text = content;
            text.fontSize = size;
            text.color = Color.white;
            text.alignment = TextAnchor.MiddleCenter;
            return text;
        }

        private Button CreateButton(string label)
        {
            var go = new GameObject(label, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(transform, false);
            var image = go.GetComponent<Image>();
            image.color = new Color(0.3f, 0.3f, 0.8f, 1f);
            var text = new GameObject("Label", typeof(RectTransform));
            text.transform.SetParent(go.transform, false);
            var txt = text.AddComponent<Text>();
            txt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            txt.text = label;
            txt.fontSize = 24;
            txt.color = Color.white;
            txt.alignment = TextAnchor.MiddleCenter;
            var rect = txt.rectTransform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var button = go.GetComponent<Button>();
            button.targetGraphic = image;
            return button;
        }
    }
}
