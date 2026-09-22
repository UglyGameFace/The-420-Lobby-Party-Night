#!/usr/bin/env python3
import json
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
EXPECTED_UNITY = "6000.3.24f1"
EXPECTED_REVISION = "4e7b9b5b6244"
EXPECTED_BUILD_SCENE = "Assets/PartyNight/Scenes/PartyNightFoundation.unity"
EXPECTED_BUILD_SCENE_GUID = "7b8f6f38b4e84b4aa2bb5dc6b124d5a1"
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


def validate_project_version() -> None:
    path = ROOT / "ProjectSettings" / "ProjectVersion.txt"
    if not path.is_file():
        fail(f"missing {path.relative_to(ROOT)}")

    content = path.read_text(encoding="utf-8")
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
        fail("exactly one build scene must be enabled during the foundation scene task")


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
    validate_assets_metadata()
    validate_build_scene()
    validate_generated_directories_absent()
    validate_no_stale_artifacts()
    validate_conflict_markers()
    print("Party Night Unity foundation static validation passed.")


if __name__ == "__main__":
    main()
