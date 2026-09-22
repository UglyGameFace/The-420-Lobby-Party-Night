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

**IMPLEMENTATION**

Working branch: `foundation/unity-project`.

## Findings / root cause

The previous bootstrap task was post-merge verified and left no Unity project files.

Current upstream research on 2026-09-21 established:
- Unity `6000.3.24f1` is the latest verified 6.3 LTS patch found before this task began, released 2026-09-10;
- its changeset is `4e7b9b5b6244`;
- Unity 6.3 uses URP `17.3.0`;
- Input System `1.20.0` is in the Unity 6.3 patch stream;
- Netcode for GameObjects `2.13.2` is updated in Unity `6000.3.24f1`;
- Unity Transport `2.7.4` is in the Unity 6.3 patch stream;
- the official Unity packages selected here are Unity-dependent packages under Unity's Companion License family, with package-specific third-party notices where applicable.

## Execution path

For this task the relevant project bootstrap path is:

```text
ProjectVersion.txt
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
- URP: `17.3.0`
- Input System: `1.20.0`
- Netcode for GameObjects: `2.13.2`
- Unity Transport: `2.7.4`
- Unity Test Framework: `1.6.0`

The package presence does not mean networking or input gameplay has been implemented.

URP renderer assets, actual Input Actions, scenes, prefabs, and multiplayer objects are intentionally excluded until they can be created and validated with the Unity Editor rather than hand-authoring serialized Unity assets blindly.

## Changes

Implementation in progress.

## Validation

Planned:
- parse and verify project version;
- parse and verify package manifest exact pins;
- verify required project files and Unity `.meta` files;
- reject tracked Unity generated folders;
- validate static checks in GitHub Actions;
- inspect exact PR diff/head;
- inspect CI/status checks;
- run Unity batch-mode validation and Edit Mode test when a Unity 6000.3.24f1 editor/license-capable runner is available.

Do not report Unity compilation, package resolution, URP activation, or Edit Mode test success until Unity actually runs.

## Cleanup

No temporary/debug code is intended.

## Conflicts

No competing Unity project, runtime assembly, input system, networking manager, scene architecture, or gameplay implementation exists in this repository.

## Blockers / risks

Potential validation blocker:
- this connected execution environment does not currently expose an installed/licensed Unity Editor. If GitHub CI also lacks Unity licensing, editor compile/test validation will remain explicitly blocked rather than fabricated.

Repository visibility remains public.

## Backlog

Not active:
- evaluate/import a polished modular character base, including the ithappy packs discussed with the user; no Roblox/block-character visual direction;
- create URP renderer/pipeline assets in Unity;
- activate/configure the Input System and create abstract action maps;
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

PR: not opened yet.

Merge status: not merged.

## Next step

Create the pinned Unity project files, assembly/test boundaries, validation scripts/workflow, and dependency record; then validate the resulting branch before opening a PR.
