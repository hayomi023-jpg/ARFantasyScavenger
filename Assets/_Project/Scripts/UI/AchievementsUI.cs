using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using ARFantasy.Core;

namespace ARFantasy.UI
{
    /// <summary>
    /// UI controller for displaying and managing achievements
    /// </summary>
    public class AchievementsUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform achievementListParent;
        [SerializeField] private GameObject achievementButtonPrefab;
        [SerializeField] private GameObject achievementPopupPrefab;
        [SerializeField] private Transform popupParent;

        [Header("Categories")]
        [SerializeField] private bool showCollectionAchievements = true;
        [SerializeField] private bool showHuntAchievements = true;
        [SerializeField] private bool showSpecialAchievements = true;

        [Header("Popup Settings")]
        [SerializeField] private float popupDuration = 3f;
        [SerializeField] private Sprite achievementIcon;
        [SerializeField] private Color commonColor = Color.gray;
        [SerializeField] private Color rareColor = Color.blue;
        [SerializeField] private Color epicColor = new Color(0.8f, 0.2f, 0.8f);
        [SerializeField] private Color legendaryColor = new Color(1f, 0.8f, 0.2f);

        private List<GameObject> achievementButtons = new List<GameObject>();
        private Dictionary<string, GameObject> achievementLookup = new Dictionary<string, GameObject>();

        private void Start()
        {
            if (PlayerProgressManager.Instance != null)
            {
                PlayerProgressManager.Instance.OnAchievementUnlocked += OnAchievementUnlocked;
            }

            RefreshAchievements();
        }

        private void OnDestroy()
        {
            if (PlayerProgressManager.Instance != null)
            {
                PlayerProgressManager.Instance.OnAchievementUnlocked -= OnAchievementUnlocked;
            }
        }

        /// <summary>
        /// Refresh the achievement list
        /// </summary>
        public void RefreshAchievements()
        {
            ClearAchievementButtons();
            PopulateAchievements();
        }

        private void ClearAchievementButtons()
        {
            foreach (var button in achievementButtons)
            {
                if (button != null)
                {
                    Destroy(button);
                }
            }
            achievementButtons.Clear();
            achievementLookup.Clear();
        }

        private void PopulateAchievements()
        {
            // Collection achievements
            if (showCollectionAchievements)
            {
                CreateAchievementEntry("first_item", "First Find", "Collect your first item", "collection", commonColor);
                CreateAchievementEntry("collector_10", "Collector", "Collect 10 items total", "collection", commonColor);
                CreateAchievementEntry("collector_25", "Avid Collector", "Collect 25 items total", "collection", rareColor);
                CreateAchievementEntry("collector_50", "Master Collector", "Collect 50 items total", "collection", epicColor);
                CreateAchievementEntry("collector_100", "Legendary Collector", "Collect 100 items total", "collection", legendaryColor);
            }

            // Hunt achievements
            if (showHuntAchievements)
            {
                CreateAchievementEntry("first_hunt", "First Hunt", "Complete your first hunt", "hunt", commonColor);
                CreateAchievementEntry("hunt_5", "Hunting Pro", "Complete 5 hunts", "hunt", rareColor);
                CreateAchievementEntry("hunt_10", "Master Hunter", "Complete 10 hunts", "hunt", epicColor);
                CreateAchievementEntry("perfect_hunt", "Perfectionist", "Complete a hunt with all items", "hunt", rareColor);
            }

            // Rarity achievements
            if (showSpecialAchievements)
            {
                CreateAchievementEntry("rarity_uncommon", "Uncommon Find", "Find an Uncommon item", "rarity", commonColor);
                CreateAchievementEntry("rarity_rare", "Rare Discovery", "Find a Rare item", "rarity", rareColor);
                CreateAchievementEntry("rarity_epic", "Epic Find", "Find an Epic item", "rarity", epicColor);
                CreateAchievementEntry("rarity_legendary", "Legendary Moment", "Find a Legendary item", "rarity", legendaryColor);
                CreateAchievementEntry("special_finder", "Special Hunter", "Find a Special item", "rarity", epicColor);
            }
        }

        private void CreateAchievementEntry(string id, string name, string description, string category, Color color)
        {
            if (achievementButtonPrefab == null || achievementListParent == null) return;

            bool isUnlocked = PlayerProgressManager.Instance?.HasAchievement(id) ?? false;

            GameObject entry = Instantiate(achievementButtonPrefab, achievementListParent);
            achievementButtons.Add(entry);
            achievementLookup[id] = entry;

            // Setup entry visuals
            SetupAchievementEntry(entry, id, name, description, category, color, isUnlocked);
        }

