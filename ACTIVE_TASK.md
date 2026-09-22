# ACTIVE TASK

## Active task

**Local player movement + camera foundation**

Single active implementation task for Party Night.

Repository:
`UglyGameFace/The-420-Lobby-Party-Night`

Base:
`main`

Base head:
`7e646167c01f5d2ec9f9414091e208d0fbc4472a`

Working branch:
`foundation/local-player-movement-camera`

State:
**INVESTIGATION / IMPLEMENTATION**

## Outcome

Create the first real runtime local-player foundation without breaking the future authoritative multiplayer architecture.

The task must:
- consume `PartyNightInputFrame`, not physical device keys;
- add CharacterController-based local movement;
- add jump/gravity and camera-relative movement;
- add a third-person orbit camera;
- correctly distinguish mouse/pointer delta look from stick/touch look-rate semantics;
- wire the authoritative Input Action Asset as Unity Input System's project-wide actions so runtime code can use it without Resources duplication or hardcoded asset loading;
- give the foundation scene one explicit composition root;
- add real Play Mode runtime tests;
- keep Discord completely outside the runtime dependency graph.

## Constraints

- Unity remains pinned to `6000.3.24f1`.
- Input System remains pinned to `1.20.0`.
- No Cinemachine package is added just to solve this small foundation.
- No second input framework.
- No Rigidbody player-controller rewrite in parallel.
- No network authority is claimed in this task.
- Local movement is a client-side foundation/prediction surface only; future authoritative server validation remains mandatory.
- No Hotbox Havoc mechanics, scoring, grabs/items, networking, matchmaking, Discord integration, or final character art.
- Closed consoles remain unsupported.
- No placeholder character model is introduced. The engineering scene may use an invisible collision body until the actual character-art task.
- No temporary generator, backup, duplicate controller, compatibility shim, or abandoned code may remain when the task closes.

## Research decisions

### Character movement

Use Unity's built-in `CharacterController` for this first movement foundation.

Reason:
- `CharacterController.Move` provides collision-constrained displacement and collision flags;
- it does not apply gravity itself, so gravity/jump behavior remains explicit and testable;
- it avoids committing Party Night's player avatar to uncontrolled Rigidbody authority before the network movement model exists.

The motor must use one canonical movement implementation. A future networking task may drive/validate the same movement contract rather than creating a second controller.

### Camera update order

The third-person camera updates in `LateUpdate` after movement. This follows Unity's documented use of LateUpdate for follow cameras.

### Look input semantics

The current `Look` action intentionally combines mouse delta and generic Gamepad right stick.

Those values are not the same kind of quantity:
- pointer delta is accumulated per-update motion in pixels;
- a gamepad/on-screen stick is a persistent normalized rate-like value.

The input frame therefore needs an explicit look semantic:
- `Delta` for pointer/mouse;
- `Rate` for physical or virtual Gamepad sticks.

The camera converts them separately so mouse look is not accidentally frame-rate dependent and controller/touch look is not FPS-dependent.

### Project-wide Input Actions

Unity Input System 1.20 recommends assigning a single Action Asset as project-wide when a project has one shared asset.

Party Night will assign its existing `PartyNightInputActions.inputactions` through `EditorBuildSettings` as the project-wide actions. This:
- preloads the Action Asset in Player builds;
- exposes it through `InputSystem.actions`;
- avoids moving the asset into Resources;
- avoids a duplicate serialized reference or generated wrapper.

### Scene composition

The existing `PartyNightFoundation` scene gains one explicit `FoundationSceneComposition` MonoBehaviour.

It owns only foundation-scene composition:
- invisible floor collider;
- local CharacterController player root;
- local input/controller hookup;
- orbit camera hookup.

It is not a network spawner and must not become one.

## Validation plan

Before merge:
1. static CI validates the new gameplay assembly, project-wide Input Action assignment, expected scene composition script, and absence of duplicate movement implementations;
2. Unity imports/compiles all runtime, editor, Edit Mode and Play Mode assemblies;
3. existing Edit Mode tests still pass;
4. Play Mode tests load the real foundation scene;
5. Play Mode tests prove the runtime composition root creates one player rig;
6. Play Mode tests prove movement is camera-relative and collision-constrained;
7. Play Mode tests prove jump/gravity behavior;
8. Play Mode tests prove mouse delta and Gamepad/touch rate look use different timing semantics;
9. pre-export validation inspects the committed runtime scene/input wiring;
10. Linux Player export succeeds on the exact final PR head;
11. cleanup/diff review passes;
12. only then may the PR merge;
13. merged `main` is verified and the task is closed.

## Unity Build Automation requirement

This task is the first runtime GameObject/player-controller milestone.

The existing build target must keep:
- Edit Mode tests ON;
- fail build when tests fail ON.

It must now also enable:
- **Play Mode tests ON**.

Do not run the Unity build until the branch implementation is complete and GitHub static CI is green.

## Out of scope

- final player model/animations;
- camera collision/occlusion system;
- aim/lock-on;
- dash implementation;
- grab/interact/use-item gameplay;
- touch HUD visuals;
- safe-area UI;
- rebinding UI;
- multiplayer transport or authority;
- server reconciliation/prediction;
- Hotbox Havoc rules;
- Discord integration.

## Discord boundary

Discord remains an external service/integration surface. No movement, input, camera, scene lifecycle, or active match runtime may depend on Discord availability.

## Next step

Implement the single gameplay assembly, project-wide input wiring, scene composition root, runtime movement/camera components, validators, and Play Mode tests on this branch.
