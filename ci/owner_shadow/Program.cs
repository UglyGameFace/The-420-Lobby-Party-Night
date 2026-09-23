using System;
using PartyNight.Gameplay;
using PartyNight.Input;
using PartyNight.Networking;
using Unity.Netcode;
using UnityEngine;

internal static class Program
{
    private sealed class Rig
    {
        public GameObject Standalone;
        public CharacterController StandaloneController;
        public PartyNightCharacterMotor StandaloneMotor;
        public PartyNightLocalPlayerController LocalController;
        public PartyNightOrbitCamera Orbit;
        public HotboxHavocPrototype Hotbox;
        public PartyNightNetworkBootstrap Bootstrap;
        public PartyNightNetworkOwnerBridge Bridge;
        public NetworkManager NetworkManager;
    }

    private static int checks;

    private static void Main()
    {
        Console.WriteLine("PARTY_NIGHT_OWNER_SHADOW | START");

        TestStandaloneBaseline();
        TestDedicatedServerSuppressionAndRestore();
        TestClientOwnershipBindingAndSubmission();
        TestRejectsNonOwnedPlayer();
        TestDespawnDoesNotRestoreLocalAuthorityMidSession();
        TestDisconnectRestoresStandaloneMode();

        Console.WriteLine(
            $"PARTY_NIGHT_OWNER_SHADOW | PASS | checks={checks}");
    }

    private static Rig CreateRig()
    {
        var standalone = new GameObject("Standalone Player");
        standalone.transform.position = new Vector3(0f, 0.05f, 0f);
        var standaloneController =
            standalone.AddComponent<CharacterController>();
        var standaloneMotor =
            standalone.AddComponent<PartyNightCharacterMotor>();

        var cameraObject = new GameObject("Camera");
        var orbit = cameraObject.AddComponent<PartyNightOrbitCamera>();
        orbit.Initialize(standalone.transform);

        var localController =
            standalone.AddComponent<PartyNightLocalPlayerController>();
        localController.Initialize(standaloneMotor, orbit);

        var hotboxObject = new GameObject("Hotbox");
        var hotbox = hotboxObject.AddComponent<HotboxHavocPrototype>();

        var bootstrapObject = new GameObject("Network Bootstrap");
        var bootstrap =
            bootstrapObject.AddComponent<PartyNightNetworkBootstrap>();
        bootstrap.Initialize();

        var bridgeObject = new GameObject("Owner Bridge");
        var bridge =
            bridgeObject.AddComponent<PartyNightNetworkOwnerBridge>();
        bridge.Initialize(
            bootstrap,
            localController,
            orbit,
            standalone.transform,
            hotbox);

        return new Rig
        {
            Standalone = standalone,
            StandaloneController = standaloneController,
            StandaloneMotor = standaloneMotor,
            LocalController = localController,
            Orbit = orbit,
            Hotbox = hotbox,
            Bootstrap = bootstrap,
            Bridge = bridge,
            NetworkManager = bootstrap.NetworkManager
        };
    }

    private static (
        NetworkObject networkObject,
        PartyNightNetworkPlayer identity,
        PartyNightNetworkMovement movement)
        CreateClientPlayer(
            NetworkManager manager,
            ulong ownerClientId,
            bool isOwner)
    {
        var player = new GameObject("Network Player");
        player.transform.position = new Vector3(3f, 0.05f, 2f);
        player.AddComponent<CharacterController>();
        player.AddComponent<PartyNightCharacterMotor>();

        var networkObject = player.AddComponent<NetworkObject>();
        networkObject.IsSpawned = true;
        networkObject.IsOwner = isOwner;
        networkObject.OwnerClientId = ownerClientId;

        var identity = player.AddComponent<PartyNightNetworkPlayer>();
        identity.IsSpawned = true;
        identity.IsClient = true;
        identity.IsServer = false;
        identity.IsOwner = isOwner;
        identity.OwnerClientId = ownerClientId;
        identity.NetworkManager = manager;

        var movement = player.AddComponent<PartyNightNetworkMovement>();
        movement.IsSpawned = true;
        movement.IsClient = true;
        movement.IsServer = false;
        movement.IsOwner = isOwner;
        movement.OwnerClientId = ownerClientId;
        movement.NetworkManager = manager;
        movement.OnNetworkSpawn();

        return (networkObject, identity, movement);
    }

