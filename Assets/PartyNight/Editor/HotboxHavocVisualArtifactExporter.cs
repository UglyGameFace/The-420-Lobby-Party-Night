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
            var projectRoot = Path.GetFullPath(
                Path.Combine(Application.dataPath, ".."));
            var source = Path.Combine(
                projectRoot,
                HotboxHavocPrototype.VisualCaptureRelativePath);

            if (!File.Exists(source))
            {
                throw new BuildFailedException(
                    "Unity Cloud build is missing Hotbox Havoc visual validation " +
                    $"capture: {source}. Play Mode visual validation must run first.");
            }

            var outputPath = report.summary.outputPath;
            var outputDirectory = Directory.Exists(outputPath)
                ? outputPath
                : Path.GetDirectoryName(outputPath);

            if (string.IsNullOrWhiteSpace(outputDirectory))
            {
                throw new BuildFailedException(
                    $"Could not resolve build output directory from {outputPath}.");
            }

            var destinationDirectory = Path.Combine(outputDirectory, "VisualValidation");
            Directory.CreateDirectory(destinationDirectory);

            var destination = Path.Combine(
                destinationDirectory,
                Path.GetFileName(source));
            File.Copy(source, destination, overwrite: true);

            File.WriteAllText(
                Path.Combine(destinationDirectory, "HotboxHavoc_VisualValidation.txt"),
                "The 420 Lobby: Party Night\n" +
                "Artifact: real Unity runtime visual validation\n" +
                "Milestone: Local Hotbox Havoc prototype\n" +
                $"Unity: {Application.unityVersion}\n" +
                $"Build target: {report.summary.platform}\n" +
                $"Build result: {report.summary.result}\n" +
                $"Capture: {Path.GetFileName(source)}\n");

            Debug.Log(
                $"Party Night visual validation artifact copied to {destination}.");
#endif
        }
    }
}
