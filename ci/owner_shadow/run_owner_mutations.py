#!/usr/bin/env python3
import shutil
import subprocess
import sys
import tempfile
from pathlib import Path

REPO = Path(sys.argv[1]).resolve()

mutations = [
    (
        "duplicate_input_reader",
        "Assets/PartyNight/Gameplay/Runtime/PartyNightNetworkOwnerBridge.cs",
        "private bool initialized;",
        "private bool initialized;\n        // mutation\n        private string duplicateReaderMarker = nameof(PartyNightInputReader.CreateFromProjectWideActions);",
        "network bridge introduces a second project-wide input-reader path",
    ),
    (
        "scene_search_owner",
        "Assets/PartyNight/Gameplay/Runtime/PartyNightNetworkOwnerBridge.cs",
        "private bool initialized;",
        "private bool initialized;\n        // mutation\n        private string sceneSearchMarker = nameof(Object.FindObjectsByType);",
        "owner discovery introduces a scene-wide search",
    ),
    (
        "remove_localclient_playerobject",
        "Assets/PartyNight/Gameplay/Runtime/PartyNightNetworkOwnerBridge.cs",
        "networkManager.LocalClient.PlayerObject",
        "networkManager.LocalClientId.ToString()",
        "owner binding no longer uses LocalClient.PlayerObject",
    ),
    (
        "remove_hotbox_suppression",
        "Assets/PartyNight/Gameplay/Runtime/PartyNightNetworkOwnerBridge.cs",
        "hotboxPrototype.gameObject.SetActive(false)",
        "hotboxPrototype.gameObject.SetActive(true)",
        "network session leaves local Hotbox authority active",
    ),
    (
        "remove_standalone_collision_suppression",
        "Assets/PartyNight/Gameplay/Runtime/PartyNightNetworkOwnerBridge.cs",
        "standaloneCharacterController.enabled = !active",
        "standaloneCharacterController.enabled = true",
        "network session leaves standalone CharacterController collision active",
    ),
    (
        "remove_sequence_increment",
        "Assets/PartyNight/Gameplay/Runtime/PartyNightNetworkOwnerBridge.cs",
        "nextSequence = unchecked(sequence + 1u)",
        "nextSequence = sequence",
        "owner movement intent sequence stops advancing",
    ),
    (
        "client_transform_authority",
        "Assets/PartyNight/Gameplay/Runtime/PartyNightNetworkOwnerBridge.cs",
        "var desiredWorldMove =",
        "transform.position = Vector3.zero;\n\n            var desiredWorldMove =",
        "network owner bridge writes transform position directly",
    ),
    (
        "introduce_host_path",
        "Assets/PartyNight/Networking/Runtime/PartyNightNetworkBootstrap.cs",
        "if (!networkManager.StartServer())",
        "networkManager.StartHost();\n            if (!networkManager.StartServer())",
        "NGO host authority is introduced",
    ),
    (
        "remove_connection_event_subscription",
        "Assets/PartyNight/Networking/Runtime/PartyNightNetworkBootstrap.cs",
        "networkManager.OnConnectionEvent += HandleConnectionEvent;",
        "// networkManager.OnConnectionEvent subscription removed;",
        "bootstrap stops listening to NGO connection lifecycle",
    ),
    (
        "remove_player_despawn_signal",
        "Assets/PartyNight/Networking/Runtime/PartyNightNetworkPlayer.cs",
        "NetworkDespawned",
        "RemovedNetworkDespawned",
        "network player loses deterministic despawn signal",
    ),
    (
        "remove_owner_bridge_playmode_coverage",
        "Assets/PartyNight/Tests/PlayMode/NetworkOwnerBridgeRuntimeTests.cs",
        "DedicatedServerSuppressesAndRestoresStandaloneAuthority",
        "DedicatedServerCoverageRemoved",
        "owner-handoff session suppression test disappears",
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
    print("OWNER_MUTATION_BASELINE | FAIL")
    print(baseline.stdout)
    raise SystemExit(1)

print("OWNER_MUTATION_BASELINE | PASS")

passes = 0
with tempfile.TemporaryDirectory(prefix="party-night-owner-mutation-") as tmp:
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

        if old not in text:
            print(f"OWNER_MUTATION_SETUP | FAIL | {name} | token not found")
            raise SystemExit(1)

        if name == "remove_localclient_playerobject":
            text = text.replace(old, new)
        else:
            text = text.replace(old, new, 1)

        target.write_text(text, encoding="utf-8")

        result = run_validator(case_root)
        if result.returncode == 0:
            print(
                f"OWNER_MUTATION_GUARD | FAIL | {name} | validator accepted: {description}"
            )
            raise SystemExit(1)

        output = result.stdout.strip().replace("\n", " | ")
        print(
            f"OWNER_MUTATION_GUARD | PASS | {name} | rejected: {description} | {output[-500:]}"
        )
        passes += 1

print(
    f"OWNER_MUTATION_RUNTIME | PASS | mutations={passes}/{len(mutations)}"
)
