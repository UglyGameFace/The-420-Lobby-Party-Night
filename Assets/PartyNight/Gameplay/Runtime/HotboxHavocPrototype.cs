using UnityEngine;

namespace PartyNight.Gameplay
{
    [DisallowMultipleComponent]
    public sealed class HotboxHavocPrototype : MonoBehaviour
    {
        public const string RuntimeName = "Hotbox Havoc Local Prototype";
        public const string VisualCaptureRelativePath =
            "VisualValidation/HotboxHavoc_Overview.png";

        private HotboxHavocRoundController roundController;
        private HotboxHavocPrototypeVisuals visuals;
        private HotboxHavocPrototypeHud hud;
        private bool initialized;

        public bool IsInitialized => initialized;
        public HotboxHavocRoundController RoundController => roundController;
        public HotboxHavocPrototypeVisuals Visuals => visuals;
        public HotboxHavocPrototypeHud Hud => hud;

        public void Initialize(
            Transform localPlayer,
            PartyNightCharacterMotor motor,
            PartyNightOrbitCamera orbitCamera)
        {
            if (initialized)
            {
                throw new System.InvalidOperationException(
                    "HotboxHavocPrototype is already initialized.");
            }

            if (localPlayer == null) throw new System.ArgumentNullException(nameof(localPlayer));
            if (motor == null) throw new System.ArgumentNullException(nameof(motor));
            if (orbitCamera == null) throw new System.ArgumentNullException(nameof(orbitCamera));

            roundController = gameObject.AddComponent<HotboxHavocRoundController>();
            roundController.Initialize(localPlayer, motor, transform.position);

            visuals = gameObject.AddComponent<HotboxHavocPrototypeVisuals>();
            visuals.Initialize(roundController, localPlayer);

            hud = gameObject.AddComponent<HotboxHavocPrototypeHud>();
            hud.Initialize(roundController);

            initialized = true;
        }
    }
}
