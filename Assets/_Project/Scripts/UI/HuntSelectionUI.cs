using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using ARFantasy.Core;
using ARFantasy.Data;

namespace ARFantasy.UI
{
    /// <summary>
    /// UI controller for hunt selection screen - choose which hunt to play
    /// </summary>
    public class HuntSelectionUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private List<HuntConfig> availableHunts = new List<HuntConfig>();
        [SerializeField] private PlayerProgressManager progressManager;
        [SerializeField] private HuntManager huntManager;

        [Header("UI Components")]
        [SerializeField] private GameObject selectionPanel;
        [SerializeField] private Transform huntListParent;
        [SerializeField] private GameObject huntButtonPrefab;
        [SerializeField] private TextMeshProUGUI headerText;

        [Header("Hunt Preview")]
        [SerializeField] private GameObject previewPanel;
        [SerializeField] private TextMeshProUGUI previewName;
        [SerializeField] private TextMeshProUGUI previewDescription;
        [SerializeField] private TextMeshProUGUI previewDifficulty;
        [SerializeField] private TextMeshProUGUI previewItemCount;
        [SerializeField] private TextMeshProUGUI previewTimeLimit;
        [SerializeField] private TextMeshProUGUI previewHighScore;
        [SerializeField] private Button startHuntButton;
        [SerializeField] private Button backButton;

        [Header("Lock Overlay")]
        [SerializeField] private GameObject lockedOverlay;
        [SerializeField] private TextMeshProUGUI lockedRequirementText;

        [Header("Difficulty Colors")]
        [SerializeField] private Color easyColor = Color.green;
        [SerializeField] private Color mediumColor = Color.yellow;
        [SerializeField] private Color hardColor = Color.red;

        private List<GameObject> huntButtons = new List<GameObject>();
        private HuntConfig selectedHunt;
        private bool isPreviewLocked = false;

        private void Start()
        {
            // Setup buttons
            if (startHuntButton != null)
            {
                startHuntButton.onClick.AddListener(OnStartHuntClicked);
            }

            if (backButton != null)
            {
                backButton.onClick.AddListener(HidePreview);
            }

            // Initial refresh
            RefreshHuntList();
        }

        /// <summary>
        /// Show the hunt selection screen
        /// </summary>
        public void ShowSelection()
        {
            selectionPanel.SetActive(true);
            previewPanel.SetActive(false);
            RefreshHuntList();
        }

        /// <summary>
        /// Hide the hunt selection screen
        /// </summary>
        public void HideSelection()
        {
            selectionPanel.SetActive(false);
            previewPanel.SetActive(false);
        }

        /// <summary>
        /// Refresh the hunt list based on unlock status
        /// </summary>
        public void RefreshHuntList()
        {
            ClearHuntButtons();

            foreach (var hunt in availableHunts)
            {
                if (hunt == null) continue;

                bool isUnlocked = progressManager?.IsHuntUnlocked(hunt.huntId) ?? true;
                bool isCompleted = progressManager?.IsHuntCompleted(hunt.huntId) ?? false;

                CreateHuntButton(hunt, isUnlocked, isCompleted);
            }
        }

