#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using AffaireZero.Core;

namespace AffaireZero.Editor
{
    /// <summary>Creates the bootstrap scene and Android player settings for a clean checkout.</summary>
    [InitializeOnLoad]
    public static class ProjectSetup
    {
        public const string ScenePath = "Assets/Scenes/Main.unity";
        private static bool queued;

        static ProjectSetup()
        {
            if (queued) return;
            queued = true;
            EditorApplication.delayCall += () =>
            {
                if (EditorApplication.isPlayingOrWillChangePlaymode) return;
                EnsureProject();
            };
        }

        public static void EnsureProject()
        {
            Directory.CreateDirectory("Assets/Scenes");
            if (!File.Exists(ScenePath)) CreateMainScene();
            ConfigurePlayerSettings();
            ConfigureInputHandling();
            ConfigureBuildScenes();
            AssetDatabase.SaveAssets();
        }

        public static void CreateMainScene()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var bootstrap = new GameObject("Affaire Zero · Bootstrap");
            bootstrap.AddComponent<GameBootstrap>();
            EditorSceneManager.SaveScene(scene, ScenePath);
            ConfigureBuildScenes();
            Debug.Log("Affaire Zero: generated Main.unity bootstrap scene.");
        }

        private static void ConfigureInputHandling()
        {
            // Unity 6 templates can default to the new Input System. This project uses the new
            // Input System for player controls and InputSystemUIInputModule for touch/mouse UI.
            // Enable both backends globally so editor-created/host tooling remains compatible too.
#pragma warning disable 618
            PlayerSettings.SetPropertyInt("activeInputHandler", 2, BuildTargetGroup.Unknown);
#pragma warning restore 618
        }

        public static void ConfigureBuildScenes()
        {
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
        }

        public static void ConfigurePlayerSettings()
        {
            PlayerSettings.companyName = "Affaire Zero Studio";
            PlayerSettings.productName = "Affaire Zéro";
            PlayerSettings.bundleVersion = "0.1.0";
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.AutoRotation;
            PlayerSettings.allowedAutorotateToPortrait = true;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft = true;
            PlayerSettings.allowedAutorotateToLandscapeRight = true;
            PlayerSettings.runInBackground = true;
            PlayerSettings.colorSpace = ColorSpace.Gamma;
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, "com.affairezero.mystere");
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel26;
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.Android.forceInternetPermission = true;
            PlayerSettings.Android.forceSDCardPermission = false;
            PlayerSettings.SetScriptingBackend(UnityEditor.Build.NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.SetManagedStrippingLevel(UnityEditor.Build.NamedBuildTarget.Android, ManagedStrippingLevel.Medium);
            EditorUserBuildSettings.buildAppBundle = false;
            EditorUserBuildSettings.exportAsGoogleAndroidProject = false;
        }
    }
}
#endif
