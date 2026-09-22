#!/usr/bin/env python3
import json
import re
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
EXPECTED_UNITY = "6000.3.24f1"
EXPECTED_REVISION = "4e7b9b5b6244"
EXPECTED_BUILD_SCENE = "Assets/PartyNight/Scenes/PartyNightFoundation.unity"
EXPECTED_BUILD_SCENE_GUID = "7b8f6f38b4e84b4aa2bb5dc6b124d5a1"

EXPECTED_PIPELINE_ASSET = "Assets/PartyNight/Settings/PartyNightURP.asset"
EXPECTED_PIPELINE_GUID = "90b464dba38a24ac9935f6ce106d4e73"
EXPECTED_RENDERER_ASSET = "Assets/PartyNight/Settings/PartyNightUniversalRenderer.asset"
EXPECTED_RENDERER_GUID = "2cc4cedf4b9a943c09551b040e743ef4"
EXPECTED_GLOBAL_SETTINGS_ASSET = "Assets/PartyNight/Settings/PartyNightURPGlobalSettings.asset"
EXPECTED_GLOBAL_SETTINGS_GUID = "1858f607251d94a518662b55b78f6619"
EXPECTED_VOLUME_PROFILE_ASSET = "Assets/PartyNight/Settings/PartyNightDefaultVolumeProfile.asset"
EXPECTED_VOLUME_PROFILE_GUID = "c55b34362de09465f8a9b944f73c290e"
EXPECTED_SETTINGS_FOLDER_GUID = "6235a995f08434dfc81e53495c5c2027"
EXPECTED_INPUT_ACTION_ASSET = "Assets/PartyNight/Input/PartyNightInputActions.inputactions"
EXPECTED_INPUT_ACTION_GUID = "ad5fffad5d744af6939605235845fa84"
EXPECTED_FOUNDATION_COMPOSITION_GUID = "f5beee5f4dbc4d43870a8db089e7f00f"
EXPECTED_INPUT_ACTIONS = {
    "Move": ("Value", "Vector2"),
    "Look": ("Value", "Vector2"),
    "Jump": ("Button", "Button"),
    "Interact": ("Button", "Button"),
    "Grab": ("Button", "Button"),
    "Dash": ("Button", "Button"),
    "UseItem": ("Button", "Button"),
    "Emote": ("Button", "Button"),
}
EXPECTED_HOTBOX_PROTOTYPE_FILES = (
    "Assets/PartyNight/Gameplay/Runtime/HotboxHavocRoundPhase.cs",
    "Assets/PartyNight/Gameplay/Runtime/HotboxHavocRoundController.cs",
    "Assets/PartyNight/Gameplay/Runtime/HotboxHavocPrototype.cs",
    "Assets/PartyNight/Gameplay/Runtime/HotboxHavocPrototypeVisuals.cs",
    "Assets/PartyNight/Gameplay/Runtime/HotboxHavocPrototypeHud.cs",
)
EXPECTED_GAMEPAD_BINDINGS = {
    "Move": "<Gamepad>/leftStick",
    "Look": "<Gamepad>/rightStick",
    "Jump": "<Gamepad>/buttonSouth",
    "Interact": "<Gamepad>/buttonWest",
    "Grab": "<Gamepad>/leftShoulder",
    "Dash": "<Gamepad>/rightShoulder",
    "UseItem": "<Gamepad>/rightTrigger",
    "Emote": "<Gamepad>/dpad/up",
}

EXPECTED_PACKAGES = {
    "com.unity.inputsystem": "1.20.0",
    "com.unity.modules.audio": "1.0.0",
    "com.unity.modules.imageconversion": "1.0.0",
    "com.unity.modules.imgui": "1.0.0",
    "com.unity.modules.physics": "1.0.0",
    "com.unity.netcode.gameobjects": "2.13.2",
    "com.unity.render-pipelines.universal": "17.3.0",
    "com.unity.test-framework": "1.6.0",
    "com.unity.transport": "2.7.4",
}

FORBIDDEN_ROOT_DIRS = {
    "Library",
    "Temp",
    "Obj",
    "Logs",
    "UserSettings",
    "Build",
    "Builds",
}
FORBIDDEN_STALE_SUFFIXES = {
    ".bak",
    ".old",
    ".orig",
    ".rej",
    ".tmp",
}
FORBIDDEN_STALE_DIR_NAMES = {
    "backup",
    "deprecated",
    "legacy",
    "obsolete",
    "old",
    "temp",
    "temporary",
}
TEXT_SUFFIXES = {
    ".asset",
    ".asmdef",
    ".cs",
    ".inputactions",
    ".json",
    ".md",
    ".meta",
    ".py",
    ".txt",
    ".yml",
    ".yaml",
}

def fail(message: str) -> None:
    print(f"ERROR: {message}", file=sys.stderr)
    raise SystemExit(1)