        private void CreateHuntButton(HuntConfig hunt, bool isUnlocked, bool isCompleted)
        {
            if (huntButtonPrefab == null || huntListParent == null) return;

            GameObject button = Instantiate(huntButtonPrefab, huntListParent);
            huntButtons.Add(button);

            // Setup button visuals
            SetupHuntButton(button, hunt, isUnlocked, isCompleted);

            // Click handler
            Button btn = button.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.AddListener(() => OnHuntSelected(hunt, !isUnlocked));
            }
        }

        private void SetupHuntButton(GameObject button, HuntConfig hunt, bool isUnlocked, bool isCompleted)
        {
            // Name
            TextMeshProUGUI nameText = button.transform.Find("Name")?.GetComponent<TextMeshProUGUI>();
            if (nameText != null)
            {
                nameText.text = isUnlocked ? hunt.huntName : "???";
                nameText.color = isUnlocked ? Color.white : new Color(0.5f, 0.5f, 0.5f, 0.7f);
            }

            // Description (brief)
            TextMeshProUGUI descText = button.transform.Find("Description")?.GetComponent<TextMeshProUGUI>();
            if (descText != null)
            {
                descText.text = isUnlocked ? hunt.description : "Complete previous hunts to unlock";
            }

            // Difficulty badge
            Image difficultyBadge = button.transform.Find("DifficultyBadge")?.GetComponent<Image>();
            TextMeshProUGUI difficultyText = button.transform.Find("DifficultyBadge/Difficulty")?.GetComponent<TextMeshProUGUI>();
            if (difficultyBadge != null && difficultyText != null && isUnlocked)
            {
                string difficulty = CalculateDifficulty(hunt);
                difficultyText.text = difficulty;
                difficultyBadge.color = GetDifficultyColor(difficulty);
            }

            // Item count
            TextMeshProUGUI countText = button.transform.Find("ItemCount")?.GetComponent<TextMeshProUGUI>();
            if (countText != null && isUnlocked)
            {
                countText.text = $"{hunt.itemCount} Items";
            }

            // Completed badge
            GameObject completedBadge = button.transform.Find("CompletedBadge")?.gameObject;
            if (completedBadge != null)
            {
                completedBadge.SetActive(isCompleted);
            }

            // Lock icon
            GameObject lockIcon = button.transform.Find("LockIcon")?.gameObject;
            if (lockIcon != null)
            {
                lockIcon.SetActive(!isUnlocked);
            }

            // High score
            if (isUnlocked && isCompleted)
            {
                var highScore = progressManager?.GetHuntHighScore(hunt.huntId);
                TextMeshProUGUI scoreText = button.transform.Find("HighScore")?.GetComponent<TextMeshProUGUI>();
                if (scoreText != null && highScore != null)
                {
                    scoreText.text = $"Best: {highScore.bestScore:N0}";
                }
            }

            // Button interactability
            Button btn = button.GetComponent<Button>();
            if (btn != null)
            {
                btn.interactable = isUnlocked;
            }
        }

        private void OnHuntSelected(HuntConfig hunt, bool isLocked)
        {
            selectedHunt = hunt;
            isPreviewLocked = isLocked;
            ShowPreview(hunt, isLocked);
        }

        private void ShowPreview(HuntConfig hunt, bool isLocked)
        {
            previewPanel.SetActive(true);

            // Lock overlay
            if (lockedOverlay != null)
            {
                lockedOverlay.SetActive(isLocked);
            }

            // Start button
            if (startHuntButton != null)
            {
                startHuntButton.interactable = !isLocked;
                startHuntButton.GetComponentInChildren<TextMeshProUGUI>().text =
                    isLocked ? "Locked" : "Start Hunt";
            }

            // Lock requirement text
            if (lockedRequirementText != null && isLocked)
            {
                string requiredHunt = hunt.unlocksHunt != null ? hunt.unlocksHunt.huntName : "Previous hunt";
                lockedRequirementText.text = $"Complete \"{requiredHunt}\" to unlock";
            }

            // Hunt info
            if (previewName != null)
            {
                previewName.text = hunt.huntName;
            }

            if (previewDescription != null)
            {
                previewDescription.text = hunt.description;
            }

            if (previewDifficulty != null)
            {
                string difficulty = CalculateDifficulty(hunt);
                previewDifficulty.text = $"Difficulty: {difficulty}";
                previewDifficulty.color = GetDifficultyColor(difficulty);
            }

            if (previewItemCount != null)
            {
                previewItemCount.text = $"Items to find: {hunt.itemCount}";
            }

            if (previewTimeLimit != null)
            {
                if (hunt.timeLimit > 0)
                {
                    previewTimeLimit.text = $"Time limit: {hunt.timeLimit:N0}s";
                    previewTimeLimit.gameObject.SetActive(true);
                }
                else
                {
                    previewTimeLimit.gameObject.SetActive(false);
                }
            }

            // High score
            if (previewHighScore != null && !isLocked)
            {
                var highScore = progressManager?.GetHuntHighScore(hunt.huntId);
                if (highScore != null && highScore.attempts > 0)
                {
                    previewHighScore.text = $"Personal Best: {highScore.bestScore:N0}\n" +
                                            $"Best Time: {highScore.bestTime:N1}s\n" +
                                            $"Attempts: {highScore.attempts}";
                }
                else
                {
                    previewHighScore.text = "No attempts yet";
                }
            }
        }

        private void HidePreview()
        {
            previewPanel.SetActive(false);
            selectedHunt = null;
        }

        private void OnStartHuntClicked()
        {
            if (selectedHunt == null || isPreviewLocked) return;

            // Start the hunt
            AudioManager.Instance?.PlayUIClickSound();

            // Configure hunt with selected hunt settings
            if (selectedHunt != null)
            {
                GameManager.Instance?.ConfigureHunt(selectedHunt.itemCount, selectedHunt.timeLimit);
                huntManager?.SetHuntConfig(selectedHunt);
            }

            // Start the hunt
            GameManager.Instance?.StartNewHunt();

            HideSelection();
        }

        private void ClearHuntButtons()
        {
            foreach (var button in huntButtons)
            {
                if (button != null)
                {
                    Destroy(button);
                }
            }
            huntButtons.Clear();
        }

        private string CalculateDifficulty(HuntConfig hunt)
        {
            float difficultyScore = hunt.itemCount * 10f;

            if (hunt.timeLimit > 0)
            {
                difficultyScore += (100f - hunt.timeLimit) / 10f;
            }

            if (difficultyScore < 40) return "Easy";
            if (difficultyScore < 70) return "Medium";
            return "Hard";
        }

        private Color GetDifficultyColor(string difficulty)
        {
            return difficulty switch
            {
                "Easy" => easyColor,
                "Medium" => mediumColor,
                "Hard" => hardColor,
                _ => Color.white
            };
        }

        /// <summary>
        /// Unlock a hunt programmatically
        /// </summary>
        public void UnlockHunt(string huntId)
        {
            progressManager?.UnlockHunt(huntId);
            RefreshHuntList();
        }
    }
}
