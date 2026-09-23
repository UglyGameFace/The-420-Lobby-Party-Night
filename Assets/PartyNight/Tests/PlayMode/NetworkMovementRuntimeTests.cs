using System.Collections;
using NUnit.Framework;
using PartyNight.Networking;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace PartyNight.Gameplay.Tests
{
    public sealed class NetworkMovementRuntimeTests
    {
        private const string FoundationScenePath =
            "Assets/PartyNight/Scenes/PartyNightFoundation.unity";
        private const ushort TestServerPort = 19779;

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
        public void CanonicalNetworkPlayerUsesExistingCharacterMotor()
        {
            var prefab = FindComposition().NetworkPlayerPrefab;

            Assert.That(prefab, Is.Not.Null);
            Assert.That(prefab.GetComponent<NetworkObject>(), Is.Not.Null);
            Assert.That(prefab.GetComponent<PartyNightNetworkPlayer>(), Is.Not.Null);
            Assert.That(prefab.GetComponent<CharacterController>(), Is.Not.Null);
            Assert.That(prefab.GetComponent<PartyNightCharacterMotor>(), Is.Not.Null);
            Assert.That(prefab.GetComponent<PartyNightNetworkMovement>(), Is.Not.Null);
            Assert.That(
                prefab.GetComponent<PartyNightLocalPlayerController>(),
                Is.Null,
                "Network player must reuse the motor without cloning the local input controller.");
        }

        [UnityTest]
        public IEnumerator ServerMovesSpawnedPlayerThroughCanonicalMotor()
        {
            var setup = SpawnServerOwnedPlayer();
            while (setup.MoveNext())
            {
                yield return setup.Current;
            }

            var networkObject = spawnedPlayer.GetComponent<NetworkObject>();
            var movement = spawnedPlayer.GetComponent<PartyNightNetworkMovement>();
            var controller = spawnedPlayer.GetComponent<CharacterController>();
            var start = spawnedPlayer.transform.position;

            Assert.That(controller.enabled, Is.True);
            Assert.That(
                movement.PositionWritePermission,
                Is.EqualTo(NetworkVariableWritePermission.Server));
            Assert.That(
                movement.YawWritePermission,
                Is.EqualTo(NetworkVariableWritePermission.Server));
            Assert.That(
                movement.SubmitOwnerIntent(Vector3.forward, false, 1),
                Is.False,
                "Dedicated server is not a client and must not use the client intent submission path.");

            Assert.That(
                movement.TryAcceptServerIntent(
                    networkObject.OwnerClientId,
                    Vector3.forward,
                    false,
                    1),
                Is.True);

            for (var i = 0; i < 8; i++)
            {
                Assert.That(
                    movement.SimulateAuthoritativeStep(0.05f),
                    Is.True);
            }

            Assert.That(
                spawnedPlayer.transform.position.z,
                Is.GreaterThan(start.z + 0.1f),
                "Server-authoritative intent must advance the canonical CharacterController motor.");
            Assert.That(
                Vector3.Distance(
                    movement.AuthoritativePosition,
                    spawnedPlayer.transform.position),
                Is.LessThan(0.001f),
                "Server must publish the authoritative motor position.");
        }

        [UnityTest]
        public IEnumerator ServerRejectsInvalidDuplicateAndStaleMovementIntent()
        {
            var setup = SpawnServerOwnedPlayer();
            while (setup.MoveNext())
            {
                yield return setup.Current;
            }

            var networkObject = spawnedPlayer.GetComponent<NetworkObject>();
            var movement = spawnedPlayer.GetComponent<PartyNightNetworkMovement>();
            var ownerId = networkObject.OwnerClientId;
            var wrongOwnerId =
                ownerId == ulong.MaxValue ? ownerId - 1 : ownerId + 1;

            Assert.That(
                movement.TryAcceptServerIntent(
                    wrongOwnerId,
                    Vector3.forward,
                    false,
                    10),
                Is.False,
                "Server must reject movement intent from a non-owner.");

            Assert.That(
                movement.TryAcceptServerIntent(
                    ownerId,
                    new Vector3(float.NaN, 0f, 0f),
                    false,
                    10),
                Is.False,
                "Server must reject non-finite movement intent.");

            Assert.That(
                movement.TryAcceptServerIntent(
                    ownerId,
                    new Vector3(10f, 0f, 0f),
                    true,
                    10),
                Is.True);
            Assert.That(
                movement.CurrentServerMoveIntent.magnitude,
                Is.EqualTo(1f).Within(0.0001f),
                "Server must clamp movement intent magnitude.");

            Assert.That(
                movement.TryAcceptServerIntent(
                    ownerId,
                    Vector3.forward,
                    false,
                    10),
                Is.False,
                "Duplicate movement sequence must be rejected.");
            Assert.That(
                movement.TryAcceptServerIntent(
                    ownerId,
                    Vector3.forward,
                    false,
                    9),
                Is.False,
                "Older movement sequence must be rejected.");
            Assert.That(
                movement.TryAcceptServerIntent(
                    ownerId,
                    Vector3.forward,
                    false,
                    11),
                Is.True,
                "Newer owner movement sequence must be accepted.");

            Assert.That(
                movement.SimulateAuthoritativeStep(
                    PartyNightNetworkMovement.IntentTimeoutSeconds + 0.01f),
                Is.True);
            Assert.That(
                movement.CurrentServerMoveIntent,
                Is.EqualTo(Vector3.zero),
                "Stale network intent must decay to zero on the authoritative server.");
        }

        private IEnumerator SpawnServerOwnedPlayer()
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
            Assert.That(networkObject, Is.Not.Null);
            networkObject.Spawn();
            yield return null;

            var movement = spawnedPlayer.GetComponent<PartyNightNetworkMovement>();

            Assert.That(networkObject.IsSpawned, Is.True);
            Assert.That(movement.IsServer, Is.True);
            Assert.That(movement.IsClient, Is.False);
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
