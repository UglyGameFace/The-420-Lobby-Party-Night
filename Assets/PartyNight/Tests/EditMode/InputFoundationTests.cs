using System.Linq;
using NUnit.Framework;
using PartyNight.Foundation.Editor;
using PartyNight.Input;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PartyNight.Foundation.Tests
{
    public sealed class InputFoundationTests
    {
        [Test]
        public void ImportedInputFoundationMatchesContract()
        {
            Assert.DoesNotThrow(InputFoundationValidator.Validate);
        }

        [Test]
        public void GenericGamepadAndKeyboardBindingsResolveToRealDevices()
        {
            var source = AssetDatabase.LoadAssetAtPath<InputActionAsset>(PartyNightInputNames.AssetPath);
            Assert.That(source, Is.Not.Null);

            var gamepad = InputSystem.AddDevice<Gamepad>();
            var keyboard = InputSystem.AddDevice<Keyboard>();
            var mouse = InputSystem.AddDevice<Mouse>();
            var actions = Object.Instantiate(source);

            try
            {
                actions.devices = new InputDevice[] { gamepad, keyboard, mouse };
                actions.Enable();

                var gameplay = actions.FindActionMap(PartyNightInputNames.GameplayMap, true);
                var move = gameplay.FindAction(PartyNightInputNames.Move, true);
                var look = gameplay.FindAction(PartyNightInputNames.Look, true);
                var jump = gameplay.FindAction(PartyNightInputNames.Jump, true);

                Assert.That(move.controls.Any(control => control.device == gamepad), Is.True);
                Assert.That(move.controls.Any(control => control.device == keyboard), Is.True);
                Assert.That(look.controls.Any(control => control.device == gamepad), Is.True);
                Assert.That(look.controls.Any(control => control.device == mouse), Is.True);
                Assert.That(jump.controls.Any(control => control.device == gamepad), Is.True);
                Assert.That(jump.controls.Any(control => control.device == keyboard), Is.True);
            }
            finally
            {
                actions.Disable();
                Object.DestroyImmediate(actions);
                InputSystem.RemoveDevice(gamepad);
                InputSystem.RemoveDevice(keyboard);
                InputSystem.RemoveDevice(mouse);
            }
        }

        [Test]
        public void ProjectWideReaderUsesAssignedAsset()
        {
            Assert.That(InputSystem.actions, Is.Not.Null);
            Assert.That(
                AssetDatabase.GetAssetPath(InputSystem.actions),
                Is.EqualTo(PartyNightInputNames.AssetPath));

            using var reader = PartyNightInputReader.CreateFromProjectWideActions();
            reader.Enable();
            Assert.That(reader.Enabled, Is.True);
        }

        [Test]
        public void RuntimeReaderConsumesLogicalActionFrame()
        {
            var source = AssetDatabase.LoadAssetAtPath<InputActionAsset>(PartyNightInputNames.AssetPath);
            Assert.That(source, Is.Not.Null);

            using var reader = new PartyNightInputReader(source);
            reader.Enable();
            Assert.That(reader.Enabled, Is.True);
            Assert.DoesNotThrow(() => reader.ReadFrame());
        }
    }
}
