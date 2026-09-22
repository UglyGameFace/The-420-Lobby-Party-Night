using System;
using NUnit.Framework;
using UnityEditor;

namespace PartyNight.Foundation.Tests
{
    public sealed class BuildSceneTests
    {
        private const string ExpectedScenePath =
            "Assets/PartyNight/Scenes/PartyNightFoundation.unity";

        [Test]
        public void FoundationSceneExistsAndIsOnlyEnabledBuildScene()
        {
            var sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(ExpectedScenePath);
            Assert.That(sceneAsset, Is.Not.Null, $"Missing scene asset: {ExpectedScenePath}");

            var enabledCount = 0;
            var foundExpectedScene = false;

            foreach (var scene in EditorBuildSettings.scenes)
            {
                if (!scene.enabled)
                {
                    continue;
                }

                enabledCount++;
                if (string.Equals(scene.path, ExpectedScenePath, StringComparison.Ordinal))
                {
                    foundExpectedScene = true;
                }
            }

            Assert.That(foundExpectedScene, Is.True, "Foundation scene is not enabled for builds.");
            Assert.That(enabledCount, Is.EqualTo(1), "Exactly one foundation scene must be enabled.");
        }
    }
}
