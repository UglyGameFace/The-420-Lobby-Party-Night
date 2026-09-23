using System.Collections;
using NUnit.Framework;
using PartyNight.Input;
using PartyNight.Networking;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace PartyNight.Gameplay.Tests
{
    public sealed class NetworkOwnerBridgeRuntimeTests
    {
        private const string FoundationScenePath =
            "Assets/PartyNight/Scenes/PartyNightFoundation.unity";
        private const ushort TestServerPort = 19781;

        private Scene loadedScene;
        private GameObject spawnedPlayer;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            DestroyAllNetworkBootstraps();
            yield return null;

            var existing = SceneManager.GetSceneByPath(FoundationScenePath);
            if (existing.IsValid() && existing.isLoaded)
            {
                var unloadExisting = SceneManager.UnloadSceneAsync(existing);
                if (unloadExisting != null)
                {
                    while (!unloadExisting.isDone)
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
            if (spawnedPlayer != null)
            {
                var networkObject = spawnedPlayer.GetComponent<NetworkObject>();
                if (networkObject != null &&
                    networkObject.IsSpawned &&
                    NetworkManager.Singleton != null &&
                    NetworkManager.Singleton.IsServer)
                {
                    networkObject.Despawn(true);
                }
                else
                {
                    Object.Destroy(spawnedPlayer);
                }

                spawnedPlayer = null;
                yield return null;
            }

            DestroyAllNetworkBootstraps();
            yield return null;

            if (loadedScene.IsValid() && loadedScene.isLoaded)
            {
                var unload = SceneManager.UnloadSceneAsync(loadedScene);
                if (unload != null)
                {
                    while (!unload.isDone)
                    {
                        yield return null;
                    }
                }
            }
        }

        [Test]
        public void FoundationComposesOneOwnerBridgeInStandaloneMode()
        {
            var composition = FindComposition();
            var bridge = composition.NetworkOwnerBridge;

            Assert.That(bridge, Is.Not.Null);
            Assert.That(bridge.IsInitialized, Is.True);
            Assert.That(bridge.InputSource, Is.SameAs(composition.LocalController));
            Assert.That(bridge.NetworkSessionActive, Is.False);
            Assert.That(bridge.IsBound, Is.False);
            Assert.That(
                composition.LocalController.AutomaticMotorControlEnabled,
                Is.True);
            Assert.That(
                composition.LocalPlayer.GetComponent<CharacterController>().enabled,
                Is.True,
                "Standalone CharacterController must return only after the network session ends.");
            Assert.That(composition.HotboxPrototype.gameObject.activeSelf, Is.True);
            Assert.That(
                composition.OrbitCamera.Target,
                Is.EqualTo(composition.LocalPlayer.transform));
        }

        [UnityTest]
        public IEnumerator DedicatedServerSuppressesAndRestoresStandaloneAuthority()
        {
            var composition = FindComposition();
            var bridge = composition.NetworkOwnerBridge;
            var bootstrap = composition.NetworkBootstrap;

            Assert.That(
                bootstrap.StartDedicatedServer(TestServerPort),
                Is.True);
            yield return null;

            Assert.That(
                bootstrap.Mode,
                Is.EqualTo(PartyNightNetworkMode.DedicatedServer));
            Assert.That(bridge.NetworkSessionActive, Is.True);
            Assert.That(bridge.IsBound, Is.False);
            Assert.That(bridge.StandalonePrototypeSuppressed, Is.True);
            Assert.That(
                composition.LocalController.AutomaticMotorControlEnabled,
                Is.False);
            Assert.That(
                composition.LocalPlayer.GetComponent<CharacterController>().enabled,
                Is.False,
                "Standalone prototype collider must not interfere with authoritative network players.");
            Assert.That(composition.HotboxPrototype.gameObject.activeSelf, Is.False);

            bootstrap.Shutdown();
            yield return null;

            Assert.That(
                bootstrap.Mode,
                Is.EqualTo(PartyNightNetworkMode.None));
            Assert.That(bridge.NetworkSessionActive, Is.False);
            Assert.That(bridge.IsBound, Is.False);
            Assert.That(
                composition.LocalController.AutomaticMotorControlEnabled,
                Is.True);
            Assert.That(
                composition.LocalPlayer.GetComponent<CharacterController>().enabled,
                Is.True,
                "Standalone CharacterController must be restored when the network session ends.");
            Assert.That(composition.HotboxPrototype.gameObject.activeSelf, Is.True);
            Assert.That(
                composition.OrbitCamera.Target,
                Is.EqualTo(composition.LocalPlayer.transform));
        }

        [Test]
        public void OwnerBridgeKeepsLookLocalAndMapsMoveThroughCameraBasis()
        {
            var composition = FindComposition();
            var bridge = composition.NetworkOwnerBridge;
            var orbit = composition.OrbitCamera;

            orbit.SetOrbit(0f, 18f);
            var yawBefore = orbit.YawDegrees;

            var frame = new PartyNightInputFrame(
                Vector2.up,
                new Vector2(1f, 0f),
                PartyNightLookInputMode.Rate,
                jumpPressed: true,
                interactPressed: false,
                grabPressed: false,
                dashPressed: false,
                useItemPressed: false,
                emotePressed: false);

            var desiredWorldMove =
                bridge.PrepareDesiredWorldMove(frame, 0.5f);

            Assert.That(
                orbit.YawDegrees,
                Is.GreaterThan(yawBefore),
                "Look must update the local orbit camera immediately.");

            var expectedDirection = orbit.PlanarForward;
            Assert.That(
                Vector3.Distance(desiredWorldMove, expectedDirection),
                Is.LessThan(0.0001f),
                "Move must be converted through the current orbit-camera basis.");
            Assert.That(
                desiredWorldMove.y,
                Is.EqualTo(0f).Within(0.0001f));
            Assert.That(
                desiredWorldMove.magnitude,
                Is.LessThanOrEqualTo(1.0001f));
        }

        [UnityTest]
        public IEnumerator NetworkPlayerRaisesDespawnLifecycleSignal()
        {
            var composition = FindComposition();
            var bootstrap = composition.NetworkBootstrap;

            Assert.That(
                bootstrap.StartDedicatedServer(TestServerPort),
                Is.True);
            yield return null;

            spawnedPlayer = Object.Instantiate(composition.NetworkPlayerPrefab);
            SceneManager.MoveGameObjectToScene(spawnedPlayer, loadedScene);
            spawnedPlayer.transform.position = new Vector3(0f, 0.05f, 0f);
            Physics.SyncTransforms();

            var networkObject = spawnedPlayer.GetComponent<NetworkObject>();
            var identity = spawnedPlayer.GetComponent<PartyNightNetworkPlayer>();
            Assert.That(networkObject, Is.Not.Null);
            Assert.That(identity, Is.Not.Null);

            var despawned = false;
            identity.NetworkDespawned += _ => despawned = true;

            networkObject.Spawn();
            yield return null;

            Assert.That(networkObject.IsSpawned, Is.True);

            networkObject.Despawn(true);
            spawnedPlayer = null;
            yield return null;

            Assert.That(
                despawned,
                Is.True,
                "Owned-player bridge requires a deterministic despawn release signal.");
        }

        private FoundationSceneComposition FindComposition()
        {
            foreach (var root in loadedScene.GetRootGameObjects())
            {
                var composition =
                    root.GetComponentInChildren<FoundationSceneComposition>(true);
                if (composition != null)
                {
                    return composition;
                }
            }

            Assert.Fail("FoundationSceneComposition was not found.");
            return null;
        }

        private static void DestroyAllNetworkBootstraps()
        {
            var bootstraps =
                Object.FindObjectsByType<PartyNightNetworkBootstrap>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None);

            foreach (var bootstrap in bootstraps)
            {
                bootstrap.Shutdown();
                Object.Destroy(bootstrap.gameObject);
            }
        }
    }
}
