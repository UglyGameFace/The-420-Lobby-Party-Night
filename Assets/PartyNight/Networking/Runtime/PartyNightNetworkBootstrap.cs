using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

namespace PartyNight.Networking
{
    [DisallowMultipleComponent]
    public sealed class PartyNightNetworkBootstrap : MonoBehaviour
    {
        public const string RuntimeName = "Party Night Network Bootstrap";
        public const string DefaultClientAddress = "127.0.0.1";
        public const string DefaultServerListenAddress = "0.0.0.0";
        public const ushort DefaultPort = 7777;

        private NetworkManager networkManager;
        private UnityTransport transport;
        private PartyNightNetworkMode mode;
        private bool initialized;

        public bool IsInitialized => initialized;
        public event System.Action<PartyNightNetworkMode> ModeChanged;
        public NetworkManager NetworkManager => networkManager;
        public UnityTransport Transport => transport;
        public PartyNightNetworkMode Mode => mode;
        public GameObject PlayerPrefab =>
            networkManager != null && networkManager.NetworkConfig != null
                ? networkManager.NetworkConfig.PlayerPrefab
                : null;

        public bool IsAuthoritativeServer =>
            initialized &&
            mode == PartyNightNetworkMode.DedicatedServer &&
            networkManager != null &&
            networkManager.IsServer &&
            !networkManager.IsClient;

        public static PartyNightNetworkBootstrap GetOrCreateRuntime(
            out bool created)
        {
            var existing =
                Object.FindObjectsByType<PartyNightNetworkBootstrap>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None);

            if (existing.Length > 1)
            {
                throw new System.InvalidOperationException(
                    "Party Night requires exactly one network bootstrap.");
            }

            if (existing.Length == 1)
            {
                created = false;
                existing[0].Initialize();
                return existing[0];
            }

            var runtimeObject = new GameObject(RuntimeName);
            runtimeObject.SetActive(false);

            var bootstrap =
                runtimeObject.AddComponent<PartyNightNetworkBootstrap>();
            bootstrap.Initialize();

            created = true;
            runtimeObject.SetActive(true);
            return bootstrap;
        }

        public void Initialize()
        {
            if (initialized)
            {
                return;
            }

            transport = GetComponent<UnityTransport>();
            if (transport == null)
            {
                transport = gameObject.AddComponent<UnityTransport>();
            }

            networkManager = GetComponent<NetworkManager>();
            if (networkManager == null)
            {
                networkManager = gameObject.AddComponent<NetworkManager>();
            }

            if (networkManager.NetworkConfig == null)
            {
                networkManager.NetworkConfig = new NetworkConfig();
            }

            networkManager.NetworkConfig.NetworkTransport = transport;
            networkManager.NetworkConfig.EnableSceneManagement = false;
            networkManager.OnConnectionEvent += HandleConnectionEvent;

            mode = PartyNightNetworkMode.None;
            initialized = true;
        }

        private void Start()
        {
#if UNITY_SERVER
            if (initialized && !networkManager.IsListening && !StartDedicatedServer())
            {
                throw new System.InvalidOperationException(
                    "Party Night dedicated server failed to start.");
            }
#endif
        }

        public void ConfigurePlayerPrefab(GameObject playerPrefab)
        {
            if (!initialized || networkManager == null ||
                networkManager.NetworkConfig == null)
            {
                throw new System.InvalidOperationException(
                    "PartyNightNetworkBootstrap must be initialized before configuring the player prefab.");
            }

            if (playerPrefab == null)
            {
                throw new System.ArgumentNullException(nameof(playerPrefab));
            }

            var networkObject = playerPrefab.GetComponent<NetworkObject>();
            if (networkObject == null)
            {
                throw new System.InvalidOperationException(
                    "Party Night network player prefab requires a root NetworkObject.");
            }

            if (playerPrefab.GetComponent<PartyNightNetworkPlayer>() == null)
            {
                throw new System.InvalidOperationException(
                    "Party Night network player prefab requires PartyNightNetworkPlayer.");
            }

            if (networkObject.PrefabIdHash == 0)
            {
                throw new System.InvalidOperationException(
                    "Party Night network player prefab must have a non-zero NGO prefab hash.");
            }

            var current = networkManager.NetworkConfig.PlayerPrefab;
            if (current == playerPrefab)
            {
                return;
            }

            if (networkManager.IsListening)
            {
                throw new System.InvalidOperationException(
                    "Party Night player prefab cannot change while a network session is running.");
            }

            networkManager.NetworkConfig.PlayerPrefab = playerPrefab;
        }

        public bool StartDedicatedServer(ushort port = DefaultPort)
        {
            EnsureCanStart();

            transport.SetConnectionData(
                DefaultClientAddress,
                port,
                DefaultServerListenAddress);

            if (!networkManager.StartServer())
            {
                return false;
            }

            SetMode(PartyNightNetworkMode.DedicatedServer);
            return true;
        }

        public bool StartClient(
            string address = DefaultClientAddress,
            ushort port = DefaultPort)
        {
            if (string.IsNullOrWhiteSpace(address))
            {
                throw new System.ArgumentException(
                    "Party Night client address must not be empty.",
                    nameof(address));
            }

            EnsureCanStart();
            transport.SetConnectionData(address, port);

            if (!networkManager.StartClient())
            {
                return false;
            }

            SetMode(PartyNightNetworkMode.Client);
            return true;
        }

        public void Shutdown()
        {
            if (networkManager != null && networkManager.IsListening)
            {
                networkManager.Shutdown();
            }

            SetMode(PartyNightNetworkMode.None);
        }

        private void HandleConnectionEvent(
            NetworkManager manager,
            ConnectionEventData eventData)
        {
            if (!initialized ||
                manager != networkManager ||
                mode != PartyNightNetworkMode.Client)
            {
                return;
            }

            if (eventData.EventType == ConnectionEvent.ClientDisconnected)
            {
                SetMode(PartyNightNetworkMode.None);
            }
        }

        private void SetMode(PartyNightNetworkMode nextMode)
        {
            if (mode == nextMode)
            {
                return;
            }

            mode = nextMode;
            ModeChanged?.Invoke(mode);
        }

        private void EnsureCanStart()
        {
            if (!initialized || networkManager == null || transport == null)
            {
                throw new System.InvalidOperationException(
                    "PartyNightNetworkBootstrap must be initialized before starting a session.");
            }

            if (networkManager.IsListening)
            {
                throw new System.InvalidOperationException(
                    "Party Night network session is already running.");
            }
        }

        private void OnDestroy()
        {
            Shutdown();

            if (networkManager != null)
            {
                networkManager.OnConnectionEvent -= HandleConnectionEvent;
            }

            initialized = false;
        }
    }
}
