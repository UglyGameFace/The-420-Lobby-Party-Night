using PartyNight.Input;
using PartyNight.Networking;
using Unity.Netcode;
using UnityEngine;

namespace PartyNight.Gameplay
{
    [DisallowMultipleComponent]
    public sealed class PartyNightNetworkOwnerBridge : MonoBehaviour
    {
        private PartyNightNetworkBootstrap bootstrap;
        private NetworkManager networkManager;
        private PartyNightLocalPlayerController localController;
        private PartyNightOrbitCamera orbitCamera;
        private HotboxHavocPrototype hotboxPrototype;
        private Transform standaloneLocalPlayer;
        private CharacterController standaloneCharacterController;
        private PartyNightNetworkPlayer boundPlayer;
        private PartyNightNetworkMovement boundMovement;
        private bool initialized;
        private bool networkSessionActive;
        private uint nextSequence = 1;
        private uint lastSubmittedSequence;

        public bool IsInitialized => initialized;
        public bool NetworkSessionActive => networkSessionActive;
        public bool IsBound => boundPlayer != null && boundMovement != null;
        public PartyNightNetworkPlayer BoundPlayer => boundPlayer;
        public PartyNightNetworkMovement BoundMovement => boundMovement;
        public PartyNightLocalPlayerController InputSource => localController;
        public uint NextSequence => nextSequence;
        public uint LastSubmittedSequence => lastSubmittedSequence;
        public bool StandalonePrototypeSuppressed =>
            networkSessionActive &&
            localController != null &&
            !localController.AutomaticMotorControlEnabled &&
            standaloneCharacterController != null &&
            !standaloneCharacterController.enabled &&
            hotboxPrototype != null &&
            !hotboxPrototype.gameObject.activeSelf;

        public void Initialize(
            PartyNightNetworkBootstrap networkBootstrap,
            PartyNightLocalPlayerController inputSource,
            PartyNightOrbitCamera cameraController,
            Transform standalonePlayer,
            HotboxHavocPrototype localHotboxPrototype)
        {
            if (initialized)
            {
                throw new System.InvalidOperationException(
                    "PartyNightNetworkOwnerBridge is already initialized.");
            }

            bootstrap = networkBootstrap != null
                ? networkBootstrap
                : throw new System.ArgumentNullException(nameof(networkBootstrap));
            networkManager = bootstrap.NetworkManager != null
                ? bootstrap.NetworkManager
                : throw new System.InvalidOperationException(
                    "Party Night network bootstrap has no NetworkManager.");
            localController = inputSource != null
                ? inputSource
                : throw new System.ArgumentNullException(nameof(inputSource));
            orbitCamera = cameraController != null
                ? cameraController
                : throw new System.ArgumentNullException(nameof(cameraController));
            standaloneLocalPlayer = standalonePlayer != null
                ? standalonePlayer
                : throw new System.ArgumentNullException(nameof(standalonePlayer));
            hotboxPrototype = localHotboxPrototype != null
                ? localHotboxPrototype
                : throw new System.ArgumentNullException(nameof(localHotboxPrototype));
            standaloneCharacterController =
                standaloneLocalPlayer.GetComponent<CharacterController>();
            if (standaloneCharacterController == null)
            {
                throw new System.InvalidOperationException(
                    "Standalone Party Night player requires CharacterController.");
            }

            bootstrap.ModeChanged += HandleModeChanged;
            networkManager.OnConnectionEvent += HandleConnectionEvent;

            initialized = true;
            ApplyNetworkSessionState(
                bootstrap.Mode != PartyNightNetworkMode.None,
                restartStandalone: false);

            if (bootstrap.Mode == PartyNightNetworkMode.Client)
            {
                TryBindCurrentLocalPlayer();
            }
        }

        private void Update()
        {
            if (!initialized || !IsBound)
            {
                return;
            }

            if (!boundPlayer.IsLocallyOwnedPlayer ||
                !boundMovement.IsSpawned ||
                !boundMovement.IsClient ||
                !boundMovement.IsOwner)
            {
                ReleaseOwnedPlayer();
                return;
            }

            SubmitBoundInputFrame(
                localController.ReadInputFrame(),
                Time.deltaTime);
        }

        public bool TryBindCurrentLocalPlayer()
        {
            if (!initialized ||
                bootstrap.Mode != PartyNightNetworkMode.Client ||
                !networkManager.IsClient)
            {
                return false;
            }

            var localClient = networkManager.LocalClient;
            if (localClient == null)
            {
                return false;
            }

            var playerObject = localClient.PlayerObject;
            if (playerObject == null ||
                !playerObject.IsSpawned ||
                !playerObject.IsOwner ||
                playerObject.OwnerClientId != networkManager.LocalClientId)
            {
                return false;
            }

            return BindOwnedPlayer(playerObject);
        }

        public Vector3 PrepareDesiredWorldMove(
            PartyNightInputFrame frame,
            float deltaTime)
        {
            EnsureInitialized();

            orbitCamera.ApplyLook(
                frame.Look,
                frame.LookMode,
                deltaTime);

            var desiredWorldMove =
                orbitCamera.PlanarRight * frame.Move.x +
                orbitCamera.PlanarForward * frame.Move.y;

            return Vector3.ClampMagnitude(desiredWorldMove, 1f);
        }