    private static void TestStandaloneBaseline()
    {
        var rig = CreateRig();

        Check(rig.Bridge.IsInitialized, "owner bridge initializes");
        Check(!rig.Bridge.NetworkSessionActive, "standalone starts outside network session");
        Check(!rig.Bridge.IsBound, "standalone starts without network owner binding");
        Check(
            rig.LocalController.AutomaticMotorControlEnabled,
            "standalone local motor control starts enabled");
        Check(
            rig.StandaloneController.enabled,
            "standalone CharacterController starts enabled");
        Check(rig.Hotbox.gameObject.activeSelf, "standalone Hotbox starts active");
        Check(
            ReferenceEquals(rig.Orbit.Target, rig.Standalone.transform),
            "standalone camera starts on local player");
    }

    private static void TestDedicatedServerSuppressionAndRestore()
    {
        var rig = CreateRig();

        Check(
            rig.Bootstrap.StartDedicatedServer(17771),
            "dedicated server starts");
        Check(
            rig.Bootstrap.Mode == PartyNightNetworkMode.DedicatedServer,
            "bootstrap enters dedicated-server mode");
        Check(
            rig.Bridge.NetworkSessionActive,
            "network session becomes active on dedicated server");
        Check(
            rig.Bridge.StandalonePrototypeSuppressed,
            "dedicated server fully suppresses standalone prototype");
        Check(
            !rig.LocalController.AutomaticMotorControlEnabled,
            "dedicated server disables standalone automatic motor control");
        Check(
            !rig.StandaloneController.enabled,
            "dedicated server disables standalone collision");
        Check(
            !rig.Hotbox.gameObject.activeSelf,
            "dedicated server disables local Hotbox authority");

        var restartsBefore = rig.Hotbox.RoundController.RestartCount;
        rig.Bootstrap.Shutdown();

        Check(
            rig.Bootstrap.Mode == PartyNightNetworkMode.None,
            "shutdown returns bootstrap to no-session mode");
        Check(
            !rig.Bridge.NetworkSessionActive,
            "shutdown clears network-session state");
        Check(
            rig.LocalController.AutomaticMotorControlEnabled,
            "shutdown restores standalone automatic motor control");
        Check(
            rig.StandaloneController.enabled,
            "shutdown restores standalone collision");
        Check(
            rig.Hotbox.gameObject.activeSelf,
            "shutdown restores local Hotbox prototype");
        Check(
            rig.Hotbox.RoundController.RestartCount == restartsBefore + 1,
            "shutdown restarts standalone Hotbox state once");
        Check(
            ReferenceEquals(rig.Orbit.Target, rig.Standalone.transform),
            "shutdown restores standalone camera target");
    }

    private static void TestClientOwnershipBindingAndSubmission()
    {
        var rig = CreateRig();
        rig.NetworkManager.LocalClientId = 42;

        Check(
            rig.Bootstrap.StartClient("127.0.0.1", 17772),
            "client session starts");
        Check(
            rig.Bridge.NetworkSessionActive,
            "client start suppresses standalone authority before connection");
        Check(
            !rig.Bridge.IsBound,
            "client is not bound before NGO assigns PlayerObject");

        var player = CreateClientPlayer(
            rig.NetworkManager,
            ownerClientId: 42,
            isOwner: true);
        rig.NetworkManager.LocalClient.PlayerObject = player.networkObject;

        rig.Orbit.SetOrbit(30f, 18f);
        rig.NetworkManager.RaiseConnectionEvent(
            ConnectionEvent.ClientConnected,
            42);

        Check(rig.Bridge.IsBound, "local ClientConnected binds owned PlayerObject");
        Check(
            ReferenceEquals(rig.Bridge.BoundPlayer, player.identity),
            "bridge binds canonical network identity");
        Check(
            ReferenceEquals(rig.Bridge.BoundMovement, player.movement),
            "bridge binds authoritative movement bridge");
        Check(
            ReferenceEquals(rig.Orbit.Target, player.networkObject.transform),
            "camera retargets to owned PlayerObject");
        Check(
            MathF.Abs(rig.Orbit.YawDegrees - 30f) < 0.0001f,
            "camera retarget preserves current view yaw");
        Check(rig.Bridge.NextSequence == 1, "first owner sequence starts at one");

        var frame = new PartyNightInputFrame(
            Vector2.up,
            Vector2.right,
            PartyNightLookInputMode.Rate,
            jumpPressed: true,
            interactPressed: false,
            grabPressed: false,
            dashPressed: false,
            useItemPressed: false,
            emotePressed: false);

        var yawBefore = rig.Orbit.YawDegrees;
        Check(
            rig.Bridge.SubmitBoundInputFrame(frame, 0.1f),
            "owned player submits movement frame");
        Check(
            rig.Orbit.YawDegrees > yawBefore,
            "owner look updates locally before network authority");
        Check(
            rig.Bridge.LastSubmittedSequence == 1,
            "successful owner submission records sequence one");
        Check(
            rig.Bridge.NextSequence == 2,
            "successful owner submission increments sequence");

        var desired = rig.Bridge.PrepareDesiredWorldMove(
            new PartyNightInputFrame(
                Vector2.up,
                Vector2.zero,
                PartyNightLookInputMode.Rate,
                false,
                false,
                false,
                false,
                false,
                false),
            0.02f);
        Check(
            Vector3.Distance(desired, rig.Orbit.PlanarForward) < 0.0001f,
            "owner move uses camera-relative planar basis");
        Check(
            MathF.Abs(desired.y) < 0.0001f,
            "owner move remains planar");
    }

