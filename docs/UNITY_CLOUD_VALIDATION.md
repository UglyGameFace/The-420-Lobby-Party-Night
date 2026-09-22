# Unity Cloud Validation

## Purpose

The Party Night owner works from a phone. Unity validation therefore runs in the cloud rather than depending on a local desktop editor.

The authoritative service for the foundation is **Unity Build Automation (UBA)**.

## One-time Unity Dashboard connection

From a phone browser:

1. Sign in to Unity Dashboard with the Unity account that owns the cloud project.
2. Open **Build Automation**.
3. Connect version control and select **GitHub**.
4. Authorize access to `UglyGameFace/The-420-Lobby-Party-Night`.
5. Select branch `foundation/unity-project` while PR #2 is under validation.
6. Use Unity version **6000.3.24f1** if the build configuration does not auto-detect `ProjectSettings/ProjectVersion.txt`.

For this public repository, Unity documentation states that a classic GitHub PAT can use `public_repo` for repository access. Auto-build additionally requires repository-hook write access. Prefer the minimum permissions required by the current Unity Dashboard flow.

Never commit a PAT or Unity credential to this repository.

## Foundation build configuration

For the PR #2 foundation target:

- editor: `6000.3.24f1`;
- source branch: `foundation/unity-project`;
- enable project unit tests;
- enable **Edit Mode** tests;
- mark the build failed when a test fails;
- pre-export method:
  `PartyNight.Foundation.Editor.ProjectFoundationValidator.PreExport`;
- do not enable automatic production distribution.

The pre-export method runs after script compilation and calls the same exact-version/package validator used by optional batch-mode validation.

## Evidence required before PR #2 can merge

A successful static GitHub workflow alone is insufficient.

Required Unity evidence:

1. UBA successfully checks out the PR branch.
2. Unity 6000.3.24f1 starts the project.
3. Package Manager resolves the manifest.
4. Party Night scripts compile.
5. `ProjectFoundationValidator.PreExport` completes.
6. The Edit Mode smoke test passes.
7. The resolved package graph is reviewed.
8. Any Unity-generated source-controlled settings needed for a healthy project are reviewed before being added.
9. Final branch cleanup confirms no superseded or temporary implementation remains.

## Build-scene limitation

This foundation deliberately does not commit a hand-written Unity scene merely to make CI look green.

If UBA reaches compilation, package validation, and Edit Mode tests but the final Player export requires a real scene, that is recorded separately from compilation/test success. The first actual scene must be created through an authoritative Unity editor workflow in its proper active task, not fabricated as serialized YAML.

## Future build targets

After the shared Unity foundation is validated, cloud targets can be added deliberately for:

- Android;
- Web;
- Linux desktop;
- Windows;
- macOS;
- Linux dedicated server;
- iOS project generation.

Each target must be validated independently. Passing one target never proves the others.
