using System;
using System.Linq;
using System.Reflection;
using PartyNight.Gameplay;
using PartyNight.Networking;
using Unity.Netcode;
using UnityEngine;

internal static class Program
{
    private static int checks;

    private static void Main()
    {
        Console.WriteLine("PARTY_NIGHT_SHADOW_RUNTIME | START");

        TestRpcAuthorityContract();
        TestCanonicalServerAuthority();
        TestIntentValidationAndOrdering();
        TestRejectedIntentDoesNotMutateState();
        TestSequenceWrapAround();
        TestTimeoutBoundaryAndJumpQueueing();
        TestClientAndServerSubmissionGates();
        TestSimulationGuards();
        TestPlayerIdentityFlags();
        TestDespawnAndRespawnResetState();

        Console.WriteLine($"PARTY_NIGHT_SHADOW_RUNTIME | PASS | checks={checks}");
    }

    private static PartyNightNetworkMovement CreateMovement(
        bool isServer,
        bool isClient,
        bool isOwner,
        ulong ownerClientId)
    {
        var gameObject = new GameObject();
        gameObject.transform.position = new Vector3(0f, 0.05f, 0f);

        gameObject.AddComponent<CharacterController>();
        gameObject.AddComponent<PartyNightCharacterMotor>();
        gameObject.AddComponent<NetworkObject>();

        var identity = gameObject.AddComponent<PartyNightNetworkPlayer>();
        identity.IsSpawned = true;
        identity.IsServer = isServer;
        identity.IsClient = isClient;
        identity.IsOwner = isOwner;
        identity.OwnerClientId = ownerClientId;
        identity.NetworkManager = new NetworkManager { IsServer = isServer };

        var movement = gameObject.AddComponent<PartyNightNetworkMovement>();
        movement.IsSpawned = true;
        movement.IsServer = isServer;
        movement.IsClient = isClient;
        movement.IsOwner = isOwner;
        movement.OwnerClientId = ownerClientId;
        movement.NetworkManager = identity.NetworkManager;
        movement.OnNetworkSpawn();

        return movement;
    }

    private static void TestRpcAuthorityContract()
    {
        var method = typeof(PartyNightNetworkMovement).GetMethod(
            "SubmitMovementIntentRpc",
            BindingFlags.Instance | BindingFlags.NonPublic);

        Check(method != null, "movement submission RPC exists");

        var rpc = method.GetCustomAttribute<RpcAttribute>();
        Check(rpc != null, "movement submission method carries RpcAttribute");
        Check(rpc.Target == SendTo.Server, "movement RPC targets server");
        Check(
            rpc.InvokePermission == RpcInvokePermission.Owner,
            "movement RPC allows owner invocation only");

        var parameters = method.GetParameters();
        Check(parameters.Length == 4, "movement RPC has exactly four parameters");
        Check(
            parameters[0].ParameterType == typeof(Vector3) &&
            parameters[0].Name == "desiredWorldMove",
            "movement RPC accepts desired direction rather than authoritative pose");
        Check(
            parameters[1].ParameterType == typeof(bool) &&
            parameters[1].Name == "jumpPressed",
            "movement RPC carries one-shot jump intent");
        Check(
            parameters[2].ParameterType == typeof(uint) &&
            parameters[2].Name == "sequence",
            "movement RPC carries uint sequence");
        Check(
            parameters[3].ParameterType == typeof(RpcParams),
            "movement RPC receives NGO sender metadata");

        var forbiddenNames = new[]
        {
            "position",
            "authoritativePosition",
            "rotation",
            "velocity",
            "grounded",
            "collision"
        };
        Check(
            !parameters.Any(parameter =>
                forbiddenNames.Contains(
                    parameter.Name,
                    StringComparer.OrdinalIgnoreCase)),
            "movement RPC exposes no authoritative transform/physics parameter");
    }

