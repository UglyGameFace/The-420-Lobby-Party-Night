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

    public enum FindObjectsInactive
    {
        Exclude,
        Include
    }

    public enum FindObjectsSortMode
    {
        None
    }

    public class Object
    {
        public static T[] FindObjectsByType<T>(
            FindObjectsInactive inactive,
            FindObjectsSortMode sortMode) =>
            Array.Empty<T>();
    }

    public class GameObject : Object
    {
        private readonly Dictionary<Type, MonoBehaviour> components = new();

        public GameObject(string objectName = "GameObject")
        {
            name = objectName;
            transform = new Transform(this);
        }

        public string name { get; set; }
        public bool activeSelf { get; private set; } = true;
        public Transform transform { get; }

        public void SetActive(bool active)
        {
            activeSelf = active;
        }

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
        public bool enabled { get; set; } = true;

        public T GetComponent<T>() where T : class =>
            gameObject?.GetComponent<T>();
    }

    public sealed class Transform
    {
        private readonly GameObject owner;

        public Transform(GameObject owner)
        {
            this.owner = owner;
        }

        public Vector3 position;
        public Quaternion rotation = Quaternion.identity;
        public Vector3 eulerAngles => rotation.eulerAngles;

        public T GetComponent<T>() where T : class =>
            owner.GetComponent<T>();

        public void SetPositionAndRotation(
            Vector3 nextPosition,
            Quaternion nextRotation)
        {
            position = nextPosition;
            rotation = nextRotation;
        }
    }

    public struct Vector2
    {
        public float x;
        public float y;

        public Vector2(float x, float y)
        {
            this.x = x;
            this.y = y;
        }

        public static Vector2 zero => new(0f, 0f);
        public static Vector2 up => new(0f, 1f);
        public static Vector2 right => new(1f, 0f);
        public float sqrMagnitude => x * x + y * y;
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

        public static Vector3 MoveTowards(
            Vector3 current,
            Vector3 target,
            float maxDistanceDelta)
        {
            var delta = target - current;
            var distance = delta.magnitude;
            if (distance <= maxDistanceDelta || distance == 0f)
            {
                return target;
            }

            return current + delta * (maxDistanceDelta / distance);
        }

        public static Vector3 ClampMagnitude(Vector3 value, float maxLength)
        {
            var magnitude = value.magnitude;
            if (magnitude <= maxLength || magnitude <= 0f)
            {
                return value;
            }

            return value * (maxLength / magnitude);
        }

        public static float Distance(Vector3 a, Vector3 b) =>
            (a - b).magnitude;

        public bool Equals(Vector3 other) =>
            x == other.x && y == other.y && z == other.z;

        public override bool Equals(object obj) =>
            obj is Vector3 other && Equals(other);

        public override int GetHashCode() =>
            HashCode.Combine(x, y, z);
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

        public static Vector3 operator *(Quaternion rotation, Vector3 vector)
        {
            var yaw = rotation.eulerAngles.y * MathF.PI / 180f;
            var cos = MathF.Cos(yaw);
            var sin = MathF.Sin(yaw);
            return new Vector3(
                vector.x * cos + vector.z * sin,
                vector.y,
                -vector.x * sin + vector.z * cos);
        }
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
        public static float deltaTime = 0.02f;
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
            WritePerm = writePerm;
        }

        public T Value { get; set; }
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

    public enum ConnectionEvent
    {
        ClientConnected,
        PeerConnected,
        ClientDisconnected,
        PeerDisconnected
    }

    public struct ConnectionEventData
    {
        public ConnectionEvent EventType;
        public ulong ClientId;
    }

    public sealed class NetworkClient
    {
        public NetworkObject PlayerObject { get; set; }
    }

    public sealed class NetworkConfig
    {
        public object NetworkTransport { get; set; }
        public bool EnableSceneManagement { get; set; }
        public GameObject PlayerPrefab { get; set; }
    }

    public class NetworkManager : MonoBehaviour
    {
        public event Action<NetworkManager, ConnectionEventData> OnConnectionEvent;

        public bool IsServer { get; set; }
        public bool IsClient { get; set; }
        public bool IsListening { get; set; }
        public ulong LocalClientId { get; set; }
        public NetworkClient LocalClient { get; } = new();
        public NetworkConfig NetworkConfig { get; set; }

        public bool StartServer()
        {
            IsServer = true;
            IsClient = false;
            IsListening = true;
            return true;
        }

        public bool StartClient()
        {
            IsServer = false;
            IsClient = true;
            IsListening = true;
            return true;
        }

        public void Shutdown()
        {
            IsServer = false;
            IsClient = false;
            IsListening = false;
        }

        public void RaiseConnectionEvent(ConnectionEvent eventType, ulong clientId)
        {
            OnConnectionEvent?.Invoke(
                this,
                new ConnectionEventData
                {
                    EventType = eventType,
                    ClientId = clientId
                });
        }
    }

    public class NetworkObject : MonoBehaviour
    {
        public bool IsSpawned { get; set; }
        public bool IsOwner { get; set; }
        public ulong OwnerClientId { get; set; }
        public uint PrefabIdHash { get; set; } = 1;
    }

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

namespace Unity.Netcode.Transports.UTP
{
    using UnityEngine;

    public sealed class UnityTransport : MonoBehaviour
    {
        public void SetConnectionData(string address, ushort port) { }

        public void SetConnectionData(
            string address,
            ushort port,
            string listenAddress) { }
    }
}
