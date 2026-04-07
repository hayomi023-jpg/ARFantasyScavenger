#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.Build.Reporting;
using System.IO;

namespace ARFantasy.Build
{
    /// <summary>
    /// Build automation for AR Fantasy Scavenger Hunt
    /// </summary>
    public static class BuildAutomation
    {
        private const string AndroidKeystorePass = "ANDROID_KEYSTORE_PASS";
        private const string AndroidKeyAliasPass = "ANDROID_KEYALIAS_PASS";

        [MenuItem("Window/AR Fantasy/Build/Android (APK)")]
        public static void BuildAndroid()
        {
            string[] scenes = GetScenePaths();
            string outputPath = GetAndroidOutputPath();

            BuildPlayer(scenes, outputPath, BuildTarget.Android, BuildOptions.None);
        }

        [MenuItem("Window/AR Fantasy/Build/Android (AAB)")]
        public static void BuildAndroidAAB()
        {
            string[] scenes = GetScenePaths();
            string outputPath = GetAndroidOutputPath().Replace(".apk", ".aab");

            PlayerSettings.Android.useApkExpansionFiles = true;

            BuildPlayer(scenes, outputPath, BuildTarget.Android, BuildOptions.None);
        }

        [MenuItem("Window/AR Fantasy/Build/iOS")]
        public static void BuildiOS()
        {
            string[] scenes = GetScenePaths();
            string outputPath = GetiOSOutputPath();

            BuildPlayer(scenes, outputPath, BuildTarget.iOS, BuildOptions.None);
        }

        [MenuItem("Window/AR Fantasy/Build/All Platforms")]
        public static void BuildAll()
        {
            BuildAndroid();
            BuildiOS();
        }

        [MenuItem("Window/AR Fantasy/Build/Quick Build (Android)")]
        public static void QuickBuildAndroid()
        {
            // Build with no compression for faster iteration
            PlayerSettings.Android.minify = false;
            PlayerSettings.Android.splitAPK = false;

            BuildAndroid();
        }

        private static void BuildPlayer(string[] scenes, string outputPath, BuildTarget target, BuildOptions options)
        {
            if (scenes == null || scenes.Length == 0)
            {
                Debug.LogError("No scenes found! Make sure ARHunt.unity exists in Assets/_Project/Scenes/");
                return;
            }

            // Ensure output directory exists
            string directory = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            Debug.Log($"Building {target} to {outputPath}");
            Debug.Log($"Scenes: {string.Join(", ", scenes)}");

            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = outputPath,
                target = target,
                options = options
            };

            BuildReport report = UnityEditor.BuildPipeline.BuildPlayer(buildPlayerOptions);
            BuildSummary summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"Build succeeded! Size: {summary.totalSize / 1024 / 1024}MB");
                EditorUtility.RevealInFinder(outputPath);
            }
            else if (summary.result == BuildResult.Failed)
            {
                Debug.LogError($"Build failed: {summary.totalErrors} errors");
                foreach (BuildTranscript transcript in report.files)
                {
                    if (transcript.role == "compileerrors")
                    {
                        Debug.LogError($"  Error: {transcript.path}");
                    }
                }
            }
        }

        private static string[] GetScenePaths()
        {
            // Find all scenes in the project
            string[] sceneGuids = AssetDatabase.FindAssets("t:Scene", new string[] { "Assets/_Project/Scenes" });

            if (sceneGuids.Length == 0)
            {
                // Fallback: try to find ARHunt scene specifically
                string[] allSceneGuids = AssetDatabase.FindAssets("t:Scene");
                foreach (string guid in allSceneGuids)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    if (path.Contains("ARHunt") || path.Contains("Hunt"))
                    {
                        Debug.Log($"Found hunt scene: {path}");
                        return new string[] { path };
                    }
                }
                return new string[0];
            }

            string[] paths = new string[sceneGuids.Length];
            for (int i = 0; i < sceneGuids.Length; i++)
            {
                paths[i] = AssetDatabase.GUIDToAssetPath(sceneGuids[i]);
            }

            return paths;
        }

        private static string GetAndroidOutputPath()
        {
            return "Builds/Android/ARFantasyScavenger.apk";
        }

        private static string GetiOSOutputPath()
        {
            return "Builds/iOS/";
        }
    }
}
#endif
