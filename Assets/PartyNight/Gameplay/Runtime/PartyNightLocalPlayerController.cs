using PartyNight.Input;
using UnityEngine;

namespace PartyNight.Gameplay
{
    [DisallowMultipleComponent]
    public sealed class PartyNightLocalPlayerController : MonoBehaviour
    {
        private PartyNightCharacterMotor motor;
        private PartyNightOrbitCamera orbitCamera;
        private PartyNightInputReader inputReader;
        private bool initialized;

        public bool IsInitialized => initialized;
        public PartyNightCharacterMotor Motor => motor;
        public PartyNightOrbitCamera OrbitCamera => orbitCamera;

        public void Initialize(
            PartyNightCharacterMotor characterMotor,
            PartyNightOrbitCamera cameraController)
        {
            if (initialized)
            {
                throw new System.InvalidOperationException(
                    "PartyNightLocalPlayerController is already initialized.");
            }

            motor = characterMotor != null
                ? characterMotor
                : throw new System.ArgumentNullException(nameof(characterMotor));
            orbitCamera = cameraController != null
                ? cameraController
                : throw new System.ArgumentNullException(nameof(cameraController));

            inputReader = PartyNightInputReader.CreateFromProjectWideActions();
            inputReader.Enable();
            initialized = true;
        }

        private void Update()
        {
            if (!initialized)
            {
                return;
            }

            Tick(inputReader.ReadFrame(), Time.deltaTime);
        }

        public void Tick(PartyNightInputFrame frame, float deltaTime)
        {
            if (!initialized)
            {
                throw new System.InvalidOperationException(
                    "PartyNightLocalPlayerController must be initialized before Tick.");
            }

            orbitCamera.ApplyLook(frame.Look, frame.LookMode, deltaTime);

            var desiredWorldMove =
                orbitCamera.PlanarRight * frame.Move.x +
                orbitCamera.PlanarForward * frame.Move.y;
            desiredWorldMove = Vector3.ClampMagnitude(desiredWorldMove, 1f);

            motor.Tick(desiredWorldMove, frame.JumpPressed, deltaTime);
        }

        private void OnEnable()
        {
            if (initialized)
            {
                inputReader.Enable();
            }
        }

        private void OnDisable()
        {
            inputReader?.Disable();
        }

        private void OnDestroy()
        {
            inputReader?.Dispose();
            inputReader = null;
            initialized = false;
        }
    }
}
