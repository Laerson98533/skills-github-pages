using System;
using System.Collections.Generic;
using System.Linq;
using QuizRush.Services;
using UnityEngine;
using UnityEngine.UI;

namespace QuizRush.UI
{
    /// <summary>
    /// Handles the main menu interactions and displays persistent data such as highscores.
    /// </summary>
    public class MenuUI : MonoBehaviour
    {
        [SerializeField] private Button playButton;
        [SerializeField] private Button storeButton;
        [SerializeField] private Toggle soundToggle;
        [SerializeField] private Toggle vibrationToggle;
        [SerializeField] private Transform categoryContainer;
        [SerializeField] private Toggle categoryTogglePrefab;
        [SerializeField] private Text highScoreText;
        [SerializeField] private Text versionLabel;

        private readonly Dictionary<string, Toggle> categoryToggles = new();

        public event Action PlayRequested;
        public event Action StoreRequested;
        public event Action<bool> SoundToggled;
        public event Action<bool> VibrationToggled;
        public event Action<string, bool> CategoryToggled;

        private void Awake()
        {
            if (playButton == null)
            {
                BuildDefaultUI();
            }

            playButton?.onClick.AddListener(() => PlayRequested?.Invoke());
            storeButton?.onClick.AddListener(() => StoreRequested?.Invoke());
            soundToggle?.onValueChanged.AddListener(value => SoundToggled?.Invoke(value));
            vibrationToggle?.onValueChanged.AddListener(value => VibrationToggled?.Invoke(value));
            if (versionLabel != null)
            {
                versionLabel.text = $"v{Application.version}";
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
            layout.padding = new RectOffset(40, 40, 80, 40);
            layout.spacing = 20f;
            layout.childAlignment = TextAnchor.UpperCenter;

            var title = CreateText("Quiz Rush", 36);
            title.alignment = TextAnchor.MiddleCenter;

            playButton = CreateButton("Jogar");
            storeButton = CreateButton("Loja");

            soundToggle = CreateToggle("Som ligado");
            vibrationToggle = CreateToggle("Vibração");

            var categoriesHeader = CreateText("Categorias", 24);
            categoriesHeader.alignment = TextAnchor.MiddleLeft;

            categoryContainer = new GameObject("CategoriaContainer", typeof(RectTransform), typeof(VerticalLayoutGroup)).transform;
            categoryContainer.SetParent(transform, false);
            var containerLayout = categoryContainer.GetComponent<VerticalLayoutGroup>();
            containerLayout.spacing = 10f;
            containerLayout.childForceExpandHeight = false;

            categoryTogglePrefab = CreateToggle("Categoria");
            categoryTogglePrefab.gameObject.SetActive(false);
            categoryTogglePrefab.transform.SetParent(categoryContainer, false);

            highScoreText = CreateText("Melhores pontuações aparecerão aqui", 18);
            highScoreText.alignment = TextAnchor.UpperLeft;

            versionLabel = CreateText("v0.0.0", 14);
            versionLabel.alignment = TextAnchor.LowerRight;
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
            image.color = new Color(0.15f, 0.35f, 0.7f, 1f);
            var text = CreateChildText(go.transform, label, 24);
            var button = go.GetComponent<Button>();
            button.targetGraphic = image;
            return button;
        }

        private Toggle CreateToggle(string label)
        {
            var go = new GameObject(label, typeof(RectTransform));
            go.transform.SetParent(transform, false);
            var background = new GameObject("Background", typeof(RectTransform), typeof(Image));
            background.transform.SetParent(go.transform, false);
            var checkmark = new GameObject("Checkmark", typeof(RectTransform), typeof(Image));
            checkmark.transform.SetParent(background.transform, false);
            var labelObj = CreateChildText(go.transform, label, 20);
            labelObj.rectTransform.anchorMin = new Vector2(0, 0);
            labelObj.rectTransform.anchorMax = new Vector2(1, 1);
            labelObj.rectTransform.offsetMin = new Vector2(40, 0);
            labelObj.rectTransform.offsetMax = new Vector2(0, 0);

            var toggle = go.AddComponent<Toggle>();
            toggle.graphic = checkmark.GetComponent<Image>();
            toggle.targetGraphic = background.GetComponent<Image>();
            toggle.isOn = true;
            background.GetComponent<Image>().color = new Color(0.2f, 0.2f, 0.2f, 1f);
            checkmark.GetComponent<Image>().color = Color.green;
            checkmark.GetComponent<RectTransform>().sizeDelta = new Vector2(20, 20);
            background.GetComponent<RectTransform>().sizeDelta = new Vector2(30, 30);
            background.GetComponent<RectTransform>().anchorMin = new Vector2(0, 0.5f);
            background.GetComponent<RectTransform>().anchorMax = new Vector2(0, 0.5f);
            background.GetComponent<RectTransform>().anchoredPosition = new Vector2(15, 0);
            var checkmarkRect = checkmark.GetComponent<RectTransform>();
            checkmarkRect.anchorMin = new Vector2(0.5f, 0.5f);
            checkmarkRect.anchorMax = new Vector2(0.5f, 0.5f);
            checkmarkRect.anchoredPosition = Vector2.zero;
            return toggle;
        }

        private Text CreateChildText(Transform parent, string content, int size)
        {
            var go = new GameObject("Label", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var text = go.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.text = content;
            text.fontSize = size;
            text.color = Color.white;
            text.alignment = TextAnchor.MiddleCenter;
            return text;
        }

        /// <summary>
        /// Populates the category toggles based on the available dataset.
        /// </summary>
        public void SetupCategories(IEnumerable<string> categories)
        {
            foreach (Transform child in categoryContainer)
            {
                if (categoryTogglePrefab != null && child.gameObject == categoryTogglePrefab.gameObject)
                {
                    continue;
                }

                Destroy(child.gameObject);
            }

            categoryToggles.Clear();
            foreach (var category in categories.Distinct())
            {
                if (categoryTogglePrefab == null)
                {
                    continue;
                }

                var toggle = Instantiate(categoryTogglePrefab, categoryContainer);
                toggle.isOn = true;
                toggle.gameObject.SetActive(true);
                toggle.GetComponentInChildren<Text>().text = category;
                toggle.onValueChanged.AddListener(value => CategoryToggled?.Invoke(category, value));
                categoryToggles[category] = toggle;
            }
        }

        /// <summary>
        /// Updates the high score list display.
        /// </summary>
        public void ShowHighScores(IEnumerable<ScoreEntry> scores)
        {
            if (highScoreText == null)
            {
                return;
            }

            var lines = scores.Select((entry, index) =>
                $"{index + 1}. {entry.score} pts | {entry.correctAnswers} acertos | x{entry.maxStreak}").ToList();
            highScoreText.text = lines.Count > 0
                ? string.Join("\n", lines)
                : "Sem registos de pontuação";
        }

        /// <summary>
        /// Sets the toggle state when loading saved preferences.
        /// </summary>
        public void ApplySettings(bool soundEnabled, bool vibrationEnabled)
        {
            if (soundToggle != null)
            {
                soundToggle.isOn = soundEnabled;
            }

            if (vibrationToggle != null)
            {
                vibrationToggle.isOn = vibrationEnabled;
            }
        }
    }
}