def read_required(relative: str) -> str:
    path = ROOT / relative
    if not path.is_file():
        fail(f"missing {relative}")
    return path.read_text(encoding="utf-8")

def validate_project_version() -> None:
    content = read_required("ProjectSettings/ProjectVersion.txt")
    if f"m_EditorVersion: {EXPECTED_UNITY}" not in content:
        fail(f"ProjectVersion.txt does not pin Unity {EXPECTED_UNITY}")
    expected_with_revision = (
        f"m_EditorVersionWithRevision: {EXPECTED_UNITY} ({EXPECTED_REVISION})"
    )
    if expected_with_revision not in content:
        fail("ProjectVersion.txt does not pin the expected Unity changeset")

def validate_manifest() -> None:
    path = ROOT / "Packages" / "manifest.json"
    if not path.is_file():
        fail("missing Packages/manifest.json")

    manifest = json.loads(path.read_text(encoding="utf-8"))
    dependencies = manifest.get("dependencies")
    if not isinstance(dependencies, dict):
        fail("Packages/manifest.json has no dependencies object")

    for package, expected_version in EXPECTED_PACKAGES.items():
        actual = dependencies.get(package)
        if actual != expected_version:
            fail(f"{package}: expected {expected_version}, found {actual!r}")

def validate_lockfile() -> None:
    path = ROOT / "Packages" / "packages-lock.json"
    if not path.is_file():
        fail("missing authoritative Packages/packages-lock.json")

    try:
        lock = json.loads(path.read_text(encoding="utf-8"))
    except json.JSONDecodeError as exc:
        fail(f"invalid Packages/packages-lock.json: {exc}")

    dependencies = lock.get("dependencies")
    if not isinstance(dependencies, dict):
        fail("Packages/packages-lock.json has no dependencies object")

    for package, expected_version in EXPECTED_PACKAGES.items():
        entry = dependencies.get(package)
        if not isinstance(entry, dict):
            fail(f"packages-lock.json is missing {package}")
        actual_version = entry.get("version")
        if actual_version != expected_version:
            fail(
                f"packages-lock.json {package}: expected {expected_version}, "
                f"found {actual_version!r}"
            )
        if entry.get("depth") != 0:
            fail(
                f"packages-lock.json {package} must be a direct dependency "
                f"with depth 0, found {entry.get('depth')!r}"
            )

def validate_builtin_module_ownership() -> None:
    scene = read_required(EXPECTED_BUILD_SCENE)
    hotbox_capture = read_required(
        "Assets/PartyNight/Tests/PlayMode/HotboxHavocRuntimeTests.cs"
    )
    motor = read_required(
        "Assets/PartyNight/Gameplay/Runtime/PartyNightCharacterMotor.cs"
    )
    composition = read_required(
        "Assets/PartyNight/Gameplay/Runtime/FoundationSceneComposition.cs"
    )
    hud = read_required(
        "Assets/PartyNight/Gameplay/Runtime/HotboxHavocPrototypeHud.cs"
    )

    required_by_usage = {}

    if "AudioListener:" in scene:
        required_by_usage["com.unity.modules.audio"] = "foundation scene AudioListener"

    if "EncodeToPNG" in hotbox_capture:
        required_by_usage["com.unity.modules.imageconversion"] = "PNG visual capture"

    if (
        "CharacterController" in motor
        or "Physics." in motor
        or "CharacterController" in composition
        or "Physics." in composition
    ):
        required_by_usage["com.unity.modules.physics"] = "gameplay physics/CharacterController"

    if "OnGUI" in hud or "GUI." in hud or "GUIStyle" in hud:
        required_by_usage["com.unity.modules.imgui"] = "prototype IMGUI HUD"

    manifest = json.loads(read_required("Packages/manifest.json"))
    lock = json.loads(read_required("Packages/packages-lock.json"))
    manifest_deps = manifest.get("dependencies", {})
    lock_deps = lock.get("dependencies", {})

    for package, reason in sorted(required_by_usage.items()):
        if manifest_deps.get(package) != "1.0.0":
            fail(
                f"{package} must be explicitly declared because Party Night uses {reason}"
            )
        entry = lock_deps.get(package)
        if not isinstance(entry, dict) or entry.get("depth") != 0:
            fail(
                f"{package} must be locked as a direct depth-0 dependency "
                f"because Party Night uses {reason}"
            )


def validate_assets_metadata() -> None:
    assets = ROOT / "Assets"
    if not assets.is_dir():
        fail("missing Assets directory")

    for directory in sorted(p for p in assets.rglob("*") if p.is_dir()):
        meta = directory.with_name(directory.name + ".meta")
        if not meta.is_file():
            fail(f"missing folder metadata: {meta.relative_to(ROOT)}")

    for asset in sorted(p for p in assets.rglob("*") if p.is_file()):
        if asset.suffix == ".meta":
            continue
        meta = asset.with_name(asset.name + ".meta")
        if not meta.is_file():
            fail(f"missing asset metadata: {meta.relative_to(ROOT)}")

