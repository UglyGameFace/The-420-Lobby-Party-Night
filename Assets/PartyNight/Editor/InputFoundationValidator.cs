using System;
using System.Collections.Generic;
using System.Linq;
using PartyNight.Input;
using UnityEditor;
using UnityEngine.InputSystem;

namespace PartyNight.Foundation.Editor
{
    public static class InputFoundationValidator
    {
        private static readonly IReadOnlyDictionary<string, (InputActionType type, string controlType)> RequiredActions =
            new Dictionary<string, (InputActionType, string)>
            {
                [PartyNightInputNames.Move] = (InputActionType.Value, "Vector2"),
                [PartyNightInputNames.Look] = (InputActionType.Value, "Vector2"),
                [PartyNightInputNames.Jump] = (InputActionType.Button, "Button"),
                [PartyNightInputNames.Interact] = (InputActionType.Button, "Button"),
                [PartyNightInputNames.Grab] = (InputActionType.Button, "Button"),
                [PartyNightInputNames.Dash] = (InputActionType.Button, "Button"),
                [PartyNightInputNames.UseItem] = (InputActionType.Button, "Button"),
                [PartyNightInputNames.Emote] = (InputActionType.Button, "Button"),
            };

        private static readonly IReadOnlyDictionary<string, string> RequiredGamepadBindings =
            new Dictionary<string, string>
            {
                [PartyNightInputNames.Move] = PartyNightTouchControlPaths.Move,
                [PartyNightInputNames.Look] = PartyNightTouchControlPaths.Look,
                [PartyNightInputNames.Jump] = PartyNightTouchControlPaths.Jump,
                [PartyNightInputNames.Interact] = PartyNightTouchControlPaths.Interact,
                [PartyNightInputNames.Grab] = PartyNightTouchControlPaths.Grab,
                [PartyNightInputNames.Dash] = PartyNightTouchControlPaths.Dash,
                [PartyNightInputNames.UseItem] = PartyNightTouchControlPaths.UseItem,
                [PartyNightInputNames.Emote] = PartyNightTouchControlPaths.Emote,
            };

        public static void Validate()
        {
#if !ENABLE_INPUT_SYSTEM
            throw new InvalidOperationException("Party Night requires Active Input Handling = Input System Package (New).");
#endif
            var asset = AssetDatabase.LoadAssetAtPath<InputActionAsset>(PartyNightInputNames.AssetPath);
            if (asset == null) throw new InvalidOperationException($"InputActionAsset could not be imported: {PartyNightInputNames.AssetPath}");

            var projectWideActions = InputSystem.actions;
            if (projectWideActions == null)
                throw new InvalidOperationException("Party Night Project-wide Input Actions are not assigned.");
            if (!string.Equals(
                    AssetDatabase.GetAssetPath(projectWideActions),
                    PartyNightInputNames.AssetPath,
                    StringComparison.Ordinal))
                throw new InvalidOperationException(
                    $"Project-wide Input Actions must be {PartyNightInputNames.AssetPath}.");
            if (asset.actionMaps.Count != 1) throw new InvalidOperationException($"Expected exactly one Party Night action map, found {asset.actionMaps.Count}.");

            var gameplay = asset.FindActionMap(PartyNightInputNames.GameplayMap, true);
            if (gameplay.actions.Count != RequiredActions.Count)
                throw new InvalidOperationException($"Expected {RequiredActions.Count} gameplay actions, found {gameplay.actions.Count}.");

            foreach (var required in RequiredActions)
            {
                var action = gameplay.FindAction(required.Key, true);
                if (action.type != required.Value.type)
                    throw new InvalidOperationException($"{required.Key} must be {required.Value.type}, found {action.type}.");
                if (!string.Equals(action.expectedControlType, required.Value.controlType, StringComparison.Ordinal))
                    throw new InvalidOperationException($"{required.Key} expected control type must be {required.Value.controlType}, found {action.expectedControlType}.");
            }

            if (asset.controlSchemes.Count != 2)
            {
                throw new InvalidOperationException(
                    $"Expected exactly two input control schemes, found {asset.controlSchemes.Count}.");
            }

            RequireScheme(asset, PartyNightInputNames.KeyboardMouseScheme, "KeyboardMouse");
            RequireScheme(asset, PartyNightInputNames.GamepadScheme, "Gamepad");

            RequireBinding(gameplay, PartyNightInputNames.Move, "<Keyboard>/w", "KeyboardMouse");
            RequireBinding(gameplay, PartyNightInputNames.Move, "<Keyboard>/s", "KeyboardMouse");
            RequireBinding(gameplay, PartyNightInputNames.Move, "<Keyboard>/a", "KeyboardMouse");
            RequireBinding(gameplay, PartyNightInputNames.Move, "<Keyboard>/d", "KeyboardMouse");
            RequireBinding(gameplay, PartyNightInputNames.Look, "<Mouse>/delta", "KeyboardMouse");
            RequireBinding(gameplay, PartyNightInputNames.Jump, "<Keyboard>/space", "KeyboardMouse");
            RequireBinding(gameplay, PartyNightInputNames.Interact, "<Keyboard>/e", "KeyboardMouse");
            RequireBinding(gameplay, PartyNightInputNames.Grab, "<Keyboard>/f", "KeyboardMouse");
            RequireBinding(gameplay, PartyNightInputNames.Dash, "<Keyboard>/leftShift", "KeyboardMouse");
            RequireBinding(gameplay, PartyNightInputNames.UseItem, "<Mouse>/leftButton", "KeyboardMouse");
            RequireBinding(gameplay, PartyNightInputNames.Emote, "<Keyboard>/g", "KeyboardMouse");

            foreach (var required in RequiredGamepadBindings)
                RequireBinding(gameplay, required.Key, required.Value, "Gamepad");
        }

        private static void RequireScheme(InputActionAsset asset, string name, string bindingGroup)
        {
            var scheme = asset.FindControlScheme(name);
            if (!scheme.HasValue) throw new InvalidOperationException($"Missing input control scheme: {name}");
            if (!string.Equals(scheme.Value.bindingGroup, bindingGroup, StringComparison.Ordinal))
                throw new InvalidOperationException($"Control scheme {name} has binding group {scheme.Value.bindingGroup}, expected {bindingGroup}.");
        }

        private static void RequireBinding(InputActionMap map, string actionName, string path, string group)
        {
            var action = map.FindAction(actionName, true);
            var found = action.bindings.Any(binding =>
                string.Equals(binding.path, path, StringComparison.Ordinal) &&
                binding.groups.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).Contains(group));
            if (!found) throw new InvalidOperationException($"{actionName} is missing required {group} binding {path}.");
        }
    }
}
