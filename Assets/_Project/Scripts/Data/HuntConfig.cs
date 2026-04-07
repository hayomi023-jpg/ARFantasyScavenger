using UnityEngine;

namespace ARFantasy.Data
{
    /// <summary>
    /// ScriptableObject for configuring hunt sessions
    /// </summary>
    [CreateAssetMenu(fileName = "NewHuntConfig", menuName = "AR Fantasy/Hunt Config")]
    public class HuntConfig : ScriptableObject
    {
        [Header("Basic Info")]
        [Tooltip("Unique identifier for this hunt")]
        public string huntId;

        [Tooltip("Display name for the hunt")]
        public string huntName;

        [Tooltip("Description of the hunt")]
        [TextArea(2, 4)]
        public string description;

        [Header("Hunt Settings")]
        [Tooltip("Number of items to spawn")]
        public int itemCount = 5;

        [Tooltip("Time limit in seconds (0 = no limit)")]
        public float timeLimit = 0f;

        [Tooltip("Show plane detection before starting")]
        public bool requirePlaneDetection = true;

        [Tooltip("Minimum plane size for spawning")]
        public float minPlaneSize = 0.5f;

        [Header("Item Pool")]
        [Tooltip("Available items for this hunt (if empty, uses all items)")]
        public ItemData[] availableItems;

        [Tooltip("Use rarity weights for spawning")]
        public bool useRarityWeights = true;

        [Header("Difficulty")]
        [Tooltip("Minimum spawn distance from player")]
        public float minSpawnDistance = 1.5f;

        [Tooltip("Maximum spawn distance from player")]
        public float maxSpawnDistance = 5f;

        [Tooltip("Minimum distance between items")]
        public float minItemSpacing = 0.8f;

        [Header("Rewards")]
        [Tooltip("Base score bonus for completing hunt")]
        public int completionBonus = 500;

        [Tooltip("Time bonus per second remaining")]
        public int timeBonusPerSecond = 10;

        [Tooltip("Unlocks next hunt when completed")]
        public HuntConfig unlocksHunt;

        private void OnValidate()
        {
            // Auto-generate ID if empty
            if (string.IsNullOrEmpty(huntId))
            {
                huntId = System.Guid.NewGuid().ToString().Substring(0, 8);
            }

            // Ensure reasonable values
            itemCount = Mathf.Clamp(itemCount, 1, 50);
            minSpawnDistance = Mathf.Max(0.5f, minSpawnDistance);
            maxSpawnDistance = Mathf.Max(minSpawnDistance + 0.5f, maxSpawnDistance);
        }
    }
}
