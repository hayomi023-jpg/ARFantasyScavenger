using UnityEngine;
using System.Collections.Generic;
using ARFantasy.Data;

namespace ARFantasy.Core
{
    /// <summary>
    /// Central configuration for all available hunts in the game.
    /// Attach to a GameObject in the main menu scene.
    /// </summary>
    public class HuntConfigurationManager : MonoBehaviour
    {
        [Header("Available Hunts")]
        [Tooltip("All hunts available in the game, in unlock order")]
        [SerializeField] private List<HuntConfig> availableHunts = new List<HuntConfig>();

        [Header("Default Hunt")]
        [Tooltip("Hunt to use if none selected or for quick start")]
        [SerializeField] private HuntConfig defaultHunt;

        [Header("First Time Setup")]
        [SerializeField] private HuntConfig firstHunt;

        public static HuntConfigurationManager Instance { get; private set; }

        public IReadOnlyList<HuntConfig> AvailableHunts => availableHunts;
        public HuntConfig DefaultHunt => defaultHunt;
        public HuntConfig FirstHunt => firstHunt;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        /// <summary>
        /// Get hunt by its huntId
        /// </summary>
        public HuntConfig GetHuntById(string huntId)
        {
            return availableHunts.Find(h => h.huntId == huntId);
        }

        /// <summary>
        /// Get the next hunt after completing current one
        /// </summary>
        public HuntConfig GetNextHunt(HuntConfig currentHunt)
        {
            if (currentHunt == null || currentHunt.unlocksHunt == null)
            {
                // Find first incomplete hunt
                foreach (var hunt in availableHunts)
                {
                    if (!PlayerProgressManager.Instance?.IsHuntCompleted(hunt.huntId) ?? true)
                    {
                        return hunt;
                    }
                }
                return defaultHunt;
            }
            return currentHunt.unlocksHunt;
        }

        /// <summary>
        /// Get all hunts of a specific difficulty (based on HuntConfig settings)
        /// </summary>
        public List<HuntConfig> GetHuntsByDifficulty(int minItems, int maxItems, float maxTimeLimit)
        {
            return availableHunts.FindAll(h =>
                h.itemCount >= minItems &&
                h.itemCount <= maxItems &&
                (maxTimeLimit <= 0 || h.timeLimit <= maxTimeLimit || h.timeLimit == 0));
        }
    }
}
