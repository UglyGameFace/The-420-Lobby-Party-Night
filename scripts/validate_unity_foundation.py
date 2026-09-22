#!/usr/bin/env python3
import json
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
EXPECTED_UNITY = "6000.3.24f1"
EXPECTED_REVISION = "4e7b9b5b6244"
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


def validate_generated_directories_absent() -> None:
    present = sorted(name for name in FORBIDDEN_ROOT_DIRS if (ROOT / name).exists())
    if present:
        fail("generated/build directories must not be tracked: " + ", ".join(present))


def validate_conflict_markers() -> None:
    markers = ("<<<<<<<", ">>>>>>>")
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
    validate_generated_directories_absent()
    validate_conflict_markers()
    print("Party Night Unity foundation static validation passed.")


if __name__ == "__main__":
    main()
