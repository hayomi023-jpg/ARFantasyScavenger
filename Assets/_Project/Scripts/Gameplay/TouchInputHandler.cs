using UnityEngine;
using UnityEngine.XR.ARFoundation;
using System.Collections.Generic;
using ARFantasy.Gameplay;

namespace ARFantasy.Gameplay
{
    /// <summary>
    /// Handles touch input for interacting with AR objects
    /// </summary>
    public class TouchInputHandler : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private LayerMask interactableLayers;
        [SerializeField] private float maxRaycastDistance = 10f;
        [SerializeField] private bool requireTap = true; // false = continuous touch
        [SerializeField] private float touchCooldown = 0.2f;

        [Header("Visual Feedback")]
        [SerializeField] private bool showDebugRay = false;

        private ARRaycastManager arRaycastManager;
        private Camera arCamera;
        private float lastTouchTime = 0f;

        private void Start()
        {
            arRaycastManager = ARSessionController.Instance?.RaycastManager;
            arCamera = ARSessionController.Instance?.SessionOrigin?.camera;

            if (arCamera == null)
            {
                arCamera = Camera.main;
            }
        }

        private void Update()
        {
            if (requireTap)
            {
                HandleTapInput();
            }
            else
            {
                HandleContinuousInput();
            }
        }

        private void HandleTapInput()
        {
            // Check for touch begin
            if (Input.touchCount == 0) return;

            Touch touch = Input.GetTouch(0);

            // Only process on touch began (tap)
            if (touch.phase != TouchPhase.Began) return;

            // Check cooldown
            if (Time.time - lastTouchTime < touchCooldown) return;

            lastTouchTime = Time.time;

            // Get touch position
            Vector2 touchPosition = touch.position;

            // Try to interact with objects
            TryInteractWithObject(touchPosition);
        }

        private void HandleContinuousInput()
        {
            if (Input.touchCount == 0) return;

            Touch touch = Input.GetTouch(0);
            Vector2 touchPosition = touch.position;

            if (Time.time - lastTouchTime >= touchCooldown)
            {
                lastTouchTime = Time.time;
                TryInteractWithObject(touchPosition);
            }
        }

        private void TryInteractWithObject(Vector2 screenPosition)
        {
            // Cast ray from touch position
            Ray ray = arCamera.ScreenPointToRay(screenPosition);

            if (showDebugRay)
            {
                Debug.DrawRay(ray.origin, ray.direction * maxRaycastDistance, Color.red, 1f);
            }

            // Check for physics raycast first (3D objects)
            if (Physics.Raycast(ray, out RaycastHit hit, maxRaycastDistance, interactableLayers))
            {
                ProcessHit(hit.collider.gameObject, hit.point);
                return;
            }

            // If no physics hit, try AR raycast for placement
            List<ARRaycastHit> arHits = new List<ARRaycastHit>();
            if (arRaycastManager != null && arRaycastManager.Raycast(screenPosition, arHits))
            {
                // We hit a plane but no object - could provide feedback here
                if (showDebugRay)
                {
                    Debug.Log("Hit AR plane at: " + arHits[0].pose.position);
                }
            }
        }

        private void ProcessHit(GameObject hitObject, Vector3 hitPoint)
        {
            // Check if we hit a collectible
            CollectibleItem collectible = hitObject.GetComponent<CollectibleItem>();
            if (collectible != null)
            {
                collectible.OnTouch();

                if (showDebugRay)
                {
                    Debug.Log($"Collected: {collectible.ItemName}");
                }
                return;
            }

            // Check for other interactable components
            IInteractable interactable = hitObject.GetComponent<IInteractable>();
            if (interactable != null)
            {
                interactable.OnInteract();
            }
        }

        /// <summary>
        /// Set whether input requires a tap or continuous touch
        /// </summary>
        public void SetRequireTap(bool require)
        {
            requireTap = require;
        }

        /// <summary>
        /// Set which layers are interactable
        /// </summary>
        public void SetInteractableLayers(LayerMask layers)
        {
            interactableLayers = layers;
        }
    }

    /// <summary>
    /// Interface for interactable objects
    /// </summary>
    public interface IInteractable
    {
        void OnInteract();
        void OnHoverEnter();
        void OnHoverExit();
    }
}
