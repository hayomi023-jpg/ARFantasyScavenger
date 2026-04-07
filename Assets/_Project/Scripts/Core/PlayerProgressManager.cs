using UnityEngine;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using ARFantasy.Data;

namespace ARFantasy.Core
{
    /// <summary>
    /// Manages player progress, collection journal, and save/load
    /// </summary>
    public class PlayerProgressManager : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private bool autoSave = true;
        [SerializeField] private float autoSaveInterval = 30f;
        [SerializeField] private bool encryptSaveData = false;

        [Header("Starting Unlocks")]
        [SerializeField] private List<HuntConfig> initiallyUnlockedHunts = new List<HuntConfig>();

        public static PlayerProgressManager Instance { get; private set; }

        // Current session data
        private PlayerProgressData currentProgress;
        private float sessionStartTime;
        private float timeSinceLastSave;

        // Events
        public delegate void ItemDiscoveredEvent(ItemData item);
        public event ItemDiscoveredEvent OnItemDiscovered;

        public delegate void HuntCompletedEvent(HuntConfig hunt, int score, bool isPerfect);
        public event HuntCompletedEvent OnHuntCompleted;

        public delegate void AchievementUnlockedEvent(string achievementId);
        public event AchievementUnlockedEvent OnAchievementUnlocked;

        // File path
        private string SaveFilePath => Path.Combine(Application.persistentDataPath, "playerProgress.json");

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            LoadProgress();
            InitializeStartingUnlocks();
            sessionStartTime = Time.time;

