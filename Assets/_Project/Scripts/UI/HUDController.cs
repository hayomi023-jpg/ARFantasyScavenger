using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ARFantasy.Core;

namespace ARFantasy.UI
{
    /// <summary>
    /// Controls the in-game HUD elements (score, progress, item counter)
    /// </summary>
    public class HUDController : MonoBehaviour
    {
        [Header("Text Elements")]
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private TextMeshProUGUI itemsText;
        [SerializeField] private TextMeshProUGUI timerText;

        [Header("Progress UI")]
        [SerializeField] private Slider progressSlider;
        [SerializeField] private Image progressFill;
        [SerializeField] private Color progressStartColor = Color.red;
        [SerializeField] private Color progressEndColor = Color.green;

        [Header("Collection Feedback")]
        [SerializeField] private TextMeshProUGUI collectionPopupText;
        [SerializeField] private Animator collectionPopupAnimator;
        [SerializeField] private float popupDuration = 1.5f;

        [Header("Optional")]
        [SerializeField] private bool showTimer = false;
        [SerializeField] private bool useCollectionAnimation = true;

        private void Start()
        {
            // Subscribe to game events
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnScoreChanged += OnScoreChanged;
                GameManager.Instance.OnItemCollected += OnItemCollected;
                GameManager.Instance.OnTimeTick += OnTimeTick;
                GameManager.Instance.OnTimeExpired += OnTimeExpired;
            }

            // Hide collection popup initially
            if (collectionPopupText != null)
            {
                collectionPopupText.gameObject.SetActive(false);
            }

            // Initialize HUD
            UpdateHUD();
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnScoreChanged -= OnScoreChanged;
                GameManager.Instance.OnItemCollected -= OnItemCollected;
                GameManager.Instance.OnTimeTick -= OnTimeTick;
                GameManager.Instance.OnTimeExpired -= OnTimeExpired;
            }
        }

        private void OnTimeTick(int secondsRemaining)
        {
            UpdateTimer(secondsRemaining);
        }

        private void OnTimeExpired()
        {
            ShowCollectionPopup("Time's Up!");
        }

        private void Update()
        {
            // Timer is now handled via OnTimeTick event
        }

        private void UpdateTimer(int seconds)
        {
            if (timerText == null || !showTimer) return;

            int minutes = seconds / 60;
            int secs = seconds % 60;
            timerText.text = string.Format("{0:00}:{1:00}", minutes, secs);

            // Change color when time is low
            if (seconds <= 10)
            {
                timerText.color = Color.red;
            }
            else if (seconds <= 30)
            {
                timerText.color = Color.yellow;
            }
            else
            {
                timerText.color = Color.white;
            }
        }

        /// <summary>
        /// Updates all HUD elements to current game state
        /// </summary>
        public void UpdateHUD()
        {
            if (GameManager.Instance == null) return;

            UpdateScore(GameManager.Instance.CurrentScore);
            UpdateItemsCollected(GameManager.Instance.ItemsCollected, GameManager.Instance.TotalItemsToCollect);
            UpdateProgress(GameManager.Instance.ItemsCollected, GameManager.Instance.TotalItemsToCollect);

            // Show/hide timer based on whether hunt has time limit
            bool hasTimeLimit = GameManager.Instance.HasTimeLimit;
            if (timerText != null)
            {
                timerText.gameObject.SetActive(hasTimeLimit && showTimer);
            }
        }

        private void OnScoreChanged(int newScore)
        {
            UpdateScore(newScore);
        }

        private void OnItemCollected(int collected, int total)
        {
            UpdateItemsCollected(collected, total);
            UpdateProgress(collected, total);

            // Show collection popup
            if (useCollectionAnimation)
            {
                ShowCollectionPopup($"Item Found! +{GameManager.Instance.CurrentScore}");
            }
        }

        private void UpdateScore(int score)
        {
            if (scoreText != null)
            {
                scoreText.text = $"Score: {score:N0}";
            }
        }

        private void UpdateItemsCollected(int collected, int total)
        {
            if (itemsText != null)
            {
                itemsText.text = $"Items: {collected}/{total}";
            }
        }

        private void UpdateProgress(int collected, int total)
        {
            if (progressSlider != null)
            {
                float progress = total > 0 ? (float)collected / total : 0f;
                progressSlider.value = progress;

                // Update color based on progress
                if (progressFill != null)
                {
                    progressFill.color = Color.Lerp(progressStartColor, progressEndColor, progress);
                }
            }
        }

        private void ShowCollectionPopup(string message)
        {
            if (collectionPopupText == null) return;

            collectionPopupText.text = message;
            collectionPopupText.gameObject.SetActive(true);

            if (collectionPopupAnimator != null)
            {
                collectionPopupAnimator.SetTrigger("Show");
            }

            // Auto-hide after duration
            CancelInvoke(nameof(HideCollectionPopup));
            Invoke(nameof(HideCollectionPopup), popupDuration);
        }

        private void HideCollectionPopup()
        {
            if (collectionPopupText != null)
            {
                collectionPopupText.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Show a custom message popup
        /// </summary>
        public void ShowMessage(string message, float duration = 2f)
        {
            ShowCollectionPopup(message);
        }
    }
}
