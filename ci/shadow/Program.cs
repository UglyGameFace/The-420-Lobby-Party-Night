using System;
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

        TestCanonicalServerAuthority();
        TestIntentValidationAndOrdering();
        TestSequenceWrapAround();
        TestTimeoutAndJumpConsumption();
        TestClientAndServerSubmissionGates();
        TestDespawnResetsState();

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
                new Vector3(0f, 0f, 1f),
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
            movement.transform.position.z > 0.1f,
            "actual PartyNightCharacterMotor source advances server transform");
        Check(
            Vector3.Distance(
                movement.AuthoritativePosition,
                movement.transform.position) < 0.0001f,
            "server publishes authoritative position");
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

    private static void TestTimeoutAndJumpConsumption()
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
            movement.SimulateAuthoritativeStep(0.02f),
            "first jump simulation executes");

        var firstVertical = motor.Velocity.y;
        Check(firstVertical > 0f, "first simulation consumes jump");

        Check(
            movement.SimulateAuthoritativeStep(0.02f),
            "second jump simulation executes");

        var secondVertical = motor.Velocity.y;
        Check(
            secondVertical < firstVertical,
            "jump request is one-shot rather than re-triggered");

        Check(
            movement.TryAcceptServerIntent(
                5,
                Vector3.right,
                false,
                2),
            "fresh movement intent is accepted before timeout test");

        Check(
            movement.SimulateAuthoritativeStep(
                PartyNightNetworkMovement.IntentTimeoutSeconds + 0.01f),
            "timeout simulation executes");

        Check(
            movement.CurrentServerMoveIntent.Equals(Vector3.zero),
            "stale movement intent decays to zero");
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
    }

    private static void TestDespawnResetsState()
    {
        var movement = CreateMovement(true, false, false, 77);
        Check(
            movement.TryAcceptServerIntent(
                77,
                Vector3.forward,
                false,
                123),
            "state exists before despawn");
        Check(movement.HasAcceptedSequence, "sequence state recorded before despawn");

        movement.OnNetworkDespawn();

        Check(
            !movement.HasAcceptedSequence,
            "despawn clears accepted-sequence state");
        Check(
            movement.CurrentServerMoveIntent.Equals(Vector3.zero),
            "despawn clears server movement intent");
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
