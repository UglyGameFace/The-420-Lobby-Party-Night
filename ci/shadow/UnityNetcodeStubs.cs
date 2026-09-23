using System;
using System.Collections.Generic;

namespace UnityEngine
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class DisallowMultipleComponent : Attribute { }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class RequireComponent : Attribute
    {
        public RequireComponent(Type t1) { }
        public RequireComponent(Type t1, Type t2) { }
        public RequireComponent(Type t1, Type t2, Type t3) { }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public sealed class SerializeField : Attribute { }

    [AttributeUsage(AttributeTargets.Field)]
    public sealed class MinAttribute : Attribute
    {
        public MinAttribute(float value) { }
    }

    public class Object { }

    public class GameObject : Object
    {
        private readonly Dictionary<Type, MonoBehaviour> components = new();

        public GameObject()
        {
            transform = new Transform();
        }

        public Transform transform { get; }

        public T AddComponent<T>() where T : MonoBehaviour, new()
        {
            var component = new T
            {
                gameObject = this
            };
            components[typeof(T)] = component;

            var awake = typeof(T).GetMethod(
                "Awake",
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Public);
            awake?.Invoke(component, null);
            return component;
        }

        public T GetComponent<T>() where T : class
        {
            foreach (var component in components.Values)
            {
                if (component is T match)
                {
                    return match;
                }
            }

            return null;
        }
    }

    public class MonoBehaviour : Object
    {
        public GameObject gameObject { get; internal set; }
        public Transform transform => gameObject?.transform;

        public T GetComponent<T>() where T : class =>
            gameObject?.GetComponent<T>();
    }

    public sealed class Transform
    {
        public Vector3 position;
        public Quaternion rotation = Quaternion.identity;

        public Vector3 eulerAngles => rotation.eulerAngles;

        public void SetPositionAndRotation(Vector3 nextPosition, Quaternion nextRotation)
        {
            position = nextPosition;
            rotation = nextRotation;
        }
    }

    public struct Vector3 : IEquatable<Vector3>
    {
        public float x;
        public float y;
        public float z;

        public Vector3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public static Vector3 zero => new(0f, 0f, 0f);
        public static Vector3 up => new(0f, 1f, 0f);
        public static Vector3 forward => new(0f, 0f, 1f);
        public static Vector3 right => new(1f, 0f, 0f);

        public float sqrMagnitude => x * x + y * y + z * z;
        public float magnitude => MathF.Sqrt(sqrMagnitude);

        public static Vector3 operator +(Vector3 a, Vector3 b) =>
            new(a.x + b.x, a.y + b.y, a.z + b.z);

        public static Vector3 operator -(Vector3 a, Vector3 b) =>
            new(a.x - b.x, a.y - b.y, a.z - b.z);

        public static Vector3 operator *(Vector3 value, float scalar) =>
            new(value.x * scalar, value.y * scalar, value.z * scalar);

        public static Vector3 operator *(float scalar, Vector3 value) =>
            value * scalar;

        public static Vector3 ClampMagnitude(Vector3 value, float maxLength)
        {
            var magnitude = value.magnitude;
            if (magnitude <= maxLength || magnitude <= 0f)
            {
                return value;
            }

            return value * (maxLength / magnitude);
        }

        public static Vector3 MoveTowards(Vector3 current, Vector3 target, float maxDistanceDelta)
        {
            var delta = target - current;
            var distance = delta.magnitude;
            if (distance <= maxDistanceDelta || distance == 0f)
            {
                return target;
            }

            return current + delta * (maxDistanceDelta / distance);
        }

        public static float Distance(Vector3 a, Vector3 b) =>
            (a - b).magnitude;

        public bool Equals(Vector3 other) =>
            x == other.x && y == other.y && z == other.z;

        public override bool Equals(object obj) =>
            obj is Vector3 other && Equals(other);

        public override int GetHashCode() =>
            HashCode.Combine(x, y, z);

        public override string ToString() =>
            $"({x:F4}, {y:F4}, {z:F4})";
    }

    public struct Quaternion
    {
        private readonly Vector3 storedEuler;

        private Quaternion(Vector3 euler)
        {
            storedEuler = euler;
        }

        public static Quaternion identity => Euler(0f, 0f, 0f);
        public Vector3 eulerAngles => storedEuler;

        public static Quaternion Euler(float x, float y, float z) =>
            new(new Vector3(x, y, z));

        public static Quaternion LookRotation(Vector3 forward, Vector3 up)
        {
            var yaw = MathF.Atan2(forward.x, forward.z) * 180f / MathF.PI;
            return Euler(0f, yaw, 0f);
        }

        public static Quaternion RotateTowards(
            Quaternion from,
            Quaternion to,
            float maxDegreesDelta) => to;
    }

    [Flags]
    public enum CollisionFlags
    {
        None = 0,
        Sides = 1,
        Above = 2,
        Below = 4
    }

    public sealed class CharacterController : MonoBehaviour
    {
        public bool enabled = true;
        public bool isGrounded = true;

        public CollisionFlags Move(Vector3 motion)
        {
            transform.position += motion;

            if (transform.position.y <= 0.05f)
            {
                transform.position = new Vector3(
                    transform.position.x,
                    0.05f,
                    transform.position.z);
                isGrounded = true;
                return CollisionFlags.Below;
            }

            isGrounded = false;
            return CollisionFlags.None;
        }
    }

    public static class Mathf
    {
        public static float Sqrt(float value) => MathF.Sqrt(value);
        public static float Max(float a, float b) => MathF.Max(a, b);
        public static float Clamp(float value, float min, float max) =>
            MathF.Min(MathF.Max(value, min), max);

        public static float Repeat(float value, float length)
        {
            if (length == 0f)
            {
                return 0f;
            }

            return value - MathF.Floor(value / length) * length;
        }
    }

    public static class Time
    {
        public static float fixedDeltaTime = 0.02f;
    }
}

