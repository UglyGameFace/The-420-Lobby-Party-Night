#!/usr/bin/env python3
import shutil
import subprocess
import sys
import tempfile
from pathlib import Path

REPO = Path(sys.argv[1]).resolve()
VALIDATOR = REPO / "scripts" / "validate_unity_foundation.py"

if not VALIDATOR.is_file():
    raise SystemExit(f"missing validator: {VALIDATOR}")

mutations = [
    (
        "owner_writable_pose",
        "Assets/PartyNight/Gameplay/Runtime/PartyNightNetworkMovement.cs",
        "NetworkVariableWritePermission.Server",
        "NetworkVariableWritePermission.Owner",
        "authoritative pose becomes owner-writable",
    ),
    (
        "rpc_not_owner_only",
        "Assets/PartyNight/Gameplay/Runtime/PartyNightNetworkMovement.cs",
        "RpcInvokePermission.Owner",
        "RpcInvokePermission.Everyone",
        "movement RPC loses owner-only invocation",
    ),
    (
        "rpc_not_server_targeted",
        "Assets/PartyNight/Gameplay/Runtime/PartyNightNetworkMovement.cs",
        "SendTo.Server",
        "SendTo.Everyone",
        "movement RPC no longer targets server",
    ),
    (
        "remove_sender_ownership_check",
        "Assets/PartyNight/Gameplay/Runtime/PartyNightNetworkMovement.cs",
        "senderClientId != OwnerClientId",
        "senderClientId == ulong.MaxValue",
        "server no longer validates sender against owner",
    ),
    (
        "client_character_controller_enabled",
        "Assets/PartyNight/Gameplay/Runtime/PartyNightNetworkMovement.cs",
        "characterController.enabled = IsServer",
        "characterController.enabled = true",
        "clients regain authoritative CharacterController",
    ),
    (
        "introduce_host_mode",
        "Assets/PartyNight/Networking/Runtime/PartyNightNetworkBootstrap.cs",
        "if (!networkManager.StartServer())",
        "networkManager.StartHost();\n            if (!networkManager.StartServer())",
        "forbidden NGO host authority is introduced",
    ),
    (
        "network_prefab_local_controller",
        "Assets/PartyNight/Networking/Prefabs/PartyNightNetworkPlayer.prefab",
        "  m_Name:\n  m_EditorClassIdentifier:\n",
        "  m_Name:\n  m_EditorClassIdentifier:\n  injectedForbiddenGuid: {fileID: 11500000, guid: 6957627954ac480aa3c298aeddf141b4, type: 3}\n",
        "network prefab gains forbidden local-controller GUID",
    ),
    (
        "remove_movement_playmode_coverage",
        "Assets/PartyNight/Tests/PlayMode/NetworkMovementRuntimeTests.cs",
        "ServerRejectsInvalidDuplicateAndStaleMovementIntent",
        "ServerMovementRegressionCoverageRemoved",
        "required movement PlayMode regression coverage disappears",
    ),
    (
        "restore_stale_identity_only_test",
        "Assets/PartyNight/Tests/PlayMode/NetworkingRuntimeTests.cs",
        "playerPrefab.GetComponent<CharacterController>(),\n                Is.Not.Null",
        "playerPrefab.GetComponent<CharacterController>(),\n                Is.Null",
        "stale identity-only CharacterController expectation returns",
    ),
]

def run_validator(root: Path):
    return subprocess.run(
        [sys.executable, str(root / "scripts" / "validate_unity_foundation.py")],
        cwd=root,
        text=True,
        stdout=subprocess.PIPE,
        stderr=subprocess.STDOUT,
    )

baseline = run_validator(REPO)
if baseline.returncode != 0:
    print("MUTATION_BASELINE | FAIL")
    print(baseline.stdout)
    raise SystemExit(1)

print("MUTATION_BASELINE | PASS")

passes = 0
with tempfile.TemporaryDirectory(prefix="party-night-mutation-") as tmp:
    temp_root = Path(tmp)

    for name, relative, old, new, description in mutations:
        case_root = temp_root / name
        shutil.copytree(
            REPO,
            case_root,
            ignore=shutil.ignore_patterns(".git", "Library", "Temp", "Obj", "Logs"),
        )

        target = case_root / relative
        text = target.read_text(encoding="utf-8")

        count = text.count(old)
        if count == 0:
            print(f"MUTATION_SETUP | FAIL | {name} | token not found")
            raise SystemExit(1)

        if name == "network_prefab_local_controller":
            text = text.replace(old, new, 1)
        else:
            text = text.replace(old, new, 1)

        target.write_text(text, encoding="utf-8")

        result = run_validator(case_root)
        if result.returncode == 0:
            print(
                f"MUTATION_GUARD | FAIL | {name} | validator accepted: {description}"
            )
            raise SystemExit(1)

        output = result.stdout.strip().replace("\n", " | ")
        print(
            f"MUTATION_GUARD | PASS | {name} | rejected: {description} | {output[-500:]}"
        )
        passes += 1

print(f"MUTATION_RUNTIME | PASS | mutations={passes}/{len(mutations)}")
