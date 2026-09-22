using System;
using System.Collections.Generic;
using PackageManagerPackageInfo = UnityEditor.PackageManager.PackageInfo;
using UnityEditor;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace PartyNight.Foundation.Editor
{
    public static class ProjectFoundationValidator
    {
        private const string RequiredUnityVersion = "6000.3.24f1";
        private const string RequiredBuildScene = "Assets/PartyNight/Scenes/PartyNightFoundation.unity";
        private const string RequiredPipelineAsset =
            "Assets/PartyNight/Settings/PartyNightURP.asset";
        private const string RequiredRendererAsset =
            "Assets/PartyNight/Settings/PartyNightUniversalRenderer.asset";
        private const string RequiredGlobalSettingsAsset =
            "Assets/PartyNight/Settings/PartyNightURPGlobalSettings.asset";
        private const string RequiredDefaultVolumeProfile =
            "Assets/PartyNight/Settings/PartyNightDefaultVolumeProfile.asset";

        private static readonly IReadOnlyDictionary<string, string> RequiredPackages =
            new Dictionary<string, string>
            {
                ["com.unity.inputsystem"] = "1.20.0",
                ["com.unity.netcode.gameobjects"] = "2.13.2",
                ["com.unity.render-pipelines.universal"] = "17.3.0",
                ["com.unity.test-framework"] = "1.6.0",
                ["com.unity.transport"] = "2.7.4",
            };

        public static void PreExport()
        {
            Validate();
            InputFoundationValidator.Validate();
        }

        public static void Validate()
        {
            ValidateEditorAndPackages();
            ValidateBuildScene();
            ValidateAuthoritativeProjectSettings();

            Debug.Log("Party Night Unity foundation validation passed.");
        }

        private static void ValidateEditorAndPackages()
        {
            if (!string.Equals(Application.unityVersion, RequiredUnityVersion, StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    $"Expected Unity {RequiredUnityVersion}, but running {Application.unityVersion}.");
            }

            var installed = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (var package in PackageManagerPackageInfo.GetAllRegisteredPackages())
            {
                installed[package.name] = package.version;
            }

            foreach (var required in RequiredPackages)
            {
                if (!installed.TryGetValue(required.Key, out var actualVersion))
                {
                    throw new InvalidOperationException($"Required package {required.Key} is not resolved.");
                }

                if (!string.Equals(actualVersion, required.Value, StringComparison.Ordinal))
                {
                    throw new InvalidOperationException(
                        $"Package {required.Key} expected {required.Value}, resolved {actualVersion}.");
                }
            }
        }

        private static void ValidateBuildScene()
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(RequiredBuildScene) == null)
            {
                throw new InvalidOperationException(
                    $"Required build scene {RequiredBuildScene} could not be imported.");
            }

            var enabledSceneCount = 0;
            var requiredSceneEnabled = false;
            foreach (var scene in EditorBuildSettings.scenes)
            {
                if (!scene.enabled)
                {
                    continue;
                }

                enabledSceneCount++;
                if (string.Equals(scene.path, RequiredBuildScene, StringComparison.Ordinal))
                {
                    requiredSceneEnabled = true;
                }
            }

            if (!requiredSceneEnabled)
            {
                throw new InvalidOperationException(
                    $"Required build scene {RequiredBuildScene} is not enabled.");
            }

            if (enabledSceneCount != 1)
            {
                throw new InvalidOperationException(
                    $"Expected exactly one enabled foundation scene, found {enabledSceneCount}.");
            }
        }

        private static void ValidateAuthoritativeProjectSettings()
        {
            var pipelineAsset =
                AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(RequiredPipelineAsset);
            if (pipelineAsset == null)
            {
                throw new InvalidOperationException(
                    $"Required URP asset could not be imported: {RequiredPipelineAsset}");
            }

            var rendererAsset =
                AssetDatabase.LoadAssetAtPath<ScriptableRendererData>(RequiredRendererAsset);
            if (rendererAsset == null)
            {
                throw new InvalidOperationException(
                    $"Required Universal Renderer could not be imported: {RequiredRendererAsset}");
            }

            var pipelineSerialized = new SerializedObject(pipelineAsset);
            var rendererDataList = pipelineSerialized.FindProperty("m_RendererDataList");
            var defaultRendererIndex = pipelineSerialized.FindProperty("m_DefaultRendererIndex");

            if (rendererDataList == null ||
                !rendererDataList.isArray ||
                rendererDataList.arraySize == 0 ||
                rendererDataList.GetArrayElementAtIndex(0).objectReferenceValue != rendererAsset)
            {
                throw new InvalidOperationException(
                    "Party Night URP asset does not own the expected Universal Renderer.");
            }

            if (defaultRendererIndex == null || defaultRendererIndex.intValue != 0)
            {
                throw new InvalidOperationException(
                    "Party Night URP asset does not use renderer index 0 as its default renderer.");
            }

            if (pipelineAsset.GetRenderer(0) == null)
            {
                throw new InvalidOperationException(
                    "Party Night URP asset cannot create its configured default renderer.");
            }

            if (GraphicsSettings.defaultRenderPipeline != pipelineAsset)
            {
                throw new InvalidOperationException(
                    "Party Night URP asset is not assigned as the default render pipeline.");
            }

            var globalSettings =
                EditorGraphicsSettings.GetRenderPipelineGlobalSettingsAsset(
                    typeof(UniversalRenderPipeline));

            if (globalSettings == null ||
                !string.Equals(
                    AssetDatabase.GetAssetPath(globalSettings),
                    RequiredGlobalSettingsAsset,
                    StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    "Party Night URP Global Settings are not registered correctly.");
            }

            if (AssetDatabase.LoadAssetAtPath<VolumeProfile>(RequiredDefaultVolumeProfile) == null)
            {
                throw new InvalidOperationException(
                    $"Required default volume profile could not be imported: {RequiredDefaultVolumeProfile}");
            }

            if (EditorSettings.serializationMode != SerializationMode.ForceText)
            {
                throw new InvalidOperationException(
                    "Unity asset serialization must remain Force Text.");
            }

            if (!string.Equals(PlayerSettings.productName, ProjectIdentity.ProductName, StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    $"Expected product name '{ProjectIdentity.ProductName}', found '{PlayerSettings.productName}'.");
            }

            var originalQualityLevel = QualitySettings.GetQualityLevel();
            try
            {
                for (var index = 0; index < QualitySettings.names.Length; index++)
                {
                    QualitySettings.SetQualityLevel(index, false);
                    if (QualitySettings.renderPipeline != null)
                    {
                        throw new InvalidOperationException(
                            $"Quality level '{QualitySettings.names[index]}' overrides the project render pipeline.");
                    }
                }
            }
            finally
            {
                QualitySettings.SetQualityLevel(originalQualityLevel, false);
            }
        }
    }
}
