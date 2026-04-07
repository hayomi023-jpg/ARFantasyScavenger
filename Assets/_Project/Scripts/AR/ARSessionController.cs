using UnityEngine;
using UnityEngine.XR.ARFoundation;

namespace ARFantasy.AR
{
    public class ARSessionController : MonoBehaviour
    {
        [Header("AR Components")]
        [SerializeField] private ARSession arSession;
        [SerializeField] private ARSessionOrigin arSessionOrigin;
        [SerializeField] private ARPlaneManager arPlaneManager;
        [SerializeField] private ARRaycastManager arRaycastManager;

        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = true;

        public static ARSessionController Instance { get; private set; }
        public ARSessionOrigin SessionOrigin => arSessionOrigin;
        public ARPlaneManager PlaneManager => arPlaneManager;
        public ARRaycastManager RaycastManager => arRaycastManager;

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
            // Subscribe to plane detection events
            if (arPlaneManager != null)
            {
                arPlaneManager.planesChanged += OnPlanesChanged;
            }

            // Start in scanning mode
            SetPlaneDetectionEnabled(true);

            if (showDebugLogs)
            {
                Debug.Log("AR Session Controller initialized");
            }
        }

        private void OnDestroy()
        {
            if (arPlaneManager != null)
            {
                arPlaneManager.planesChanged -= OnPlanesChanged;
            }
        }

        private void OnPlanesChanged(ARPlanesChangedEventArgs args)
        {
            if (args.added.Count > 0 && showDebugLogs)
            {
                Debug.Log($"New planes detected: {args.added.Count}");
            }
        }

        public void SetPlaneDetectionEnabled(bool enabled)
        {
            if (arPlaneManager != null)
            {
                arPlaneManager.enabled = enabled;
                arPlaneManager.requestedDetectionMode = enabled
                    ? UnityEngine.XR.ARSubsystems.PlaneDetection.Horizontal
                    : UnityEngine.XR.ARSubsystems.PlaneDetection.None;
            }
        }

        public void ResetSession()
        {
            if (arSession != null)
            {
                arSession.Reset();
            }
        }

        public bool IsSessionReady()
        {
            return arSession != null && arSession.state == ARSessionState.SessionTracking;
        }

        /// <summary>
        /// Gets the camera transform for AR positioning
        /// </summary>
        public Transform GetCameraTransform()
        {
            return arSessionOrigin?.cameraTransform;
        }

        /// <summary>
        /// Gets the current camera position
        /// </summary>
        public Vector3 GetCameraPosition()
        {
            return arSessionOrigin?.cameraTransform.position ?? Vector3.zero;
        }
    }
}
