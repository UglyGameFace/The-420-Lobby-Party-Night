using System;
using System.Collections.Generic;
using PackageManagerPackageInfo = UnityEditor.PackageManager.PackageInfo;
using UnityEngine;

namespace PartyNight.Foundation.Editor
{
    public static class ProjectFoundationValidator
    {
        private const string RequiredUnityVersion = "6000.3.24f1";
        private const string RequiredBuildScene = "Assets/PartyNight/Scenes/PartyNightFoundation.unity";

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
            AuthoritativeSettingsBootstrap.Prepare();
            Validate();
        }

        public static void Validate()
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

            if (UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEditor.SceneAsset>(RequiredBuildScene) == null)
            {
                throw new InvalidOperationException($"Required build scene {RequiredBuildScene} could not be imported.");
            }

            var enabledSceneCount = 0;
            var requiredSceneEnabled = false;
            foreach (var scene in UnityEditor.EditorBuildSettings.scenes)
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
                throw new InvalidOperationException($"Required build scene {RequiredBuildScene} is not enabled.");
            }

            if (enabledSceneCount != 1)
            {
                throw new InvalidOperationException(
                    $"Expected exactly one enabled foundation scene, found {enabledSceneCount}.");
            }

            Debug.Log("Party Night Unity foundation validation passed.");
        }
    }
}
