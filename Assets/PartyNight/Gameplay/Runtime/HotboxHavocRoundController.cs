using UnityEngine;

namespace PartyNight.Gameplay
{
    [DisallowMultipleComponent]
    public sealed class HotboxHavocRoundController : MonoBehaviour
    {
        public const float CountdownSeconds = 3f;
        public const float ActiveRoundSeconds = 20f;
        public const float ResultsSeconds = 4f;
        public const float StartClearRadius = 8.5f;
        public const float EndClearRadius = 3f;
        public const float ExposureToEliminateSeconds = 2.5f;
        public const float ExposureRecoveryPerSecond = 1.5f;
        public const float FallEliminationY = -5f;

        private Transform localPlayer;
        private PartyNightCharacterMotor motor;
        private Vector3 arenaCenter;
        private Vector3 spawnPosition;
        private float phaseElapsed;
        private float activeElapsed;
        private float exposure;
        private bool initialized;
        private int roundIndex;

        public HotboxHavocRoundPhase Phase { get; private set; }
        public bool IsInitialized => initialized;
        public int RoundIndex => roundIndex;
        public float HazeNormalized => Mathf.Clamp01(activeElapsed / ActiveRoundSeconds);
        public float ExposureNormalized => Mathf.Clamp01(exposure / ExposureToEliminateSeconds);
        public float ClearRadius => Mathf.Lerp(StartClearRadius, EndClearRadius, HazeNormalized);
        public float ActiveTimeElapsed => activeElapsed;
        public float ExposureSeconds => exposure;
        public Vector3 ArenaCenter => arenaCenter;
        public Vector3 SpawnPosition => spawnPosition;

        public float TimeRemaining => Phase switch
        {
            HotboxHavocRoundPhase.Countdown => Mathf.Max(0f, CountdownSeconds - phaseElapsed),
            HotboxHavocRoundPhase.Active => Mathf.Max(0f, ActiveRoundSeconds - activeElapsed),
            _ => Mathf.Max(0f, ResultsSeconds - phaseElapsed),
        };

        public bool IsPlayerOutsideClearZone
        {
            get
            {
                if (!initialized || localPlayer == null)
                {
                    return false;
                }

                var offset = localPlayer.position - arenaCenter;
                offset.y = 0f;
                return offset.sqrMagnitude > ClearRadius * ClearRadius;
            }
        }

        public void Initialize(
            Transform player,
            PartyNightCharacterMotor characterMotor,
            Vector3 center)
        {
            if (initialized)
            {
                throw new System.InvalidOperationException(
                    "HotboxHavocRoundController is already initialized.");
            }

            localPlayer = player != null
                ? player
                : throw new System.ArgumentNullException(nameof(player));
            motor = characterMotor != null
                ? characterMotor
                : throw new System.ArgumentNullException(nameof(characterMotor));
            arenaCenter = center;
            spawnPosition = localPlayer.position;
            initialized = true;
            RestartRound();
        }

        private void Update()
        {
            if (initialized)
            {
                Tick(Time.deltaTime);
            }
        }

        public void Tick(float deltaTime)
        {
            if (!initialized)
            {
                throw new System.InvalidOperationException(
                    "HotboxHavocRoundController must be initialized before Tick.");
            }

            if (deltaTime <= 0f)
            {
                return;
            }

            if (Phase == HotboxHavocRoundPhase.Countdown)
            {
                phaseElapsed += deltaTime;
                if (phaseElapsed >= CountdownSeconds)
                {
                    Phase = HotboxHavocRoundPhase.Active;
                    phaseElapsed = 0f;
                    activeElapsed = 0f;
                    exposure = 0f;
                }

                return;
            }

            if (Phase == HotboxHavocRoundPhase.Active)
            {
                activeElapsed = Mathf.Min(ActiveRoundSeconds, activeElapsed + deltaTime);

                if (localPlayer.position.y < FallEliminationY)
                {
                    EnterResult(HotboxHavocRoundPhase.Eliminated);
                    return;
                }

                if (IsPlayerOutsideClearZone)
                {
                    exposure = Mathf.Min(
                        ExposureToEliminateSeconds,
                        exposure + deltaTime);
                }
                else
                {
                    exposure = Mathf.Max(
                        0f,
                        exposure - ExposureRecoveryPerSecond * deltaTime);
                }

                if (exposure >= ExposureToEliminateSeconds)
                {
                    EnterResult(HotboxHavocRoundPhase.Eliminated);
                    return;
                }

                if (activeElapsed >= ActiveRoundSeconds)
                {
                    EnterResult(HotboxHavocRoundPhase.Won);
                }

                return;
            }

            phaseElapsed += deltaTime;
            if (phaseElapsed >= ResultsSeconds)
            {
                RestartRound();
            }
        }

        public void RestartRound()
        {
            if (!initialized)
            {
                throw new System.InvalidOperationException(
                    "HotboxHavocRoundController must be initialized before restart.");
            }

            roundIndex++;
            Phase = HotboxHavocRoundPhase.Countdown;
            phaseElapsed = 0f;
            activeElapsed = 0f;
            exposure = 0f;

            var characterController = motor.CharacterController;
            var wasEnabled = characterController != null && characterController.enabled;
            if (characterController != null && wasEnabled)
            {
                characterController.enabled = false;
            }

            localPlayer.position = spawnPosition;
            localPlayer.rotation = Quaternion.identity;
            motor.ResetMotion();

            if (characterController != null && wasEnabled)
            {
                characterController.enabled = true;
            }

            Physics.SyncTransforms();
        }

        private void EnterResult(HotboxHavocRoundPhase result)
        {
            Phase = result;
            phaseElapsed = 0f;
        }
    }
}
