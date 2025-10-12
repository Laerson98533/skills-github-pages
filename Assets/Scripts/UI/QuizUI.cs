using System;
using UnityEngine;
using UnityEngine.UI;

namespace QuizRush.UI
{
    /// <summary>
    /// Controls the in-round quiz interface elements.
    /// </summary>
    public class QuizUI : MonoBehaviour
    {
        [SerializeField] private Text timerText;
        [SerializeField] private Text questionText;
        [SerializeField] private Button[] optionButtons = Array.Empty<Button>();
        [SerializeField] private Text scoreText;
        [SerializeField] private Text streakText;
        [SerializeField] private GameObject rewardPanel;
        [SerializeField] private Button rewardButton;
        [SerializeField] private Button declineRewardButton;
        [SerializeField] private Text rewardMessage;

        public event Action<int> AnswerSelected;
        public event Action RewardRequested;
        public event Action RewardDeclined;

        private Color defaultColor;

        private void Awake()
        {
            if (timerText == null)
            {
                BuildDefaultUI();
            }

            for (int i = 0; i < optionButtons.Length; i++)
            {
                var index = i;
                optionButtons[i]?.onClick.AddListener(() => AnswerSelected?.Invoke(index));
                var colors = optionButtons[i].colors;
                defaultColor = colors.normalColor;
            }

            if (rewardButton != null)
            {
                rewardButton.onClick.AddListener(() => RewardRequested?.Invoke());
            }

            if (declineRewardButton != null)
            {
                declineRewardButton.onClick.AddListener(() => RewardDeclined?.Invoke());
            }

            HideRewardPanel();
        }

        private void BuildDefaultUI()
        {
            var rect = GetComponent<RectTransform>() ?? gameObject.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 0);
            rect.anchorMax = new Vector2(1, 1);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var layout = gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(40, 40, 40, 40);
            layout.spacing = 15f;
            layout.childAlignment = TextAnchor.UpperCenter;

            timerText = CreateText("10", 48, TextAnchor.MiddleCenter);
            questionText = CreateText("Pergunta aparece aqui", 28, TextAnchor.MiddleCenter);
            scoreText = CreateText("Pontuação: 0", 20, TextAnchor.MiddleCenter);
            streakText = CreateText("Streak x1", 20, TextAnchor.MiddleCenter);

            optionButtons = new Button[4];
            for (int i = 0; i < 4; i++)
            {
                optionButtons[i] = CreateButton($"Opção {i + 1}");
            }

            rewardPanel = new GameObject("RewardPanel", typeof(RectTransform));
            rewardPanel.transform.SetParent(transform, false);
            var rewardLayout = rewardPanel.AddComponent<VerticalLayoutGroup>();
            rewardLayout.childAlignment = TextAnchor.MiddleCenter;
            rewardLayout.spacing = 10f;
            rewardPanel.AddComponent<Image>().color = new Color(0, 0, 0, 0.5f);

            rewardMessage = CreateText("Assista para continuar", 20, TextAnchor.MiddleCenter);
            rewardMessage.transform.SetParent(rewardPanel.transform, false);
            rewardButton = CreateButtonIntern(rewardPanel.transform, "Assistir anúncio");
            declineRewardButton = CreateButtonIntern(rewardPanel.transform, "Não obrigado");
        }

        private Text CreateText(string content, int size, TextAnchor anchor)
        {
            var go = new GameObject("Text", typeof(RectTransform));
            go.transform.SetParent(transform, false);
            var text = go.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.text = content;
            text.fontSize = size;
            text.color = Color.white;
            text.alignment = anchor;
            return text;
        }

        private Button CreateButton(string label)
        {
            return CreateButtonIntern(transform, label);
        }

        private Button CreateButtonIntern(Transform parent, string label)
        {
            var go = new GameObject(label, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var image = go.GetComponent<Image>();
            image.color = new Color(0.2f, 0.4f, 0.8f, 1f);
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

        /// <summary>
        /// Displays the question data on the UI.
        /// </summary>
        public void ShowQuestion(Question question, int score, int streak)
        {
            if (questionText != null)
            {
                questionText.text = question.Text;
            }

            for (int i = 0; i < optionButtons.Length; i++)
            {
                if (i < question.Options.Length)
                {
                    optionButtons[i].GetComponentInChildren<Text>().text = question.Options[i];
                    optionButtons[i].interactable = true;
                    var colors = optionButtons[i].colors;
                    colors.normalColor = defaultColor;
                    colors.selectedColor = defaultColor;
                    optionButtons[i].colors = colors;
                }
            }

            UpdateScore(score, streak);
            HideRewardPanel();
        }

        /// <summary>
        /// Updates the timer label.
        /// </summary>
        public void UpdateTimer(float seconds)
        {
            if (timerText != null)
            {
                timerText.text = Mathf.CeilToInt(seconds).ToString("00");
            }
        }

        /// <summary>
        /// Updates the score and streak labels.
        /// </summary>
        public void UpdateScore(int score, int streak)
        {
            if (scoreText != null)
            {
                scoreText.text = $"Pontuação: {score}";
            }

            if (streakText != null)
            {
                streakText.text = $"Streak x{Mathf.Max(1, streak)}";
            }
        }

        /// <summary>
        /// Provides visual feedback after an answer is evaluated.
        /// </summary>
        public void ShowAnswerResult(int selectedIndex, int correctIndex)
        {
            for (int i = 0; i < optionButtons.Length; i++)
            {
                var colors = optionButtons[i].colors;
                if (i == correctIndex)
                {
                    colors.normalColor = Color.green;
                    colors.selectedColor = Color.green;
                }
                else if (i == selectedIndex)
                {
                    colors.normalColor = Color.red;
                    colors.selectedColor = Color.red;
                }
                else
                {
                    colors.normalColor = defaultColor;
                    colors.selectedColor = defaultColor;
                }

                optionButtons[i].colors = colors;
                optionButtons[i].interactable = false;
            }
        }

        /// <summary>
        /// Shows the rewarded ad offer panel.
        /// </summary>
        public void ShowRewardPanel(string message)
        {
            if (rewardPanel != null)
            {
                rewardPanel.SetActive(true);
            }

            if (rewardMessage != null)
            {
                rewardMessage.text = message;
            }
        }

        /// <summary>
        /// Hides the rewarded ad offer panel.
        /// </summary>
        public void HideRewardPanel()
        {
            if (rewardPanel != null)
            {
                rewardPanel.SetActive(false);
            }
        }
    }
}