def validate_meta_guids() -> None:
    seen = {}
    guid_pattern = re.compile(r"^[0-9a-f]{32}$")

    for meta in sorted((ROOT / "Assets").rglob("*.meta")):
        guid_lines = [
            line.split(":", 1)[1].strip()
            for line in meta.read_text(encoding="utf-8", errors="replace").splitlines()
            if line.startswith("guid:")
        ]
        if len(guid_lines) != 1:
            fail(f"{meta.relative_to(ROOT)} must contain exactly one top-level guid")

        guid = guid_lines[0]
        if not guid_pattern.fullmatch(guid):
            fail(f"{meta.relative_to(ROOT)} has invalid Unity GUID {guid!r}")

        previous = seen.get(guid)
        if previous is not None:
            fail(
                f"duplicate Unity GUID {guid} in {previous} and {meta.relative_to(ROOT)}"
            )
        seen[guid] = meta.relative_to(ROOT)

def validate_assembly_definitions() -> None:
    asmdefs = {}
    parsed = []

    for path in sorted((ROOT / "Assets" / "PartyNight").rglob("*.asmdef")):
        try:
            data = json.loads(path.read_text(encoding="utf-8"))
        except json.JSONDecodeError as exc:
            fail(f"invalid asmdef JSON in {path.relative_to(ROOT)}: {exc}")

        name = data.get("name")
        if not isinstance(name, str) or not name:
            fail(f"{path.relative_to(ROOT)} has no valid assembly name")
        if name in asmdefs:
            fail(
                f"duplicate assembly name {name!r}: "
                f"{asmdefs[name]} and {path.relative_to(ROOT)}"
            )

        asmdefs[name] = path.relative_to(ROOT)
        parsed.append((path, data))

    if not parsed:
        fail("no Party Night assembly definitions found")

    for path, data in parsed:
        references = data.get("references", [])
        if not isinstance(references, list):
            fail(f"{path.relative_to(ROOT)} references must be a list")

        for reference in references:
            if (
                isinstance(reference, str)
                and reference.startswith("PartyNight.")
                and reference not in asmdefs
            ):
                fail(
                    f"{path.relative_to(ROOT)} references missing project assembly "
                    f"{reference!r}"
                )

def require_meta_guid(asset_path: str, expected_guid: str) -> None:
    meta = ROOT / f"{asset_path}.meta"
    if not meta.is_file():
        fail(f"missing metadata for {asset_path}")
    if f"guid: {expected_guid}" not in meta.read_text(encoding="utf-8"):
        fail(f"{asset_path}.meta does not preserve expected GUID {expected_guid}")

def validate_authoritative_project_settings() -> None:
    required_project_settings = (
        "ProjectSettings/EditorSettings.asset",
        "ProjectSettings/ProjectSettings.asset",
        "ProjectSettings/GraphicsSettings.asset",
        "ProjectSettings/QualitySettings.asset",
    )
    for relative in required_project_settings:
        read_required(relative)

    settings_folder_meta = read_required("Assets/PartyNight/Settings.meta")
    if f"guid: {EXPECTED_SETTINGS_FOLDER_GUID}" not in settings_folder_meta:
        fail("Party Night Settings folder GUID changed unexpectedly")

    pipeline = read_required(EXPECTED_PIPELINE_ASSET)
    renderer = read_required(EXPECTED_RENDERER_ASSET)
    global_settings = read_required(EXPECTED_GLOBAL_SETTINGS_ASSET)
    read_required(EXPECTED_VOLUME_PROFILE_ASSET)

    require_meta_guid(EXPECTED_PIPELINE_ASSET, EXPECTED_PIPELINE_GUID)
    require_meta_guid(EXPECTED_RENDERER_ASSET, EXPECTED_RENDERER_GUID)
    require_meta_guid(EXPECTED_GLOBAL_SETTINGS_ASSET, EXPECTED_GLOBAL_SETTINGS_GUID)
    require_meta_guid(EXPECTED_VOLUME_PROFILE_ASSET, EXPECTED_VOLUME_PROFILE_GUID)

    if f"guid: {EXPECTED_RENDERER_GUID}" not in pipeline:
        fail("Party Night URP asset does not reference the authoritative renderer")
    if "m_DefaultRendererIndex: 0" not in pipeline:
        fail("Party Night URP asset default renderer index is not 0")

    if f"guid: {EXPECTED_VOLUME_PROFILE_GUID}" not in global_settings:
        fail("Party Night URP Global Settings do not reference the authoritative volume profile")

    graphics = read_required("ProjectSettings/GraphicsSettings.asset")
    if f"guid: {EXPECTED_PIPELINE_GUID}" not in graphics:
        fail("GraphicsSettings does not reference Party Night URP")
    if f"guid: {EXPECTED_GLOBAL_SETTINGS_GUID}" not in graphics:
        fail("GraphicsSettings does not register Party Night URP Global Settings")

    editor = read_required("ProjectSettings/EditorSettings.asset")
    if "m_SerializationMode: 2" not in editor:
        fail("EditorSettings must keep Force Text serialization")

    project = read_required("ProjectSettings/ProjectSettings.asset")
    if "productName: 'The 420 Lobby: Party Night'" not in project:
        fail("ProjectSettings product name does not match Party Night")

    quality = read_required("ProjectSettings/QualitySettings.asset")
    overrides = re.findall(
        r"^\s*customRenderPipeline:\s*(.+)$",
        quality,
        flags=re.MULTILINE,
    )
    if not overrides:
        fail("QualitySettings contains no render-pipeline ownership entries")
    if any(value.strip() != "{fileID: 0}" for value in overrides):
        fail("a quality level overrides the authoritative Party Night render pipeline")

    if renderer.count("m_Name: PartyNightUniversalRenderer") != 1:
        fail("authoritative Universal Renderer asset has unexpected identity")

