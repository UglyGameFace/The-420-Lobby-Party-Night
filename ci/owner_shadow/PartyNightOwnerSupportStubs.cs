using System;
using PartyNight.Input;
using UnityEngine;

namespace PartyNight.Input
{
    public sealed class PartyNightInputReader : IDisposable
    {
        public PartyNightInputFrame NextFrame { get; set; }

        public static PartyNightInputReader CreateFromProjectWideActions() =>
            new PartyNightInputReader();

        public void Enable() { }
        public void Disable() { }
        public PartyNightInputFrame ReadFrame() => NextFrame;
        public void Dispose() { }
    }
}

namespace PartyNight.Gameplay
{
    public sealed class HotboxHavocRoundController
    {
        public int RestartCount { get; private set; }

        public void RestartRound()
        {
            RestartCount++;
        }
    }

    public sealed class HotboxHavocPrototype : MonoBehaviour
    {
        public HotboxHavocRoundController RoundController { get; } =
            new HotboxHavocRoundController();
    }
}
