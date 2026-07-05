using System;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace WastelandForge.App.Editor
{
    public static class WastelandForgeAppShellSetup
    {
        private const string ScenePath = "Assets/WastelandForge/App/Scenes/AppShell.unity";
        private const string HeatRoot = "Assets/ThirdParty/Heat - Complete Modern UI";

        [MenuItem("WastelandForge/App Shell/Create Or Update Scene")]
        public static void CreateOrUpdateScene()
        {
            Directory.CreateDirectory("Assets/WastelandForge/App/Scenes");

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var shellObject = new GameObject("WastelandForge App Shell");
            var shell = shellObject.AddComponent<WastelandForgeAppShell>();

            shell.ConfigureAssets(
                LoadAsset<Sprite>($"{HeatRoot}/Textures/Borders/Flat/Flat Filled.png"),
                LoadAsset<Sprite>($"{HeatRoot}/Textures/Borders/Flat/Flat Outline - 2x.png"),
                LoadAsset<TMP_FontAsset>($"{HeatRoot}/Fonts/RobotoCondensed-Regular SDF.asset"),
                LoadAsset<TMP_FontAsset>($"{HeatRoot}/Fonts/RobotoCondensed-Bold SDF.asset"));

            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            ApplyPlayerSettings();
            AssetDatabase.SaveAssets();
        }

        [MenuItem("WastelandForge/App Shell/Build Windows Local")]
        public static void BuildWindowsLocal()
        {
            CreateOrUpdateScene();
            CopyForgeToStreamingAssets();

            var outputDirectory = Path.GetFullPath(Path.Combine("..", "..", "dist", "app", "WastelandForge"));
            Directory.CreateDirectory(outputDirectory);

            var report = BuildPipeline.BuildPlayer(
                new[] { ScenePath },
                Path.Combine(outputDirectory, "WastelandForge.exe"),
                BuildTarget.StandaloneWindows64,
                BuildOptions.None);

            if (report.summary.result != BuildResult.Succeeded)
            {
                throw new InvalidOperationException($"Unity player build failed: {report.summary.result}");
            }

            CopyForgeBesidePlayer(outputDirectory);
        }

        private static void ApplyPlayerSettings()
        {
            PlayerSettings.companyName = "WastelandForge";
            PlayerSettings.productName = "WastelandForge";
            PlayerSettings.applicationIdentifier = "com.wastelandforge.app";
            PlayerSettings.defaultScreenWidth = 1440;
            PlayerSettings.defaultScreenHeight = 900;
            PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
            PlayerSettings.runInBackground = true;
        }

        private static void CopyForgeToStreamingAssets()
        {
            var forgePath = ResolveRepoForgeExe();
            if (string.IsNullOrWhiteSpace(forgePath))
            {
                return;
            }

            Directory.CreateDirectory("Assets/StreamingAssets");
            File.Copy(forgePath, "Assets/StreamingAssets/forge.exe", true);
            AssetDatabase.Refresh();
        }

        private static void CopyForgeBesidePlayer(string outputDirectory)
        {
            var forgePath = ResolveRepoForgeExe();
            if (string.IsNullOrWhiteSpace(forgePath))
            {
                return;
            }

            File.Copy(forgePath, Path.Combine(outputDirectory, "forge.exe"), true);
        }

        private static string ResolveRepoForgeExe()
        {
            var repoRoot = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "..", ".."));
            var candidate = Path.Combine(repoRoot, "dist", "local", "forge.exe");
            return File.Exists(candidate) ? candidate : string.Empty;
        }

        private static T LoadAsset<T>(string path)
            where T : UnityEngine.Object
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset == null)
            {
                Debug.LogWarning($"Heat asset not found or not imported: {path}");
            }

            return asset;
        }
    }
}