def validate_render_pipeline_assembly_references() -> None:
    path = ROOT / "Assets" / "PartyNight" / "Editor" / "PartyNight.Foundation.Editor.asmdef"
    if not path.is_file():
        fail("missing PartyNight.Foundation.Editor.asmdef")

    try:
        data = json.loads(path.read_text(encoding="utf-8"))
    except json.JSONDecodeError as exc:
        fail(f"invalid PartyNight.Foundation.Editor.asmdef: {exc}")

    references = data.get("references")
    if not isinstance(references, list):
        fail("PartyNight.Foundation.Editor.asmdef references must be a list")

    required = {
        "PartyNight.Foundation",
        "Unity.RenderPipelines.Core.Runtime",
        "Unity.RenderPipelines.Universal.Runtime",
    }
    missing = sorted(required.difference(references))
    if missing:
        fail(
            "PartyNight.Foundation.Editor.asmdef is missing required references: "
            + ", ".join(missing)
        )


def validate_input_foundation() -> None:
    project = read_required("ProjectSettings/ProjectSettings.asset")
    if "  activeInputHandler: 1" not in project:
        fail("Active Input Handling must be Input System Package (New) only")

    asset_path = ROOT / EXPECTED_INPUT_ACTION_ASSET
    if not asset_path.is_file():
        fail(f"missing {EXPECTED_INPUT_ACTION_ASSET}")

    try:
        asset = json.loads(asset_path.read_text(encoding="utf-8"))
    except json.JSONDecodeError as exc:
        fail(f"invalid input actions JSON: {exc}")

    if asset.get("name") != "PartyNightInputActions":
        fail("input actions asset has unexpected name")

    maps = asset.get("maps")
    if not isinstance(maps, list) or len(maps) != 1 or maps[0].get("name") != "Gameplay":
        fail("input actions asset must contain exactly one Gameplay map")

    gameplay = maps[0]
    actions = gameplay.get("actions")
    if not isinstance(actions, list):
        fail("Gameplay actions must be a list")

    actual_actions = {action.get("name"): action for action in actions}
    if set(actual_actions) != set(EXPECTED_INPUT_ACTIONS):
        fail("Gameplay action set does not match Party Night input contract")

    for name, (expected_type, expected_control) in EXPECTED_INPUT_ACTIONS.items():
        action = actual_actions[name]
        if action.get("type") != expected_type:
            fail(f"{name}: expected action type {expected_type}")
        if action.get("expectedControlType") != expected_control:
            fail(f"{name}: expected control type {expected_control}")

    bindings = gameplay.get("bindings")
    if not isinstance(bindings, list):
        fail("Gameplay bindings must be a list")

    def has_binding(action_name: str, path: str, group: str) -> bool:
        return any(
            binding.get("action") == action_name
            and binding.get("path") == path
            and group in str(binding.get("groups", "")).split(";")
            for binding in bindings
        )

    required_keyboard_mouse = {
        ("Move", "<Keyboard>/w"),
        ("Move", "<Keyboard>/s"),
        ("Move", "<Keyboard>/a"),
        ("Move", "<Keyboard>/d"),
        ("Look", "<Mouse>/delta"),
        ("Jump", "<Keyboard>/space"),
        ("Interact", "<Keyboard>/e"),
        ("Grab", "<Keyboard>/f"),
        ("Dash", "<Keyboard>/leftShift"),
        ("UseItem", "<Mouse>/leftButton"),
        ("Emote", "<Keyboard>/g"),
    }
    for action_name, path in sorted(required_keyboard_mouse):
        if not has_binding(action_name, path, "KeyboardMouse"):
            fail(f"{action_name} missing KeyboardMouse binding {path}")

    for action_name, path in EXPECTED_GAMEPAD_BINDINGS.items():
        if not has_binding(action_name, path, "Gamepad"):
            fail(f"{action_name} missing Gamepad binding {path}")

    schemes = asset.get("controlSchemes")
    if not isinstance(schemes, list):
        fail("input control schemes must be a list")

    scheme_groups = {scheme.get("name"): scheme.get("bindingGroup") for scheme in schemes}
    if scheme_groups != {"KeyboardMouse": "KeyboardMouse", "Gamepad": "Gamepad"}:
        fail("input control schemes must be exactly KeyboardMouse and Gamepad")

    scheme_devices = {
        scheme.get("name"): [device.get("devicePath") for device in scheme.get("devices", [])]
        for scheme in schemes
    }
    if scheme_devices.get("KeyboardMouse") != ["<Keyboard>", "<Mouse>"]:
        fail("KeyboardMouse scheme must require Keyboard and Mouse")
    if scheme_devices.get("Gamepad") != ["<Gamepad>"]:
        fail("Gamepad scheme must require the generic Gamepad layout")

    build_settings = read_required("ProjectSettings/EditorBuildSettings.asset")
    expected_project_wide_actions = (
        "com.unity.input.settings.actions: "
        "{fileID: -944628639613478452, "
        "guid: " + EXPECTED_INPUT_ACTION_GUID + ", type: 3}"
    )
    if expected_project_wide_actions not in build_settings:
        fail("Project-wide Input Actions must be assigned in EditorBuildSettings")

    input_meta = read_required(EXPECTED_INPUT_ACTION_ASSET + ".meta")
    if f"guid: {EXPECTED_INPUT_ACTION_GUID}" not in input_meta:
        fail("input actions asset GUID changed unexpectedly")
    if "guid: 8404be70184654265930450def6a9037" not in input_meta:
        fail("input actions asset is not using Unity Input System's InputAction importer")
    if "generateWrapperCode: 0" not in input_meta:
        fail("generated Input Action wrapper code must remain disabled")

    input_asm = json.loads(read_required("Assets/PartyNight/Input/Runtime/PartyNight.Input.asmdef"))
    if "Unity.InputSystem" not in input_asm.get("references", []):
        fail("PartyNight.Input must reference Unity.InputSystem")
    if "ENABLE_INPUT_SYSTEM" not in input_asm.get("defineConstraints", []):
        fail("PartyNight.Input must require ENABLE_INPUT_SYSTEM")

    editor_asm = json.loads(read_required("Assets/PartyNight/Editor/PartyNight.Foundation.Editor.asmdef"))
    editor_refs = set(editor_asm.get("references", []))
    for required in ("PartyNight.Input", "Unity.InputSystem"):
        if required not in editor_refs:
            fail(f"PartyNight.Foundation.Editor missing input reference {required}")

    test_asm = json.loads(read_required("Assets/PartyNight/Tests/EditMode/PartyNight.Foundation.EditModeTests.asmdef"))
    test_refs = set(test_asm.get("references", []))
    for required in ("PartyNight.Input", "Unity.InputSystem"):
        if required not in test_refs:
            fail(f"EditMode tests missing input reference {required}")


