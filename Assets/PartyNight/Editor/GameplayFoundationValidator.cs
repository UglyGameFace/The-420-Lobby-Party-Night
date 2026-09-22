using System;
using System.Linq;
using PartyNight.Gameplay;
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