namespace Unity.Netcode
{
    using System;
    using UnityEngine;

    public enum NetworkVariableReadPermission
    {
        Everyone
    }

    public enum NetworkVariableWritePermission
    {
        Server,
        Owner
    }

    public sealed class NetworkVariable<T>
    {
        public NetworkVariable(
            T value,
            NetworkVariableReadPermission readPerm,
            NetworkVariableWritePermission writePerm)
        {
            Value = value;
            ReadPerm = readPerm;
            WritePerm = writePerm;
        }

        public T Value { get; set; }
        public NetworkVariableReadPermission ReadPerm { get; }
        public NetworkVariableWritePermission WritePerm { get; }
    }

    public enum SendTo
    {
        Server
    }

    public enum RpcInvokePermission
    {
        Owner
    }

    [AttributeUsage(AttributeTargets.Method)]
    public sealed class RpcAttribute : Attribute
    {
        public RpcAttribute(SendTo target)
        {
            Target = target;
        }

        public SendTo Target { get; }
        public RpcInvokePermission InvokePermission { get; set; }
    }

    public struct RpcReceiveParams
    {
        public ulong SenderClientId;
    }

    public struct RpcParams
    {
        public RpcReceiveParams Receive;
    }

    public class NetworkManager
    {
        public bool IsServer { get; set; }
    }

    public class NetworkObject : MonoBehaviour { }

    public class NetworkBehaviour : MonoBehaviour
    {
        public bool IsSpawned { get; set; }
        public bool IsServer { get; set; }
        public bool IsClient { get; set; }
        public bool IsOwner { get; set; }
        public ulong OwnerClientId { get; set; }
        public NetworkManager NetworkManager { get; set; }

        public virtual void OnNetworkSpawn() { }
        public virtual void OnNetworkDespawn() { }
    }
}
