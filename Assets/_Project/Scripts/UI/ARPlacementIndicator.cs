using UnityEngine;
using UnityEngine.XR.ARFoundation;
using System.Collections.Generic;

namespace ARFantasy.UI
{
    /// <summary>
    /// Visual indicator for AR plane placement - shows where items can spawn
    /// </summary>
    public class ARPlacementIndicator : MonoBehaviour
    {
        [Header("Visuals")]
        [SerializeField] private GameObject placementVisual;
        [SerializeField] private GameObject validPlacementVisual;
        [SerializeField] private GameObject invalidPlacementVisual;

        [Header("Settings")]
        [SerializeField] private bool followTouchPosition = true;
        [SerializeField] private bool snapToPlane = true;
        [SerializeField] private float visualHeightOffset = 0.01f;
        [SerializeField] private float movementSmoothing = 10f;

        [Header("Animation")]
        [SerializeField] private float pulseSpeed = 2f;
        [SerializeField] private float pulseScale = 0.1f;
        [SerializeField] private float rotationSpeed = 50f;

        private ARRaycastManager raycastManager;
        private Transform arCameraTransform;
        private Vector3 targetPosition;
        private bool isPlacementValid = false;
        private Vector3 initialScale;

        public bool IsPlacementValid => isPlacementValid;
        public Vector3 CurrentPlacementPosition => transform.position;

        private void Start()
        {
            raycastManager = ARSessionController.Instance?.RaycastManager;
            arCameraTransform = ARSessionController.Instance?.GetCameraTransform();

            // Store initial scale for pulse animation
            if (placementVisual != null)
            {
                initialScale = placementVisual.transform.localScale;
            }

            // Start hidden
            SetVisualActive(false);
        }

        private void Update()
        {
            if (followTouchPosition && Input.touchCount > 0)
            {
                UpdatePlacementPosition(Input.GetTouch(0).position);
            }

            // Smooth movement
            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * movementSmoothing);

            // Pulse animation
            AnimateVisual();
        }

        private void UpdatePlacementPosition(Vector2 screenPosition)
        {
            List<ARRaycastHit> hits = new List<ARRaycastHit>();

            if (raycastManager != null && raycastManager.Raycast(screenPosition, hits))
            {
                Pose hitPose = hits[0].pose;
                targetPosition = hitPose.position + Vector3.up * visualHeightOffset;

                isPlacementValid = true;
                SetVisualActive(true);
                UpdateVisualState(true);

                // Make indicator face camera
                if (arCameraTransform != null)
                {
                    Vector3 lookPos = transform.position - arCameraTransform.position;
                    lookPos.y = 0;
                    if (lookPos != Vector3.zero)
                    {
                        transform.rotation = Quaternion.LookRotation(lookPos);
                    }
                }
            }
            else
            {
                isPlacementValid = false;
                UpdateVisualState(false);
            }
        }

        private void AnimateVisual()
        {
            if (placementVisual == null) return;

            // Pulse scale
            float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseScale;
            placementVisual.transform.localScale = initialScale * pulse;

            // Rotate
            placementVisual.transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.Self);
        }

        private void UpdateVisualState(bool valid)
        {
            if (validPlacementVisual != null)
            {
                validPlacementVisual.SetActive(valid);
            }
            if (invalidPlacementVisual != null)
            {
                invalidPlacementVisual.SetActive(!valid);
            }
        }

        private void SetVisualActive(bool active)
        {
            if (placementVisual != null)
            {
                placementVisual.SetActive(active);
            }
        }

        /// <summary>
        /// Enable the placement indicator
        /// </summary>
        public void ShowIndicator()
        {
            SetVisualActive(true);
        }

        /// <summary>
        /// Disable the placement indicator
        /// </summary>
        public void HideIndicator()
        {
            SetVisualActive(false);
        }

        /// <summary>
        /// Set the indicator position directly (e.g., from detected planes)
        /// </summary>
        public void SetPosition(Vector3 position)
        {
            targetPosition = position + Vector3.up * visualHeightOffset;
        }

        /// <summary>
        /// Check if current placement position is valid for spawning
        /// </summary>
        public bool CheckValidPlacement(Vector2 screenPosition)
        {
            List<ARRaycastHit> hits = new List<ARRaycastHit>();
            return raycastManager != null && raycastManager.Raycast(screenPosition, hits);
        }
    }
}
