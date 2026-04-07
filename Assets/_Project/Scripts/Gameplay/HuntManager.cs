using UnityEngine;
using ARFantasy.Core;
using ARFantasy.AR;

namespace ARFantasy.Gameplay
{
    /// <summary>
    /// Orchestrates the hunt session - coordinates spawning, scoring, and game flow
    /// </summary>
    public class HuntManager : MonoBehaviour
    {
        [Header("Hunt Configuration")]
        [SerializeField] private int itemsToSpawn = 5;
        [SerializeField] private bool requirePlanesBeforeSpawn = true;

        [Header("References")]
        [SerializeField] private ItemSpawner itemSpawner;
        [SerializeField] private AdvancedItemSpawner advancedItemSpawner;
        [SerializeField] private PlaneDetectionManager planeDetectionManager;
        [SerializeField] private HuntConfig currentHuntConfig;

        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = true;

        private bool isHuntActive = false;
        private bool hasSpawnedItems = false;

        public static HuntManager Instance { get; private set; }
        public bool IsHuntActive => isHuntActive;
        public int ItemsRemaining => GameManager.Instance?.TotalItemsToCollect - GameManager.Instance?.ItemsCollected ?? 0;
        public HuntConfig CurrentHuntConfig => currentHuntConfig;

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
            // Subscribe to game state changes
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGameStateChanged += OnGameStateChanged;
            }

            // Get references if not assigned
            if (itemSpawner == null)
            {
                itemSpawner = ItemSpawner.Instance;
            }
            if (planeDetectionManager == null)
            {
                planeDetectionManager = FindObjectOfType<PlaneDetectionManager>();
            }

            if (showDebugLogs)
            {
                Debug.Log("HuntManager initialized");
            }
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGameStateChanged -= OnGameStateChanged;
            }
        }

        private void OnGameStateChanged(GameState newState)
        {
            switch (newState)
            {
                case GameState.Scanning:
                    OnEnterScanningState();
                    break;
                case GameState.Playing:
                    OnEnterPlayingState();
                    break;
                case GameState.Completed:
                    OnHuntCompleted();
                    break;
            }
        }

        private void OnEnterScanningState()
        {
            isHuntActive = true;
            hasSpawnedItems = false;

            // Start plane detection
            if (planeDetectionManager != null)
            {
                planeDetectionManager.StartScanning();
                planeDetectionManager.OnPlanesDetected += OnPlanesDetected;
            }

            // Clear any existing items
            itemSpawner?.DespawnAllItems();

            if (showDebugLogs)
            {
                Debug.Log("Entered Scanning state - looking for planes");
            }
        }

        private void OnPlanesDetected()
        {
            if (hasSpawnedItems) return;

            if (showDebugLogs)
            {
                Debug.Log("Planes detected! Ready to spawn items.");
            }

            // Optionally auto-spawn or wait for player input
            // For now, we'll spawn automatically when planes are found
            SpawnItems();

            // Unsubscribe to prevent multiple spawns
            if (planeDetectionManager != null)
            {
                planeDetectionManager.OnPlanesDetected -= OnPlanesDetected;
            }
        }

        private void OnEnterPlayingState()
        {
            if (!hasSpawnedItems)
            {
                SpawnItems();
            }

            if (planeDetectionManager != null)
            {
                planeDetectionManager.StopScanning();
            }

            if (showDebugLogs)
            {
                Debug.Log("Hunt started! Find all the magical items!");
            }
        }

        private void SpawnItems()
        {
            if (hasSpawnedItems) return;

            // Use AdvancedItemSpawner with HuntConfig if available
            if (advancedItemSpawner != null && currentHuntConfig != null)
            {
                advancedItemSpawner.SpawnHuntItems(currentHuntConfig);
                hasSpawnedItems = true;
                GameManager.Instance?.StartPlaying();
            }
            else if (itemSpawner != null)
            {
                itemSpawner.SpawnItems(itemsToSpawn);
                hasSpawnedItems = true;
                GameManager.Instance?.StartPlaying();
            }
            else
            {
                Debug.LogError("No item spawner available!");
            }
        }

        /// <summary>
        /// Configure the current hunt. Called by HuntSelectionUI before starting.
        /// </summary>
        public void SetHuntConfig(HuntConfig config)
        {
            currentHuntConfig = config;
            if (config != null)
            {
                itemsToSpawn = config.itemCount;
                requirePlanesBeforeSpawn = config.requirePlaneDetection;
            }
        }

        /// <summary>
        /// Get the current hunt config
        /// </summary>
        public HuntConfig GetHuntConfig() => currentHuntConfig;

        private void OnHuntCompleted()
        {
            isHuntActive = false;

            // Play victory effects
            AudioManager.Instance?.PlayWinSound();

            if (showDebugLogs)
            {
                Debug.Log($"Hunt completed! Final score: {GameManager.Instance?.CurrentScore}");
            }
        }

        /// <summary>
        /// Force start the hunt (for testing or manual start)
        /// </summary>
        public void ForceStartHunt()
        {
            GameManager.Instance?.StartNewHunt();
        }

        /// <summary>
        /// Reset the current hunt
        /// </summary>
        public void ResetHunt()
        {
            itemSpawner?.DespawnAllItems();
            hasSpawnedItems = false;
            isHuntActive = false;
        }

        /// <summary>
        /// Get current hunt progress (0-1)
        /// </summary>
        public float GetProgress()
        {
            if (GameManager.Instance == null) return 0f;
            return (float)GameManager.Instance.ItemsCollected / GameManager.Instance.TotalItemsToCollect;
        }
    }
}