            // Subscribe to game events
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnItemCollected += OnItemCollected;
            }
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnItemCollected -= OnItemCollected;
            }

            if (autoSave)
            {
                SaveProgress();
            }
        }

        private void Update()
        {
            if (autoSave)
            {
                timeSinceLastSave += Time.deltaTime;
                if (timeSinceLastSave >= autoSaveInterval)
                {
                    SaveProgress();
                    timeSinceLastSave = 0f;
                }
            }
        }

        #region Save/Load

        public void SaveProgress()
        {
            try
            {
                currentProgress.lastSaveDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                currentProgress.totalPlayTime += (Time.time - sessionStartTime);
                sessionStartTime = Time.time;

                string json = JsonUtility.ToJson(currentProgress, true);

                if (encryptSaveData)
                {
                    json = EncryptString(json);
                }

                File.WriteAllText(SaveFilePath, json);
                Debug.Log("Progress saved successfully");
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to save progress: {e.Message}");
            }
        }

        public void LoadProgress()
        {
            try
            {
                if (File.Exists(SaveFilePath))
                {
                    string json = File.ReadAllText(SaveFilePath);

                    if (encryptSaveData)
                    {
                        json = DecryptString(json);
                    }

                    currentProgress = JsonUtility.FromJson<PlayerProgressData>(json);

                    // Validate version
                    if (currentProgress.saveVersion != 1)
                    {
                        Debug.LogWarning($"Save version mismatch: {currentProgress.saveVersion}");
                        // Could implement migration here
                    }

                    Debug.Log("Progress loaded successfully");
                }
                else
                {
                    // Create new progress
                    currentProgress = new PlayerProgressData();
                    Debug.Log("New progress created");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to load progress: {e.Message}");
                currentProgress = new PlayerProgressData();
            }
        }

        public void ResetProgress()
        {
            currentProgress = new PlayerProgressData();
            SaveProgress();
            InitializeStartingUnlocks();
            Debug.Log("Progress reset");
        }

        private void InitializeStartingUnlocks()
        {
            foreach (var hunt in initiallyUnlockedHunts)
            {
                if (hunt != null && !string.IsNullOrEmpty(hunt.huntId))
                {
                    UnlockHunt(hunt.huntId);
                }
            }
        }

        #endregion

        #region Collection Tracking

        private void OnItemCollected(int collected, int total)
        {
            // This is called when item is collected, but we need item data
            // HuntManager should call RecordItemCollected instead
        }

        /// <summary>
        /// Record an item being collected
        /// </summary>
        public void RecordItemCollected(ItemData item, string huntId)
        {
            if (item == null) return;

            string itemId = item.itemId;

            // First discovery
            if (!currentProgress.discoveredItemIds.Contains(itemId))
            {
                currentProgress.discoveredItemIds.Add(itemId);
                OnItemDiscovered?.Invoke(item);
            }

            // Track collection
            if (!currentProgress.collectedItemIds.Contains(itemId))
            {
                currentProgress.collectedItemIds.Add(itemId);
            }

            // Update count
            if (currentProgress.itemCollectCounts.ContainsKey(itemId))
            {
                currentProgress.itemCollectCounts[itemId]++;
            }
            else
            {
                currentProgress.itemCollectCounts[itemId] = 1;
            }

            // Update totals
            currentProgress.totalItemsCollected++;
            currentProgress.totalScore += item.pointValue;

            // Check achievements
            CheckCollectionAchievements(item);
        }

        /// <summary>
        /// Check if item has been discovered
        /// </summary>
        public bool IsItemDiscovered(string itemId)
        {
            return currentProgress.discoveredItemIds.Contains(itemId);
        }

        /// <summary>
        /// Get collection count for item
        /// </summary>
        public int GetItemCollectionCount(string itemId)
        {
            return currentProgress.itemCollectCounts.TryGetValue(itemId, out int count) ? count : 0;
        }

        /// <summary>
        /// Get total unique items discovered
        /// </summary>
        public int GetTotalDiscoveredItems()
        {
            return currentProgress.discoveredItemIds.Count;
        }

        /// <summary>
        /// Get completion percentage
        /// </summary>
        public float GetCollectionCompletion(int totalItemsAvailable)
        {
            if (totalItemsAvailable == 0) return 0f;
            return (float)currentProgress.discoveredItemIds.Count / totalItemsAvailable;
        }

        #endregion

        #region Hunt Completion

        /// <summary>
        /// Record hunt completion
        /// </summary>
        public void RecordHuntCompletion(HuntConfig hunt, int score, float time, int itemsCollected, bool isPerfect)
        {
            if (hunt == null) return;

            string huntId = hunt.huntId;

            // Track completion
            if (!currentProgress.completedHuntIds.Contains(huntId))
            {
                currentProgress.completedHuntIds.Add(huntId);
            }

            // Update high score
            if (!currentProgress.huntHighScores.TryGetValue(huntId, out var highScore))
            {
                highScore = new HuntHighScore { huntId = huntId, attempts = 0 };
            }

            highScore.attempts++;
            bool isNewRecord = false;

            if (score > highScore.bestScore)
            {
                highScore.bestScore = score;
                highScore.bestTime = time;
                highScore.bestItemCount = itemsCollected;
                highScore.dateAchieved = DateTime.Now.ToString("yyyy-MM-dd");
                isNewRecord = true;
            }

            currentProgress.huntHighScores[huntId] = highScore;

            // Update totals
            currentProgress.huntsCompleted++;
            if (isPerfect)
            {
                currentProgress.perfectHunts++;
            }

            // Unlock next hunt
            if (hunt.unlocksHunt != null && !string.IsNullOrEmpty(hunt.unlocksHunt.huntId))
            {
                UnlockHunt(hunt.unlocksHunt.huntId);
            }

            // Event
            OnHuntCompleted?.Invoke(hunt, score, isPerfect);

            Debug.Log($"Hunt completed: {hunt.huntName}, Score: {score}, New Record: {isNewRecord}");
        }

        /// <summary>
        /// Check if hunt is completed
        /// </summary>
        public bool IsHuntCompleted(string huntId)
        {
            return currentProgress.completedHuntIds.Contains(huntId);
        }

        /// <summary>
        /// Get high score for hunt
        /// </summary>
        public HuntHighScore GetHuntHighScore(string huntId)
        {
            return currentProgress.huntHighScores.TryGetValue(huntId, out var score) ? score : null;
        }

        #endregion

        #region Unlocks

        public void UnlockHunt(string huntId)
        {
            if (!currentProgress.unlockedHuntIds.Contains(huntId))
            {
                currentProgress.unlockedHuntIds.Add(huntId);
            }
        }

        public void UnlockItem(string itemId)
        {
            if (!currentProgress.unlockedItemIds.Contains(itemId))
            {
                currentProgress.unlockedItemIds.Add(itemId);
            }
        }

        public bool IsHuntUnlocked(string huntId)
        {
            return currentProgress.unlockedHuntIds.Contains(huntId);
        }

        public bool IsItemUnlocked(string itemId)
        {
            return currentProgress.unlockedItemIds.Contains(itemId);
        }

        #endregion

        #region Achievements

        private void CheckCollectionAchievements(ItemData item)
        {
            // Example achievements
            CheckAchievement("first_item", currentProgress.totalItemsCollected >= 1);
            CheckAchievement("collector_10", currentProgress.totalItemsCollected >= 10);
            CheckAchievement("collector_50", currentProgress.totalItemsCollected >= 50);

            if (item != null)
            {
                CheckAchievement($"rarity_{item.rarity.ToString().ToLower()}", true);

                if (item.isSpecialItem)
                {
                    CheckAchievement("special_finder", true);
                }
            }
        }

        private void CheckAchievement(string achievementId, bool condition)
        {
            if (!condition) return;
            if (currentProgress.unlockedAchievements.Contains(achievementId)) return;

            currentProgress.unlockedAchievements.Add(achievementId);
            OnAchievementUnlocked?.Invoke(achievementId);

            Debug.Log($"Achievement unlocked: {achievementId}");
        }

        public bool HasAchievement(string achievementId)
        {
            return currentProgress.unlockedAchievements.Contains(achievementId);
        }

        #endregion

        #region Settings

        public void SetSoundEnabled(bool enabled)
        {
            currentProgress.soundEnabled = enabled;
        }

        public void SetMusicEnabled(bool enabled)
        {
            currentProgress.musicEnabled = enabled;
        }

        public void SetSFXVolume(float volume)
        {
            currentProgress.sfxVolume = Mathf.Clamp01(volume);
        }

        public void SetMusicVolume(float volume)
        {
            currentProgress.musicVolume = Mathf.Clamp01(volume);
        }

        #endregion

        #region Encryption (Simple XOR for basic obfuscation)

        private string EncryptString(string text)
        {
            // Simple XOR encryption - replace with proper encryption if needed
            char[] key = { 'A', 'R', 'F', 'a', 'n', 't', 'a', 's', 'y' };
            char[] output = new char[text.Length];

            for (int i = 0; i < text.Length; i++)
            {
                output[i] = (char)(text[i] ^ key[i % key.Length]);
            }

            return new string(output);
        }

        private string DecryptString(string text)
        {
            // XOR is symmetric, so encryption = decryption
            return EncryptString(text);
        }

        #endregion

        #region Public Accessors

        public PlayerProgressData CurrentProgress => currentProgress;
        public int TotalItemsCollected => currentProgress?.totalItemsCollected ?? 0;
        public int TotalScore => currentProgress?.totalScore ?? 0;
        public int HuntsCompleted => currentProgress?.huntsCompleted ?? 0;
        public IReadOnlyList<string> DiscoveredItemIds => currentProgress?.discoveredItemIds;
        public IReadOnlyList<string> CompletedHuntIds => currentProgress?.completedHuntIds;

        #endregion
    }
}