def validate_gameplay_foundation() -> None:
    gameplay_asm_path = (
        "Assets/PartyNight/Gameplay/Runtime/PartyNight.Gameplay.asmdef"
    )
    gameplay_asm = json.loads(read_required(gameplay_asm_path))
    if gameplay_asm.get("references") != ["PartyNight.Input"]:
        fail("PartyNight.Gameplay must depend on PartyNight.Input only")
    if "ENABLE_INPUT_SYSTEM" not in gameplay_asm.get("defineConstraints", []):
        fail("PartyNight.Gameplay must require ENABLE_INPUT_SYSTEM")

    required_runtime_files = (
        "Assets/PartyNight/Gameplay/Runtime/PartyNightCharacterMotor.cs",
        "Assets/PartyNight/Gameplay/Runtime/PartyNightOrbitCamera.cs",
        "Assets/PartyNight/Gameplay/Runtime/PartyNightLocalPlayerController.cs",
        "Assets/PartyNight/Gameplay/Runtime/FoundationSceneComposition.cs",
        "Assets/PartyNight/Gameplay/Runtime/HotboxHavocRoundPhase.cs",
        "Assets/PartyNight/Gameplay/Runtime/HotboxHavocRoundController.cs",
        "Assets/PartyNight/Gameplay/Runtime/HotboxHavocPrototype.cs",
        "Assets/PartyNight/Gameplay/Runtime/HotboxHavocPrototypeVisuals.cs",
        "Assets/PartyNight/Gameplay/Runtime/HotboxHavocPrototypeHud.cs",
    )
    for relative in required_runtime_files:
        read_required(relative)

    motor = read_required(
        "Assets/PartyNight/Gameplay/Runtime/PartyNightCharacterMotor.cs"
    )
    if "CharacterController" not in motor:
        fail("Party Night movement motor must use CharacterController")
    if "Rigidbody" in motor:
        fail("Party Night movement motor must not introduce a parallel Rigidbody controller")

    for relative in required_runtime_files:
        text = read_required(relative)
        if "UnityEngine.InputSystem" in text:
            fail(
                f"{relative} bypasses the PartyNight.Input abstraction with direct Input System access"
            )
        for physical_path in ("<Keyboard>", "<Mouse>", "<Gamepad>", "<Touchscreen>"):
            if physical_path in text:
                fail(
                    f"{relative} hardcodes physical input path {physical_path}"
                )

    editor_asm = json.loads(
        read_required("Assets/PartyNight/Editor/PartyNight.Foundation.Editor.asmdef")
    )
    if "PartyNight.Gameplay" not in editor_asm.get("references", []):
        fail("PartyNight.Foundation.Editor must reference PartyNight.Gameplay")

    playmode_asm_path = (
        "Assets/PartyNight/Tests/PlayMode/PartyNight.Gameplay.PlayModeTests.asmdef"
    )
    playmode_asm = json.loads(read_required(playmode_asm_path))
    playmode_refs = set(playmode_asm.get("references", []))
    required_playmode_refs = {
        "PartyNight.Gameplay",
        "PartyNight.Input",
        "Unity.InputSystem",
    }
    if not required_playmode_refs.issubset(playmode_refs):
        fail("PlayMode tests are missing required gameplay/input references")
    if playmode_asm.get("includePlatforms") != []:
        fail("PlayMode test assembly must not be Editor-only")

    local_player_tests = read_required(
        "Assets/PartyNight/Tests/PlayMode/LocalPlayerRuntimeTests.cs"
    )
    jump_signature = (
        "[UnityTest]\n"
        "        public IEnumerator JumpUsesExplicitGravityAndReturnsToGround()"
    )
    if jump_signature not in local_player_tests:
        fail(
            "jump grounding regression test must run as a UnityTest across real frames"
        )
    jump_start = local_player_tests.index(jump_signature)
    jump_end = local_player_tests.find(
        "        [", jump_start + len(jump_signature)
    )
    jump_body = (
        local_player_tests[jump_start:]
        if jump_end < 0
        else local_player_tests[jump_start:jump_end]
    )
    if jump_body.count("yield return null;") < 3:
        fail(
            "jump grounding regression test must advance Unity frames while settling, "
            "jumping and landing"
        )
    if "PARTY_NIGHT_TEST_RESULT" not in local_player_tests:
        fail("PlayMode tests must emit durable cloud result diagnostics")

    composition_meta = read_required(
        "Assets/PartyNight/Gameplay/Runtime/FoundationSceneComposition.cs.meta"
    )
    if f"guid: {EXPECTED_FOUNDATION_COMPOSITION_GUID}" not in composition_meta:
        fail("FoundationSceneComposition script GUID changed unexpectedly")

    input_frame = read_required(
        "Assets/PartyNight/Input/Runtime/PartyNightInputFrame.cs"
    )
    if "PartyNightLookInputMode LookMode" not in input_frame:
        fail("PartyNightInputFrame must preserve pointer-delta vs look-rate semantics")

    input_reader = read_required(
        "Assets/PartyNight/Input/Runtime/PartyNightInputReader.cs"
    )
    if "CreateFromProjectWideActions" not in input_reader:
        fail("PartyNightInputReader must consume the assigned project-wide actions")
    if "cloneSource: false" not in input_reader:
        fail("PartyNightInputReader must borrow the project-wide Action Asset without cloning it")
    if "manageActionMapState: false" not in input_reader:
        fail("project-wide input reader must not disable or destroy the shared Action Asset")
    if "device is Pointer" not in input_reader:
        fail("PartyNightInputReader must classify pointer delta look separately")


