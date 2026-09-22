using System;
using PartyNight.Gameplay;
using UnityEngine;

namespace PartyNight.Foundation.Editor
{
    public static class HotboxHavocPrototypeValidator
    {
        public static void Validate()
        {
            if (Shader.Find("Universal Render Pipeline/Unlit") == null)
            {
                throw new InvalidOperationException(
                    "URP Unlit shader is unavailable for Hotbox Havoc prototype visuals.");
            }

            if (HotboxHavocRoundController.CountdownSeconds <= 0f ||
                HotboxHavocRoundController.ActiveRoundSeconds <= 0f ||
                HotboxHavocRoundController.ExposureToEliminateSeconds <= 0f)
            {
                throw new InvalidOperationException(
                    "Hotbox Havoc prototype timing constants must stay positive.");
            }

            if (HotboxHavocRoundController.EndClearRadius <= 0f ||
                HotboxHavocRoundController.EndClearRadius >=
                    HotboxHavocRoundController.StartClearRadius)
            {
                throw new InvalidOperationException(
                    "Hotbox Havoc clear zone must shrink to a positive smaller radius.");
            }

            if (HotboxHavocPrototype.VisualCaptureRelativePath.StartsWith(
                    "Assets/",
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    "Visual validation output must never be generated into Assets.");
            }

            Debug.Log("Party Night Hotbox Havoc prototype validation passed.");
        }
    }
}
