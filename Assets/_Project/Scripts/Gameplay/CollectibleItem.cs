using UnityEngine;
using ARFantasy.Core;

namespace ARFantasy.Gameplay
{
    /// <summary>
    /// Handles collectible item behavior - animations, collection, and visual effects
    /// </summary>
    public class CollectibleItem : MonoBehaviour
    {
        [Header("Item Settings")]
        [SerializeField] private string itemName = "Mystic Crystal";
        [SerializeField] private int pointValue = 100;
        [SerializeField] private float floatAmplitude = 0.1f;
        [SerializeField] private float floatSpeed = 2f;
        [SerializeField] private float rotationSpeed = 30f;

        [Header("Collection Settings")]
        [SerializeField] private float collectionAnimationDuration = 0.5f;
        [SerializeField] private AnimationCurve collectionScaleCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);

        [Header("Visual Effects")]
        [SerializeField] private ParticleSystem collectParticles;
        [SerializeField] private Light glowLight;
        [SerializeField] private Renderer itemRenderer;

        [Header("Audio")]
        [SerializeField] private AudioClip collectSound;

        private Vector3 initialPosition;
        private bool isCollected = false;
        private float floatOffset;

        public string ItemName => itemName;
        public int PointValue => pointValue;
        public bool IsCollected => isCollected;

        private void Start()
        {
            initialPosition = transform.position;
            floatOffset = Random.Range(0f, Mathf.PI * 2f);

            // Ensure we have a renderer reference
            if (itemRenderer == null)
            {
                itemRenderer = GetComponentInChildren<Renderer>();
            }

            // Start with a spawn animation
            StartCoroutine(SpawnAnimation());
        }

        private void Update()
        {
            if (isCollected) return;

            // Floating animation
            float newY = initialPosition.y + Mathf.Sin((Time.time + floatOffset) * floatSpeed) * floatAmplitude;
            transform.position = new Vector3(initialPosition.x, newY, initialPosition.z);

            // Rotation animation
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
        }

        /// <summary>
        /// Called when player taps on this item
        /// </summary>        public void OnTouch()
        {
            if (isCollected) return;
            Collect();
        }

        private void Collect()
        {
            isCollected = true;

            // Update game state
            GameManager.Instance?.CollectItem(pointValue);

            // Play effects
            PlayCollectionEffects();

            // Animate and destroy
            StartCoroutine(CollectionAnimation());
        }

        private void PlayCollectionEffects()
        {
            // Spawn particles
            if (collectParticles != null)
            {
                ParticleSystem particles = Instantiate(collectParticles, transform.position, Quaternion.identity);
                particles.Play();
                Destroy(particles.gameObject, particles.main.duration + particles.main.startLifetime.constantMax);
            }

            // Play sound
            AudioManager.Instance?.PlayCollectSound();
        }

        private System.Collections.IEnumerator CollectionAnimation()
        {
            float elapsed = 0f;
            Vector3 startScale = transform.localScale;

            while (elapsed < collectionAnimationDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / collectionAnimationDuration;
                float scale = collectionScaleCurve.Evaluate(t);
                transform.localScale = startScale * scale;

                // Spin faster while collecting
                transform.Rotate(Vector3.up, rotationSpeed * 3f * Time.deltaTime, Space.World);

                yield return null;
            }

            // Disable collider immediately so it can't be collected again
            if (TryGetComponent(out Collider col))
            {
                col.enabled = false;
            }

            Destroy(gameObject);
        }

        private System.Collections.IEnumerator SpawnAnimation()
        {
            transform.localScale = Vector3.zero;
            float duration = 0.3f;
            float elapsed = 0f;

            AudioManager.Instance?.PlaySpawnSound();

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                // Bounce effect
                float scale = Mathf.Sin(t * Mathf.PI * 0.5f);
                transform.localScale = Vector3.one * scale;
                yield return null;
            }

            transform.localScale = Vector3.one;
        }

        /// <summary>
        /// Set up the item with custom data
        /// </summary>
        public void Initialize(string name, int points)
        {
            itemName = name;
            pointValue = points;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, 0.1f);

            // Show float range
            if (Application.isPlaying)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(
                    new Vector3(transform.position.x, initialPosition.y - floatAmplitude, transform.position.z),
                    new Vector3(transform.position.x, initialPosition.y + floatAmplitude, transform.position.z)
                );
            }
        }
    }
}
