using UnityEngine;

namespace PartyNight.Input
{
    public readonly struct PartyNightInputFrame
    {
        public PartyNightInputFrame(
            Vector2 move,
            Vector2 look,
            PartyNightLookInputMode lookMode,
            bool jumpPressed,
            bool interactPressed,
            bool grabPressed,
            bool dashPressed,
            bool useItemPressed,
            bool emotePressed)
        {
            Move = move;
            Look = look;
            LookMode = lookMode;
            JumpPressed = jumpPressed;
            InteractPressed = interactPressed;
            GrabPressed = grabPressed;
            DashPressed = dashPressed;
            UseItemPressed = useItemPressed;
            EmotePressed = emotePressed;
        }

        public Vector2 Move { get; }
        public Vector2 Look { get; }
        public PartyNightLookInputMode LookMode { get; }
        public bool JumpPressed { get; }
        public bool InteractPressed { get; }
        public bool GrabPressed { get; }
        public bool DashPressed { get; }
        public bool UseItemPressed { get; }
        public bool EmotePressed { get; }
    }
}
