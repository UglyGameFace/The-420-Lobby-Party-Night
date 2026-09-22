using UnityEngine;

namespace PartyNight.Gameplay
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CharacterController))]
    public sealed class PartyNightCharacterMotor : MonoBehaviour
    {
        private const float GroundedVerticalSpeed = -2f;

        [SerializeField, Min(0.1f)]
        private float moveSpeed = 6f;

        [SerializeField, Min(0.1f)]
        private float acceleration = 30f;

        [SerializeField, Min(0f)]
        private float turnSpeedDegreesPerSecond = 720f;

        [SerializeField]
        private float gravity = -25f;

        [SerializeField, Min(0.1f)]
        private float jumpHeight = 1.4f;

        private CharacterController characterController;
        private Vector3 horizontalVelocity;
        private float verticalVelocity = GroundedVerticalSpeed;

        public CharacterController CharacterController => characterController;

        public Vector3 Velocity =>
            new Vector3(horizontalVelocity.x, verticalVelocity, horizontalVelocity.z);

        public bool IsGrounded =>
            characterController != null && characterController.isGrounded;

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();

            if (gravity >= 0f)
            {
                throw new System.InvalidOperationException(
                    "Party Night character gravity must be negative.");
            }
        }

        public void Tick(Vector3 desiredWorldMove, bool jumpPressed, float deltaTime)
        {
            if (characterController == null)
            {
                throw new System.InvalidOperationException(
                    "PartyNightCharacterMotor requires a CharacterController.");
            }

            if (deltaTime <= 0f)
            {
                return;
            }

            desiredWorldMove.y = 0f;
            desiredWorldMove = Vector3.ClampMagnitude(desiredWorldMove, 1f);

            var targetHorizontalVelocity = desiredWorldMove * moveSpeed;
            horizontalVelocity = Vector3.MoveTowards(
                horizontalVelocity,
                targetHorizontalVelocity,
                acceleration * deltaTime);

            var groundedBeforeMove = characterController.isGrounded;
            if (groundedBeforeMove && verticalVelocity < 0f)
            {
                verticalVelocity = GroundedVerticalSpeed;
            }

            if (jumpPressed && groundedBeforeMove)
            {
                verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }

            verticalVelocity += gravity * deltaTime;

            var motion =
                (horizontalVelocity + Vector3.up * verticalVelocity) * deltaTime;
            var collisionFlags = characterController.Move(motion);

            if ((collisionFlags & CollisionFlags.Below) != 0 && verticalVelocity < 0f)
            {
                verticalVelocity = GroundedVerticalSpeed;
            }

            if ((collisionFlags & CollisionFlags.Above) != 0 && verticalVelocity > 0f)
            {
                verticalVelocity = 0f;
            }

            if (desiredWorldMove.sqrMagnitude > 0.0001f)
            {
                var targetRotation = Quaternion.LookRotation(desiredWorldMove, Vector3.up);
                transform.rotation = Quaternion.RotateTowards(
                    transform.rotation,
                    targetRotation,
                    turnSpeedDegreesPerSecond * deltaTime);
            }
        }
    }
}
