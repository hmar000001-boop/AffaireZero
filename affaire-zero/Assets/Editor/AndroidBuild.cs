#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using AffaireZero.Core;
using AffaireZero.Gameplay;

namespace AffaireZero.Editor
{
    public static class AndroidBuild
    {
        /// <summary>Called by GameCI. Produces an installable APK at BuildOutput/Android/AffaireZero.apk.</summary>
        public static void BuildAndroid()
        {
            string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            string outputDirectory = Path.Combine(projectRoot, "BuildOutput", "Android");
            string reportPath = Path.Combine(outputDirectory, "BuildReport.txt");
            Directory.CreateDirectory(outputDirectory);
            var lines = new List<string>
            {
                "Affaire Zéro — Android build report",
                "UTC: " + DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
                "Unity: " + Application.unityVersion
            };
            try
            {
                ProjectSetup.EnsureProject();
                RunPreflightChecks(lines);
                if (!EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android))
                    throw new Exception("Could not switch Unity to Android. Install Android Build Support in Unity Hub / GameCI runner image.");

                string output = Path.Combine(outputDirectory, "AffaireZero.apk");
                Directory.CreateDirectory(Path.GetDirectoryName(output));
                var options = new BuildPlayerOptions
                {
                    scenes = new[] { ProjectSetup.ScenePath },
                    locationPathName = output,
                    target = BuildTarget.Android,
                    options = BuildOptions.None
                };
                BuildReport report = BuildPipeline.BuildPlayer(options);
                lines.Add("Build result: " + report.summary.result);
                lines.Add("Output: " + output);
                lines.Add("Size bytes: " + report.summary.totalSize);
                lines.Add("Warnings: " + report.summary.totalWarnings);
                lines.Add("Errors: " + report.summary.totalErrors);
                File.WriteAllLines(reportPath, lines);
                Debug.Log($"Affaire Zero Android build result: {report.summary.result}; size={report.summary.totalSize} bytes; errors={report.summary.totalErrors}");
                if (report.summary.result != BuildResult.Succeeded || !File.Exists(output))
                    throw new Exception("Android build failed. Review the build report and Unity Editor log from this workflow run.");
            }
            catch (Exception ex)
            {
                lines.Add("BUILD FAILED");
                lines.Add(ex.ToString());
                try { File.WriteAllLines(reportPath, lines); } catch { /* Keep the original build exception. */ }
                Debug.LogError(ex);
                throw;
            }
        }

        private static void RunPreflightChecks(List<string> lines)
        {
            if (!File.Exists(ProjectSetup.ScenePath)) throw new Exception("Main scene was not generated.");
            if (CaseCatalog.Evidence.Count != 8) throw new Exception("Expected exactly 8 clue definitions.");
            var ids = new HashSet<string>();
            foreach (EvidenceDefinition clue in CaseCatalog.Evidence)
            {
                if (string.IsNullOrWhiteSpace(clue.Id) || !ids.Add(clue.Id)) throw new Exception("Evidence IDs must be unique and non-empty.");
                if (string.IsNullOrWhiteSpace(clue.TitleEn) || string.IsNullOrWhiteSpace(clue.TitleAr) ||
                    string.IsNullOrWhiteSpace(clue.DescriptionEn) || string.IsNullOrWhiteSpace(clue.DescriptionAr))
                    throw new Exception("Every clue must include complete English and Arabic content: " + clue.Id);
            }
            var manager = new CaseManager();
            if (!manager.CanJustifyElias(new HashSet<string> { "passage", "speaker", "footprints" }))
                throw new Exception("Valid evidence-chain test failed.");
            if (manager.CanJustifyElias(new HashSet<string> { "speaker", "footprints", "locklog" }))
                throw new Exception("Incomplete evidence-chain test failed.");
            lines.Add("Preflight: PASS");
            lines.Add("Clue definitions: 8, unique IDs and both languages present");
            lines.Add("Conclusion rule: valid chain accepted; route-less chain rejected");
            lines.Add("Generated scene: " + ProjectSetup.ScenePath);
            Debug.Log("Affaire Zero preflight checks passed.");
        }
    }
}
#endif
