using UnityEngine;
using UnityEngine.XR.ARFoundation;
using System.Collections.Generic;
using System.Linq;
using ARFantasy.Core;
using ARFantasy.Data;

namespace ARFantasy.Gameplay
{
    /// <summary>
    /// Advanced item spawner with database integration and spawn rules
    /// </summary>
    public class AdvancedItemSpawner : MonoBehaviour
    {
        [Header("Database")]
        [SerializeField] private ItemDatabase itemDatabase;
        [SerializeField] private bool useWeightedSpawning = true;

        [Header("Spawn Rules")]
        [SerializeField] private SpawnRule defaultSpawnRule;
        [SerializeField] private List<SpawnRule> spawnRules = new List<SpawnRule>();

        [Header("Distance Constraints")]
        [SerializeField] private float minSpawnDistance = 1.5f;
        [SerializeField] private float maxSpawnDistance = 5f;
        [SerializeField] private float minItemSpacing = 0.8f;
        [SerializeField] private float heightOffset = 0.05f;

        [Header("Safety")]
        [SerializeField] private int maxSpawnAttempts = 25;
        [SerializeField] private LayerMask obstructionLayers;

        [Header("Pooling")]
        [SerializeField] private int poolSizePerItem = 5;

        // Runtime
        private ARRaycastManager raycastManager;
        private Dictionary<string, Queue<GameObject>> objectPools;
        private List<SpawnedItem> activeItems = new List<SpawnedItem>();
        private Transform playerTransform;

        public static AdvancedItemSpawner Instance { get; private set; }
        public int ActiveItemCount => activeItems.Count;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            raycastManager = ARSessionController.Instance?.RaycastManager;
            playerTransform = ARSessionController.Instance?.GetCameraTransform();

            InitializePools();
        }

        private void InitializePools()
        {
            objectPools = new Dictionary<string, Queue<GameObject>>();

            if (itemDatabase == null) return;

            foreach (var item in itemDatabase.AllItems)
            {
                if (item?.modelPrefab == null) continue;

                var pool = new Queue<GameObject>();
                for (int i = 0; i < poolSizePerItem; i++)
                {
                    GameObject obj = Instantiate(item.modelPrefab, transform);
                    obj.SetActive(false);
                    pool.Enqueue(obj);
                }

                objectPools[item.itemId] = pool;
            }
        }

        /// <summary>
        /// Spawn items for a hunt using spawn rules
        /// </summary>
        public void SpawnHuntItems(HuntConfig huntConfig)
        {
            ClearAllItems();

            // Generate items from database
            List<ItemData> itemsToSpawn;
            if (huntConfig.useRarityWeights)
            {
                itemsToSpawn = itemDatabase?.GenerateHuntItemsWithGuaranteedRare(huntConfig.itemCount)
                               ?? new List<ItemData>();
            }
            else
            {
                itemsToSpawn = itemDatabase?.GenerateHuntItems(huntConfig.itemCount)
                               ?? new List<ItemData>();
            }

            // Spawn each item
            for (int i = 0; i < itemsToSpawn.Count; i++)
            {
                // Apply spawn rules based on item index
                SpawnRule rule = GetSpawnRuleForIndex(i, itemsToSpawn.Count);

                Vector3? spawnPosition = FindSpawnPosition(rule);
                if (spawnPosition.HasValue)
                {
                    SpawnItem(itemsToSpawn[i], spawnPosition.Value, rule);
                }
            }

            Debug.Log($"Spawned {activeItems.Count} items for hunt");
        }

        /// <summary>
        /// Spawn a single item type multiple times
        /// </summary>
        public void SpawnItems(string itemId, int count)
        {
            var item = itemDatabase?.GetItemById(itemId);
            if (item == null) return;

            for (int i = 0; i < count; i++)
            {
                Vector3? position = FindSpawnPosition(defaultSpawnRule);
                if (position.HasValue)
                {
                    SpawnItem(item, position.Value, defaultSpawnRule);
                }
            }
        }

        private SpawnRule GetSpawnRuleForIndex(int index, int total)
        {
            // Find rule that applies to this index
            foreach (var rule in spawnRules)
            {
                if (rule.appliesToIndex == SpawnRuleIndex.Specific && rule.specificIndex == index)
                    return rule;
                if (rule.appliesToIndex == SpawnRuleIndex.First && index == 0)
                    return rule;
                if (rule.appliesToIndex == SpawnRuleIndex.Last && index == total - 1)
                    return rule;
                if (rule.appliesToIndex == SpawnRuleIndex.Random && Random.value < 0.3f)
                    return rule;
            }

            return defaultSpawnRule;
        }

        private Vector3? FindSpawnPosition(SpawnRule rule)
        {
            float minDist = rule?.overrideDistance ?? minSpawnDistance;
            float maxDist = rule?.overrideMaxDistance ?? maxSpawnDistance;

            for (int attempt = 0; attempt < maxSpawnAttempts; attempt++)
            {
                // Random position in arc in front of player
                float angle = Random.Range(-60f, 60f);
                float distance = Random.Range(minDist, maxDist);

                Vector3 direction = Quaternion.Euler(0, angle, 0) * playerTransform.forward;
                Vector3 candidatePos = playerTransform.position + direction * distance;

                // Raycast down to find plane
                if (raycastManager != null)
                {
                    List<ARRaycastHit> hits = new List<ARRaycastHit>();
                    if (raycastManager.Raycast(
                        new Vector2(Screen.width / 2f, Screen.height / 2f),
                        hits,
                        Unity.Collections.Allocator.Temp))
                    {
                        // Check all hits for valid position
                        foreach (var hit in hits)
                        {
                            Vector3 hitPos = hit.pose.position;

                            // Check distance constraints
                            float distFromPlayer = Vector3.Distance(playerTransform.position, hitPos);
                            if (distFromPlayer < minDist || distFromPlayer > maxDist)
                                continue;

                            // Check spacing from other items
                            if (IsTooCloseToOtherItems(hitPos))
                                continue;

                            // Check for obstructions
                            if (IsObstructed(hitPos))
                                continue;

                            return hitPos + Vector3.up * heightOffset;
                        }
                    }
                }
            }

            return null;
        }

