using System;
using System.IO;
using PartyNight.Gameplay;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace PartyNight.Foundation.Editor
{
    public sealed class HotboxHavocVisualArtifactExporter :
        IPostprocessBuildWithReport
    {
        public int callbackOrder => 1000;

        public void OnPostprocessBuild(BuildReport report)
        {
#if UNITY_CLOUD_BUILD
            ValidateCurrentCloudCapture();

            var projectRoot = GetProjectRoot();
            var source = Path.Combine(
                projectRoot,
                HotboxHavocPrototype.VisualCaptureRelativePath);
            var manifestSource = Path.Combine(
                projectRoot,
                HotboxHavocPrototype.VisualCaptureManifestRelativePath);

            var outputPath = report.summary.outputPath;
            var outputDirectory = Directory.Exists(outputPath)
                ? outputPath
                : Path.GetDirectoryName(outputPath);

            if (string.IsNullOrWhiteSpace(outputDirectory))
            {
                throw new BuildFailedException(
                    $"Could not resolve build output directory from {outputPath}.");
            }

            var destinationDirectory =
                Path.Combine(outputDirectory, "VisualValidation");
            Directory.CreateDirectory(destinationDirectory);

            var destination = Path.Combine(
                destinationDirectory,
                Path.GetFileName(source));
            File.Copy(source, destination, overwrite: true);

            var manifestDestination = Path.Combine(
                destinationDirectory,
                Path.GetFileName(manifestSource));
            File.Copy(
                manifestSource,
                manifestDestination,
                overwrite: true);

            var revision =
                Environment.GetEnvironmentVariable("BUILD_REVISION") ?? "unknown";
            File.WriteAllText(
                Path.Combine(
                    destinationDirectory,
                    "HotboxHavoc_VisualValidation.txt"),
                "The 420 Lobby: Party Night\n" +
                "Artifact: real Unity runtime visual validation\n" +
                "Milestone: Local Hotbox Havoc prototype\n" +
                $"Revision: {revision}\n" +
                $"Unity: {Application.unityVersion}\n" +
                $"Build target: {report.summary.platform}\n" +
                $"Build result: {report.summary.result}\n" +
                $"Capture: {Path.GetFileName(source)}\n");

            Debug.Log(
                $"Party Night visual validation artifact copied to {destination}.");
#endif
        }

        public static void ValidateCurrentCloudCapture()
        {
#if UNITY_CLOUD_BUILD
            var revision = Environment.GetEnvironmentVariable("BUILD_REVISION");
            if (string.IsNullOrWhiteSpace(revision))
            {
                throw new BuildFailedException(
                    "Unity Cloud BUILD_REVISION is unavailable for visual validation.");
            }

            var projectRoot = GetProjectRoot();
            var source = Path.Combine(
                projectRoot,
                HotboxHavocPrototype.VisualCaptureRelativePath);
            var manifestPath = Path.Combine(
                projectRoot,
                HotboxHavocPrototype.VisualCaptureManifestRelativePath);

            if (!File.Exists(source))
            {
                throw new BuildFailedException(
                    "Unity Cloud build is missing Hotbox Havoc visual validation " +
                    $"capture: {source}. Play Mode visual validation must run first.");
            }

            if (new FileInfo(source).Length <= 10_000)
            {
                throw new BuildFailedException(
                    "Hotbox Havoc visual validation PNG is unexpectedly small.");
            }

            if (!File.Exists(manifestPath))
            {
                throw new BuildFailedException(
                    $"Unity Cloud build is missing visual validation manifest: {manifestPath}.");
            }

            var manifest = File.ReadAllText(manifestPath);
            if (!manifest.Contains($"\"revision\":\"{revision}\""))
            {
                throw new BuildFailedException(
                    "Hotbox Havoc visual evidence does not match the exact BUILD_REVISION.");
            }

            if (!manifest.Contains("\"width\":1280") ||
                !manifest.Contains("\"height\":720"))
            {
                throw new BuildFailedException(
                    "Hotbox Havoc visual evidence must be exactly 1280x720.");
            }

            Debug.Log(
                $"Party Night Hotbox Havoc visual evidence validated for {revision}.");
#endif
        }

        private static string GetProjectRoot()
        {
            return Path.GetFullPath(
                Path.Combine(Application.dataPath, ".."));
        }
    }
}
