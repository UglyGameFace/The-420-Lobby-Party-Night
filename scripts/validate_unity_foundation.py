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

EXPECTED_PACKAGES = {
    "com.unity.inputsystem": "1.20.0",
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
    for required_name in ("m_Name: Main Camera", "m_Name: Directional Light", "SceneRoots:"):
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

    if "m_Script:" in scene_text:
        fail("foundation scene must not contain serialized MonoBehaviour script references yet")

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
    validate_assets_metadata()
    validate_meta_guids()
    validate_assembly_definitions()
    validate_authoritative_project_settings()
    validate_capture_cleanup()
    validate_build_scene()
    validate_csharp_namespace_hygiene()
    validate_generated_directories_absent()
    validate_no_stale_artifacts()
    validate_conflict_markers()
    print("Party Night Unity foundation static validation passed.")

if __name__ == "__main__":
    main()
