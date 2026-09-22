using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using PartyNight.Foundation;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace PartyNight.Foundation.Editor
{
    internal static class AuthoritativeSettingsBootstrap
    {
        internal const string SettingsFolder = "Assets/PartyNight/Settings";
        internal const string PipelineAssetPath = SettingsFolder + "/PartyNightURP.asset";
        internal const string RendererAssetPath = SettingsFolder + "/PartyNightUniversalRenderer.asset";
        internal const string GlobalSettingsAssetPath = SettingsFolder + "/PartyNightURPGlobalSettings.asset";
        internal const string DefaultVolumeProfilePath = SettingsFolder + "/PartyNightDefaultVolumeProfile.asset";

        private const string TemporaryRendererPath = "Assets/UniversalRenderer.asset";
        private const string TemporaryDefaultVolumeProfilePath = "Assets/DefaultVolumeProfile.asset";

        internal static void Prepare()
        {
            EnsureSettingsFolder();

            EditorSettings.serializationMode = SerializationMode.ForceText;
            EditorSettings.serializeInlineMappingsOnOneLine = true;
            PlayerSettings.productName = ProjectIdentity.ProductName;

            var pipelineAsset = EnsurePipelineAndRenderer();
            EnsureGlobalSettings();
            ConfigurePipelineOwnership(pipelineAsset);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            ValidatePreparedState(pipelineAsset);
        }

        private static void EnsureSettingsFolder()
        {
            if (!AssetDatabase.IsValidFolder(SettingsFolder))
            {
                var guid = AssetDatabase.CreateFolder("Assets/PartyNight", "Settings");
                if (string.IsNullOrEmpty(guid))
                {
                    throw new InvalidOperationException($"Unable to create {SettingsFolder}.");
                }
            }
        }

        private static UniversalRenderPipelineAsset EnsurePipelineAndRenderer()
        {
            var pipelineAsset =
                AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(PipelineAssetPath);

            if (pipelineAsset == null)
            {
                pipelineAsset = UniversalRenderPipelineAsset.Create();
                pipelineAsset.name = "PartyNightURP";
                AssetDatabase.CreateAsset(pipelineAsset, PipelineAssetPath);
            }

            var rendererData =
                AssetDatabase.LoadAssetAtPath<ScriptableRendererData>(RendererAssetPath);

            if (rendererData == null)
            {
                if (AssetDatabase.LoadMainAssetAtPath(TemporaryRendererPath) != null)
                {
                    throw new InvalidOperationException(
                        $"Unexpected existing temporary renderer asset at {TemporaryRendererPath}.");
                }

                rendererData = pipelineAsset.LoadBuiltinRendererData(RendererType.UniversalRenderer);
                if (rendererData == null)
                {
                    throw new InvalidOperationException("Unity failed to create the Universal Renderer asset.");
                }

                var sourcePath = AssetDatabase.GetAssetPath(rendererData);
                MoveAssetChecked(sourcePath, RendererAssetPath);
                rendererData = AssetDatabase.LoadAssetAtPath<ScriptableRendererData>(RendererAssetPath);
            }

            if (rendererData == null)
            {
                throw new InvalidOperationException(
                    $"Unable to load Universal Renderer at {RendererAssetPath}.");
            }

            EditorUtility.SetDirty(rendererData);
            EditorUtility.SetDirty(pipelineAsset);
            AssetDatabase.SaveAssets();

            return pipelineAsset;
        }

        private static void EnsureGlobalSettings()
        {
            var globalSettings =
                AssetDatabase.LoadAssetAtPath<RenderPipelineGlobalSettings>(GlobalSettingsAssetPath);

            if (globalSettings == null)
            {
                var globalSettingsType = typeof(UniversalRenderPipeline).Assembly.GetType(
                    "UnityEngine.Rendering.Universal.UniversalRenderPipelineGlobalSettings",
                    throwOnError: true);

                globalSettings =
                    ScriptableObject.CreateInstance(globalSettingsType) as RenderPipelineGlobalSettings;

                if (globalSettings == null)
                {
                    throw new InvalidOperationException(
                        "Unity failed to create Universal Render Pipeline Global Settings.");
                }

                globalSettings.name = "PartyNightURPGlobalSettings";
                AssetDatabase.CreateAsset(globalSettings, GlobalSettingsAssetPath);

                EditorGraphicsSettings.PopulateRenderPipelineGraphicsSettings(globalSettings);
                globalSettings.Initialize();
                EditorUtility.SetDirty(globalSettings);
                AssetDatabase.SaveAssets();

                MoveDefaultVolumeProfileIfCreated();
            }

            EditorGraphicsSettings.SetRenderPipelineGlobalSettingsAsset(
                typeof(UniversalRenderPipeline),
                globalSettings);

            EditorUtility.SetDirty(globalSettings);
        }

        private static void MoveDefaultVolumeProfileIfCreated()
        {
            var existingTarget = AssetDatabase.LoadMainAssetAtPath(DefaultVolumeProfilePath);
            if (existingTarget != null)
            {
                return;
            }

            var source = AssetDatabase.LoadMainAssetAtPath(TemporaryDefaultVolumeProfilePath);
            if (source != null)
            {
                MoveAssetChecked(TemporaryDefaultVolumeProfilePath, DefaultVolumeProfilePath);
            }
        }

        private static void ConfigurePipelineOwnership(UniversalRenderPipelineAsset pipelineAsset)
        {
            GraphicsSettings.defaultRenderPipeline = pipelineAsset;

            var originalQualityLevel = QualitySettings.GetQualityLevel();
            try
            {
                for (var index = 0; index < QualitySettings.names.Length; index++)
                {
                    QualitySettings.SetQualityLevel(index, applyExpensiveChanges: false);
                    QualitySettings.renderPipeline = null;
                }
            }
            finally
            {
                QualitySettings.SetQualityLevel(originalQualityLevel, applyExpensiveChanges: false);
            }
        }

        private static void ValidatePreparedState(UniversalRenderPipelineAsset pipelineAsset)
        {
            if (pipelineAsset == null)
            {
                throw new InvalidOperationException("Party Night URP asset is missing.");
            }

            if (pipelineAsset.GetRenderer(0) == null)
            {
                throw new InvalidOperationException("Party Night URP asset has no usable default renderer.");
            }

            if (GraphicsSettings.defaultRenderPipeline != pipelineAsset)
            {
                throw new InvalidOperationException(
                    "Party Night URP asset is not assigned as the default render pipeline.");
            }

            var registeredGlobalSettings =
                EditorGraphicsSettings.GetRenderPipelineGlobalSettingsAsset(
                    typeof(UniversalRenderPipeline));

            if (registeredGlobalSettings == null ||
                !string.Equals(
                    AssetDatabase.GetAssetPath(registeredGlobalSettings),
                    GlobalSettingsAssetPath,
                    StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    "Party Night URP Global Settings are not registered correctly.");
            }

            if (EditorSettings.serializationMode != SerializationMode.ForceText)
            {
                throw new InvalidOperationException("Unity asset serialization is not Force Text.");
            }

            var originalQualityLevel = QualitySettings.GetQualityLevel();
            try
            {
                for (var index = 0; index < QualitySettings.names.Length; index++)
                {
                    QualitySettings.SetQualityLevel(index, applyExpensiveChanges: false);
                    if (QualitySettings.renderPipeline != null)
                    {
                        throw new InvalidOperationException(
                            $"Quality level '{QualitySettings.names[index]}' overrides the project render pipeline.");
                    }
                }
            }
            finally
            {
                QualitySettings.SetQualityLevel(originalQualityLevel, applyExpensiveChanges: false);
            }

            if (!string.Equals(PlayerSettings.productName, ProjectIdentity.ProductName, StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    $"Expected product name '{ProjectIdentity.ProductName}', found '{PlayerSettings.productName}'.");
            }
        }

        private static void MoveAssetChecked(string sourcePath, string destinationPath)
        {
            if (string.Equals(sourcePath, destinationPath, StringComparison.Ordinal))
            {
                return;
            }

            var error = AssetDatabase.MoveAsset(sourcePath, destinationPath);
            if (!string.IsNullOrEmpty(error))
            {
                throw new InvalidOperationException(
                    $"Failed to move Unity asset from {sourcePath} to {destinationPath}: {error}");
            }
        }
    }

    internal sealed class AuthoritativeSettingsCapture : IPostprocessBuildWithReport
    {
        public int callbackOrder => int.MaxValue;

        public void OnPostprocessBuild(BuildReport report)
        {
            Capture(report.summary.outputPath);
        }

        private static void Capture(string outputPath)
        {
            var projectRoot = Directory.GetParent(Application.dataPath)?.FullName;
            if (string.IsNullOrEmpty(projectRoot))
            {
                throw new InvalidOperationException("Unable to determine Unity project root.");
            }

            var outputRoot = Directory.Exists(outputPath)
                ? outputPath
                : Path.GetDirectoryName(outputPath);

            if (string.IsNullOrEmpty(outputRoot))
            {
                throw new InvalidOperationException(
                    $"Unable to determine build output directory from '{outputPath}'.");
            }

            var captureRoot = Path.Combine(outputRoot, "PartyNightAuthoritativeSettings");
            if (Directory.Exists(captureRoot))
            {
                Directory.Delete(captureRoot, recursive: true);
            }

            Directory.CreateDirectory(captureRoot);

            var requiredFiles = new[]
            {
                "Packages/packages-lock.json",
                "ProjectSettings/ProjectVersion.txt",
                "ProjectSettings/EditorSettings.asset",
                "ProjectSettings/ProjectSettings.asset",
                "ProjectSettings/GraphicsSettings.asset",
                "ProjectSettings/QualitySettings.asset",
                "ProjectSettings/EditorBuildSettings.asset",
                "Assets/PartyNight/Settings.meta",
            };

            var copied = new List<string>();

            foreach (var relativePath in requiredFiles)
            {
                CopyRequired(projectRoot, captureRoot, relativePath, copied);
            }

            var settingsDirectory = Path.Combine(projectRoot, AuthoritativeSettingsBootstrap.SettingsFolder);
            if (!Directory.Exists(settingsDirectory))
            {
                throw new InvalidOperationException(
                    $"Missing generated settings directory: {AuthoritativeSettingsBootstrap.SettingsFolder}");
            }

            foreach (var sourceFile in Directory.GetFiles(settingsDirectory, "*", SearchOption.AllDirectories))
            {
                var relativePath = MakeRelativePath(projectRoot, sourceFile);
                CopyRequired(projectRoot, captureRoot, relativePath, copied);
            }

            copied.Sort(StringComparer.Ordinal);
            WriteManifest(captureRoot, copied);
            Debug.Log(
                $"Captured {copied.Count} authoritative Unity settings files to '{captureRoot}'.");
        }

        private static void CopyRequired(
            string projectRoot,
            string captureRoot,
            string relativePath,
            ICollection<string> copied)
        {
            var normalizedRelativePath = relativePath.Replace('/', Path.DirectorySeparatorChar);
            var source = Path.Combine(projectRoot, normalizedRelativePath);

            if (!File.Exists(source))
            {
                throw new FileNotFoundException(
                    $"Required authoritative Unity file was not generated: {relativePath}",
                    source);
            }

            var destination = Path.Combine(captureRoot, normalizedRelativePath);
            var destinationDirectory = Path.GetDirectoryName(destination);
            if (!string.IsNullOrEmpty(destinationDirectory))
            {
                Directory.CreateDirectory(destinationDirectory);
            }

            File.Copy(source, destination, overwrite: true);
            copied.Add(relativePath.Replace(Path.DirectorySeparatorChar, '/'));
        }

        private static void WriteManifest(string captureRoot, IReadOnlyList<string> copied)
        {
            var builder = new StringBuilder();
            builder.AppendLine("Party Night authoritative Unity settings capture");
            builder.AppendLine($"Unity: {Application.unityVersion}");
            builder.AppendLine($"Files: {copied.Count}");
            builder.AppendLine();

            using (var sha256 = SHA256.Create())
            {
                foreach (var relativePath in copied)
                {
                    var fullPath = Path.Combine(
                        captureRoot,
                        relativePath.Replace('/', Path.DirectorySeparatorChar));

                    using (var stream = File.OpenRead(fullPath))
                    {
                        var hash = sha256.ComputeHash(stream);
                        builder.Append(BitConverter.ToString(hash).Replace("-", string.Empty).ToLowerInvariant());
                    }

                    builder.Append("  ");
                    builder.AppendLine(relativePath);
                }
            }

            File.WriteAllText(
                Path.Combine(captureRoot, "SHA256SUMS.txt"),
                builder.ToString(),
                new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        }

        private static string MakeRelativePath(string root, string fullPath)
        {
            var rootUri = new Uri(AppendDirectorySeparator(root));
            var fileUri = new Uri(fullPath);
            return Uri.UnescapeDataString(rootUri.MakeRelativeUri(fileUri).ToString())
                .Replace('/', Path.DirectorySeparatorChar);
        }

        private static string AppendDirectorySeparator(string path)
        {
            return path.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal)
                ? path
                : path + Path.DirectorySeparatorChar;
        }
    }
}
