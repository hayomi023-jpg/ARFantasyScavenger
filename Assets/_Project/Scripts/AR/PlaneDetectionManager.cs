using UnityEngine;
using UnityEngine.XR.ARFoundation;
using System.Collections.Generic;

namespace ARFantasy.AR
{
    /// <summary>
    /// Manages plane detection feedback and notifies when suitable planes are available
    /// </summary>
    public class PlaneDetectionManager : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float minimumPlaneSize = 0.5f;
        [SerializeField] private int minimumPlaneCount = 1;
        [SerializeField] private float detectionTimeout = 30f;

        [Header("Events")]
        public bool PlanesDetected { get; private set; } = false;

        public delegate void PlanesDetectedEvent();
        public event PlanesDetectedEvent OnPlanesDetected;

        private ARPlaneManager arPlaneManager;
        private List<ARPlane> detectedPlanes = new List<ARPlane>();
        private float detectionTimer = 0f;
        private bool isScanning = false;

        private void Start()
        {
            arPlaneManager = ARSessionController.Instance?.PlaneManager;
            if (arPlaneManager != null)
            {
                arPlaneManager.planesChanged += OnPlanesChanged;
            }
        }

        private void OnDestroy()
        {
            if (arPlaneManager != null)
            {
                arPlaneManager.planesChanged -= OnPlanesChanged;
            }
        }

        private void Update()
        {
            if (isScanning && !PlanesDetected)
            {
                detectionTimer += Time.deltaTime;

                if (detectionTimer >= detectionTimeout)
                {
                    // Timeout - could trigger alternative placement method
                    Debug.Log("Plane detection timeout reached");
                }
            }
        }

        private void OnPlanesChanged(ARPlanesChangedEventArgs args)
        {
            foreach (var plane in args.added)
            {
                if (!detectedPlanes.Contains(plane))
                {
                    detectedPlanes.Add(plane);
                }
            }

            foreach (var plane in args.removed)
            {
                detectedPlanes.Remove(plane);
            }

            CheckForSuitablePlanes();
        }

        private void CheckForSuitablePlanes()
        {
            if (PlanesDetected) return;

            int suitablePlaneCount = 0;
            foreach (var plane in detectedPlanes)
            {
                Vector2 size = plane.size;
                if (size.x >= minimumPlaneSize && size.y >= minimumPlaneSize)
                {
                    suitablePlaneCount++;
                }
            }

            if (suitablePlaneCount >= minimumPlaneCount)
            {
                PlanesDetected = true;
                OnPlanesDetected?.Invoke();
            }
        }

        public void StartScanning()
        {
            isScanning = true;
            detectionTimer = 0f;
            PlanesDetected = false;
            detectedPlanes.Clear();

            // Enable plane detection
            if (arPlaneManager != null)
            {
                arPlaneManager.enabled = true;
            }
        }

        public void StopScanning()
        {
            isScanning = false;

            // Optionally hide plane visuals
            foreach (var plane in detectedPlanes)
            {
                if (plane.TryGetComponent<MeshRenderer>(out var renderer))
                {
                    renderer.enabled = false;
                }
            }
        }

        /// <summary>
        /// Returns a random suitable plane for object spawning
        /// </summary>
        public ARPlane GetRandomSuitablePlane()
        {
            List<ARPlane> suitablePlanes = new List<ARPlane>();
            foreach (var plane in detectedPlanes)
            {
                Vector2 size = plane.size;
                if (size.x >= minimumPlaneSize && size.y >= minimumPlaneSize)
                {
                    suitablePlanes.Add(plane);
                }
            }

            if (suitablePlanes.Count > 0)
            {
                return suitablePlanes[Random.Range(0, suitablePlanes.Count)];
            }
            return null;
        }

        /// <summary>
        /// Gets all detected planes
        /// </summary>
        public List<ARPlane> GetAllPlanes()
        {
            return new List<ARPlane>(detectedPlanes);
        }
    }
}
