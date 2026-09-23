using System;
using System.Linq;
using PartyNight.Gameplay;
using PartyNight.Networking;
using Unity.Netcode;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PartyNight.Foundation.Editor
{
    public static class GameplayFoundationValidator
    {
        private const string RequiredBuildScene =
            "Assets/PartyNight/Scenes/PartyNightFoundation.unity";

        public static void Validate()
        {
            var scene = SceneManager.GetSceneByPath(RequiredBuildScene);
            var openedHere = !scene.IsValid() || !scene.isLoaded;

            if (openedHere)
            {
                scene = EditorSceneManager.OpenScene(
                    RequiredBuildScene,
                    OpenSceneMode.Additive);
            }

            try
            {
                var roots = scene.GetRootGameObjects();
                var compositions = roots
                    .SelectMany(root =>
                        root.GetComponents<FoundationSceneComposition>())
                    .ToArray();

                if (compositions.Length != 1)
                {
                    throw new InvalidOperationException(
                        $"Expected exactly one FoundationSceneComposition, found {compositions.Length}.");
                }

                if (!string.Equals(
                    compositions[0].gameObject.name,
                    FoundationSceneComposition.SceneRootName,
                    StringComparison.Ordinal))
                {
                    throw new InvalidOperationException(
                        "FoundationSceneComposition is attached to an unexpected root.");
                }

                var networkPlayerPrefab = compositions[0].NetworkPlayerPrefab;
                if (networkPlayerPrefab == null)
                {
                    throw new InvalidOperationException(
                        "FoundationSceneComposition requires the canonical network player prefab.");
                }

                if (!string.Equals(
                    networkPlayerPrefab.name,
                    "PartyNightNetworkPlayer",
                    StringComparison.Ordinal))
                {
                    throw new InvalidOperationException(
                        "FoundationSceneComposition references an unexpected network player prefab.");
                }

                var networkObjects =
                    networkPlayerPrefab.GetComponentsInChildren<NetworkObject>(true);
                if (networkObjects.Length != 1 ||
                    networkObjects[0].gameObject != networkPlayerPrefab)
                {
                    throw new InvalidOperationException(
                        "Party Night network player prefab requires exactly one root NetworkObject.");
                }

                if (networkObjects[0].PrefabIdHash == 0)
                {
                    throw new InvalidOperationException(
                        "Party Night network player prefab requires a non-zero NGO prefab hash.");
                }

                var identities =
                    networkPlayerPrefab.GetComponentsInChildren<PartyNightNetworkPlayer>(true);
                if (identities.Length != 1 ||
                    identities[0].gameObject != networkPlayerPrefab)
                {
                    throw new InvalidOperationException(
                        "Party Night network player prefab requires exactly one root identity component.");
                }

                if (networkPlayerPrefab.GetComponent<CharacterController>() != null ||
                    networkPlayerPrefab.GetComponent<PartyNightCharacterMotor>() != null ||
                    networkPlayerPrefab.GetComponent<PartyNightLocalPlayerController>() != null)
                {
                    throw new InvalidOperationException(
                        "Network player prefab is identity-only and must not duplicate local movement/input.");
                }

                var mainCameras = roots
                    .SelectMany(root =>
                        root.GetComponentsInChildren<Camera>(true))
                    .Where(cameraComponent =>
                        cameraComponent.CompareTag("MainCamera"))
                    .ToArray();

                if (mainCameras.Length != 1)
                {
                    throw new InvalidOperationException(
                        $"Expected exactly one MainCamera-tagged Camera, found {mainCameras.Length}.");
                }
            }
            finally
            {
                if (openedHere && scene.IsValid() && scene.isLoaded)
                {
                    EditorSceneManager.CloseScene(scene, true);
                }
            }

            Debug.Log("Party Night gameplay foundation validation passed.");
        }
    }
}
