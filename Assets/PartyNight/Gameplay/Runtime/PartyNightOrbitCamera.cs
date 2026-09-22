using PartyNight.Input;
using UnityEngine;

namespace PartyNight.Gameplay
{
    [DisallowMultipleComponent]
    public sealed class PartyNightOrbitCamera : MonoBehaviour
    {
        [SerializeField, Min(0.5f)]
        private float distance = 5.5f;

        [SerializeField]
        private float pivotHeight = 1.4f;

        [SerializeField]
        private float initialPitchDegrees = 18f;

        [SerializeField]
        private float minimumPitchDegrees = -30f;

        [SerializeField]
        private float maximumPitchDegrees = 65f;

        [SerializeField, Min(0.001f)]
        private float pointerDegreesPerPixel = 0.12f;

        [SerializeField, Min(1f)]
        private float stickDegreesPerSecond = 180f;

        private Transform target;
        private float yawDegrees;
        private float pitchDegrees;

        public Transform Target => target;
        public float YawDegrees => yawDegrees;
        public float PitchDegrees => pitchDegrees;

        public Vector3 PlanarForward =>
            Quaternion.Euler(0f, yawDegrees, 0f) * Vector3.forward;

        public Vector3 PlanarRight =>
            Quaternion.Euler(0f, yawDegrees, 0f) * Vector3.right;

        public void Initialize(Transform followTarget)
        {
            if (followTarget == null)
            {
                throw new System.ArgumentNullException(nameof(followTarget));
            }

            target = followTarget;
            SetOrbit(followTarget.eulerAngles.y, initialPitchDegrees);
            SnapNow();
        }

        public void SetOrbit(float yaw, float pitch)
        {
            yawDegrees = NormalizeAngle(yaw);
            pitchDegrees = Mathf.Clamp(
                pitch,
                minimumPitchDegrees,
                maximumPitchDegrees);
        }

        public void ApplyLook(
            Vector2 look,
            PartyNightLookInputMode lookMode,
            float deltaTime)
        {
            if (look.sqrMagnitude <= 0f)
            {
                return;
            }

            var scale = lookMode == PartyNightLookInputMode.Delta
                ? pointerDegreesPerPixel
                : stickDegreesPerSecond * Mathf.Max(0f, deltaTime);

            yawDegrees = NormalizeAngle(yawDegrees + look.x * scale);
            pitchDegrees = Mathf.Clamp(
                pitchDegrees - look.y * scale,
                minimumPitchDegrees,
                maximumPitchDegrees);
        }

        private void LateUpdate()
        {
            SnapNow();
        }

        public void SnapNow()
        {
            if (target == null)
            {
                return;
            }

            var rotation = Quaternion.Euler(
                pitchDegrees,
                yawDegrees,
                0f);
            var pivot = target.position + Vector3.up * pivotHeight;
            var position = pivot - (rotation * Vector3.forward) * distance;

            transform.SetPositionAndRotation(position, rotation);
        }

        private static float NormalizeAngle(float angle)
        {
            return Mathf.Repeat(angle + 180f, 360f) - 180f;
        }
    }
}