def validate_hotbox_havoc_prototype() -> None:
    for relative in EXPECTED_HOTBOX_PROTOTYPE_FILES:
        read_required(relative)

    round_controller = read_required(
        "Assets/PartyNight/Gameplay/Runtime/HotboxHavocRoundController.cs"
    )
    required_constants = (
        "CountdownSeconds = 3f",
        "ActiveRoundSeconds = 20f",
        "StartClearRadius = 8.5f",
        "EndClearRadius = 3f",
        "ExposureToEliminateSeconds = 2.5f",
    )
    for value in required_constants:
        if value not in round_controller:
            fail(f"Hotbox Havoc prototype is missing required tuning constant: {value}")

    prototype = read_required(
        "Assets/PartyNight/Gameplay/Runtime/HotboxHavocPrototype.cs"
    )
    if "VisualValidation/HotboxHavoc_Overview.png" not in prototype:
        fail("Hotbox Havoc prototype visual artifact path changed unexpectedly")
    if "VisualValidation/HotboxHavoc_Overview.json" not in prototype:
        fail("Hotbox Havoc exact-revision manifest path changed unexpectedly")

    composition = read_required(
        "Assets/PartyNight/Gameplay/Runtime/FoundationSceneComposition.cs"
    )
    if composition.count("AddComponent<HotboxHavocPrototype>()") != 1:
        fail("FoundationSceneComposition must create exactly one HotboxHavocPrototype")
    if "new GameObject(HotboxHavocPrototype.RuntimeName)" not in composition:
        fail("Hotbox Havoc prototype must own a child root, not rename Foundation Runtime")

    prototype_source = read_required(
        "Assets/PartyNight/Gameplay/Runtime/HotboxHavocPrototype.cs"
    )
    if "gameObject.name = RuntimeName" in prototype_source:
        fail("Hotbox Havoc prototype must not rename its composition host")

    visual_code = read_required(
        "Assets/PartyNight/Gameplay/Runtime/HotboxHavocPrototypeVisuals.cs"
    )
    if "Universal Render Pipeline/Unlit" not in visual_code:
        fail("Hotbox prototype visuals must use the project URP pipeline")
    if "SpawnMarkerCount = 16" not in visual_code:
        fail("Hotbox prototype must expose 16 future multiplayer spawn markers")

    runtime_files = list(EXPECTED_HOTBOX_PROTOTYPE_FILES) + [
        "Assets/PartyNight/Gameplay/Runtime/FoundationSceneComposition.cs",
    ]
    for relative in runtime_files:
        text = read_required(relative)
        if "UnityEngine.InputSystem" in text:
            fail(f"{relative} bypasses PartyNight.Input with direct Input System access")
        for physical_path in ("<Keyboard>", "<Mouse>", "<Gamepad>", "<Touchscreen>"):
            if physical_path in text:
                fail(f"{relative} hardcodes physical input path {physical_path}")

    capture_test = read_required(
        "Assets/PartyNight/Tests/PlayMode/HotboxHavocRuntimeTests.cs"
    )
    for required in (
        "CapturesRealUnityVisualProgressArtifact",
        "camera.Render()",
        "Texture2D",
        "EncodeToPNG",
        "1280",
        "720",
    ):
        if required not in capture_test:
            fail(f"visual validation Play Mode test missing {required}")

    exporter = read_required(
        "Assets/PartyNight/Editor/HotboxHavocVisualArtifactExporter.cs"
    )
    if "#if UNITY_CLOUD_BUILD" not in exporter:
        fail("visual artifact exporter must enforce capture presence on Unity Cloud")
    if "BuildFailedException" not in exporter:
        fail("Unity Cloud visual artifact exporter must fail when evidence is missing")
    if "BUILD_REVISION" not in exporter:
        fail("visual artifact exporter must bind evidence to BUILD_REVISION")
    if "ValidateCurrentCloudCapture" not in exporter:
        fail("visual artifact exporter must validate evidence before Player export")
    if "HotboxHavocPrototype.VisualCaptureManifestRelativePath" not in exporter:
        fail("visual artifact exporter must copy the centralized exact-revision evidence manifest")

    project_validator = read_required(
        "Assets/PartyNight/Editor/ProjectFoundationValidator.cs"
    )
    if "HotboxHavocVisualArtifactExporter.ValidateCurrentCloudCapture();" not in project_validator:
        fail("pre-export must validate exact-revision Hotbox visual evidence")

    gitignore = read_required(".gitignore")
    if "/VisualValidation/" not in gitignore:
        fail("generated VisualValidation directory must be gitignored")

    if (ROOT / "VisualValidation").exists():
        fail("generated VisualValidation output must never be committed")


