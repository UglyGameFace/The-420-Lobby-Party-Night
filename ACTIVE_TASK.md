# ACTIVE TASK

## Active task / outcome

Create the first **real buildable Unity scene** for **The 420 Lobby: Party Night** and remove the exact Player-export blocker proven by Unity Build Automation build #1.

Outcome:
- commit a real Unity scene rather than fabricating one only inside CI;
- configure that scene as the enabled Player build scene;
- validate the scene through Unity 6000.3.24f1;
- keep the scene intentionally foundation-only, with no Hotbox Havoc gameplay;
- preserve the no-stale-code rule.

## Scope

Included:
- Party Night repository only;
- one source-controlled foundation scene;
- Main Camera and Directional Light only;
- Unity build-scene configuration;
- scene/build-settings static validation;
- Edit Mode regression coverage for build-scene configuration;
- pre-export validation of the configured scene;
- exact-head GitHub and Unity Build Automation validation.

Excluded:
- Hotbox Havoc gameplay;
- player movement;
- character models;
- arena art;
- URP pipeline asset tuning;
- Input Actions;
- networking;
- Discord/backend integration;
- changes to any other repository.

## Status

**VALIDATION**

Working branch:
`foundation/first-buildable-scene`

## Root cause

### UBA build #1

The previous foundation passed package resolution, script compilation, Edit Mode tests, and the Party Night pre-export validator.

Player export then failed because no build scene existed:

`ERROR: There were no scenes configured to build!`

This task added the real source-controlled scene and build settings required to fix that blocker.

### UBA build #2

Build #2 checked out the intended scene-task head:

`158718866c95fd2c95b5d74154c5172e7ebdab39`

and correctly detected Unity:

`6000.3.24f1 (4e7b9b5b6244)`

Package resolution succeeded and Unity generated a package lock in the cloud workspace.

The build then stopped during C# compilation before tests or Player export. Exact compiler failure:

`CS0104: 'PackageInfo' is an ambiguous reference between 'UnityEditor.PackageManager.PackageInfo' and 'UnityEditor.PackageInfo'`

Root cause:
- the scene validator needed `UnityEditor.AssetDatabase`, `SceneAsset`, and `EditorBuildSettings`;
- a broad `using UnityEditor;` was added next to `using UnityEditor.PackageManager;`;
- both namespaces expose a type named `PackageInfo`;
- the unqualified `PackageInfo.GetAllRegisteredPackages()` therefore became ambiguous.

Correction:
- removed the broad Package Manager namespace import;
- aliased the exact package type as `PackageManagerPackageInfo`;
- fully qualified the other UnityEditor APIs;
- added a GitHub static rule that rejects the broad `using UnityEditor.PackageManager;` pattern in Party Night source so this exact regression is stopped before another cloud build.

Build #2's dashboard-level "unit tests failed" is not an assertion failure. The log proves Edit Mode test compilation never completed because the editor assembly did not compile.

## Architecture

The source-controlled scene is:
`Assets/PartyNight/Scenes/PartyNightFoundation.unity`

It is enabled through:
`ProjectSettings/EditorBuildSettings.asset`

The scene contains only foundational rendering objects:
- Main Camera;
- Audio Listener;
- Directional Light.

URP package support remains installed, but pipeline assets and URP-specific scene components are intentionally deferred to their own validated task rather than being guessed into this build-fix.

No temporary runtime scene generator is used. No CI-only fake scene is used.

## Changes

Implemented:
- added `Assets/PartyNight/Scenes/PartyNightFoundation.unity`;
- added tracked scene/folder `.meta` files;
- added `ProjectSettings/EditorBuildSettings.asset` with exactly one enabled scene;
- extended the Unity pre-export validator to require the imported/enabled scene;
- added an Edit Mode regression test for scene/build-settings ownership;
- extended static validation to enforce the exact scene path/GUID and reject multiple enabled foundation scenes;
- fixed the UnityEditor/`PackageInfo` namespace collision found by UBA build #2;
- added a static preflight guard for that namespace-collision pattern;
- added static validation for duplicate/invalid Unity GUIDs;
- added static validation for malformed/duplicate Party Night assembly definitions and missing Party Night assembly references;
- strengthened scene validation so the foundation scene has no serialized script references and no hidden extra/disabled scene ownership;
- added `docs/ENGINEERING_GATES.md` with the researched source → compile → test → editor → Player-build → platform validation chain.

