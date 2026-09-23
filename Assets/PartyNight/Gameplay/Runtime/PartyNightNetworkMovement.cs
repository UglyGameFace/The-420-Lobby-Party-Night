using PartyNight.Networking;
using Unity.Netcode;
using UnityEngine;

namespace PartyNight.Gameplay
{
    [DisallowMultipleComponent]
    [RequireComponent(
        typeof(CharacterController),
        typeof(PartyNightCharacterMotor),
        typeof(PartyNightNetworkPlayer))]
    public sealed class PartyNightNetworkMovement : NetworkBehaviour
    {
        public const float IntentTimeoutSeconds = 0.25f;

        private readonly NetworkVariable<Vector3> authoritativePosition =
            new NetworkVariable<Vector3>(
                Vector3.zero,
                NetworkVariableReadPermission.Everyone,
                NetworkVariableWritePermission.Server);

        private readonly NetworkVariable<float> authoritativeYaw =
            new NetworkVariable<float>(
                0f,
                NetworkVariableReadPermission.Everyone,
                NetworkVariableWritePermission.Server);

        private CharacterController characterController;
        private PartyNightCharacterMotor motor;
        private Vector3 serverMoveIntent;
        private bool pendingJump;
        private bool hasAcceptedSequence;
        private uint lastAcceptedSequence;
        private float secondsSinceAcceptedIntent;

        public Vector3 AuthoritativePosition => authoritativePosition.Value;
        public float AuthoritativeYaw => authoritativeYaw.Value;
        public NetworkVariableWritePermission PositionWritePermission =>
            authoritativePosition.WritePerm;
        public NetworkVariableWritePermission YawWritePermission =>
            authoritativeYaw.WritePerm;
        public Vector3 CurrentServerMoveIntent => serverMoveIntent;
        public bool HasAcceptedSequence => hasAcceptedSequence;
        public uint LastAcceptedSequence => lastAcceptedSequence;

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
            motor = GetComponent<PartyNightCharacterMotor>();

            if (characterController == null || motor == null)
            {
                throw new System.InvalidOperationException(
                    "PartyNightNetworkMovement requires the canonical CharacterController motor.");
            }
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            serverMoveIntent = Vector3.zero;
            pendingJump = false;
            hasAcceptedSequence = false;
            lastAcceptedSequence = 0;
            secondsSinceAcceptedIntent = 0f;
            motor.ResetMotion();

            characterController.enabled = IsServer;

            if (IsServer)
            {
                PublishAuthoritativePose();
            }
            else
            {
                ApplyAuthoritativePose();
            }
        }

        public override void OnNetworkDespawn()
        {
            serverMoveIntent = Vector3.zero;
            pendingJump = false;
            hasAcceptedSequence = false;
            secondsSinceAcceptedIntent = 0f;
            base.OnNetworkDespawn();
        }

        private void Update()
        {
            if (!IsSpawned || IsServer)
            {
                return;
            }

            ApplyAuthoritativePose();
        }

        private void FixedUpdate()
        {
            if (!IsSpawned || !IsServer)
            {
                return;
            }

            SimulateAuthoritativeStep(Time.fixedDeltaTime);
        }

        public bool SubmitOwnerIntent(
            Vector3 desiredWorldMove,
            bool jumpPressed,
            uint sequence)
        {
            if (!IsSpawned || !IsClient || !IsOwner)
            {
                return false;
            }

            SubmitMovementIntentRpc(
                desiredWorldMove,
                jumpPressed,
                sequence);
            return true;
        }

        [Rpc(
            SendTo.Server,
            InvokePermission = RpcInvokePermission.Owner)]
        private void SubmitMovementIntentRpc(
            Vector3 desiredWorldMove,
            bool jumpPressed,
            uint sequence,
            RpcParams rpcParams = default)
        {
            TryAcceptServerIntent(
                rpcParams.Receive.SenderClientId,
                desiredWorldMove,
                jumpPressed,
                sequence);
        }

        public bool TryAcceptServerIntent(
            ulong senderClientId,
            Vector3 desiredWorldMove,
            bool jumpPressed,
            uint sequence)
        {
            if (!IsSpawned || !IsServer)
            {
                return false;
            }

            if (senderClientId != OwnerClientId)
            {
                return false;
            }

            if (!IsFinite(desiredWorldMove))
            {
                return false;
            }

            if (hasAcceptedSequence &&
                !IsNewerSequence(sequence, lastAcceptedSequence))
            {
                return false;
            }

            desiredWorldMove.y = 0f;
            serverMoveIntent = Vector3.ClampMagnitude(desiredWorldMove, 1f);
            pendingJump |= jumpPressed;
            lastAcceptedSequence = sequence;
            hasAcceptedSequence = true;
            secondsSinceAcceptedIntent = 0f;
            return true;
        }

        public bool SimulateAuthoritativeStep(float deltaTime)
        {
            if (!IsSpawned || !IsServer || deltaTime <= 0f)
            {
                return false;
            }

            secondsSinceAcceptedIntent += deltaTime;
            if (secondsSinceAcceptedIntent > IntentTimeoutSeconds)
            {
                serverMoveIntent = Vector3.zero;
                pendingJump = false;
            }

            var jumpThisStep = pendingJump;
            pendingJump = false;

            motor.Tick(serverMoveIntent, jumpThisStep, deltaTime);
            PublishAuthoritativePose();
            return true;
        }

        private void PublishAuthoritativePose()
        {
            authoritativePosition.Value = transform.position;
            authoritativeYaw.Value = transform.eulerAngles.y;
        }

        private void ApplyAuthoritativePose()
        {
            transform.SetPositionAndRotation(
                authoritativePosition.Value,
                Quaternion.Euler(0f, authoritativeYaw.Value, 0f));
        }

        private static bool IsFinite(Vector3 value)
        {
            return
                !float.IsNaN(value.x) &&
                !float.IsInfinity(value.x) &&
                !float.IsNaN(value.y) &&
                !float.IsInfinity(value.y) &&
                !float.IsNaN(value.z) &&
                !float.IsInfinity(value.z);
        }

        private static bool IsNewerSequence(uint candidate, uint previous)
        {
            return unchecked((int)(candidate - previous)) > 0;
        }
    }
}