def validate_capture_cleanup() -> None:
    forbidden = (
        "Assets/PartyNight/Editor/AuthoritativeSettingsBootstrap.cs",
        "Assets/PartyNight/Editor/AuthoritativeSettingsBootstrap.cs.meta",
        ".github/authoritative-settings-capture.part1.b64",
        ".github/authoritative-settings-capture.part2.b64",
        ".github/authoritative-settings-capture.part3.b64",
        ".github/authoritative-settings-capture.part4.b64",
        ".github/workflows/import-authoritative-settings.yml",
    )
    leftovers = [relative for relative in forbidden if (ROOT / relative).exists()]
    if leftovers:
        fail("temporary settings-capture implementation remains: " + ", ".join(leftovers))

def validate_build_scene() -> None:
    scene = ROOT / EXPECTED_BUILD_SCENE
    if not scene.is_file():
        fail(f"missing build scene: {EXPECTED_BUILD_SCENE}")

    scene_text = scene.read_text(encoding="utf-8")
    for required_name in (
        "m_Name: Main Camera",
        "m_Name: Directional Light",
        "m_Name: Foundation Scene Composition",
        "SceneRoots:",
    ):
        if required_name not in scene_text:
            fail(f"build scene is missing required serialized content: {required_name}")

    scene_meta = scene.with_name(scene.name + ".meta")
    if not scene_meta.is_file():
        fail(f"missing build scene metadata: {scene_meta.relative_to(ROOT)}")
    if f"guid: {EXPECTED_BUILD_SCENE_GUID}" not in scene_meta.read_text(encoding="utf-8"):
        fail("build scene metadata GUID does not match the expected build settings GUID")

    settings = ROOT / "ProjectSettings" / "EditorBuildSettings.asset"
    if not settings.is_file():
        fail("missing ProjectSettings/EditorBuildSettings.asset")

    settings_text = settings.read_text(encoding="utf-8")
    required_block = (
        "  - enabled: 1\n"
        f"    path: {EXPECTED_BUILD_SCENE}\n"
        f"    guid: {EXPECTED_BUILD_SCENE_GUID}"
    )
    if required_block not in settings_text:
        fail("foundation scene is not enabled with the expected path/GUID")

    if settings_text.count("  - enabled: 1") != 1:
        fail("exactly one build scene must be enabled")

    scene_paths = [
        line.strip()
        for line in settings_text.splitlines()
        if line.strip().startswith("path: Assets/")
    ]
    if scene_paths != [f"path: {EXPECTED_BUILD_SCENE}"]:
        fail("EditorBuildSettings must contain only the intended foundation scene")

    expected_script = (
        "m_Script: {fileID: 11500000, "
        "guid: " + EXPECTED_FOUNDATION_COMPOSITION_GUID + ", type: 3}"
    )
    if scene_text.count("m_Script:") != 1:
        fail("foundation scene must contain exactly one serialized MonoBehaviour")
    if expected_script not in scene_text:
        fail("foundation scene does not reference FoundationSceneComposition")