    private static void TestCanonicalServerAuthority()
    {
        var movement = CreateMovement(
            isServer: true,
            isClient: false,
            isOwner: false,
            ownerClientId: 41);

        Check(
            movement.PositionWritePermission == NetworkVariableWritePermission.Server,
            "authoritative position is server-write-only");
        Check(
            movement.YawWritePermission == NetworkVariableWritePermission.Server,
            "authoritative yaw is server-write-only");

        var controller = movement.GetComponent<CharacterController>();
        Check(controller.enabled, "CharacterController remains enabled on server");

        Check(
            movement.TryAcceptServerIntent(
                41,
                Vector3.right,
                false,
                1),
            "server accepts current owner's intent");

        for (var i = 0; i < 8; i++)
        {
            Check(
                movement.SimulateAuthoritativeStep(0.05f),
                $"server simulation step {i + 1}");
        }

        Check(
            movement.transform.position.x > 0.1f,
            "actual PartyNightCharacterMotor source advances server transform");
        Check(
            Vector3.Distance(
                movement.AuthoritativePosition,
                movement.transform.position) < 0.0001f,
            "server publishes authoritative position");
        Check(
            MathF.Abs(
                movement.AuthoritativeYaw -
                movement.transform.eulerAngles.y) < 0.0001f,
            "server publishes authoritative yaw");
    }

    private static void TestIntentValidationAndOrdering()
    {
        var movement = CreateMovement(true, false, false, 7);

        Check(
            !movement.TryAcceptServerIntent(
                8,
                Vector3.forward,
                false,
                10),
            "wrong owner is rejected");

        Check(
            !movement.TryAcceptServerIntent(
                7,
                new Vector3(float.NaN, 0f, 0f),
                false,
                10),
            "NaN intent is rejected");

        Check(
            !movement.TryAcceptServerIntent(
                7,
                new Vector3(float.PositiveInfinity, 0f, 0f),
                false,
                10),
            "infinite intent is rejected");

        Check(
            movement.TryAcceptServerIntent(
                7,
                new Vector3(10f, 99f, 0f),
                false,
                10),
            "finite owner intent is accepted");

        Check(
            MathF.Abs(movement.CurrentServerMoveIntent.magnitude - 1f) < 0.0001f,
            "movement intent magnitude is clamped to one");
        Check(
            MathF.Abs(movement.CurrentServerMoveIntent.y) < 0.0001f,
            "client vertical intent is discarded");

        Check(
            !movement.TryAcceptServerIntent(
                7,
                Vector3.forward,
                false,
                10),
            "duplicate sequence is rejected");

        Check(
            !movement.TryAcceptServerIntent(
                7,
                Vector3.forward,
                false,
                9),
            "older sequence is rejected");

        Check(
            movement.TryAcceptServerIntent(
                7,
                Vector3.forward,
                false,
                11),
            "newer sequence is accepted");
    }

    private static void TestRejectedIntentDoesNotMutateState()
    {
        var movement = CreateMovement(true, false, false, 12);

        Check(
            movement.TryAcceptServerIntent(
                12,
                Vector3.right,
                false,
                20),
            "baseline owner intent is accepted");

        var priorIntent = movement.CurrentServerMoveIntent;
        var priorSequence = movement.LastAcceptedSequence;

        Check(
            !movement.TryAcceptServerIntent(
                13,
                Vector3.forward,
                true,
                21),
            "wrong-owner mutation attempt is rejected");
        Check(
            movement.CurrentServerMoveIntent.Equals(priorIntent),
            "wrong-owner rejection preserves prior movement intent");
        Check(
            movement.LastAcceptedSequence == priorSequence,
            "wrong-owner rejection preserves accepted sequence");

        Check(
            !movement.TryAcceptServerIntent(
                12,
                new Vector3(float.NaN, 0f, 0f),
                true,
                21),
            "invalid owner mutation attempt is rejected");
        Check(
            movement.CurrentServerMoveIntent.Equals(priorIntent),
            "invalid-vector rejection preserves prior movement intent");
        Check(
            movement.LastAcceptedSequence == priorSequence,
            "invalid-vector rejection preserves accepted sequence");
    }

