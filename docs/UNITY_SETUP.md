# Unity Project Setup

## Required editor

Use **Unity 6000.3.24f1**.

The project pins the editor in `ProjectSettings/ProjectVersion.txt`. Do not casually open and save the project with a different editor patch.

## Recommended Unity Hub modules

Install only the modules needed for the platform being validated.

Foundation targets eventually require:
- Windows build support where needed;
- Linux build support;
- Linux Dedicated Server build support;
- Web Build Support;
- Android Build Support including the Unity-recommended SDK/NDK/JDK;
- iOS Build Support for iOS project generation.

Final iOS compilation/signing requires Apple's toolchain on macOS.

## First editor open

Expected behavior:
1. Unity reads `ProjectSettings/ProjectVersion.txt`.
2. Package Manager resolves the exact top-level dependencies in `Packages/manifest.json`.
3. Unity generates local `Library/`, `Temp/`, and related folders. They are ignored and must not be committed.
4. Unity generates `Packages/packages-lock.json`. Review it before committing it because it is the evidence of the actual resolved transitive dependency graph.

Do not create or commit a fabricated `packages-lock.json` before Unity resolves the project.

## Static validation

From the repository root:

```bash
python3 scripts/validate_unity_foundation.py
```

This verifies repository structure and pins. It does **not** compile Unity code.

## Unity batch-mode foundation validation

With Unity 6000.3.24f1 installed and licensed:

```bash
Unity \
  -batchmode \
  -nographics \
  -quit \
  -projectPath . \
  -executeMethod PartyNight.Foundation.Editor.ProjectFoundationValidator.Validate \
  -logFile -
```

Use the platform-specific Unity executable path as appropriate.

This validator checks:
- exact editor version;
- exact resolved versions for the five foundation packages.

A successful editor invocation also proves the project reached script compilation far enough to execute the validation method.

## Edit Mode tests

Run the Party Night Edit Mode test assembly through Unity Test Framework after package resolution.

The first smoke test verifies the immutable product identity and the 12–16 player initial match bounds.

## What is intentionally not configured yet

This foundation does not hand-author serialized Unity settings/assets that should be created by the editor, including:
- URP pipeline/renderer assets;
- graphics quality tiers;
- Input Actions;
- scenes;
- build profiles;
- prefabs;
- NetworkManager objects.

Those are subsequent tasks and must be created with the Unity Editor so GUIDs/references/serialization can be validated instead of guessed.
