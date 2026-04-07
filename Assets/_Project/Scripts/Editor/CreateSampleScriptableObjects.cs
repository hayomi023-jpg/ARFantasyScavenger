#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using ARFantasy.Data;

namespace ARFantasy.Setup
{
    /// <summary>
    /// Creates sample ScriptableObject assets for testing.
    /// Run via Window → AR Fantasy → Create Sample Data
    /// </summary>
    public class CreateSampleScriptableObjects
    {
        [MenuItem("Window/AR Fantasy/Create Sample ItemData")]
        public static void CreateItemDataAssets()
        {
            CreateItemDataAsset("MysticCrystal", "Mystic Crystal", "A glowing magical crystal", 100, ItemRarity.Common);
            CreateItemDataAsset("AncientRune", "Ancient Rune", "Runes of power inscribed by mages", 250, ItemRarity.Uncommon);
            CreateItemDataAsset("MoonstoneGem", "Moonstone Gem", "A gem found only under moonlight", 500, ItemRarity.Rare);
            CreateItemDataAsset("DragonScale", "Dragon Scale", "A scale from an ancient dragon", 1000, ItemRarity.Epic);
            CreateItemDataAsset("PhoenixFeather", "Phoenix Feather", "A feather from the legendary phoenix", 2500, ItemRarity.Legendary);

            Debug.Log("Created 5 sample ItemData assets");
            AssetDatabase.SaveAssets();
        }

        [MenuItem("Window/AR Fantasy/Create Sample HuntConfigs")]
        public static void CreateHuntConfigAssets()
        {
            // Create hunts directory
            if (!AssetDatabase.IsValidFolder("Assets/_Project/ScriptableObjects/Hunts"))
            {
                AssetDatabase.CreateFolder("Assets/_Project/ScriptableObjects", "Hunts");
            }

            // Beginner's Hunt - no time limit
            HuntConfig beginner = CreateHuntConfigAsset("Hunts/BeginnersHunt", "Beginner's Hunt", "Your first magical scavenger hunt!", 5, 0f);

            // Timed Challenge - 60 seconds
            HuntConfig timed = CreateHuntConfigAsset("Hunts/TimedChallenge", "Timed Challenge", "Find all items before time runs out!", 5, 60f);
            timed.unlocksHunt = null; // Will set after creating expert

            // Expert Hunt - 90 seconds, more items
            HuntConfig expert = CreateHuntConfigAsset("Hunts/ExpertHunt", "Expert Hunt", "For seasoned hunters only!", 8, 90f);

            // Set unlock chain
            timed.unlocksHunt = expert;
            beginner.unlocksHunt = timed;

            EditorUtility.SetDirty(timed);
            EditorUtility.SetDirty(expert);
            EditorUtility.SetDirty(beginner);

            Debug.Log("Created 3 sample HuntConfig assets with unlock chain");
            AssetDatabase.SaveAssets();
        }

        [MenuItem("Window/AR Fantasy/Create ItemDatabase")]
        public static void CreateItemDatabaseAsset()
        {
            string path = "Assets/_Project/ScriptableObjects/ItemDatabase.asset";
            ItemDatabase database = ScriptableObject.CreateInstance<ItemDatabase>();

            // Find all ItemData assets
            string[] guids = AssetDatabase.FindAssets("t:ItemData");
            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                ItemData item = AssetDatabase.LoadAssetAtPath<ItemData>(assetPath);
                if (item != null)
                {
                    database.AddItem(item);
                }
            }

            AssetDatabase.CreateAsset(database, path);
            Debug.Log($"Created ItemDatabase at {path} with {database.TotalItemCount} items");
            AssetDatabase.SaveAssets();
        }

        private static ItemData CreateItemDataAsset(string name, string displayName, string description, int points, ItemRarity rarity)
        {
            ItemData item = ScriptableObject.CreateInstance<ItemData>();
            item.itemId = name.ToLower();
            item.displayName = displayName;
            item.description = description;
            item.pointValue = points;
            item.rarity = rarity;
            item.glowColor = GetRarityColor(rarity);

            string path = $"Assets/_Project/ScriptableObjects/{name}.asset";
            AssetDatabase.CreateAsset(item, path);

            return item;
        }

        private static HuntConfig CreateHuntConfigAsset(string path, string huntName, string description, int itemCount, float timeLimit)
        {
            HuntConfig config = ScriptableObject.CreateInstance<HuntConfig>();
            config.huntId = huntName.ToLower().Replace(" ", "_");
            config.huntName = huntName;
            config.description = description;
            config.itemCount = itemCount;
            config.timeLimit = timeLimit;
            config.requirePlaneDetection = true;
            config.minSpawnDistance = 1.5f;
            config.maxSpawnDistance = 4f;
            config.minItemSpacing = 0.8f;

            AssetDatabase.CreateAsset(config, $"Assets/_Project/ScriptableObjects/{path}.asset");

            return config;
        }

        private static Color GetRarityColor(ItemRarity rarity)
        {
            return rarity switch
            {
                ItemRarity.Common => Color.gray,
                ItemRarity.Uncommon => new Color(0.2f, 0.8f, 0.2f),
                ItemRarity.Rare => new Color(0.2f, 0.4f, 1f),
                ItemRarity.Epic => new Color(0.8f, 0.2f, 0.8f),
                ItemRarity.Legendary => new Color(1f, 0.8f, 0.2f),
                _ => Color.white
            };
        }
    }
}
#endif
