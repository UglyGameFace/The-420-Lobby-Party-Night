using System.Collections;
using System.Linq;
using NUnit.Framework;
using PartyNight.Networking;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace PartyNight.Gameplay.Tests
{
    public sealed class NetworkingRuntimeTests
    {
        private const string FoundationScenePath =
            "Assets/PartyNight/Scenes/PartyNightFoundation.unity";
        private const ushort TestServerPort = 19777;

        private Scene loadedScene;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            var existing = SceneManager.GetSceneByPath(FoundationScenePath);
            if (existing.IsValid() && existing.isLoaded)
            {
                var existingUnload = SceneManager.UnloadSceneAsync(existing);
                if (existingUnload != null)
                {
                    while (!existingUnload.isDone)
                    {
                        yield return null;
                    }
                }
            }

            var load = SceneManager.LoadSceneAsync(
                FoundationScenePath,
                LoadSceneMode.Additive);

            Assert.That(load, Is.Not.Null);
            while (!load.isDone)
            {
                yield return null;
            }

            loadedScene = SceneManager.GetSceneByPath(FoundationScenePath);
            Assert.That(loadedScene.IsValid(), Is.True);
            Assert.That(loadedScene.isLoaded, Is.True);
            yield return null;
        }

        [TearDown]
        public void ReportTestResult()
        {
            var context = TestContext.CurrentContext;
            var outcome = context.Result.Outcome;
            var message = context.Result.Message ?? string.Empty;
            var stackTrace = context.Result.StackTrace ?? string.Empty;

            Debug.Log(
                $"PARTY_NIGHT_TEST_RESULT | {context.Test.FullName} | " +
                $"{outcome.Status} | {outcome.Label ?? string.Empty} | {message} | " +
                $"STACK: {stackTrace}");
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            var bootstrap = FindBootstrapOrNull();
            bootstrap?.Shutdown();
            yield return null;

            if (!loadedScene.IsValid() || !loadedScene.isLoaded)
            {
                yield break;
            }

            var unload = SceneManager.UnloadSceneAsync(loadedScene);
            if (unload == null)
            {
                yield break;
            }

            while (!unload.isDone)
            {
                yield return null;
            }
        }

        [Test]
        public void FoundationSceneComposesExactlyOneNetworkBootstrap()
        {
            var composition = FindComposition();
            var bootstraps = loadedScene
                .GetRootGameObjects()
                .SelectMany(root =>
                    root.GetComponentsInChildren<PartyNightNetworkBootstrap>(true))
                .ToArray();

            Assert.That(bootstraps, Has.Length.EqualTo(1));
            Assert.That(composition.NetworkBootstrap, Is.SameAs(bootstraps[0]));
            Assert.That(bootstraps[0].IsInitialized, Is.True);
            Assert.That(bootstraps[0].NetworkManager, Is.Not.Null);
            Assert.That(bootstraps[0].Transport, Is.Not.Null);
        }

        [Test]
        public void DefaultFoundationSceneIsNotAuthoritative()
        {
            var bootstrap = FindBootstrap();

            Assert.That(bootstrap.Mode, Is.EqualTo(PartyNightNetworkMode.None));
            Assert.That(bootstrap.NetworkManager.IsListening, Is.False);
            Assert.That(bootstrap.NetworkManager.IsServer, Is.False);
            Assert.That(bootstrap.NetworkManager.IsClient, Is.False);
            Assert.That(bootstrap.IsAuthoritativeServer, Is.False);
        }

        [UnityTest]
        public IEnumerator DedicatedServerStartsWithoutBecomingAClient()
        {
            var bootstrap = FindBootstrap();

            Assert.That(
                bootstrap.StartDedicatedServer(TestServerPort),
                Is.True,
                "Party Night dedicated server session failed to start.");
            yield return null;

            Assert.That(
                bootstrap.Mode,
                Is.EqualTo(PartyNightNetworkMode.DedicatedServer));
            Assert.That(bootstrap.NetworkManager.IsListening, Is.True);
            Assert.That(bootstrap.NetworkManager.IsServer, Is.True);
            Assert.That(
                bootstrap.NetworkManager.IsClient,
                Is.False,
                "Dedicated Party Night authority must not use NGO host mode.");
            Assert.That(bootstrap.IsAuthoritativeServer, Is.True);

            bootstrap.Shutdown();
            yield return null;

            Assert.That(bootstrap.NetworkManager.IsListening, Is.False);
            Assert.That(bootstrap.Mode, Is.EqualTo(PartyNightNetworkMode.None));
            Assert.That(bootstrap.IsAuthoritativeServer, Is.False);
        }

        private FoundationSceneComposition FindComposition()
        {
            var matches = loadedScene
                .GetRootGameObjects()
                .SelectMany(root =>
                    root.GetComponentsInChildren<FoundationSceneComposition>(true))
                .ToArray();

            Assert.That(matches, Has.Length.EqualTo(1));
            return matches[0];
        }

        private PartyNightNetworkBootstrap FindBootstrap()
        {
            var bootstrap = FindBootstrapOrNull();
            Assert.That(bootstrap, Is.Not.Null);
            return bootstrap;
        }

        private PartyNightNetworkBootstrap FindBootstrapOrNull()
        {
            if (!loadedScene.IsValid() || !loadedScene.isLoaded)
            {
                return null;
            }

            var matches = loadedScene
                .GetRootGameObjects()
                .SelectMany(root =>
                    root.GetComponentsInChildren<PartyNightNetworkBootstrap>(true))
                .ToArray();

            return matches.Length == 1 ? matches[0] : null;
        }
    }
}
