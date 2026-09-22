# Unity Cloud Validation

## Purpose

The Party Night owner works from a phone. All required Unity editor, test, and Player-build validation must therefore be reproducible in **Unity Build Automation (UBA)** without a local desktop Unity installation.

The repository is the source of truth for project content. UBA is the authoritative Unity execution environment.

## Repository connection

Unity Build Automation is connected to:

`UglyGameFace/The-420-Lobby-Party-Night`

Credentials and GitHub Personal Access Tokens must exist only in Unity/GitHub account configuration. Never commit credentials to the repository.

## Canonical foundation target

Build target name:

`Party Night - Foundation Validation`

Expected target configuration:

- target platform: **Linux desktop 64-bit**;
- project subfolder: blank;
- Unity version: **Auto detect Unity version**;
- `Build with closest version`: **off**;
- repository pin: `ProjectSettings/ProjectVersion.txt`;
- builder OS: **macOS Sequoia**;
- Unity Editor architecture: **Apple-Silicon**;
- machine: **Standard**;
- Build Profile path: blank until a source-controlled Party Night Build Profile is created;
- Development Build: off;
- headless/dedicated-server options: off for this client validation target;
- Addressables build: off until Addressables are intentionally adopted;
- AssetBundle build: off until intentionally adopted;
- automatic distribution: off;
- automatic production deployment: off.

The source branch changes to the **current active task branch**. Never replay an old build when the active branch/head changed.

## Tests

Required for every foundation/editor/configuration build:

- `Run my project's unit tests when building`: on;
- `Run EditMode tests`: on;
- `Run PlayMode tests`: off until runtime behavior exists;
- `Mark build as failed if any test fails`: on.

When runtime systems exist, Play Mode testing is enabled as an additional gate rather than replacing Edit Mode tests.

## Script hook

Pre-Export method:

`PartyNight.Foundation.Editor.ProjectFoundationValidator.PreExport`

Leave unrelated pre-build, post-build, and post-export script fields blank unless an active task explicitly introduces a reviewed script.

UBA invokes the pre-export method after Unity script compilation and before Player export. Therefore:
- compiler errors are **compile-gate failures**, even if the dashboard reports them during the test phase;
- the pre-export validator cannot catch code that fails to compile;
- static GitHub checks run first to catch predictable source errors before cloud minutes are consumed.

## Scene ownership

UBA's **Scene List** override must remain empty unless an active task explicitly requires a target-specific scene set.

With no UBA override, Unity uses the source-controlled build-scene configuration in:

`ProjectSettings/EditorBuildSettings.asset`

This prevents the Unity Dashboard from silently owning a different scene list than Git.

Current foundation scene:

`Assets/PartyNight/Scenes/PartyNightFoundation.unity`

## Cache policy

Normal script iterations may use the project/Library cache.

Use a clean/non-stale validation run when:
- Unity editor version changes;
- package versions or the package graph changes;
- render pipeline configuration changes;
- core ProjectSettings change;
- input backend/platform configuration changes;
- the build result appears inconsistent with source;
- validating a release candidate.

Do not delete/disable caching merely because a normal C# edit occurred. The goal is reproducibility without wasting cloud minutes.

## Required evidence from a cloud run

Before a task that affects Unity buildability can merge, record:

1. exact Git commit checked out by UBA;
2. exact Unity editor version and changeset;
3. package resolution result;
4. C# compile result;
5. Edit Mode test result;
6. Play Mode test result when applicable;
7. pre-export validator result;
8. Player export result for the affected platform;
9. warnings/errors that require action;
10. artifact/runtime smoke result when the task reaches that stage.

A dashboard label is not enough. The build log determines which gate actually failed.

## Failure classification

Classify failures before changing code:

- **Checkout/configuration**: wrong branch/head, credentials, target configuration.
- **Package resolution**: manifest/registry/version/dependency problem.
- **Compilation**: C# or assembly-definition error.
- **Edit Mode test**: assertion/runtime failure after successful compilation.
- **Pre-export validation**: imported asset/project-setting invariant failed.
- **Player export**: scenes, platform settings, shaders, native tooling, packaging.
- **Runtime smoke**: exported Player starts incorrectly or behavior fails.

Do not patch a later stage when an earlier stage is the actual failure.

## Exact-head rule

The UBA log must show the same commit that is being proposed for merge.

If any implementation or bookkeeping commit lands afterward, that newer head must pass the applicable GitHub/Unity gates before merge. A successful older commit is evidence for the older commit only.

## Phone-only workflow

The owner only needs a phone to:
- change the UBA target's active branch when instructed;
- start the build;
- inspect/download the build log or artifact;
- upload evidence when repository automation cannot access Unity Cloud directly;
- install Android artifacts later for device testing.

No required workflow may quietly depend on a local PC.

## Official references

- Build configuration: https://docs.unity.com/en-us/build-automation/basic-build-configuration/overview
- Unit tests: https://docs.unity.com/en-us/build-automation/reference/unit-tests
- Advanced settings: https://docs.unity.com/en-us/build-automation/advanced-build-configuration/overview
- Pre/post-export methods: https://docs.unity.com/en-us/build-automation/advanced-build-configuration/run-custom-scripts-during-the-build-process
- Scene selection: https://docs.unity.com/en-us/build-automation/advanced-build-configuration/specify-the-scene-to-be-built
- Build-speed/caching guidance: https://docs.unity.com/en-us/build-automation/optimize-build-speed
- Unity clean-build option: https://docs.unity.com/en-us/engine/6000.3/script-reference/unityeditor/buildoptions/cleanbuildcache