    private static void TestRejectsNonOwnedPlayer()
    {
        var rig = CreateRig();
        rig.NetworkManager.LocalClientId = 50;
        Check(
            rig.Bootstrap.StartClient("127.0.0.1", 17773),
            "non-owner test starts client");

        var player = CreateClientPlayer(
            rig.NetworkManager,
            ownerClientId: 51,
            isOwner: false);
        rig.NetworkManager.LocalClient.PlayerObject = player.networkObject;

        Check(
            !rig.Bridge.TryBindCurrentLocalPlayer(),
            "bridge rejects non-owned LocalClient.PlayerObject");
        Check(!rig.Bridge.IsBound, "non-owned rejection leaves bridge unbound");
        Check(
            rig.Bridge.NetworkSessionActive,
            "non-owned rejection does not restore standalone authority mid-session");
        Check(
            rig.Bridge.StandalonePrototypeSuppressed,
            "non-owned rejection keeps standalone prototype suppressed");
    }

    private static void TestDespawnDoesNotRestoreLocalAuthorityMidSession()
    {
        var rig = CreateRig();
        rig.NetworkManager.LocalClientId = 60;
        Check(
            rig.Bootstrap.StartClient("127.0.0.1", 17774),
            "despawn test starts client");

        var player = CreateClientPlayer(
            rig.NetworkManager,
            ownerClientId: 60,
            isOwner: true);
        rig.NetworkManager.LocalClient.PlayerObject = player.networkObject;
        rig.NetworkManager.RaiseConnectionEvent(
            ConnectionEvent.ClientConnected,
            60);

        Check(rig.Bridge.IsBound, "despawn test binds owner");
        player.identity.OnNetworkDespawn();

        Check(!rig.Bridge.IsBound, "player despawn releases owner binding");
        Check(
            rig.Bridge.NetworkSessionActive,
            "player despawn keeps network session active");
        Check(
            rig.Bridge.StandalonePrototypeSuppressed,
            "player despawn does not reactivate local authority");
        Check(
            ReferenceEquals(rig.Orbit.Target, rig.Standalone.transform),
            "despawn releases camera target safely");
    }

    private static void TestDisconnectRestoresStandaloneMode()
    {
        var rig = CreateRig();
        rig.NetworkManager.LocalClientId = 70;
        Check(
            rig.Bootstrap.StartClient("127.0.0.1", 17775),
            "disconnect test starts client");

        var player = CreateClientPlayer(
            rig.NetworkManager,
            ownerClientId: 70,
            isOwner: true);
        rig.NetworkManager.LocalClient.PlayerObject = player.networkObject;
        rig.NetworkManager.RaiseConnectionEvent(
            ConnectionEvent.ClientConnected,
            70);
        Check(rig.Bridge.IsBound, "disconnect test binds owner");

        rig.NetworkManager.RaiseConnectionEvent(
            ConnectionEvent.ClientDisconnected,
            70);

        Check(
            rig.Bootstrap.Mode == PartyNightNetworkMode.None,
            "local disconnect clears bootstrap client mode");
        Check(!rig.Bridge.IsBound, "local disconnect releases owner binding");
        Check(
            !rig.Bridge.NetworkSessionActive,
            "local disconnect clears network-session suppression");
        Check(
            rig.LocalController.AutomaticMotorControlEnabled,
            "local disconnect restores standalone motor control");
        Check(
            rig.StandaloneController.enabled,
            "local disconnect restores standalone collision");
        Check(
            rig.Hotbox.gameObject.activeSelf,
            "local disconnect restores local Hotbox prototype");
        Check(
            ReferenceEquals(rig.Orbit.Target, rig.Standalone.transform),
            "local disconnect restores standalone camera target");
    }

    private static void Check(bool condition, string description)
    {
        checks++;
        if (!condition)
        {
            throw new InvalidOperationException(
                $"PARTY_NIGHT_OWNER_SHADOW | FAIL | {description}");
        }

        Console.WriteLine(
            $"PARTY_NIGHT_OWNER_CHECK | PASS | {description}");
    }
}