def validate_csharp_namespace_hygiene() -> None:
    party_night_assets = ROOT / "Assets" / "PartyNight"
    forbidden_imports = {
        "using UnityEditor.PackageManager;": (
            "broad UnityEditor.PackageManager import can collide with UnityEditor types; "
            "alias or fully qualify the required Package Manager type instead"
        ),
    }

    for path in sorted(party_night_assets.rglob("*.cs")):
        text = path.read_text(encoding="utf-8", errors="replace")
        for forbidden, reason in forbidden_imports.items():
            if forbidden in text:
                fail(f"{path.relative_to(ROOT)}: {reason}")

def validate_generated_directories_absent() -> None:
    present = sorted(name for name in FORBIDDEN_ROOT_DIRS if (ROOT / name).exists())
    if present:
        fail("generated/build directories must not be tracked: " + ", ".join(present))

def validate_no_stale_artifacts() -> None:
    for path in ROOT.rglob("*"):
        if ".git" in path.parts:
            continue

        if path.is_dir() and path.name.lower() in FORBIDDEN_STALE_DIR_NAMES:
            fail(f"stale/superseded directory is not allowed: {path.relative_to(ROOT)}")

        if path.is_file() and path.suffix.lower() in FORBIDDEN_STALE_SUFFIXES:
            fail(f"stale/superseded file is not allowed: {path.relative_to(ROOT)}")

def validate_conflict_markers() -> None:
    markers = ("<" * 7, ">" * 7)
    for path in ROOT.rglob("*"):
        if not path.is_file() or path.suffix.lower() not in TEXT_SUFFIXES:
            continue
        text = path.read_text(encoding="utf-8", errors="replace")
        for marker in markers:
            if marker in text:
                fail(f"merge conflict marker {marker!r} found in {path.relative_to(ROOT)}")

def main() -> None:
    validate_project_version()
    validate_manifest()
    validate_lockfile()
    validate_builtin_module_ownership()
    validate_assets_metadata()
    validate_meta_guids()
    validate_assembly_definitions()
    validate_authoritative_project_settings()
    validate_render_pipeline_assembly_references()
    validate_input_foundation()
    validate_gameplay_foundation()
    validate_hotbox_havoc_prototype()
    validate_capture_cleanup()
    validate_build_scene()
    validate_csharp_namespace_hygiene()
    validate_generated_directories_absent()
    validate_no_stale_artifacts()
    validate_conflict_markers()
    print("Party Night Unity foundation static validation passed.")

if __name__ == "__main__":
    main()
