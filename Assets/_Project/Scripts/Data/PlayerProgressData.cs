using System;
using System.Collections.Generic;
using UnityEngine;

namespace ARFantasy.Data
{
    /// <summary>
    /// Serializable player progress data for save/load
    /// </summary>
    [Serializable]
    public class PlayerProgressData
    {
        // Version for save compatibility
        public int saveVersion = 1;
        public string lastSaveDate;

        // Total stats
        public int totalItemsCollected;
        public int totalScore;
        public int huntsCompleted;
        public int perfectHunts; // All items collected
        public float totalPlayTime; // in seconds

        // Collection tracking
        public List<string> discoveredItemIds = new List<string>();
        public List<string> collectedItemIds = new List<string>();
        public Dictionary<string, int> itemCollectCounts = new Dictionary<string, int>();

        // Hunt completion tracking
        public List<string> completedHuntIds = new List<string>();
        public Dictionary<string, HuntHighScore> huntHighScores = new Dictionary<string, HuntHighScore>();

        // Unlocks
        public List<string> unlockedHuntIds = new List<string>();
        public List<string> unlockedItemIds = new List<string>();

        // Settings
        public bool soundEnabled = true;
        public bool musicEnabled = true;
        public bool vibrationEnabled = true;
        public float sfxVolume = 1f;
        public float musicVolume = 0.5f;

        // Achievements
        public List<string> unlockedAchievements = new List<string>();
        public Dictionary<string, int> achievementProgress = new Dictionary<string, int>();
    }

    /// <summary>
    /// High score data for a specific hunt
    /// </summary>
    [Serializable]
    public class HuntHighScore
    {
        public string huntId;
        public int bestScore;
        public float bestTime;
        public int bestItemCount;
        public string dateAchieved;
        public int attempts;
    }

    /// <summary>
    /// Individual item collection record
    /// </summary>
    [Serializable]
    public class CollectionRecord
    {
        public string itemId;
        public string firstFoundDate;
        public string lastFoundDate;
        public int timesCollected;
        public List<string> foundInHunts = new List<string>();
    }
}