        public bool SubmitBoundInputFrame(
            PartyNightInputFrame frame,
            float deltaTime)
        {
            if (!initialized || !IsBound)
            {
                return false;
            }

            var desiredWorldMove =
                PrepareDesiredWorldMove(frame, deltaTime);
            var sequence = nextSequence;

            if (!boundMovement.SubmitOwnerIntent(
                    desiredWorldMove,
                    frame.JumpPressed,
                    sequence))
            {
                return false;
            }

            lastSubmittedSequence = sequence;
            nextSequence = unchecked(sequence + 1u);
            return true;
        }

        private bool BindOwnedPlayer(NetworkObject playerObject)
        {
            if (networkManager.LocalClient == null ||
                playerObject != networkManager.LocalClient.PlayerObject ||
                !playerObject.IsSpawned ||
                !playerObject.IsOwner ||
                playerObject.OwnerClientId != networkManager.LocalClientId)
            {
                return false;
            }

            var identity =
                playerObject.GetComponent<PartyNightNetworkPlayer>();
            var movement =
                playerObject.GetComponent<PartyNightNetworkMovement>();

            if (identity == null || movement == null)
            {
                throw new System.InvalidOperationException(
                    "Party Night LocalClient.PlayerObject is not the canonical network player.");
            }

            if (!identity.IsLocallyOwnedPlayer ||
                !movement.IsSpawned ||
                !movement.IsClient ||
                !movement.IsOwner)
            {
                return false;
            }

            if (boundPlayer == identity && boundMovement == movement)
            {
                return true;
            }

            ReleaseOwnedPlayer();

            boundPlayer = identity;
            boundMovement = movement;
            boundPlayer.NetworkDespawned += HandleBoundPlayerDespawned;
            nextSequence = 1;
            lastSubmittedSequence = 0;

            ApplyNetworkSessionState(
                active: true,
                restartStandalone: false);
            orbitCamera.Retarget(playerObject.transform);
            return true;
        }

        private void HandleModeChanged(PartyNightNetworkMode nextMode)
        {
            if (!initialized)
            {
                return;
            }

            if (nextMode == PartyNightNetworkMode.None)
            {
                ReleaseOwnedPlayer();
                ApplyNetworkSessionState(
                    active: false,
                    restartStandalone: true);
                return;
            }

            ApplyNetworkSessionState(
                active: true,
                restartStandalone: false);

            if (nextMode == PartyNightNetworkMode.Client)
            {
                TryBindCurrentLocalPlayer();
            }
            else
            {
                ReleaseOwnedPlayer();
            }
        }

        private void HandleConnectionEvent(
            NetworkManager manager,
            ConnectionEventData eventData)
        {
            if (!initialized || manager != networkManager)
            {
                return;
            }

            if (eventData.EventType == ConnectionEvent.ClientConnected &&
                bootstrap.Mode == PartyNightNetworkMode.Client &&
                manager.IsClient &&
                eventData.ClientId == manager.LocalClientId)
            {
                TryBindCurrentLocalPlayer();
            }
            else if (
                eventData.EventType == ConnectionEvent.ClientDisconnected &&
                bootstrap.Mode == PartyNightNetworkMode.Client)
            {
                ReleaseOwnedPlayer();
            }
        }

        private void HandleBoundPlayerDespawned(
            PartyNightNetworkPlayer player)
        {
            if (player == boundPlayer)
            {
                ReleaseOwnedPlayer();
            }
        }

        private void ApplyNetworkSessionState(
            bool active,
            bool restartStandalone)
        {
            networkSessionActive = active;
            localController.SetAutomaticMotorControlEnabled(!active);
            standaloneCharacterController.enabled = !active;

            if (active)
            {
                if (hotboxPrototype.gameObject.activeSelf)
                {
                    hotboxPrototype.gameObject.SetActive(false);
                }

                return;
            }

            if (!hotboxPrototype.gameObject.activeSelf)
            {
                hotboxPrototype.gameObject.SetActive(true);
            }

            orbitCamera.Retarget(standaloneLocalPlayer);

            if (restartStandalone &&
                hotboxPrototype.RoundController != null)
            {
                hotboxPrototype.RoundController.RestartRound();
            }
        }

        private void ReleaseOwnedPlayer()
        {
            if (boundPlayer != null)
            {
                boundPlayer.NetworkDespawned -= HandleBoundPlayerDespawned;
            }

            boundPlayer = null;
            boundMovement = null;
            nextSequence = 1;
            lastSubmittedSequence = 0;

            if (initialized && standaloneLocalPlayer != null)
            {
                orbitCamera.Retarget(standaloneLocalPlayer);
            }
        }

        private void EnsureInitialized()
        {
            if (!initialized)
            {
                throw new System.InvalidOperationException(
                    "PartyNightNetworkOwnerBridge must be initialized before use.");
            }
        }

        private void OnDestroy()
        {
            if (bootstrap != null)
            {
                bootstrap.ModeChanged -= HandleModeChanged;
            }

            if (networkManager != null)
            {
                networkManager.OnConnectionEvent -= HandleConnectionEvent;
            }

            if (boundPlayer != null)
            {
                boundPlayer.NetworkDespawned -= HandleBoundPlayerDespawned;
            }

            boundPlayer = null;
            boundMovement = null;
            initialized = false;
        }
    }
}
