# Unity Project Setup

## Required editor

Use **Unity 6000.3.24f1**.

The project pins the editor in `ProjectSettings/ProjectVersion.txt`. Do not casually open and save the project with a different editor patch.

## Authoritative validation path

Party Night is developed with a **phone-only owner workflow**.

The authoritative editor/build validation path is **Unity Build Automation (UBA)** connected to this GitHub repository. A local desktop Unity installation is optional and must never be required for the project owner to continue development.

UBA currently supports the Unity 6000.3 LTS line. The repository exposes:

`PartyNight.Foundation.Editor.ProjectFoundationValidator.PreExport`

as the pre-export validation hook. It runs after Unity script compilation and verifies the exact editor and resolved foundation package versions.

Enable Edit Mode tests for the UBA build target and configure the build to fail when tests fail.

See `docs/UNITY_CLOUD_VALIDATION.md` for the exact phone-only configuration.

## Package resolution

Expected cloud/editor behavior:

1. Unity reads `ProjectSettings/ProjectVersion.txt`.
2. Package Manager resolves the exact top-level dependencies in `Packages/manifest.json`.
3. Unity generates local/cloud workspace folders such as `Library/`, `Temp/`, and `Logs/`. They must never be committed.
4. Unity generates `Packages/packages-lock.json` after dependency resolution.

Do not hand-author a fake `packages-lock.json`.

The resolved lock file must be reviewed from authoritative Unity output before it is added to source control.

## Static validation

GitHub runs:

```bash
python3 scripts/validate_unity_foundation.py
```

This verifies repository structure, exact pins, Unity metadata coverage, conflict markers, generated-directory hygiene, and stale/superseded artifacts.

It does **not** compile Unity code.

## Optional local equivalent

A developer who happens to have Unity 6000.3.24f1 installed may run:

```bash
Unity \
  -batchmode \
  -nographics \
  -quit \
  -projectPath . \
  -executeMethod PartyNight.Foundation.Editor.ProjectFoundationValidator.Validate \
  -logFile -
```

This is an optional diagnostic equivalent. It is not the required Party Night workflow.

## Edit Mode tests

The Party Night Edit Mode assembly contains the first foundation smoke test.

The cloud target must enable:
- Run project unit tests;
- Edit Mode tests;
- fail the build when tests fail.

The initial smoke test verifies the immutable product identity and the 12–16 player initial match bounds.

## Serialized Unity assets

Do not blindly hand-author serialized Unity settings/assets that should be created by the pinned editor, including:
- URP pipeline/renderer assets;
- graphics quality tiers;
- Input Actions;
- scenes;
- build profiles;
- prefabs;
- NetworkManager objects.

Those assets are created only when their active task requires them and must be validated by Unity.

## No stale code rule

When an implementation is replaced, the superseded implementation must be removed in the same task after callers/references are verified.

Do not retain:
- backup copies;
- `.old`, `.bak`, `.orig`, `.rej`, or temporary files;
- duplicate managers/services;
- abandoned compatibility paths;
- commented-out replaced implementations;
- temporary debug helpers after their purpose ends;
- deprecated copies kept "just in case."

Git history is the backup. The working tree is not a museum.
