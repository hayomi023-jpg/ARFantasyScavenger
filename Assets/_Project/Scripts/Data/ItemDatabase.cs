using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace ARFantasy.Data
{
    /// <summary>
    /// Central database for all collectible items in the game
    /// </summary>
    [CreateAssetMenu(fileName = "ItemDatabase", menuName = "AR Fantasy/Item Database")]
    public class ItemDatabase : ScriptableObject
    {
        [Header("All Items")]
        [SerializeField] private List<ItemData> allItems = new List<ItemData>();

        [Header("Rarity Weights")]
        [Tooltip("Spawn weight for each rarity (higher = more common)")]
        [SerializeField] private int commonWeight = 50;
        [SerializeField] private int uncommonWeight = 30;
        [SerializeField] private int rareWeight = 15;
        [SerializeField] private int epicWeight = 4;
        [SerializeField] private int legendaryWeight = 1;

        [Header("Category Filters")]
        [SerializeField] private List<ItemCategory> categories = new List<ItemCategory>();

        // Runtime lookup cache
        private Dictionary<string, ItemData> idLookup;
        private Dictionary<ItemRarity, List<ItemData>> rarityLookup;
        private Dictionary<ItemCategory, List<ItemData>> categoryLookup;

        private void OnEnable()
        {
            BuildCache();
        }

        private void BuildCache()
        {
            idLookup = new Dictionary<string, ItemData>();
            rarityLookup = new Dictionary<ItemRarity, List<ItemData>>();
            categoryLookup = new Dictionary<ItemCategory, List<ItemData>>();

            foreach (var item in allItems)
            {
                if (item == null) continue;

                // ID lookup
                if (!string.IsNullOrEmpty(item.itemId))
                {
                    idLookup[item.itemId] = item;
                }

                // Rarity lookup
                if (!rarityLookup.ContainsKey(item.rarity))
                {
                    rarityLookup[item.rarity] = new List<ItemData>();
                }
                rarityLookup[item.rarity].Add(item);

                // Category lookup (based on item name or custom field)
                // This is simplified - you could add a category field to ItemData
            }
        }

        /// <summary>
        /// Get item by ID
        /// </summary>
        public ItemData GetItemById(string id)
        {
            if (idLookup == null) BuildCache();
            return idLookup.TryGetValue(id, out var item) ? item : null;
        }

        /// <summary>
        /// Get all items of a specific rarity
        /// </summary>
        public List<ItemData> GetItemsByRarity(ItemRarity rarity)
        {
            if (rarityLookup == null) BuildCache();
            return rarityLookup.TryGetValue(rarity, out var items) ? items : new List<ItemData>();
        }

        /// <summary>
        /// Get random item based on rarity weights
        /// </summary>
        public ItemData GetRandomWeightedItem()
        {
            if (rarityLookup == null) BuildCache();

            int totalWeight = 0;
            var weightedItems = new List<(ItemData item, int weight)>();

            foreach (var rarity in rarityLookup.Keys)
            {
                int weight = GetWeightForRarity(rarity);
                foreach (var item in rarityLookup[rarity])
                {
                    weightedItems.Add((item, weight));
                    totalWeight += weight;
                }
            }

            if (totalWeight == 0) return null;

            int randomValue = Random.Range(0, totalWeight);
            int currentWeight = 0;

            foreach (var (item, weight) in weightedItems)
            {
                currentWeight += weight;
                if (randomValue < currentWeight)
                {
                    return item;
                }
            }

            return weightedItems.Count > 0 ? weightedItems[0].item : null;
        }

        /// <summary>
        /// Get random item from specific rarities only
        /// </summary>
        public ItemData GetRandomFromRarities(ItemRarity[] allowedRarities)
        {
            if (rarityLookup == null) BuildCache();

            var availableItems = new List<(ItemData item, int weight)>();
            int totalWeight = 0;

            foreach (var rarity in allowedRarities)
            {
                if (!rarityLookup.ContainsKey(rarity)) continue;

                int weight = GetWeightForRarity(rarity);
                foreach (var item in rarityLookup[rarity])
                {
                    availableItems.Add((item, weight));
                    totalWeight += weight;
                }
            }

            if (totalWeight == 0) return null;

            int randomValue = Random.Range(0, totalWeight);
            int currentWeight = 0;

            foreach (var (item, weight) in availableItems)
            {
                currentWeight += weight;
                if (randomValue < currentWeight)
                {
                    return item;
                }
            }

            return availableItems[0].item;
        }

        /// <summary>
        /// Get multiple random items for a hunt
        /// </summary>
        public List<ItemData> GenerateHuntItems(int count, ItemRarity[] allowedRarities = null)
        {
            var items = new List<ItemData>();

            for (int i = 0; i < count; i++)
            {
                ItemData item = allowedRarities != null
                    ? GetRandomFromRarities(allowedRarities)
                    : GetRandomWeightedItem();

                if (item != null)
                {
                    items.Add(item);
                }
            }

            return items;
        }

        /// <summary>
        /// Ensure at least one rare+ item in the selection
        /// </summary>
        public List<ItemData> GenerateHuntItemsWithGuaranteedRare(int count)
        {
            var items = new List<ItemData>();

            // Guarantee at least one rare+ if count > 2
            if (count > 2)
            {
                var rarePlus = new[] { ItemRarity.Rare, ItemRarity.Epic, ItemRarity.Legendary };
                items.Add(GetRandomFromRarities(rarePlus));
                count--;
            }

            // Fill rest with normal weighted selection
            items.AddRange(GenerateHuntItems(count));

            // Shuffle
            return items.OrderBy(x => Random.value).ToList();
        }

        private int GetWeightForRarity(ItemRarity rarity)
        {
            return rarity switch
            {
                ItemRarity.Common => commonWeight,
                ItemRarity.Uncommon => uncommonWeight,
                ItemRarity.Rare => rareWeight,
                ItemRarity.Epic => epicWeight,
                ItemRarity.Legendary => legendaryWeight,
                _ => 1
            };
        }

        /// <summary>
        /// Add new item to database (editor extension)
        /// </summary>
        public void AddItem(ItemData item)
        {
            if (item != null && !allItems.Contains(item))
            {
                allItems.Add(item);
                BuildCache();
            }
        }

        /// <summary>
        /// Get total item count
        /// </summary>
        public int TotalItemCount => allItems.Count;

        /// <summary>
        /// Get all items (read-only)
        /// </summary>
        public IReadOnlyList<ItemData> AllItems => allItems;
    }

    /// <summary>
    /// Category for organizing items (optional system)
    /// </summary>
    [System.Serializable]
    public class ItemCategory
    {
        public string categoryId;
        public string categoryName;
        public Sprite categoryIcon;
        public Color categoryColor;
    }
}