    private static void TestSequenceWrapAround()
    {
        var movement = CreateMovement(true, false, false, 99);

        Check(
            movement.TryAcceptServerIntent(
                99,
                Vector3.forward,
                false,
                uint.MaxValue),
            "maximum uint sequence is accepted initially");

        Check(
            movement.TryAcceptServerIntent(
                99,
                Vector3.forward,
                false,
                0),
            "sequence zero is newer after uint wrap-around");

        Check(
            movement.LastAcceptedSequence == 0,
            "wrapped sequence becomes last accepted sequence");

        var halfRange = 0x80000000u;
        Check(
            !movement.TryAcceptServerIntent(
                99,
                Vector3.forward,
                false,
                halfRange),
            "ambiguous half-range sequence is rejected");
    }

    private static void TestTimeoutBoundaryAndJumpQueueing()
    {
        var movement = CreateMovement(true, false, false, 5);
        var motor = movement.GetComponent<PartyNightCharacterMotor>();

        Check(
            movement.TryAcceptServerIntent(
                5,
                Vector3.forward,
                true,
                1),
            "jump intent is accepted");

        Check(
            movement.TryAcceptServerIntent(
                5,
                Vector3.right,
                false,
                2),
            "later non-jump intent is accepted before simulation");

        Check(
            movement.SimulateAuthoritativeStep(0.02f),
            "queued jump simulation executes");

        var firstVertical = motor.Velocity.y;
        Check(
            firstVertical > 0f,
            "queued jump survives a later non-jump intent");

        Check(
            movement.SimulateAuthoritativeStep(0.02f),
            "post-jump simulation executes");

        var secondVertical = motor.Velocity.y;
        Check(
            secondVertical < firstVertical,
            "jump request is one-shot rather than re-triggered");

        Check(
            movement.TryAcceptServerIntent(
                5,
                Vector3.right,
                false,
                3),
            "fresh movement intent is accepted before timeout boundary test");

        Check(
            movement.SimulateAuthoritativeStep(
                PartyNightNetworkMovement.IntentTimeoutSeconds),
            "exact timeout-boundary simulation executes");
        Check(
            !movement.CurrentServerMoveIntent.Equals(Vector3.zero),
            "intent remains active exactly at timeout boundary");

        Check(
            movement.TryAcceptServerIntent(
                5,
                Vector3.forward,
                false,
                4),
            "fresh movement intent resets timeout clock");

        Check(
            movement.SimulateAuthoritativeStep(
                PartyNightNetworkMovement.IntentTimeoutSeconds + 0.01f),
            "stale timeout simulation executes");
        Check(
            movement.CurrentServerMoveIntent.Equals(Vector3.zero),
            "intent decays only after timeout is exceeded");
    }

    private static void TestClientAndServerSubmissionGates()
    {
        var dedicatedServer = CreateMovement(true, false, false, 1);
        Check(
            !dedicatedServer.SubmitOwnerIntent(
                Vector3.forward,
                false,
                1),
            "dedicated server cannot use owner-client submission API");

        var remoteNonOwner = CreateMovement(false, true, false, 1);
        Check(
            !remoteNonOwner.SubmitOwnerIntent(
                Vector3.forward,
                false,
                1),
            "non-owner client cannot submit movement intent");

        var ownerClient = CreateMovement(false, true, true, 1);
        Check(
            ownerClient.SubmitOwnerIntent(
                Vector3.forward,
                false,
                1),
            "owner client is admitted to RPC submission boundary");

        Check(
            !ownerClient.GetComponent<CharacterController>().enabled,
            "non-server client disables CharacterController");

        Check(
            !ownerClient.TryAcceptServerIntent(
                1,
                Vector3.forward,
                false,
                1),
            "client cannot directly accept authoritative intent");
    }

