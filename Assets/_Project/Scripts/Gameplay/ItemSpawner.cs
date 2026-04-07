using UnityEngine;
using UnityEngine.XR.ARFoundation;
using System.Collections.Generic;
using ARFantasy.Core;

namespace ARFantasy.Gameplay
{
    /// <summary>
    /// Handles spawning of collectible items on AR planes with object pooling
    /// </summary>
    public class ItemSpawner : MonoBehaviour
    {
        [Header("Spawn Settings")]
        [SerializeField] private GameObject[] collectiblePrefabs;
        [SerializeField] private int poolSize = 10;
        [SerializeField] private float minSpawnDistance = 1.5f;
        [SerializeField] private float maxSpawnDistance = 4f;
        [SerializeField] private float minDistanceBetweenItems = 0.8f;
        [SerializeField] private float heightOffset = 0.1f;

        [Header("Safety Settings")]
        [SerializeField] private int maxSpawnAttempts = 20;
        [SerializeField] private LayerMask collisionLayers;

        private List<GameObject> objectPool = new List<GameObject>();
        private List<Vector3> spawnedPositions = new List<Vector3>();
        private ARRaycastManager raycastManager;
        private ARPlaneManager planeManager;

        public static ItemSpawner Instance { get; private set; }
        public int SpawnedCount { get; private set; } = 0;

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
            planeManager = ARSessionController.Instance?.PlaneManager;

            // Initialize object pool
            InitializeObjectPool();
        }

        private void InitializeObjectPool()
        {
            if (collectiblePrefabs.Length == 0)
            {
                Debug.LogError("No collectible prefabs assigned!");
                return;
            }

            // Create pool for each prefab type
            foreach (var prefab in collectiblePrefabs)
            {
                for (int i = 0; i < poolSize / collectiblePrefabs.Length; i++)
                {
                    GameObject obj = Instantiate(prefab, transform);
                    obj.SetActive(false);
                    objectPool.Add(obj);
                }
            }
        }

        /// <summary>
        /// Spawn items for a hunt session
        /// </summary>
        public void SpawnItems(int count)
        {
            if (raycastManager == null || planeManager == null)
            {
                Debug.LogError("AR managers not initialized!");
                return;
            }

            spawnedPositions.Clear();
            SpawnedCount = 0;

            // Despawn any existing items
            DespawnAllItems();

            // Get camera position
            Vector3 cameraPosition = ARSessionController.Instance?.GetCameraPosition() ?? Vector3.zero;

            for (int i = 0; i < count; i++)
            {
                Vector3? spawnPosition = FindValidSpawnPosition(cameraPosition);
                if (spawnPosition.HasValue)
                {
                    SpawnItem(spawnPosition.Value);
                    spawnedPositions.Add(spawnPosition.Value);
                    SpawnedCount++;
                }
                else
                               {
                    Debug.LogWarning($"Could not find valid spawn position for item {i + 1}");
                }
            }

            Debug.Log($"Spawned {SpawnedCount} items");
        }

        private Vector3? FindValidSpawnPosition(Vector3 cameraPos)
        {
            for (int attempt = 0; attempt < maxSpawnAttempts; attempt++)
            {
                // Random angle and distance
                float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
                float distance = Random.Range(minSpawnDistance, maxSpawnDistance);

                // Calculate position in front of camera
                Vector3 direction = new Vector3(Mathf.Sin(angle), 0, Mathf.Cos(angle));
                Vector3 candidatePos = cameraPos + direction * distance;

                // Raycast down to find plane
                List<ARRaycastHit> hits = new List<ARRaycastHit>();
                if (raycastManager.Raycast(
                    new Vector2(Screen.width / 2f, Screen.height / 2f),
                    hits,
                    Unity.Collections.Allocator.Temp))
                {
                    // Find closest hit to our candidate position
                    foreach (var hit in hits)
                    {
                        Vector3 hitPos = hit.pose.position;
                        float distFromCamera = Vector3.Distance(cameraPos, hitPos);

                        if (distFromCamera >= minSpawnDistance && distFromCamera <= maxSpawnDistance)
                        {
                            // Check distance from other spawned items
                            bool tooClose = false;
                            foreach (var spawnedPos in spawnedPositions)
                            {
                                if (Vector3.Distance(hitPos, spawnedPos) < minDistanceBetweenItems)
                                {
                                    tooClose = true;
                                    break;
                                }
                            }

                            if (!tooClose)
                            {
                                return hitPos + Vector3.up * heightOffset;
                            }
                        }
                    }
                }
            }

            return null;
        }

        private void SpawnItem(Vector3 position)
        {
            if (collectiblePrefabs.Length == 0) return;

            // Select random prefab
            GameObject prefab = collectiblePrefabs[Random.Range(0, collectiblePrefabs.Length)];

            // Try to get from pool, otherwise instantiate
            GameObject item = GetPooledItem(prefab);
            if (item == null)
            {
                item = Instantiate(prefab);
            }

            item.transform.position = position;
            item.transform.rotation = Quaternion.identity;
            item.SetActive(true);

            // Re-enable collider if it was disabled
            if (item.TryGetComponent(out Collider col))
            {
                col.enabled = true;
            }
        }

        private GameObject GetPooledItem(GameObject prefab)
        {
            // Find inactive item of same type
            foreach (var obj in objectPool)
            {
                if (!obj.activeInHierarchy && obj.name.StartsWith(prefab.name))
                {
                    return obj;
                }
            }
            return null;
        }

        /// <summary>
        /// Despawn all active items
        /// </summary>
        public void DespawnAllItems()
        {
            foreach (var obj in objectPool)
            {
                if (obj != null)
                {
                    obj.SetActive(false);
                }
            }

            // Also find and destroy any non-pooled items
            CollectibleItem[] activeItems = FindObjectsOfType<CollectibleItem>();
            foreach (var item in activeItems)
            {
                if (item.gameObject != null && !objectPool.Contains(item.gameObject))
                {
                    Destroy(item.gameObject);
                }
            }

            spawnedPositions.Clear();
            SpawnedCount = 0;
        }

        /// <summary>
        /// Despawn a specific item
        /// </summary>
        public void DespawnItem(GameObject item)
        {
            if (objectPool.Contains(item))
            {
                item.SetActive(false);
            }
            else
            {
                Destroy(item);
            }

            spawnedPositions.Remove(item.transform.position);
            SpawnedCount--;
        }
    }
}