Research-backed rules now documented:
- UBA Edit Mode tests are mandatory for editor/foundation changes;
- pre-export validation is after compilation and cannot substitute for the compile gate;
- exact scene ownership is validated in source and Unity;
- clean/non-stale builds are required after foundational package/editor/render-pipeline/project-setting changes or cache suspicion;
- URP is not considered active merely because its package is installed;
- logical Input Actions are the future input ownership boundary;
- Web clients remain client-only and require browser-compatible transport;
- platform validation is independent; a Linux pass never proves Android/Web/iOS.

## Validation

Repository preflight completed on hardened pre-bookkeeping head:

`45ab3a4f52a6a89c4e2f33206748200610794e9c`

GitHub static workflow run #23: **PASS**

That run validated:
- exact Unity/package pins;
- asset/folder metadata pairings;
- valid and unique Unity GUIDs;
- Party Night assembly-definition JSON, names, and internal references;
- the exact foundation scene path/GUID;
- exactly one scene entry and one enabled scene;
- no serialized MonoBehaviour scripts in the minimal foundation scene;
- rejection of the namespace-import pattern that caused UBA build #2;
- no Unity-generated directories;
- no stale/backup/temp artifacts;
- no conflict markers.

The source gate currently emits a deliberate warning that `Packages/packages-lock.json` has not yet been captured into source control. Unity's documentation states that the lock file preserves deterministic dependency resolution and should be kept in source control. It will be captured from Party Night's pinned editor rather than fabricated manually.

Still required before merge:
- this bookkeeping head passes the same GitHub static gate;
- Unity imports the scene on the exact final head;
- Party Night scripts compile;
- Edit Mode regression tests pass;
- pre-export validator passes;
- Linux Player export succeeds or exposes a new, separately root-caused platform/export failure;
- final diff and stale-artifact review pass.

## Cleanup rule

No superseded, obsolete, duplicate, temporary, debug, backup, or abandoned implementation may remain when this task closes.

In particular, this task will not add:
- scene generator scripts that become unnecessary after export;
- backup copies of scene/build settings;
- duplicate scene ownership;
- old disabled foundation scenes.

## Git state

Repository:
`UglyGameFace/The-420-Lobby-Party-Night`

Base/default branch:
`main`

Verified base head:
`f0677c0ec3012006f629aa28d7b5e82d0617837c`

Working branch:
`foundation/first-buildable-scene`

PR:
#3, draft.

Last cloud-tested head:
`158718866c95fd2c95b5d74154c5172e7ebdab39`

Build #2 failure fix commit:
`98a5c44648138ad27258abebd539abad248c94c7`

Current branch contains additional static hardening and engineering-gate documentation after that fix; exact current head must pass GitHub static CI before another UBA run.

## Known foundation gaps discovered by research

These are not being silently implemented inside this scene task, but they are now hard prerequisites before gameplay expands:
- capture and commit the authoritative `Packages/packages-lock.json` generated by the pinned Unity editor;
- capture and commit Party Night's authoritative `ProjectSettings/EditorSettings.asset` with version-control-safe serialization;
- create Party Night's real URP Asset + Universal Renderer through Unity and assign it through Graphics/Quality settings;
- capture the resulting `ProjectSettings/GraphicsSettings.asset` and `QualitySettings.asset`;
- capture authoritative `ProjectSettings/ProjectSettings.asset` before platform/player settings begin;
- create the abstract Input Action asset before player movement.

Do not copy these settings from another game and do not guess opaque serialized assets merely to satisfy CI.

## Next step

Validate this task-record bookkeeping head with GitHub static CI. If it passes and the final PR diff remains clean, Unity Build Automation may be run once more on `foundation/first-buildable-scene` using the existing canonical target.