        private void SetupAchievementEntry(GameObject entry, string id, string name, string description, string category, Color color, bool isUnlocked)
        {
            // Name
            TextMeshProUGUI nameText = entry.transform.Find("Name")?.GetComponent<TextMeshProUGUI>();
            if (nameText != null)
            {
                nameText.text = isUnlocked ? name : "???";
                nameText.color = isUnlocked ? Color.white : new Color(0.5f, 0.5f, 0.5f, 0.7f);
            }

            // Description
            TextMeshProUGUI descText = entry.transform.Find("Description")?.GetComponent<TextMeshProUGUI>();
            if (descText != null)
            {
                descText.text = isUnlocked ? description : "Locked";
                descText.color = isUnlocked ? Color.white : new Color(0.5f, 0.5f, 0.5f, 0.5f);
            }

            // Icon/badge color
            Image badge = entry.transform.Find("Badge")?.GetComponent<Image>();
            if (badge != null)
            {
                badge.color = isUnlocked ? color : new Color(0.3f, 0.3f, 0.3f, 0.5f);
            }

            // Checkmark for unlocked
            GameObject checkmark = entry.transform.Find("Checkmark")?.gameObject;
            if (checkmark != null)
            {
                checkmark.SetActive(isUnlocked);
            }

            // Lock icon for locked
            GameObject lockIcon = entry.transform.Find("LockIcon")?.gameObject;
            if (lockIcon != null)
            {
                lockIcon.SetActive(!isUnlocked);
            }
        }

        private void OnAchievementUnlocked(string achievementId)
        {
            ShowAchievementPopup(achievementId);
            RefreshAchievements();
        }

        private void ShowAchievementPopup(string achievementId)
        {
            if (achievementPopupPrefab == null) return;

            // Get achievement info
            string name = GetAchievementName(achievementId);
            Color color = GetAchievementColor(achievementId);

            // Create popup
            GameObject popup = Instantiate(achievementPopupPrefab, popupParent != null ? popupParent : transform);
            popup.transform.SetAsLastSibling();

            // Setup popup content
            TextMeshProUGUI titleText = popup.transform.Find("Title")?.GetComponent<TextMeshProUGUI>();
            if (titleText != null)
            {
                titleText.text = "Achievement Unlocked!";
            }

            TextMeshProUGUI nameText = popup.transform.Find("Name")?.GetComponent<TextMeshProUGUI>();
            if (nameText != null)
            {
                nameText.text = name;
                nameText.color = color;
            }

            TextMeshProUGUI descText = popup.transform.Find("Description")?.GetComponent<TextMeshProUGUI>();
            if (descText != null)
            {
                descText.text = GetAchievementDescription(achievementId);
            }

            // Animate
            StartCoroutine(PopupAnimation(popup));
        }

        private System.Collections.IEnumerator PopupAnimation(GameObject popup)
        {
            CanvasGroup canvasGroup = popup.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = popup.AddComponent<CanvasGroup>();
            }

            // Fade in
            canvasGroup.alpha = 0f;
            float fadeInDuration = 0.3f;
            float elapsed = 0f;

            while (elapsed < fadeInDuration)
            {
                elapsed += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeInDuration);
                yield return null;
            }
            canvasGroup.alpha = 1f;

            // Wait
            yield return new WaitForSeconds(popupDuration);

            // Fade out
            elapsed = 0f;
            while (elapsed < fadeInDuration)
            {
                elapsed += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeInDuration);
                yield return null;
            }

            Destroy(popup);
        }

        private string GetAchievementName(string id)
        {
            return id switch
            {
                "first_item" => "First Find",
                "collector_10" => "Collector",
                "collector_25" => "Avid Collector",
                "collector_50" => "Master Collector",
                "collector_100" => "Legendary Collector",
                "first_hunt" => "First Hunt",
                "hunt_5" => "Hunting Pro",
                "hunt_10" => "Master Hunter",
                "perfect_hunt" => "Perfectionist",
                "rarity_uncommon" => "Uncommon Find",
                "rarity_rare" => "Rare Discovery",
                "rarity_epic" => "Epic Find",
                "rarity_legendary" => "Legendary Moment",
                "special_finder" => "Special Hunter",
                _ => id
            };
        }

        private string GetAchievementDescription(string id)
        {
            return id switch
            {
                "first_item" => "Collect your first item",
                "collector_10" => "Collect 10 items total",
                "collector_25" => "Collect 25 items total",
                "collector_50" => "Collect 50 items total",
                "collector_100" => "Collect 100 items total",
                "first_hunt" => "Complete your first hunt",
                "hunt_5" => "Complete 5 hunts",
                "hunt_10" => "Complete 10 hunts",
                "perfect_hunt" => "Complete a hunt with all items",
                "rarity_uncommon" => "Find an Uncommon item",
                "rarity_rare" => "Find a Rare item",
                "rarity_epic" => "Find an Epic item",
                "rarity_legendary" => "Find a Legendary item",
                "special_finder" => "Find a Special item",
                _ => ""
            };
        }

        private Color GetAchievementColor(string id)
        {
            return id switch
            {
                "collector_50" or "hunt_10" or "rarity_epic" => epicColor,
                "collector_100" or "rarity_legendary" => legendaryColor,
                "collector_25" or "hunt_5" or "rarity_rare" or "perfect_hunt" => rareColor,
                _ => commonColor
            };
        }
    }
}
