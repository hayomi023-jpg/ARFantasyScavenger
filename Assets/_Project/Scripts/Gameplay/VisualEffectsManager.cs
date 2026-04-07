using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ARFantasy.Core;
using ARFantasy.Data;

namespace ARFantasy.Gameplay
{
    /// <summary>
    /// Manages visual effects for the game - spawn, collection, ambient effects
    /// </summary>
    public class VisualEffectsManager : MonoBehaviour
    {
        [Header("Collection Effects")]
        [SerializeField] private ParticleSystem collectionBurstPrefab;
        [SerializeField] private ParticleSystem collectionSparklesPrefab;
        [SerializeField] private ParticleSystem rareCollectionEffectPrefab;
        [SerializeField] private ParticleSystem legendaryCollectionEffectPrefab;

        [Header("Spawn Effects")]
        [SerializeField] private ParticleSystem spawnPoofPrefab;
        [SerializeField] private ParticleSystem spawnRingsPrefab;
        [SerializeField] private float spawnEffectScale = 1f;

        [Header("Screen Effects")]
        [SerializeField] private Canvas screenEffectsCanvas;
        [SerializeField] private Animator screenFlashAnimator;
        [SerializeField] private Animator screenShakeAnimator;
        [SerializeField] private ParticleSystem screenSparklesPrefab;

        [Header("Camera Effects")]
        [SerializeField] private bool enableCameraShake = true;
        [SerializeField] private float shakeDuration = 0.2f;
        [SerializeField] private float shakeMagnitude = 0.1f;

        [Header("Trail Effects")]
        [SerializeField] private TrailRenderer trailPrefab;
        [SerializeField] private float trailDuration = 1f;

        [Header("Floating Text")]
        [SerializeField] private GameObject floatingTextPrefab;
        [SerializeField] private Transform floatingTextParent;

        // Object pools
        private Queue<ParticleSystem> collectionEffectPool = new Queue<ParticleSystem>();
        private Queue<ParticleSystem> spawnEffectPool = new Queue<ParticleSystem>();

        public static VisualEffectsManager Instance { get; private set; }
        private Camera arCamera;

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
            arCamera = ARSessionController.Instance?.SessionOrigin?.camera;

            // Initialize pools
            InitializePools();

            // Subscribe to events
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
        }

        private void InitializePools()
        {
            // Pre-warm pools
            for (int i = 0; i < 5; i++)
            {
                if (collectionBurstPrefab != null)
                {
                    var effect = Instantiate(collectionBurstPrefab, transform);
                    effect.gameObject.SetActive(false);
                    collectionEffectPool.Enqueue(effect);
                }
            }
        }

        private void OnItemCollected(int collected, int total)
        {
            // Could trigger screen-wide effects here
        }

        #region Collection Effects

        /// <summary>
        /// Play collection effect at position based on item rarity
        /// </summary>
        public void PlayCollectionEffect(Vector3 position, ItemRarity rarity, int points)
        {
            // Select effect based on rarity
            ParticleSystem effectPrefab = rarity switch
            {
                ItemRarity.Common => collectionBurstPrefab,
                ItemRarity.Uncommon => collectionSparklesPrefab,
                ItemRarity.Rare => rareCollectionEffectPrefab,
                ItemRarity.Epic => rareCollectionEffectPrefab,
                ItemRarity.Legendary => legendaryCollectionEffectPrefab,
                _ => collectionBurstPrefab
            };

            if (effectPrefab != null)
            {
                PlayParticleEffect(effectPrefab, position);
            }

            // Screen effects for rare+ items
            if (rarity >= ItemRarity.Rare)
            {
                StartCoroutine(PlayScreenFlash(rarity));
                StartCoroutine(PlayScreenShake());
            }

            // Floating text
            ShowFloatingText(position, $"+{points}", GetRarityColor(rarity));
        }

        /// <summary>
        /// Play effect for special item types
        /// </summary>
        public void PlaySpecialCollectionEffect(Vector3 position, string effectName)
        {
            // Could load different effects by name
            PlayParticleEffect(collectionSparklesPrefab, position);
            StartCoroutine(PlayScreenShake(0.3f, 0.15f));
        }

        private IEnumerator PlayScreenFlash(ItemRarity rarity)
        {
            if (screenFlashAnimator == null) yield break;

            string triggerName = rarity switch
            {
                ItemRarity.Rare => "FlashRare",
                ItemRarity.Epic => "FlashEpic",
                ItemRarity.Legendary => "FlashLegendary",
                _ => "Flash"
            };

            screenFlashAnimator.SetTrigger(triggerName);
            yield return new WaitForSeconds(0.5f);
        }

        private IEnumerator PlayScreenShake(float duration = -1, float magnitude = -1)
        {
            if (!enableCameraShake || arCamera == null) yield break;

            float shakeTime = duration > 0 ? duration : shakeDuration;
            float shakeAmount = magnitude > 0 ? magnitude : shakeMagnitude;

            Vector3 originalPosition = arCamera.transform.localPosition;
            float elapsed = 0f;

            while (elapsed < shakeTime)
            {
                elapsed += Time.deltaTime;

                // Random shake
                float x = Random.Range(-1f, 1f) * shakeAmount;
                float y = Random.Range(-1f, 1f) * shakeAmount;

                arCamera.transform.localPosition = originalPosition + new Vector3(x, y, 0);

                yield return null;
            }

            arCamera.transform.localPosition = originalPosition;
        }

