#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEditor;
using UnityEditor.SceneManagement;
using ARFantasy.Core;
using ARFantasy.AR;
using ARFantasy.Gameplay;
using ARFantasy.UI;
using ARFantasy.Data;

namespace ARFantasy.Setup
{
    /// <summary>
    /// Editor utility to set up the AR Hunt scene automatically.
    /// Run via Window → AR Fantasy → Setup Scene
    /// </summary>
    public class SceneSetup : EditorWindow
    {
        [MenuItem("Window/AR Fantasy/Setup Scene")]
        public static void ShowWindow()
        {
            GetWindow<SceneSetup>("AR Fantasy Setup");
        }

        [MenuItem("GameObject/AR Fantasy/Setup AR Hunt Scene", false, 10)]
        public static void SetupScene()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;

            CreateScene();
        }

        public static void CreateScene()
        {
            // Create new scene
            EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            // Create root managers object
            GameObject managersRoot = CreateManagers();
            GameObject uiRoot = CreateUI();
            CreateARVisuals();

            // Save scene
            string scenePath = "Assets/_Project/Scenes/ARHunt.unity";
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), scenePath);

            Selection.activeGameObject = managersRoot;
            Debug.Log("AR Fantasy scene created successfully!");
        }

        private static GameObject CreateManagers()
        {
            // Create root
            GameObject managers = new GameObject("=== MANAGERS ===");
            Undo.RegisterCreatedObjectUndo(managers, "Create Managers");

            // GameManager
            GameObject gameManager = CreateManagerObject(managers, "GameManager", typeof(GameManager));

            // AudioManager
            GameObject audioManager = CreateManagerObject(managers, "AudioManager", typeof(AudioManager));

            // PlayerProgressManager
            GameObject progressManager = CreateManagerObject(managers, "PlayerProgressManager", typeof(PlayerProgressManager));

            // ARSessionController
            GameObject arSession = CreateManagerObject(managers, "ARSessionController", typeof(ARSessionController));

            // PlaneDetectionManager
            GameObject planeManager = CreateManagerObject(managers, "PlaneDetectionManager", typeof(PlaneDetectionManager));

            // ItemSpawner
            GameObject itemSpawner = CreateManagerObject(managers, "ItemSpawner", typeof(ItemSpawner));

            // AdvancedItemSpawner
            GameObject advancedSpawner = CreateManagerObject(managers, "AdvancedItemSpawner", typeof(AdvancedItemSpawner));

            // HuntManager
            GameObject huntManager = CreateManagerObject(managers, "HuntManager", typeof(HuntManager));

            // TouchInputHandler
            GameObject touchHandler = CreateManagerObject(managers, "TouchInputHandler", typeof(TouchInputHandler));

            // HuntConfigurationManager
            GameObject configManager = CreateManagerObject(managers, "HuntConfigurationManager", typeof(HuntConfigurationManager));

            return managers;
        }

        private static GameObject CreateManagerObject(GameObject parent, string name, System.Type componentType)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent.transform);
            obj.AddComponent(componentType);
            Undo.RegisterCreatedObjectUndo(obj, "Create " + name);
            return obj;
        }

        private static GameObject CreateUI()
        {
            // Create Canvas
            GameObject canvas = new GameObject("Canvas");
            Canvas canvasComponent = canvas.AddComponent<Canvas>();
            canvasComponent.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.AddComponent<CanvasScaler>();
            canvas.AddComponent<GraphicRaycaster>();
            Undo.RegisterCreatedObjectUndo(canvas, "Create Canvas");

            // UIManager
            GameObject uiManagerObj = new GameObject("UIManager");
            uiManagerObj.transform.SetParent(canvas.transform);
            UIManager uiManager = uiManagerObj.AddComponent<UIManager>();
            Undo.RegisterCreatedObjectUndo(uiManagerObj, "Create UIManager");

            // Create panels
            CreatePanel(canvas.transform, "MainMenuPanel", true);
            CreatePanel(canvas.transform, "ScanningPanel", false);
            CreatePanel(canvas.transform, "HUDPanel", false);
            CreatePanel(canvas.transform, "PausePanel", false);
            CreatePanel(canvas.transform, "WinPanel", false);
            CreatePanel(canvas.transform, "HuntSelectionPanel", false);
            CreatePanel(canvas.transform, "CollectionJournalPanel", false);

            return canvas;
        }

        private static void CreatePanel(Transform parent, string name, bool initiallyActive)
        {
            GameObject panel = new GameObject(name);
            panel.transform.SetParent(parent);
            panel.SetActive(initiallyActive);

            RectTransform rect = panel.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = Vector2.zero;

            Undo.RegisterCreatedObjectUndo(panel, "Create " + name);
        }

        private static void CreateARVisuals()
        {
            GameObject arVisuals = new GameObject("=== AR VISUALS ===");
            Undo.RegisterCreatedObjectUndo(arVisuals, "Create AR Visuals");

            GameObject placementIndicator = new GameObject("PlacementIndicator");
            placementIndicator.transform.SetParent(arVisuals.transform);
            placementIndicator.AddComponent<ARPlacementIndicator>();
            Undo.RegisterCreatedObjectUndo(placementIndicator, "Create Placement Indicator");
        }

        private static void CreateXR()
        {
            // XR Origin
            GameObject xrOrigin = new GameObject("XR Origin");
            xrOrigin.AddComponent<XROrigin>();
            xrOrigin.AddComponent<ARPlaneManager>();
            xrOrigin.AddComponent<ARRaycastManager>();
            Undo.RegisterCreatedObjectUndo(xrOrigin, "Create XR Origin");

            // Camera Offset
            GameObject cameraOffset = new GameObject("Camera Offset");
            cameraOffset.transform.SetParent(xrOrigin.transform);
            Undo.RegisterCreatedObjectUndo(cameraOffset, "Create Camera Offset");

            // AR Camera
            GameObject arCamera = new GameObject("Main Camera");
            arCamera.transform.SetParent(cameraOffset.transform);
            Camera camera = arCamera.AddComponent<Camera>();
            camera.tag = "MainCamera";
            arCamera.AddComponent<ARCameraBackground>();
            Undo.RegisterCreatedObjectUndo(arCamera, "Create AR Camera");

            // AR Session
            GameObject arSession = new GameObject("AR Session");
            arSession.AddComponent<ARSession>();
            Undo.RegisterCreatedObjectUndo(arSession, "Create AR Session");
        }

        protected void OnGUI()
        {
            GUILayout.Label("AR Fantasy Scene Setup", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            EditorGUILayout.HelpBox(
                "This will create a new AR Hunt scene with all managers and UI setup.\n\n" +
                "Steps:\n" +
                "1. Click 'Create Scene' below\n" +
                "2. Create/assign ScriptableObjects in Assets/_Project/ScriptableObjects/\n" +
                "3. Wire up references in the Inspector\n" +
                "4. Build to device",
                MessageType.Info);

            EditorGUILayout.Space();

            if (GUILayout.Button("Create AR Hunt Scene", GUILayout.Height(40)))
            {
                CreateScene();
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Next Steps", EditorStyles.boldLabel);

            if (GUILayout.Button("Create Sample ItemData ScriptableObjects"))
            {
                CreateSampleScriptableObjects.CreateItemDataAssets();
            }

            if (GUILayout.Button("Create Sample HuntConfig ScriptableObjects"))
            {
                CreateSampleScriptableObjects.CreateHuntConfigAssets();
            }

            if (GUILayout.Button("Create ItemDatabase ScriptableObject"))
            {
                CreateSampleScriptableObjects.CreateItemDatabaseAsset();
            }
        }
    }
}
#endif
