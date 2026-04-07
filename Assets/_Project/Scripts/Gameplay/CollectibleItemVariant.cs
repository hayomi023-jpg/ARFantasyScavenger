using UnityEngine;
using ARFantasy.Core;
using ARFantasy.Data;
using System.Collections;

namespace ARFantasy.Gameplay
{
    /// <summary>
    /// Extended collectible with different behavior types and special effects
    /// </summary>
    public class CollectibleItemVariant : MonoBehaviour
    {
        [Header("Item Data")]
        [SerializeField] private ItemData itemData;
        [SerializeField] private bool autoInitializeFromData = true;

        [Header("Behavior Type")]
        [SerializeField] private CollectibleBehavior behaviorType = CollectibleBehavior.Standard;

        [Header("Movement")]
        [SerializeField] private float floatAmplitude = 0.15f;
        [SerializeField] private float floatSpeed = 1.5f;
        [SerializeField] private float rotationSpeed = 45f;
        [SerializeField] private float orbitRadius = 0f;
        [SerializeField] private float orbitSpeed = 30f;

        [Header("Magnetic Effect")]
        [SerializeField] private float magneticRadius = 2f;
        [SerializeField] private float magneticStrength = 5f;

        [Header("Collection")]
        [SerializeField] private float collectionDuration = 0.5f;
        [SerializeField] private AnimationCurve collectionCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);
        [SerializeField] private bool spawnChainReaction = false;
        [SerializeField] private int chainReactionCount = 3;
        [SerializeField] private float chainReactionRadius = 2f;

        [Header("Visuals")]
        [SerializeField] private ParticleSystem ambientParticles;
        [SerializeField] private Light glowLight;
        [SerializeField] private TrailRenderer trailRenderer;

        // State
        private Vector3 initialPosition;
        private Vector3 orbitCenter;
        private float orbitAngle;
        private float floatOffset;
        private bool isCollected = false;
        private Transform playerTransform;

        public ItemData ItemData => itemData;
        public bool IsCollected => isCollected;
        public CollectibleBehavior BehaviorType => behaviorType;

        private void Start()
        {
            initialPosition = transform.position;
            orbitCenter = initialPosition;
            orbitAngle = Random.Range(0f, 360f);
            floatOffset = Random.Range(0f, Mathf.PI * 2f);

            playerTransform = ARSessionController.Instance?.GetCameraTransform();

            if (autoInitializeFromData && itemData != null)
            {
                ApplyItemData();
            }

            StartCoroutine(SpawnAnimation());
        }

        private void Update()
        {
            if (isCollected) return;

            switch (behaviorType)
            {
                case CollectibleBehavior.Standard:
                    UpdateStandardMovement();
                    break;
                case CollectibleBehavior.Orbit:
                    UpdateOrbitMovement();
                    break;
                case CollectibleBehavior.Magnetic:
                    UpdateMagneticMovement();
                    break;
                case CollectibleBehavior.Evasive:
                    UpdateEvasiveMovement();
                    break;
                case CollectibleBehavior.Stationary:
                    UpdateStationaryMovement();
                    break;
            }
        }

        private void ApplyItemData()
        {
            if (itemData == null) return;

            // Apply visual settings
            if (glowLight != null)
            {
                glowLight.color = itemData.glowColor;
            }

            // Apply animation settings
            floatAmplitude = itemData.floatAmplitude;
            floatSpeed = itemData.floatSpeed;
            rotationSpeed = itemData.rotationSpeed;

            // Determine behavior from rarity
            behaviorType = GetBehaviorForRarity(itemData.rarity);
        }

        private CollectibleBehavior GetBehaviorForRarity(ItemRarity rarity)
        {
            return rarity switch
            {
                ItemRarity.Common => CollectibleBehavior.Standard,
                ItemRarity.Uncommon => CollectibleBehavior.Orbit,
                ItemRarity.Rare => CollectibleBehavior.Magnetic,
                ItemRarity.Epic => CollectibleBehavior.Evasive,
                ItemRarity.Legendary => CollectibleBehavior.Stationary,
                _ => CollectibleBehavior.Standard
            };
        }

        #region Movement Types

        private void UpdateStandardMovement()
        {
            // Sine wave float
            float newY = initialPosition.y + Mathf.Sin((Time.time + floatOffset) * floatSpeed) * floatAmplitude;
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);

