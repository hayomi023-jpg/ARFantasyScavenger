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
    /// UI controller for the Collection Journal - shows discovered items
    /// </summary>
    public class CollectionJournalUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private ItemDatabase itemDatabase;
        [SerializeField] private PlayerProgressManager progressManager;

        [Header("UI Components")]
        [SerializeField] private GameObject journalPanel;
        [SerializeField] private Transform itemGridParent;
        [SerializeField] private GameObject itemSlotPrefab;
        [SerializeField] private TextMeshProUGUI collectionProgressText;
        [SerializeField] private Slider collectionProgressSlider;
        [SerializeField] private TextMeshProUGUI totalCollectedText;

        [Header("Item Detail View")]
        [SerializeField] private GameObject detailPanel;
        [SerializeField] private Image detailIcon;
        [SerializeField] private TextMeshProUGUI detailName;
        [SerializeField] private TextMeshProUGUI detailDescription;
        [SerializeField] private TextMeshProUGUI detailRarity;
        [SerializeField] private TextMeshProUGUI detailTimesCollected;
        [SerializeField] private TextMeshProUGUI detailPoints;
        [SerializeField] private Button closeDetailButton;

        [Header("Filters")]
        [SerializeField] private Button filterAllButton;
        [SerializeField] private Button filterDiscoveredButton;
        [SerializeField] private Button filterUndiscoveredButton;
        [SerializeField] private TMP_Dropdown rarityFilterDropdown;

        [Header("Rarity Colors")]
        [SerializeField] private Color commonColor = Color.gray;
        [SerializeField] private Color uncommonColor = Color.green;
        [SerializeField] private Color rareColor = Color.blue;
        [SerializeField] private Color epicColor = Color.magenta;
        [SerializeField] private Color legendaryColor = Color.yellow;

        private List<GameObject> itemSlots = new List<GameObject>();
        private ItemData selectedItem;
        private FilterType currentFilter = FilterType.All;

        public enum FilterType { All, Discovered, Undiscovered }

        private void Start()
        {
            // Subscribe to events
            if (progressManager != null)
            {
                progressManager.OnItemDiscovered += OnItemDiscovered;
            }

            // Setup buttons
            if (closeDetailButton != null)
            {
                closeDetailButton.onClick.AddListener(HideDetailView);
            }

            if (filterAllButton != null)
            {
                filterAllButton.onClick.AddListener(() => SetFilter(FilterType.All));
            }

            if (filterDiscoveredButton != null)
            {
                filterDiscoveredButton.onClick.AddListener(() => SetFilter(FilterType.Discovered));
            }

            if (filterUndiscoveredButton != null)
            {
                filterUndiscoveredButton.onClick.AddListener(() => SetFilter(FilterType.Undiscovered));
            }

            if (rarityFilterDropdown != null)
            {
                rarityFilterDropdown.onValueChanged.AddListener(OnRarityFilterChanged);
            }

            // Initial refresh
            RefreshJournal();
        }

        private void OnDestroy()
        {
            if (progressManager != null)
            {
                progressManager.OnItemDiscovered -= OnItemDiscovered;
            }
        }

        private void OnItemDiscovered(ItemData item)
        {
            // Could show a notification
            RefreshJournal();
        }

        /// <summary>
        /// Show the collection journal
        /// </summary>
        public void ShowJournal()
        {
            journalPanel.SetActive(true);
            RefreshJournal();
        }

        /// <summary>
        /// Hide the collection journal
        /// </summary>
        public void HideJournal()
        {
            journalPanel.SetActive(false);
            HideDetailView();
        }

        /// <summary>
        /// Refresh the journal display
        /// </summary>
        public void RefreshJournal()
        {
            ClearItemSlots();

            if (itemDatabase == null || progressManager == null) return;

            var allItems = itemDatabase.AllItems.ToList();
            var filteredItems = FilterItems(allItems);

            // Create slots for each item
            foreach (var item in filteredItems)
            {
                CreateItemSlot(item);
            }

            // Update progress
            UpdateProgressDisplay(allItems.Count);
        }

        private List<ItemData> FilterItems(List<ItemData> items)
        {
            var filtered = items;

            // Apply discovered/undiscovered filter
            switch (currentFilter)
            {
                case FilterType.Discovered:
                    filtered = filtered.Where(i => progressManager.IsItemDiscovered(i.itemId)).ToList();
                    break;
                case FilterType.Undiscovered:
                    filtered = filtered.Where(i => !progressManager.IsItemDiscovered(i.itemId)).ToList();
                    break;
            }

            // Apply rarity filter if not "All"
            if (rarityFilterDropdown != null && rarityFilterDropdown.value > 0)
            {
                ItemRarity selectedRarity = (ItemRarity)(rarityFilterDropdown.value - 1);
                filtered = filtered.Where(i => i.rarity == selectedRarity).ToList();
            }

            // Sort by rarity (legendary first), then by discovered status
            return filtered
                .OrderByDescending(i => (int)i.rarity)
                .ThenBy(i => progressManager.IsItemDiscovered(i.itemId) ? 0 : 1)
                .ToList();
        }

        private void CreateItemSlot(ItemData item)
        {
            if (itemSlotPrefab == null || itemGridParent == null) return;

            GameObject slot = Instantiate(itemSlotPrefab, itemGridParent);
            itemSlots.Add(slot);

            bool isDiscovered = progressManager.IsItemDiscovered(item.itemId);
            int collectCount = progressManager.GetItemCollectionCount(item.itemId);

            // Setup slot visuals
            SetupItemSlot(slot, item, isDiscovered, collectCount);

            // Add click handler
            Button button = slot.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.AddListener(() => OnItemClicked(item));

                // Only allow clicking discovered items
                button.interactable = isDiscovered;
            }
        }

        private void SetupItemSlot(GameObject slot, ItemData item, bool isDiscovered, int collectCount)
        {
            // Icon
            Image iconImage = slot.transform.Find("Icon")?.GetComponent<Image>();
            if (iconImage != null)
            {
                if (isDiscovered && item.uiIcon != null)
                {
                    iconImage.sprite = item.uiIcon;
                    iconImage.color = Color.white;
                }
                else
                {
                    // Show question mark or silhouette
                    iconImage.sprite = null;
                    iconImage.color = new Color(0.3f, 0.3f, 0.3f, 0.5f);
                }
            }

            // Rarity border
            Image borderImage = slot.transform.Find("Border")?.GetComponent<Image>();
            if (borderImage != null)
            {
                borderImage.color = GetRarityColor(item.rarity);
            }

            // Discovered indicator
            GameObject discoveredIndicator = slot.transform.Find("DiscoveredIndicator")?.gameObject;
            if (discoveredIndicator != null)
            {
                discoveredIndicator.SetActive(isDiscovered);
            }

            // Count text
            TextMeshProUGUI countText = slot.transform.Find("Count")?.GetComponent<TextMeshProUGUI>();
            if (countText != null)
            {
                countText.text = isDiscovered ? $"x{collectCount}" : "???";
                countText.gameObject.SetActive(isDiscovered && collectCount > 1);
            }

            // Name (only if discovered)
            TextMeshProUGUI nameText = slot.transform.Find("Name")?.GetComponent<TextMeshProUGUI>();
            if (nameText != null)
            {
                nameText.text = isDiscovered ? item.displayName : "???";
            }
        }

        private void OnItemClicked(ItemData item)
        {
            if (!progressManager.IsItemDiscovered(item.itemId)) return;

            selectedItem = item;
            ShowDetailView(item);
        }

        private void ShowDetailView(ItemData item)
        {
            if (detailPanel == null) return;

            detailPanel.SetActive(true);

            // Icon
            if (detailIcon != null)
            {
                detailIcon.sprite = item.uiIcon;
            }

            // Name
            if (detailName != null)
            {
                detailName.text = item.displayName;
                detailName.color = GetRarityColor(item.rarity);
            }

            // Description
            if (detailDescription != null)
            {
                detailDescription.text = item.description;
            }

            // Rarity
            if (detailRarity != null)
            {
                detailRarity.text = $"Rarity: {item.rarity}";
                detailRarity.color = GetRarityColor(item.rarity);
            }

            // Times collected
            if (detailTimesCollected != null)
            {
                int count = progressManager.GetItemCollectionCount(item.itemId);
                detailTimesCollected.text = $"Collected: {count} time{(count != 1 ? "s" : "")}";
            }

            // Points
            if (detailPoints != null)
            {
                detailPoints.text = $"Points: {item.pointValue:N0}";
            }
        }

        private void HideDetailView()
        {
            if (detailPanel != null)
            {
                detailPanel.SetActive(false);
            }
            selectedItem = null;
        }

        private void UpdateProgressDisplay(int totalItems)
        {
            int discovered = progressManager.GetTotalDiscoveredItems();
            float progress = totalItems > 0 ? (float)discovered / totalItems : 0f;

            if (collectionProgressText != null)
            {
                collectionProgressText.text = $"{discovered} / {totalItems}";
            }

            if (collectionProgressSlider != null)
            {
                collectionProgressSlider.value = progress;
            }

            if (totalCollectedText != null)
            {
                totalCollectedText.text = $"Total Collected: {progressManager.TotalItemsCollected:N0}";
            }
        }

        private void ClearItemSlots()
        {
            foreach (var slot in itemSlots)
            {
                if (slot != null)
                {
                    Destroy(slot);
                }
            }
            itemSlots.Clear();
        }

        private void SetFilter(FilterType filter)
        {
            currentFilter = filter;
            RefreshJournal();
        }

        private void OnRarityFilterChanged(int index)
        {
            RefreshJournal();
        }

        private Color GetRarityColor(ItemRarity rarity)
        {
            return rarity switch
            {
                ItemRarity.Common => commonColor,
                ItemRarity.Uncommon => uncommonColor,
                ItemRarity.Rare => rareColor,
                ItemRarity.Epic => epicColor,
                ItemRarity.Legendary => legendaryColor,
                _ => Color.white
            };
        }
    }
}
