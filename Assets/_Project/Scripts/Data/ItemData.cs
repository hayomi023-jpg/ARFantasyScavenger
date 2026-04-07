using UnityEngine;

namespace ARFantasy.Data
{
    /// <summary>
    /// ScriptableObject containing data for collectible items
    /// </summary>
    [CreateAssetMenu(fileName = "NewItemData", menuName = "AR Fantasy/Item Data")]
    public class ItemData : ScriptableObject
    {
        [Header("Basic Info")]
        [Tooltip("Unique identifier for this item")]
        public string itemId;

        [Tooltip("Display name shown to player")]
        public string displayName;

        [Tooltip("Description shown in collection UI")]
        [TextArea(2, 4)]
        public string description;

        [Header("Gameplay")]
        [Tooltip("Points awarded when collected")]
        public int pointValue = 100;

        [Tooltip("Rarity affects spawn rate")]
        public ItemRarity rarity = ItemRarity.Common;

        [Tooltip("Does this item unlock something special?")]
        public bool isSpecialItem = false;

        [Header("Visuals")]
        [Tooltip("3D model prefab for this item")]
        public GameObject modelPrefab;

        [Tooltip("Icon for UI display")]
        public Sprite uiIcon;

        [Tooltip("Color tint for this item")]
        public Color glowColor = Color.cyan;

        [Header("Effects")]
        [Tooltip("Particle effect on collection")]
        public GameObject collectionEffect;

        [Tooltip("Sound effect on collection")]
        public AudioClip collectionSound;

        [Header("Animation")]
        [Tooltip("Animation type for floating")]
        public FloatAnimationType floatAnimation = FloatAnimationType.Sine;

        [Tooltip("Float amplitude")]
        public float floatAmplitude = 0.1f;

        [Tooltip("Float speed")]
        public float floatSpeed = 2f;

        [Tooltip("Rotation speed")]
        public float rotationSpeed = 30f;

        private void OnValidate()
        {
            // Auto-generate ID if empty
            if (string.IsNullOrEmpty(itemId))
            {
                itemId = System.Guid.NewGuid().ToString().Substring(0, 8);
            }

            // Set point value based on rarity if not customized
            if (pointValue == 100) // Default value
            {
                pointValue = GetDefaultPointsForRarity(rarity);
            }
        }

        private int GetDefaultPointsForRarity(ItemRarity r)
        {
            return r switch
            {
                ItemRarity.Common => 100,
                ItemRarity.Uncommon => 250,
                ItemRarity.Rare => 500,
                ItemRarity.Epic => 1000,
                ItemRarity.Legendary => 2500,
                _ => 100
            };
        }
    }

    public enum ItemRarity
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary
    }

    public enum FloatAnimationType
    {
        Sine,
        Bob,
        Spiral,
        Static
    }
}