        #endregion

        #region Spawn Effects

        /// <summary>
        /// Play spawn effect when item appears
        /// </summary>
        public void PlaySpawnEffect(Vector3 position, ItemRarity rarity)
        {
            // Base poof
            if (spawnPoofPrefab != null)
            {
                PlayParticleEffect(spawnPoofPrefab, position);
            }

            // Rings for higher rarities
            if (rarity >= ItemRarity.Uncommon && spawnRingsPrefab != null)
            {
                PlayParticleEffect(spawnRingsPrefab, position);
            }

            // Light flash for rare+
            if (rarity >= ItemRarity.Rare)
            {
                StartCoroutine(SpawnLightFlash(position));
            }
        }

        private IEnumerator SpawnLightFlash(Vector3 position)
        {
            // Create temporary light
            GameObject lightObj = new GameObject("SpawnFlash");
            lightObj.transform.position = position + Vector3.up * 0.5f;

            Light light = lightObj.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = Color.cyan;
            light.intensity = 2f;
            light.range = 3f;

            // Fade out
            float duration = 0.5f;
            float elapsed = 0f;
            float startIntensity = light.intensity;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                light.intensity = Mathf.Lerp(startIntensity, 0, elapsed / duration);
                yield return null;
            }

            Destroy(lightObj);
        }

        #endregion

        #region Ambient Effects

        /// <summary>
        /// Create ambient particle field around an item
        /// </summary>
        public ParticleSystem CreateAmbientEffect(Vector3 position, ItemRarity rarity)
        {
            if (screenSparklesPrefab == null) return null;

            ParticleSystem effect = Instantiate(screenSparklesPrefab, position, Quaternion.identity);

            // Customize based on rarity
            var main = effect.main;
            main.startColor = GetRarityColor(rarity);

            return effect;
        }

        /// <summary>
        /// Create trail following the player's view
        /// </summary>
        public void CreateCollectionTrail(Vector3 startPosition, Vector3 endPosition)
        {
            if (trailPrefab == null) return;

            GameObject trailObj = new GameObject("CollectionTrail");
            trailObj.transform.position = startPosition;

            TrailRenderer trail = Instantiate(trailPrefab, trailObj.transform);
            trail.transform.position = startPosition;

            StartCoroutine(AnimateTrail(trailObj, trail, startPosition, endPosition));
        }

        private IEnumerator AnimateTrail(GameObject trailObj, TrailRenderer trail, Vector3 start, Vector3 end)
        {
            float duration = 0.5f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;

                trailObj.transform.position = Vector3.Lerp(start, end, t);

                yield return null;
            }

            yield return new WaitForSeconds(trail.time);
            Destroy(trailObj);
        }

        #endregion

        #region Utility Methods

        private ParticleSystem PlayParticleEffect(ParticleSystem prefab, Vector3 position)
        {
            if (prefab == null) return null;

            ParticleSystem effect = Instantiate(prefab, position, Quaternion.identity);
            effect.Play();

            // Auto-destroy after duration
            Destroy(effect.gameObject, effect.main.duration + effect.main.startLifetime.constantMax);

            return effect;
        }

        private void ShowFloatingText(Vector3 worldPosition, string text, Color color)
        {
            if (floatingTextPrefab == null) return;

            // Convert world position to screen position
            Vector3 screenPos = arCamera.WorldToScreenPoint(worldPosition);

            GameObject textObj = Instantiate(floatingTextPrefab, floatingTextParent);
            textObj.transform.position = screenPos;

            TextMeshProUGUI tmp = textObj.GetComponent<TextMeshProUGUI>();
            if (tmp != null)
            {
                tmp.text = text;
                tmp.color = color;
            }

            // Animate and destroy
            StartCoroutine(AnimateFloatingText(textObj));
        }

        private IEnumerator AnimateFloatingText(GameObject textObj)
        {
            Vector3 startPos = textObj.transform.position;
            Vector3 endPos = startPos + Vector3.up * 100f;

            float duration = 1f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;

                textObj.transform.position = Vector3.Lerp(startPos, endPos, t);

                // Fade out
                CanvasGroup cg = textObj.GetComponent<CanvasGroup>();
                if (cg != null)
                {
                    cg.alpha = 1f - t;
                }

                yield return null;
            }

            Destroy(textObj);
        }

        private Color GetRarityColor(ItemRarity rarity)
        {
            return rarity switch
            {
                ItemRarity.Common => Color.gray,
                ItemRarity.Uncommon => new Color(0.2f, 0.8f, 0.2f), // Green
                ItemRarity.Rare => new Color(0.2f, 0.4f, 1f), // Blue
                ItemRarity.Epic => new Color(0.8f, 0.2f, 0.8f), // Purple
                ItemRarity.Legendary => new Color(1f, 0.8f, 0.2f), // Gold
                _ => Color.white
            };
        }

        #endregion

        #region Public API

        /// <summary>
        /// Quick spawn effect at position
        /// </summary>
        public void SpawnEffect(Vector3 position)
        {
            PlaySpawnEffect(position, ItemRarity.Common);
        }

        /// <summary>
        /// Full collection effect
        /// </summary>
        public void CollectionEffect(Vector3 position, int points)
        {
            PlayCollectionEffect(position, ItemRarity.Common, points);
        }

        #endregion
    }
}
