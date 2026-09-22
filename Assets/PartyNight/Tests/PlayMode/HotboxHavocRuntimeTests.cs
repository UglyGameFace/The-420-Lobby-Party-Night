using System.Collections;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace PartyNight.Gameplay.Tests
{
    public sealed class HotboxHavocRuntimeTests
    {
        private const string FoundationScenePath =
            "Assets/PartyNight/Scenes/PartyNightFoundation.unity";

        private Scene loadedScene;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
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
        public void SceneComposesOneVisibleLocalHotboxPrototype()
        {
            var prototype = FindPrototype();

            Assert.That(prototype.IsInitialized, Is.True);
            Assert.That(prototype.RoundController.IsInitialized, Is.True);
            Assert.That(prototype.Visuals.IsInitialized, Is.True);
            Assert.That(prototype.Visuals.VisualRoot, Is.Not.Null);
            Assert.That(prototype.Visuals.SafeZoneDisc, Is.Not.Null);
            Assert.That(prototype.Visuals.PlayerBeacon, Is.Not.Null);
            Assert.That(prototype.Hud.IsInitialized, Is.True);

            var spawnMarkers = prototype.Visuals.VisualRoot
                .GetComponentsInChildren<Transform>(true)
                .Count(child => child.name.StartsWith("Future Spawn"));

            Assert.That(
                spawnMarkers,
                Is.EqualTo(HotboxHavocPrototypeVisuals.SpawnMarkerCount));
        }

        [Test]
        public void RoundTransitionsCountdownToActiveAndWin()
        {
            var round = FindPrototype().RoundController;
            round.enabled = false;
            round.RestartRound();

            Assert.That(round.Phase, Is.EqualTo(HotboxHavocRoundPhase.Countdown));

            round.Tick(HotboxHavocRoundController.CountdownSeconds + 0.01f);
            Assert.That(round.Phase, Is.EqualTo(HotboxHavocRoundPhase.Active));

            round.Tick(HotboxHavocRoundController.ActiveRoundSeconds);
            Assert.That(round.Phase, Is.EqualTo(HotboxHavocRoundPhase.Won));
            Assert.That(round.HazeNormalized, Is.EqualTo(1f).Within(0.0001f));
            Assert.That(
                round.ClearRadius,
                Is.EqualTo(HotboxHavocRoundController.EndClearRadius)
                    .Within(0.0001f));
        }

        [Test]
        public void OutsideClearZoneBuildsExposureAndEliminates()
        {
            var round = FindPrototype().RoundController;
            round.enabled = false;
            round.RestartRound();
            round.Tick(HotboxHavocRoundController.CountdownSeconds + 0.01f);

            var player = FindComposition().LocalPlayer.transform;
            player.position =
                round.ArenaCenter +
                Vector3.right *
                (HotboxHavocRoundController.StartClearRadius + 0.6f);
            Physics.SyncTransforms();

            round.Tick(
                HotboxHavocRoundController.ExposureToEliminateSeconds + 0.05f);

            Assert.That(round.Phase, Is.EqualTo(HotboxHavocRoundPhase.Eliminated));
            Assert.That(round.ExposureNormalized, Is.EqualTo(1f).Within(0.0001f));
        }

        [Test]
        public void RestartRestoresPlayerAndRoundState()
        {
            var round = FindPrototype().RoundController;
            round.enabled = false;
            round.RestartRound();
            round.Tick(HotboxHavocRoundController.CountdownSeconds + 0.01f);

            var player = FindComposition().LocalPlayer.transform;
            player.position = round.ArenaCenter + Vector3.right * 9.2f;
            Physics.SyncTransforms();
            round.Tick(
                HotboxHavocRoundController.ExposureToEliminateSeconds + 0.05f);
            Assert.That(round.Phase, Is.EqualTo(HotboxHavocRoundPhase.Eliminated));

            round.RestartRound();

            Assert.That(round.Phase, Is.EqualTo(HotboxHavocRoundPhase.Countdown));
            Assert.That(round.ExposureSeconds, Is.EqualTo(0f));
            Assert.That(
                Vector3.Distance(player.position, round.SpawnPosition),
                Is.LessThan(0.001f));
        }

        [UnityTest]
        public IEnumerator CapturesRealUnityVisualProgressArtifact()
        {
            var prototype = FindPrototype();
            var round = prototype.RoundController;
            round.enabled = false;
            round.RestartRound();
            round.Tick(HotboxHavocRoundController.CountdownSeconds + 0.01f);
            round.Tick(9f);
            prototype.Visuals.Refresh();

            yield return null;

            var cameraObject = new GameObject("Hotbox Havoc Visual Validation Camera");
            var camera = cameraObject.AddComponent<Camera>();
            camera.enabled = false;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.025f, 0.01f, 0.045f, 1f);
            camera.fieldOfView = 52f;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 100f;
            camera.transform.position = new Vector3(13f, 11f, -14f);
            camera.transform.LookAt(new Vector3(0f, 0.7f, 0f));

            var renderTexture = new RenderTexture(1280, 720, 24);
            var texture = new Texture2D(1280, 720, TextureFormat.RGB24, false);

            try
            {
                renderTexture.Create();
                camera.targetTexture = renderTexture;
                camera.Render();

                var previous = RenderTexture.active;
                RenderTexture.active = renderTexture;
                texture.ReadPixels(new Rect(0f, 0f, 1280f, 720f), 0, 0);
                texture.Apply();
                RenderTexture.active = previous;

                var png = texture.EncodeToPNG();
                Assert.That(png, Is.Not.Null);
                Assert.That(
                    png.Length,
                    Is.GreaterThan(10000),
                    "Visual capture looks suspiciously empty or uniform.");

                var projectRoot = Path.GetFullPath(
                    Path.Combine(Application.dataPath, ".."));
                var capturePath = Path.Combine(
                    projectRoot,
                    HotboxHavocPrototype.VisualCaptureRelativePath);
                var directory = Path.GetDirectoryName(capturePath);
                Assert.That(directory, Is.Not.Null);
                Directory.CreateDirectory(directory);
                File.WriteAllBytes(capturePath, png);

                var revision =
                    System.Environment.GetEnvironmentVariable("BUILD_REVISION") ??
                    "local";
                var manifestPath = Path.Combine(
                    projectRoot,
                    HotboxHavocPrototype.VisualCaptureManifestRelativePath);
                File.WriteAllText(
                    manifestPath,
                    "{\"revision\":\"" + revision +
                    "\",\"unityVersion\":\"" + Application.unityVersion +
                    "\",\"width\":1280,\"height\":720}");

                Assert.That(File.Exists(capturePath), Is.True);
                Assert.That(File.Exists(manifestPath), Is.True);
                Assert.That(
                    new FileInfo(capturePath).Length,
                    Is.EqualTo(png.LongLength));

                Debug.Log(
                    $"Party Night visual validation captured: {capturePath}");
            }
            finally
            {
                camera.targetTexture = null;
                renderTexture.Release();
                Object.Destroy(renderTexture);
                Object.Destroy(texture);
                Object.Destroy(cameraObject);
            }

            yield return null;
        }

        private HotboxHavocPrototype FindPrototype()
        {
            var prototypes = loadedScene
                .GetRootGameObjects()
                .SelectMany(root =>
                    root.GetComponentsInChildren<HotboxHavocPrototype>(true))
                .ToArray();

            Assert.That(prototypes, Has.Length.EqualTo(1));
            return prototypes[0];
        }

        private FoundationSceneComposition FindComposition()
        {
            var compositions = loadedScene
                .GetRootGameObjects()
                .SelectMany(root =>
                    root.GetComponents<FoundationSceneComposition>())
                .ToArray();

            Assert.That(compositions, Has.Length.EqualTo(1));
            return compositions[0];
        }
    }
}
