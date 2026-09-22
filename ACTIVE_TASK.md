# ACTIVE TASK

## Active task / outcome

Create the first real Unity project foundation for **The 420 Lobby: Party Night** without starting gameplay implementation.

Outcome:
- pin the exact Unity editor patch;
- pin the initial required Unity packages;
- establish a minimal source-controlled Unity project layout;
- establish Party Night runtime/test assembly boundaries;
- add static repository validation and a Unity batch-mode validation entrypoint;
- obtain all validation available without pretending an unavailable Unity Editor run succeeded.

## Scope

Included:
- Party Night repository only;
- Unity project metadata;
- `Packages/manifest.json`;
- exact editor/package pins;
- minimal Party Night runtime assembly;
- minimal Edit Mode smoke test;
- editor-side foundation validator;
- GitHub static validation workflow;
- dependency/license documentation;
- exact-head PR validation.

Excluded:
- player movement;
- Hotbox Havoc mechanics;
- scenes/prefabs/art/audio;
- character model imports or purchases;
- input action maps and touch controls;
- network gameplay implementation;
- Discord integration;
- persistent backend/hosting;
- modifications to any other repository.

## Status

**VALIDATION**

Implementation is present on `foundation/unity-project`. Unity Editor execution remains unvalidated until an editor/license-capable runner is available.

## Findings / root cause

The previous bootstrap task was post-merge verified and left no Unity project files.

Current upstream research on 2026-09-21 established:
- Unity `6000.3.24f1` is the latest verified 6.3 LTS patch found before implementation, released 2026-09-10;
- its changeset is `4e7b9b5b6244`;
- Unity 6.3 uses URP `17.3.0`;
- Input System `1.20.0` is in the Unity 6.3 patch stream;
- Netcode for GameObjects `2.13.2` is updated in Unity `6000.3.24f1`;
- Unity Transport `2.7.4` is in the Unity 6.3 patch stream;
- the selected official Unity packages are intended for Unity-dependent projects and use Unity package licensing/third-party notices that must be retained and reviewed at release.

Unity project settings such as `ProjectSettings.asset`, URP renderer assets, and Input Action assets are intentionally not fabricated by hand. Unity can regenerate missing project settings on editor open; generated serialized settings must then be reviewed and source-controlled from the pinned editor.

## Execution path

For this task:

```text
ProjectSettings/ProjectVersion.txt
  -> Unity Editor version selection
Packages/manifest.json
  -> Unity Package Manager resolution
Assets/PartyNight runtime assembly
  -> Unity script compilation
Assets/PartyNight Edit Mode tests
  -> Unity Test Framework
Assets/PartyNight Editor validator
  -> batch-mode package/editor verification
GitHub static workflow
  -> repository structure/version checks without Unity
```

Gameplay execution does not exist yet.

## Architecture

Pinned foundation:
- Unity Editor: `6000.3.24f1`
- editor changeset: `4e7b9b5b6244`
- URP: `17.3.0`
- Input System: `1.20.0`
- Netcode for GameObjects: `2.13.2`
- Unity Transport: `2.7.4`
- Unity Test Framework: `1.6.0`

Package presence does not mean networking or input gameplay has been implemented.

URP renderer assets, actual Input Actions, scenes, prefabs, build profiles, and multiplayer objects remain excluded until created through the pinned Unity Editor.

## Changes

Implemented on the task branch:
- pinned `ProjectSettings/ProjectVersion.txt`;
- pinned `Packages/manifest.json`;
- created `PartyNight.Foundation` runtime assembly;
- created immutable project identity constants for the product name and 12–16 player initial match range;
- created an Edit Mode smoke test assembly/test;
- created an Editor-only foundation validator for exact editor/package resolution;
- tracked Unity `.meta` files for all new `Assets/` content and folders;
- added `scripts/validate_unity_foundation.py`;
- added GitHub static validation workflow;
- added dependency/license evidence documentation;
- added Unity setup/validation documentation;
- updated README foundation status.

No gameplay, scene, prefab, art, model, external service, or other-project code was added.

## Validation

Completed:
- branch compared against `main`: 0 behind;
- changed-file list inspected and limited to the expected Unity-foundation/docs/CI files;
- exact `ProjectVersion.txt` content fetched from the branch and verified;
- `Packages/manifest.json` fetched, parsed, and exact pins verified;
- all three assembly definition JSON files fetched and structurally inspected;
- Python validator fetched and syntax-compiled successfully;
- branch file list checked for required Unity `.meta` pairings;
- no `Library/`, `Temp/`, `Obj/`, `Logs/`, `UserSettings/`, `Build/`, or `Builds/` content appears in the branch diff;
- no scene/prefab/binary asset was introduced;
- Unity package/editor pins checked against current upstream evidence.

Completed after PR creation:
- draft PR #2 opened against `main`;
- first static workflow run failed because the conflict-marker scanner matched its own literal marker strings;
- root cause was corrected by constructing the marker strings without embedding literal conflict markers in the validator source;
- static workflow run #2 passed on exact head `1070af75de1003e7ce53c5b3f10182040802a8c0`;
- PR remained mergeable after the validator fix.

Still required:
- inspect final PR head/diff/status after this task-record update;
- run Unity 6000.3.24f1 package resolution, script compilation, editor validator, and Edit Mode test on an editor/license-capable runner;
- review generated `packages-lock.json` and serialized ProjectSettings before merging.

A successful static check is not a Unity compile.

## Cleanup

- no temporary/debug scripts;
- no generated Unity directories;
- no fake `packages-lock.json`;
- no hand-authored scene/prefab/URP asset;
- no unrelated repository changes.

## Conflicts

No competing Unity project, runtime assembly, input system owner, network manager, scene architecture, or gameplay implementation exists.

## Blockers / risks

Current hard validation blocker:
- this execution environment does not provide a reachable installed/licensed Unity Editor, so authoritative Unity package resolution/compilation/test execution cannot be performed here.

Additional risks:
- the first real editor open will generate additional ProjectSettings and `packages-lock.json`; those files must be inspected before merge;
- Web transport behavior is not validated by package installation;
- repository visibility remains public.

## Backlog

Not active:
- evaluate/import a polished modular character base, including the ithappy packs discussed with the user; explicitly avoid blocky/Roblox-like character art;
- create URP renderer/pipeline assets in Unity;
- activate/configure Input System and create abstract action maps;
- local movement/controller;
- Hotbox Havoc local prototype;
- authoritative multiplayer;
- lobby/round lifecycle;
- Web/mobile validation;
- performance profiling;
- persistent services;
- Discord integration;
- additional minigames.

## Git state

Repository: `UglyGameFace/The-420-Lobby-Party-Night`

Base/default branch: `main`

Base head: `8af5aad9ea96168bf790cc7d456cc77a559e21de`

Working branch: `foundation/unity-project`

Last implementation head before this task-record update: `89e7e2015c94a013a17329833567670b80379059`

PR: #2, draft.

Merge status: not merged; intentionally blocked on real Unity Editor validation.

## Next step

Validate the new exact PR head after this bookkeeping update, then keep PR #2 unmerged until the pinned Unity Editor has generated/resolved the remaining authoritative project files and passed compilation/tests.