    private static void TestSimulationGuards()
    {
        var server = CreateMovement(true, false, false, 50);
        Check(
            server.TryAcceptServerIntent(
                50,
                Vector3.forward,
                false,
                1),
            "simulation-guard baseline intent is accepted");

        var priorIntent = server.CurrentServerMoveIntent;
        var priorPosition = server.transform.position;

        Check(
            !server.SimulateAuthoritativeStep(0f),
            "zero-delta simulation is rejected");
        Check(
            !server.SimulateAuthoritativeStep(-0.1f),
            "negative-delta simulation is rejected");
        Check(
            server.CurrentServerMoveIntent.Equals(priorIntent),
            "rejected delta does not clear current intent");
        Check(
            server.transform.position.Equals(priorPosition),
            "rejected delta does not move player");

        server.IsSpawned = false;
        Check(
            !server.SimulateAuthoritativeStep(0.02f),
            "unspawned server cannot simulate");

        var unspawned = CreateMovement(true, false, false, 51);
        unspawned.IsSpawned = false;
        Check(
            !unspawned.TryAcceptServerIntent(
                51,
                Vector3.forward,
                false,
                1),
            "unspawned object cannot accept movement intent");
        Check(
            !unspawned.HasAcceptedSequence,
            "rejected unspawned intent records no sequence");
    }

    private static void TestPlayerIdentityFlags()
    {
        var serverMovement = CreateMovement(true, false, false, 61);
        var serverIdentity =
            serverMovement.GetComponent<PartyNightNetworkPlayer>();
        Check(
            serverIdentity.IsServerAuthoritativePlayer,
            "network identity reports server authority on dedicated server");
        Check(
            !serverIdentity.IsLocallyOwnedPlayer,
            "dedicated-server player is not locally owned client player");

        var ownerMovement = CreateMovement(false, true, true, 62);
        var ownerIdentity =
            ownerMovement.GetComponent<PartyNightNetworkPlayer>();
        Check(
            ownerIdentity.IsLocallyOwnedPlayer,
            "owned client identity reports local ownership");
        Check(
            !ownerIdentity.IsServerAuthoritativePlayer,
            "owned client identity does not claim server authority");

        var remoteMovement = CreateMovement(false, true, false, 63);
        var remoteIdentity =
            remoteMovement.GetComponent<PartyNightNetworkPlayer>();
        Check(
            !remoteIdentity.IsLocallyOwnedPlayer,
            "remote client identity does not claim local ownership");
    }

    private static void TestDespawnAndRespawnResetState()
    {
        var movement = CreateMovement(true, false, false, 77);
        Check(
            movement.TryAcceptServerIntent(
                77,
                Vector3.forward,
                true,
                123),
            "state exists before despawn");
        Check(
            movement.HasAcceptedSequence,
            "sequence state recorded before despawn");

        movement.OnNetworkDespawn();

        Check(
            !movement.HasAcceptedSequence,
            "despawn clears accepted-sequence state");
        Check(
            movement.CurrentServerMoveIntent.Equals(Vector3.zero),
            "despawn clears server movement intent");

        movement.IsSpawned = true;
        movement.OnNetworkSpawn();

        Check(
            !movement.HasAcceptedSequence,
            "respawn starts with no accepted sequence");
        Check(
            movement.LastAcceptedSequence == 0,
            "respawn resets last sequence value");
        Check(
            movement.CurrentServerMoveIntent.Equals(Vector3.zero),
            "respawn starts with zero movement intent");
        Check(
            movement.GetComponent<CharacterController>().enabled,
            "server respawn restores authoritative CharacterController state");

        Check(
            movement.TryAcceptServerIntent(
                77,
                Vector3.right,
                false,
                1),
            "respawn accepts a fresh low sequence after reset");
    }

    private static void Check(bool condition, string description)
    {
        checks++;
        if (!condition)
        {
            throw new InvalidOperationException(
                $"PARTY_NIGHT_SHADOW_RUNTIME | FAIL | {description}");
        }

        Console.WriteLine(
            $"PARTY_NIGHT_SHADOW_CHECK | PASS | {description}");
    }
}
