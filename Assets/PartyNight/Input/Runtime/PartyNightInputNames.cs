using System.Collections.Generic;

namespace PartyNight.Input
{
    public static class PartyNightInputNames
    {
        public const string AssetPath = "Assets/PartyNight/Input/PartyNightInputActions.inputactions";
        public const string GameplayMap = "Gameplay";
        public const string KeyboardMouseScheme = "KeyboardMouse";
        public const string GamepadScheme = "Gamepad";

        public const string Move = "Move";
        public const string Look = "Look";
        public const string Jump = "Jump";
        public const string Interact = "Interact";
        public const string Grab = "Grab";
        public const string Dash = "Dash";
        public const string UseItem = "UseItem";
        public const string Emote = "Emote";

        public static IReadOnlyList<string> RequiredGameplayActions { get; } = new[]
        {
            Move, Look, Jump, Interact, Grab, Dash, UseItem, Emote,
        };
    }

    public static class PartyNightTouchControlPaths
    {
        public const string Move = "<Gamepad>/leftStick";
        public const string Look = "<Gamepad>/rightStick";
        public const string Jump = "<Gamepad>/buttonSouth";
        public const string Interact = "<Gamepad>/buttonWest";
        public const string Grab = "<Gamepad>/leftShoulder";
        public const string Dash = "<Gamepad>/rightShoulder";
        public const string UseItem = "<Gamepad>/rightTrigger";
        public const string Emote = "<Gamepad>/dpad/up";
    }
}
