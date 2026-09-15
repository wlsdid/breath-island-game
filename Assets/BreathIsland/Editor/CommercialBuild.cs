using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BreathIsland.Editor
{
    public static class CommercialBuild
    {
        private const string ScenePath = "Assets/BreathIsland/Scenes/BeachVerticalSlice.unity";
        private const string BuildPath = "Builds/Windows/BreathIsland.exe";

        [MenuItem("숨결섬/플레이 씬 생성")]
        public static void GeneratePlayableScene()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(ScenePath) ?? "Assets/BreathIsland/Scenes");
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var game = new GameObject("Breath Island Runtime");
            game.AddComponent<BreathIslandGame>();
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene, ScenePath)) throw new InvalidOperationException("Failed to save playable scene.");
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            AssetDatabase.SaveAssets();
            Debug.Log($"Playable scene generated: {ScenePath}");
        }

        [MenuItem("숨결섬/Windows 검증 빌드 (Mono)")]
        public static void BuildWindows() => BuildWindowsInternal(false, ScriptingImplementation.Mono2x);

        [MenuItem("숨결섬/Windows 출시 빌드 (IL2CPP)")]
        public static void BuildWindowsRelease() => BuildWindowsInternal(false, ScriptingImplementation.IL2CPP);

        public static void BuildWindowsCI()
        {
            bool release = string.Equals(
                Environment.GetEnvironmentVariable("BREATH_ISLAND_BACKEND"),
                "IL2CPP",
                StringComparison.OrdinalIgnoreCase);
            BuildWindowsInternal(true, release ? ScriptingImplementation.IL2CPP : ScriptingImplementation.Mono2x);
        }

        private static void BuildWindowsInternal(bool batchMode, ScriptingImplementation backend)
        {
            LocalizationSetup.EnsureConfigured();
            if (!File.Exists(ScenePath)) GeneratePlayableScene();
            ConfigurePlayer(backend);
            Directory.CreateDirectory(Path.GetDirectoryName(BuildPath) ?? "Builds/Windows");
            BuildReport report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = new[] { ScenePath },
                locationPathName = BuildPath,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.CleanBuildCache
            });
            if (report.summary.result != BuildResult.Succeeded)
            {
                string message = $"Windows build failed: {report.summary.result}, {report.summary.totalErrors} errors";
                if (batchMode) throw new BuildFailedException(message);
                Debug.LogError(message);
                return;
            }
            Debug.Log($"Windows build complete: {BuildPath} ({report.summary.totalSize / 1048576f:0.0} MB)");
        }

        private static void ConfigurePlayer(ScriptingImplementation backend)
        {
            PlayerSettings.companyName = "Breath Island Studio";
            PlayerSettings.productName = "Breath Island";
            PlayerSettings.bundleVersion = "0.1.0-vertical-slice";
            PlayerSettings.defaultScreenWidth = 1920;
            PlayerSettings.defaultScreenHeight = 1080;
            PlayerSettings.fullScreenMode = FullScreenMode.FullScreenWindow;
            PlayerSettings.runInBackground = false;
            PlayerSettings.resizableWindow = true;
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Standalone, "com.breathisland.game");
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Standalone, backend);
            PlayerSettings.SetApiCompatibilityLevel(NamedBuildTarget.Standalone, ApiCompatibilityLevel.NET_Standard);
        }
    }
}
