using System.Collections;
using System.Linq;
using NUnit.Framework;
using PartyNight.Input;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace PartyNight.Gameplay.Tests
{
    public sealed class LocalPlayerRuntimeTests
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
        public void FoundationSceneComposesOneLocalPlayerRig()
        {
            var composition = FindComposition();

            Assert.That(composition.IsComposed, Is.True);
            Assert.That(composition.LocalPlayer, Is.Not.Null);
            Assert.That(composition.Ground, Is.Not.Null);
            Assert.That(
                composition.LocalPlayer.GetComponent<CharacterController>(),
                Is.Not.Null);
            Assert.That(
                composition.LocalPlayer.GetComponent<PartyNightCharacterMotor>(),
                Is.Not.Null);
            Assert.That(composition.LocalController, Is.Not.Null);
            Assert.That(composition.LocalController.IsInitialized, Is.True);
            Assert.That(composition.OrbitCamera, Is.Not.Null);
            Assert.That(
                composition.OrbitCamera.Target,
                Is.EqualTo(composition.LocalPlayer.transform));
        }

        [UnityTest]
        public IEnumerator MovementIsCameraRelativeAndCollisionConstrained()
        {
            var composition = FindComposition();
            var controller = composition.LocalController;
            controller.enabled = false;

            var orbit = composition.OrbitCamera;
            orbit.SetOrbit(90f, 18f);

            var start = composition.LocalPlayer.transform.position;
            var frame = Frame(move: Vector2.up);

            for (var index = 0; index < 24; index++)
            {
                controller.Tick(frame, 0.02f);
                yield return null;
            }

            var end = composition.LocalPlayer.transform.position;
            Assert.That(end.x, Is.GreaterThan(start.x + 1f));
            Assert.That(Mathf.Abs(end.z - start.z), Is.LessThan(0.5f));
            Assert.That(end.y, Is.GreaterThanOrEqualTo(-0.02f));
        }

        [Test]
        public void JumpUsesExplicitGravityAndReturnsToGround()
        {
            var composition = FindComposition();
            var controller = composition.LocalController;
            controller.enabled = false;
            var motor = controller.Motor;

            for (var index = 0; index < 8; index++)
            {
                motor.Tick(Vector3.zero, false, 0.02f);
            }

            Assert.That(motor.IsGrounded, Is.True);

            var startY = composition.LocalPlayer.transform.position.y;
            motor.Tick(Vector3.zero, true, 0.02f);
            Assert.That(motor.Velocity.y, Is.GreaterThan(0f));

            var maximumY = composition.LocalPlayer.transform.position.y;
            for (var index = 0; index < 120; index++)
            {
                motor.Tick(Vector3.zero, false, 0.02f);
                maximumY = Mathf.Max(
                    maximumY,
                    composition.LocalPlayer.transform.position.y);
            }

            Assert.That(maximumY, Is.GreaterThan(startY + 0.2f));
            Assert.That(motor.IsGrounded, Is.True);
        }

        [Test]
        public void CameraTreatsPointerDeltaAndStickRateDifferently()
        {
            var orbit = FindComposition().OrbitCamera;

            orbit.SetOrbit(0f, 18f);
            orbit.ApplyLook(
                new Vector2(10f, 0f),
                PartyNightLookInputMode.Delta,
                0.01f);
            var pointerFastFrameYaw = orbit.YawDegrees;

            orbit.SetOrbit(0f, 18f);
            orbit.ApplyLook(
                new Vector2(10f, 0f),
                PartyNightLookInputMode.Delta,
                0.5f);
            var pointerSlowFrameYaw = orbit.YawDegrees;

            Assert.That(
                pointerSlowFrameYaw,
                Is.EqualTo(pointerFastFrameYaw).Within(0.0001f));

            orbit.SetOrbit(0f, 18f);
            orbit.ApplyLook(
                Vector2.right,
                PartyNightLookInputMode.Rate,
                0.1f);
            var shortStickFrameYaw = orbit.YawDegrees;

            orbit.SetOrbit(0f, 18f);
            orbit.ApplyLook(
                Vector2.right,
                PartyNightLookInputMode.Rate,
                0.2f);
            var longStickFrameYaw = orbit.YawDegrees;

            Assert.That(
                longStickFrameYaw,
                Is.EqualTo(shortStickFrameYaw * 2f).Within(0.001f));
        }

        private FoundationSceneComposition FindComposition()
        {
            var components = loadedScene
                .GetRootGameObjects()
                .SelectMany(root =>
                    root.GetComponents<FoundationSceneComposition>())
                .ToArray();

            Assert.That(components, Has.Length.EqualTo(1));
            return components[0];
        }

        private static PartyNightInputFrame Frame(
            Vector2? move = null,
            Vector2? look = null,
            PartyNightLookInputMode lookMode = PartyNightLookInputMode.Rate,
            bool jump = false)
        {
            return new PartyNightInputFrame(
                move ?? Vector2.zero,
                look ?? Vector2.zero,
                lookMode,
                jump,
                false,
                false,
                false,
                false,
                false);
        }
    }
}