        private bool IsTooCloseToOtherItems(Vector3 position)
        {
            foreach (var item in activeItems)
            {
                if (Vector3.Distance(position, item.position) < minItemSpacing)
                {
                    return true;
                }
            }
            return false;
        }

        private bool IsObstructed(Vector3 position)
        {
            // Raycast from camera to position to check for walls/furniture
            Vector3 cameraPos = playerTransform.position;
            Vector3 direction = position - cameraPos;
            float distance = direction.magnitude;

            if (Physics.Raycast(cameraPos, direction.normalized, distance, obstructionLayers))
            {
                return true; // Something is blocking the view
            }

            return false;
        }

        private void SpawnItem(ItemData itemData, Vector3 position, SpawnRule rule)
        {
            GameObject itemObj = GetFromPool(itemData.itemId);
            if (itemObj == null)
            {
                // Fallback to instantiate
                if (itemData.modelPrefab != null)
                {
                    itemObj = Instantiate(itemData.modelPrefab);
                }
                else
                {
                    Debug.LogError($"No prefab for item {itemData.displayName}");
                    return;
                }
            }

            // Position and setup
            itemObj.transform.position = position;
            itemObj.transform.rotation = Quaternion.identity;
            itemObj.SetActive(true);

            // Initialize with item data
            var collectible = itemObj.GetComponent<CollectibleItemVariant>();
            if (collectible == null)
            {
                collectible = itemObj.AddComponent<CollectibleItemVariant>();
            }
            collectible.Initialize(itemData);

            // Apply spawn rule modifiers
            if (rule != null)
            {
                ApplySpawnRuleModifiers(collectible, rule);
            }

            // Track spawned item
            activeItems.Add(new SpawnedItem
            {
                itemId = itemData.itemId,
                gameObject = itemObj,
                position = position,
                spawnTime = Time.time
            });

            // Spawn effect
            AudioManager.Instance?.PlaySpawnSound();
        }

        private void ApplySpawnRuleModifiers(CollectibleItemVariant collectible, SpawnRule rule)
        {
            // Could modify behavior based on rule
            // For example: make first item easier to spot, last item harder
        }

        private GameObject GetFromPool(string itemId)
        {
            if (objectPools.TryGetValue(itemId, out var pool) && pool.Count > 0)
            {
                return pool.Dequeue();
            }
            return null;
        }

        private void ReturnToPool(string itemId, GameObject obj)
        {
            if (objectPools.TryGetValue(itemId, out var pool))
            {
                obj.SetActive(false);
                pool.Enqueue(obj);
            }
            else
            {
                Destroy(obj);
            }
        }

        /// <summary>
        /// Clear all spawned items
        /// </summary>
        public void ClearAllItems()
        {
            foreach (var item in activeItems)
            {
                if (item.gameObject != null)
                {
                    ReturnToPool(item.itemId, item.gameObject);
                }
            }
            activeItems.Clear();
        }

        /// <summary>
        /// Remove specific item (when collected)
        /// </summary>
        public void RemoveItem(GameObject itemObj)
        {
            var spawned = activeItems.Find(i => i.gameObject == itemObj);
            if (spawned.gameObject != null)
            {
                activeItems.Remove(spawned);
                ReturnToPool(spawned.itemId, itemObj);
            }
        }

        /// <summary>
        /// Get hint for nearest uncollected item
        /// </summary>
        public Vector3? GetNearestItemDirection()
        {
            if (activeItems.Count == 0 || playerTransform == null) return null;

            Vector3 playerPos = playerTransform.position;
            float nearestDist = float.MaxValue;
            Vector3 nearestPos = Vector3.zero;

            foreach (var item in activeItems)
            {
                float dist = Vector3.Distance(playerPos, item.position);
                if (dist < nearestDist)
                {
                    nearestDist = dist;
                    nearestPos = item.position;
                }
            }

            return nearestPos;
        }
    }

    /// <summary>
/// Spawn rule for customizing item spawning behavior
    /// </summary>
    [System.Serializable]
    public class SpawnRule
    {
        public string ruleName = "Default";
        public SpawnRuleIndex appliesToIndex = SpawnRuleIndex.Any;
        public int specificIndex = 0;

        [Header("Distance Override")]
        public bool overrideDistanceRange = false;
        public float overrideDistance = 1.5f;
        public float overrideMaxDistance = 4f;

        [Header("Rarity Override")]
        public bool forceSpecificRarity = false;
        public ItemRarity forcedRarity = ItemRarity.Common;

        [Header("Visual Override")]
        public bool makeLarger = false;
        public float sizeMultiplier = 1.5f;
        public bool addGlow = false;
    }

    public enum SpawnRuleIndex
    {
        Any,        // Any position
        First,      // First item spawned
        Last,       // Last item spawned
        Random,     // Random chance
        Specific    // Specific index
    }

    public struct SpawnedItem
    {
        public string itemId;
        public GameObject gameObject;
        public Vector3 position;
        public float spawnTime;
    }
}