            // Rotation
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
        }

        private void UpdateOrbitMovement()
        {
            // Orbit around initial position
            orbitAngle += orbitSpeed * Time.deltaTime;
            float rad = orbitAngle * Mathf.Deg2Rad;

            Vector3 offset = new Vector3(
                Mathf.Cos(rad) * orbitRadius,
                Mathf.Sin((Time.time + floatOffset) * floatSpeed) * floatAmplitude,
                Mathf.Sin(rad) * orbitRadius
            );

            transform.position = orbitCenter + offset;
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
        }

        private void UpdateMagneticMovement()
        {
            // Standard float
            float newY = initialPosition.y + Mathf.Sin((Time.time + floatOffset) * floatSpeed) * floatAmplitude;
            Vector3 position = new Vector3(transform.position.x, newY, transform.position.z);

            // Magnetic pull towards player when close
            if (playerTransform != null)
            {
                float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
                if (distanceToPlayer < magneticRadius)
                {
                    float pullStrength = (1f - distanceToPlayer / magneticRadius) * magneticStrength * Time.deltaTime;
                    Vector3 directionToPlayer = (playerTransform.position - transform.position).normalized;
                    position += directionToPlayer * pullStrength;
                }
            }

            transform.position = position;
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
        }

        private void UpdateEvasiveMovement()
        {
            // Move away from player when they get close
            Vector3 position = initialPosition;

            if (playerTransform != null)
            {
                float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
                if (distanceToPlayer < 1.5f)
                {
                    Vector3 awayFromPlayer = (transform.position - playerTransform.position).normalized;
                    position += awayFromPlayer * 0.5f;
                }
            }

            // Float
            position.y += Mathf.Sin((Time.time + floatOffset) * floatSpeed) * floatAmplitude;
            transform.position = Vector3.Lerp(transform.position, position, Time.deltaTime * 5f);
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
        }

        private void UpdateStationaryMovement()
        {
            // Just rotate, no float
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
            transform.Rotate(Vector3.right, rotationSpeed * 0.5f * Time.deltaTime, Space.World);
        }

        #endregion

        /// <summary>
        /// Called when player taps this item
        /// </summary>
        public void OnTouch()
        {
            if (isCollected) return;
            StartCoroutine(Collect());
        }

        private IEnumerator Collect()
        {
            isCollected = true;

            // Chain reaction for special items
            if (spawnChainReaction)
            {
                TriggerChainReaction();
            }

            // Update game state
            int points = itemData?.pointValue ?? 100;
            GameManager.Instance?.CollectItem(points);

            // Effects
            PlayCollectionEffects();

            // Animation
            yield return StartCoroutine(CollectionAnimation());

            Destroy(gameObject);
        }

        private void TriggerChainReaction()
        {
            Collider[] nearby = Physics.OverlapSphere(transform.position, chainReactionRadius);
            int collected = 0;

            foreach (var collider in nearby)
            {
                var item = collider.GetComponent<CollectibleItemVariant>();
                if (item != null && !item.isCollected && item != this && collected < chainReactionCount)
                {
                    item.StartCoroutine(item.AutoCollect());
                    collected++;
                }
            }
        }

        private IEnumerator AutoCollect()
        {
            // Auto-collect triggered by chain reaction
            isCollected = true;
            int points = (itemData?.pointValue ?? 100) / 2; // Half points for chain collection
            GameManager.Instance?.CollectItem(points);
            PlayCollectionEffects();
            yield return StartCoroutine(CollectionAnimation());
            Destroy(gameObject);
        }

        private void PlayCollectionEffects()
        {
            // Particles
            if (itemData?.collectionEffect != null)
            {
                Instantiate(itemData.collectionEffect, transform.position, Quaternion.identity);
            }

            // Sound
            if (itemData?.collectionSound != null)
            {
                AudioSource.PlayClipAtPoint(itemData.collectionSound, transform.position);
            }
            else
            {
                AudioManager.Instance?.PlayCollectSound();
            }
        }

        private IEnumerator CollectionAnimation()
        {
            Vector3 startScale = transform.localScale;
            Vector3 startPosition = transform.position;
            float elapsed = 0f;

            while (elapsed < collectionDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / collectionDuration;

                // Scale down
                float scale = collectionCurve.Evaluate(t);
                transform.localScale = startScale * scale;

                // Rise up
                transform.position = startPosition + Vector3.up * t * 0.5f;

                // Spin fast
                transform.Rotate(Vector3.up, rotationSpeed * 5f * Time.deltaTime, Space.World);

                yield return null;
            }
        }

        private IEnumerator SpawnAnimation()
        {
            transform.localScale = Vector3.zero;
            float duration = 0.4f;
            float elapsed = 0f;

            AudioManager.Instance?.PlaySpawnSound();

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;

                // Elastic bounce effect
                float scale = Mathf.Sin(t * Mathf.PI * 0.5f);
                scale = Mathf.Lerp(0f, 1f, scale);

                transform.localScale = Vector3.one * scale;
                yield return null;
            }

            transform.localScale = Vector3.one;

            // Start ambient particles
            if (ambientParticles != null)
            {
                ambientParticles.Play();
            }
        }

        /// <summary>
        /// Initialize from ItemData
        /// </summary>
        public void Initialize(ItemData data)
        {
            itemData = data;
            ApplyItemData();
        }

        private void OnDrawGizmosSelected()
        {
            // Show behavior radius
            switch (behaviorType)
            {
                case CollectibleBehavior.Magnetic:
                    Gizmos.color = Color.cyan;
                    Gizmos.DrawWireSphere(transform.position, magneticRadius);
                    break;
                case CollectibleBehavior.Orbit:
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawWireSphere(orbitCenter, orbitRadius);
                    break;
            }

            // Chain reaction radius
            if (spawnChainReaction)
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawWireSphere(transform.position, chainReactionRadius);
            }
        }
    }

    public enum CollectibleBehavior
    {
        Standard,   // Simple float + rotate
        Orbit,      // Orbits around spawn point
        Magnetic,   // Pulls toward player when close
        Evasive,    // Moves away when player approaches
        Stationary  // No movement, just rotation
    }
}
