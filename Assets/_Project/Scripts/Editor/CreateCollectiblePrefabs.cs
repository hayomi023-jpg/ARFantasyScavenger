#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using ARFantasy.Gameplay;
using ARFantasy.Data;

namespace ARFantasy.Setup
{
    /// <summary>
    /// Creates the collectible crystal prefab and material
    /// </summary>
    public class CreateCollectiblePrefabs
    {
        [MenuItem("Window/AR Fantasy/Create Crystal Prefab")]
        public static void CreateCrystalPrefab()
        {
            // Create prefabs directory
            if (!AssetDatabase.IsValidFolder("Assets/_Project/Prefabs"))
            {
                AssetDatabase.CreateFolder("Assets/_Project", "Prefabs");
            }
            if (!AssetDatabase.IsValidFolder("Assets/_Project/Materials"))
            {
                AssetDatabase.CreateFolder("Assets/_Project", "Materials");
            }

            // Create material
            Material crystalMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            crystalMat.name = "Crystal_Glow";
            crystalMat.color = new Color(0f, 0.5f, 0.5f, 1f);

            // Enable emission
            crystalMat.EnableKeyword("_EMISSION");
            crystalMat.SetColor("_EmissionColor", new Color(0f, 1f, 1f) * 0.5f);

            AssetDatabase.CreateAsset(crystalMat, "Assets/_Project/Materials/Crystal_Glow.mat");

            // Create crystal game object
            GameObject crystal = GameObject.CreatePrimitive(PrimitiveType.Cube);
            crystal.name = "Crystal";
            crystal.transform.localScale = new Vector3(0.3f, 0.5f, 0.3f);
            crystal.transform.Rotate(0f, 45f, 0f);

            // Remove default collider and add capsule for better touch detection
            Object.DestroyImmediate(crystal.GetComponent<BoxCollider>());
            CapsuleCollider capsule = crystal.AddComponent<CapsuleCollider>();
            capsule.radius = 0.15f;
            capsule.height = 0.5f;
            capsule.center = new Vector3(0, 0.25f, 0);

            // Apply material
            Renderer renderer = crystal.GetComponent<Renderer>();
            renderer.material = crystalMat;

            // Add CollectibleItem
            CollectibleItem collectible = crystal.AddComponent<CollectibleItem>();

            // Serialize and configure the collectible
            SerializedObject so = new SerializedObject(collectible);
            so.FindProperty("itemName").stringValue = "Mystic Crystal";
            so.FindProperty("pointValue").intValue = 100;
            so.FindProperty("floatAmplitude").floatValue = 0.1f;
            so.FindProperty("floatSpeed").floatValue = 2f;
            so.FindProperty("rotationSpeed").floatValue = 30f;
            so.ApplyModifiedProperties();

            // Create particle system child
            GameObject particles = new GameObject("Particles");
            particles.transform.SetParent(crystal.transform);
            particles.transform.localPosition = Vector3.zero;

            ParticleSystem ps = particles.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startColor = new Color(0f, 1f, 1f, 0.8f);
            main.startLifetime = 1f;
            main.startSpeed = 0.5f;
            main.startSize = 0.05f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.loop = true;

            var emission = ps.emission;
            emission.rateOverTime = 10f;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.2f;

            var colorOverLifetime = ps.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] { new GradientColorKey(new Color(0f, 1f, 1f), 0f), new GradientColorKey(new Color(0f, 1f, 1f, 0f), 1f) },
                new GradientAlphaKey[] { new GradientAlphaKey(0.8f, 0f), new GradientAlphaKey(0f, 1f) }
            );
            colorOverLifetime.color = gradient;

            // Save as prefab
            string prefabPath = "Assets/_Project/Prefabs/Crystal.prefab";
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(crystal, prefabPath);

            Debug.Log($"Created Crystal prefab at {prefabPath}");

            // Cleanup scene object
            Object.DestroyImmediate(crystal);

            AssetDatabase.SaveAssets();
        }

        [MenuItem("Window/AR Fantasy/Create All Item Prefabs")]
        public static void CreateAllItemPrefabs()
        {
            // Create Rune prefab
            CreateItemPrefab("Rune", "Ancient Rune", 250);

            // Create Gem prefab
            CreateItemPrefab("Gem", "Moonstone Gem", 500);

            Debug.Log("Created item prefabs");
            AssetDatabase.SaveAssets();
        }

        private static void CreateItemPrefab(string name, string displayName, int points)
        {
            // Load crystal material
            Material mat = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/Crystal_Glow.mat");

            GameObject item = GameObject.CreatePrimitive(PrimitiveType.Cube);
            item.name = name;
            item.transform.localScale = new Vector3(0.25f, 0.4f, 0.25f);

            if (mat != null)
            {
                item.GetComponent<Renderer>().material = mat;
            }

            Object.DestroyImmediate(item.GetComponent<BoxCollider>());
            item.AddComponent<CapsuleCollider>();

            CollectibleItem collectible = item.AddComponent<CollectibleItem>();
            SerializedObject so = new SerializedObject(collectible);
            so.FindProperty("itemName").stringValue = displayName;
            so.FindProperty("pointValue").intValue = points;
            so.ApplyModifiedProperties();

            string prefabPath = $"Assets/_Project/Prefabs/{name}.prefab";
            PrefabUtility.SaveAsPrefabAsset(item, prefabPath);

            Object.DestroyImmediate(item);
        }

        [MenuItem("Window/AR Fantasy/Setup All")]
        public static void SetupAll()
        {
            Debug.Log("Starting AR Fantasy Setup...");
            CreateCrystalPrefab();
            Debug.Log("Setup complete!");
        }
    }
}
#endif
