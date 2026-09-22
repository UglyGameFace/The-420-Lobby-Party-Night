# ACTIVE TASK

## Active task

**Local Hotbox Havoc prototype + visual validation**

Single active implementation task for Party Night.

Repository:
`UglyGameFace/The-420-Lobby-Party-Night`

Base:
`main`

Base head:
`39b65c0ae84df6b42cd50732f54168bd381ae954`

Working branch:
`prototype/hotbox-havoc-local-visuals`

State:
**CLOSED — MERGED AND POST-MERGE VALIDATED**

## Outcome

Turn the validated input/movement/camera foundation into the first local playable **Hotbox Havoc** loop and make visual progress a permanent validation artifact instead of relying only on logs.

The local prototype must provide:
- a visible stylized engineering arena;
- one local player beacon/marker without pretending placeholder geometry is final character art;
- countdown -> active round -> win/elimination -> restart lifecycle;
- fictional cartoon haze pressure;
- a shrinking clear-zone gameplay boundary;
- exposure-based elimination while outside the clear zone;
- visible future multiplayer spawn markers;
- a prototype HUD;
- deterministic Play Mode tests for the local loop;
- an automated PNG screenshot rendered from the real Unity scene;
- automatic inclusion of that PNG in Unity Cloud build artifacts.

## Product guardrails

Hotbox Havoc's haze is a fictional party-game hazard.

This prototype does **not** model realistic cannabis consumption, dosage, intoxication, health effects, or techniques.

The visuals should communicate Party Night's stoner-comedy identity through color, haze, lounge styling, fictional effects, and game-state presentation rather than realistic drug-use simulation.

## Visual direction

This task may use a deliberate engineering/prototype presentation layer for the arena.

It must:
- avoid Roblox-like block-character presentation;
- not introduce a fake final character model;
- clearly read as prototype visualization rather than final art;
- use smooth primitives, neon/lounge colors, clear zone/haze visualization, and player/spawn beacons;
- remain inexpensive enough for future mobile/Web adaptation.

The screenshot must come from the **actual Unity scene/runtime**, not generated concept art.

## Round rules for the local prototype

Initial local rule set:
- 3 second countdown;
- 20 second active survival round;
- clear zone shrinks from 8.5 m to 3.0 m during the active round;
- fictional haze level rises with round progress;
- being outside the clear zone accumulates exposure;
- 2.5 seconds of continuous outside exposure eliminates the local player;
- returning inside clears exposure progressively;
- surviving the timer wins;
- falling well below the arena eliminates immediately;
- restart restores spawn position and round state.

These values are prototype tuning, not permanent competitive balance.

## Architecture

New Hotbox Havoc runtime code remains inside the existing `PartyNight.Gameplay` assembly.

Ownership:
- `HotboxHavocRoundController` owns local prototype round state;
- `HotboxHavocPrototype` wires the round to the validated local player;
- `HotboxHavocPrototypeVisuals` owns prototype-only geometry/material presentation;
- `HotboxHavocPrototypeHud` owns prototype HUD presentation;
- `FoundationSceneComposition` remains the scene composition root and creates exactly one Hotbox Havoc prototype.

No Hotbox class may read physical keyboard/controller/touch controls directly.

No networking or Discord dependency is introduced.

## Visual artifact pipeline

Play Mode validation renders a deterministic 1280x720 overview from the actual runtime arena to:

`VisualValidation/HotboxHavoc_Overview.png`

The source capture directory is gitignored.

During Unity Cloud Player export, an editor post-build processor copies the validated screenshot beside the Player output under:

`VisualValidation/HotboxHavoc_Overview.png`

On Unity Cloud builds, missing visual evidence is a build failure rather than silently shipping a build with no visual proof.

## Validation plan

Before merge:
1. GitHub static CI validates the new Hotbox runtime ownership, visual capture pipeline, artifact ignore rule, and no direct device-input bypass;
2. Unity compiles the exact final head;
3. existing Edit Mode tests pass;
4. Play Mode tests pass;
5. Play Mode proves countdown -> active -> win;
6. Play Mode proves outside-zone exposure -> elimination;
7. Play Mode proves restart resets state/position;
8. Play Mode proves exactly one local Hotbox prototype is composed;
9. Play Mode renders a non-empty 1280x720 PNG from the real runtime arena;
10. pre-export validators pass;
11. Linux Player exports successfully;
12. build artifact contains the PNG visual evidence;
13. exact-head cleanup/diff review passes;
14. merge with expected-head protection;
15. verify merged `main` and close this task.

## Out of scope

- final environment art;
- final character model/animation;
- production smoke particles/volumetrics;
- authoritative multiplayer;
- multiple live players;
- knockback combat;
- grabs/items/dash gameplay;
- matchmaking/lobby service;
- final mobile HUD/safe-area treatment;
- controller glyphs/rebinding;
- Discord integration.

## Discord boundary

Discord remains an external integration surface. The Hotbox Havoc local prototype has no Discord runtime dependency.

## Implemented

- local Hotbox Havoc round lifecycle;
- shrinking clear zone and fictional haze progression;
- outside-zone exposure/elimination and automatic result restart;
- visible arena prototype with lounge/neon language;
- 16 future multiplayer spawn markers;
- local-player engineering beacon instead of fake character art;
- prototype HUD;
- real Unity 1280x720 visual capture Play Mode test;
- Unity Cloud visual artifact exporter;
- cloud build fails if the required PNG was not produced;
- permanent editor/static validation extensions.

## Build #10 findings

Unity Build Automation Build #10 checked out:

`c926b4189397438ba66460da53ac8d2e8ba103fe`

Confirmed before failure:
- Unity `6000.3.24f1 (4e7b9b5b6244)`;
- Edit Mode launch occurred;
- Play Mode launch occurred.

Build #10 did **not** reach test execution or Player export because the Play Mode
test assembly failed compilation at the real PNG capture:

`Texture2D.EncodeToPNG()`

Root cause:
- PNG encoding is supplied by Unity's built-in Image Conversion module;
- the project had not declared `com.unity.modules.imageconversion`;
- static validation therefore passed while Unity compilation correctly failed.

Implemented correction:
- declare `com.unity.modules.imageconversion@1.0.0` in `Packages/manifest.json`;
- lock the built-in module in `Packages/packages-lock.json`;
- require the module in the static package contract so the regression cannot recur.

The exact-revision PNG/manifest evidence design remains unchanged.

## Build #11 findings

Unity Build Automation Build #11 checked out exact revision:

`c82221c123bb27f93046d43f07d1e7de649d5f28`

Confirmed:
- Unity `6000.3.24f1 (4e7b9b5b6244)`;
- C# script compilation succeeded;
- Edit Mode test run completed with exit code 0;
- Play Mode launched and executed;
- the real 1280x720 Hotbox Havoc PNG capture was produced;
- the previous `Texture2D.EncodeToPNG()` compile failure is resolved.

Build #11 still failed during Play Mode because every load of the committed
`PartyNightFoundation` scene emitted:

`AudioListener component deleted: Component belongs to a disabled built-in package.`

The scene legitimately serializes an AudioListener, while the project package
contract did not enable Unity's built-in Audio module. Unity Test Framework treats
unexpected error/assert/exception logs as test failures.

Implemented correction:
- declare `com.unity.modules.audio@1.0.0` in `Packages/manifest.json`;
- lock the built-in Audio module in `Packages/packages-lock.json`;
- require the Audio module in static package validation.

Do not suppress the scene-load error with `LogAssert`; the correct fix is to keep
the module required by the committed scene enabled.

The exact-revision PNG/manifest validation remains unchanged.

## Build #12 findings

Unity Build Automation Build #12 checked out exact revision:

`abd3191d56bc9b4513ad2a4232a408b4ce4c2124`

Confirmed:
- Unity `6000.3.24f1 (4e7b9b5b6244)`;
- `com.unity.modules.audio@1.0.0` and `com.unity.modules.imageconversion@1.0.0` resolved;
- the Build #11 AudioListener deletion error is gone;
- Edit Mode completed with exit code 0;
- Play Mode executed and produced the real Hotbox Havoc PNG;
- Play Mode still exited with code 2.

Important correction:
the Build #11 Audio module issue was a real project defect, but Build #12 proves it was
not the underlying Play Mode assertion/test failure. The Unity Cloud console log does
not print the failing NUnit test name or assertion message; it only writes the detailed
result to its internal TestResults.xml and reports the aggregate failure.

Hardening implemented after Build #12:
- explicitly declare directly used built-in modules for Audio, Image Conversion,
  Physics and IMGUI;
- lock all directly owned modules at depth 0;
- static validation derives module requirements from actual scene/runtime/test usage;
- both Play Mode fixtures now emit durable `PARTY_NIGHT_TEST_RESULT` outcome lines,
  including the NUnit test name, status, label and message.

Do not guess at the hidden failing assertion. The next Unity run must expose the exact
test result if anything still fails.

## Build #13 findings

Unity Build Automation Build #13 checked out exact revision:

`13c71e4db43b288e33fb171058a7667a123fd989`

Confirmed by the durable Play Mode result diagnostics:
- all five Hotbox Havoc Play Mode tests passed;
- camera semantics passed;
- foundation local-player composition passed;
- movement/collision test passed;
- the sole failing test was
  `LocalPlayerRuntimeTests.JumpUsesExplicitGravityAndReturnsToGround`;
- NUnit reported `Expected: True / But was: False`;
- Edit Mode passed;
- the real Hotbox Havoc visual PNG capture passed.

Initial hypothesis after Build #13:
the jump test's synchronous `[Test]` lifecycle appeared to be the source of the
grounding failure. Build #14 disproved that as the complete root cause.

Build #13 correction:
- convert the jump test to `[UnityTest]`;
- advance one motor step per Unity frame while settling and while completing the jump;
- add distinct pre-jump and post-landing assertion messages;
- include NUnit stack traces in durable Play Mode result diagnostics;
- add static regression validation requiring this grounding test to remain frame-accurate.

No gameplay movement/camera/Hotbox runtime logic was changed for this correction.

## Build #14 findings

Unity Build Automation Build #14 checked out exact revision:

`280d1b361dbdca0ea3bb4a46ac3ceaf1fd7d31f5`

Confirmed:
- Unity `6000.3.24f1 (4e7b9b5b6244)`;
- Edit Mode passed;
- all five Hotbox Havoc Play Mode tests passed;
- camera semantics passed;
- foundation local-player composition passed;
- movement/collision passed;
- the real Hotbox Havoc PNG capture passed;
- the sole failing test remained
  `LocalPlayerRuntimeTests.JumpUsesExplicitGravityAndReturnsToGround`.

The improved diagnostics localized the failure to the pre-jump assertion at
`LocalPlayerRuntimeTests.cs:151` after eight real Unity frames:
`Player must settle onto the foundation ground before jumping. Expected: True. But was: False.`

This disproved the Build #13 hypothesis that synchronous test execution was the complete
cause. It led to an interim motor-grounding hypothesis that Build #15 later disproved
after richer position/velocity telemetry exposed the real initialization displacement.

Build #14 interim correction:
- persist the most recent `CharacterController.Move` collision flags in the motor;
- define the motor grounded contract from `CollisionFlags.Below` with Unity native
  `isGrounded` as a fallback;
- use that motor-owned grounded contract for jump gating;
- clear persisted collision flags when motion is reset;
- statically guard the motor-owned grounding contract;
- preserve player Y, vertical velocity and native grounded state in future grounding
  assertion failures.

No Hotbox round, visual, camera or input logic changed.

## Build #15 findings

Unity Build Automation Build #15 checked out exact revision:

`f2735f7dffa651ad32aa99c2e57c362764b7a1b6`

Confirmed:
- Unity `6000.3.24f1 (4e7b9b5b6244)`;
- Edit Mode passed;
- all five Hotbox Havoc Play Mode tests passed;
- camera semantics passed;
- foundation local-player composition passed;
- movement/collision passed;
- the real Hotbox Havoc PNG capture passed;
- the sole failure remained
  `LocalPlayerRuntimeTests.JumpUsesExplicitGravityAndReturnsToGround`.

The added telemetry finally exposed the physical state at the failing pre-jump assertion:
- player Y: `7.9034`;
- motor vertical velocity: `-6.0481`;
- native CharacterController grounded: `false`.

The foundation composes the player at Y `0.05`, so the test was not observing a player
that failed to recognize the floor. The player had already been displaced upward by
almost eight meters during initialization.

Root cause identified in the prototype visual primitive lifecycle:
`HotboxHavocPrototypeVisuals.CreatePrimitive` creates Unity primitives with colliders.
For visual-only primitives it called `Destroy(collider)`, but normal Unity destruction is
deferred until end-of-frame. The large clear-zone cylinder and other visual-only
colliders therefore remained active during the scene creation frame and could
participate in CharacterController overlap recovery before their deferred removal.

Build #15 correction:
- immediately set every visual-only primitive collider `enabled = false` before
  scheduling `Destroy(collider)`;
- keep the collider only for intentional arena walls;
- revert the speculative Build #14 motor-owned grounding workaround so movement code
  returns to the previously validated native CharacterController grounding contract;
- add a Play Mode regression test proving visual-only prototype primitives cannot move
  the player away from the intended spawn during initialization;
- add a static guard requiring immediate collider disable before deferred destruction;
- retain the frame-accurate jump test and detailed cloud diagnostics.

No input, camera, round-state, networking or package behavior changed in this correction.

## Build #16 final validation

Unity Build Automation Build #16 checked out exact validated revision:

`d36d71bba58d63fed5b9c3ab64377452511c2168`

Confirmed:
- Unity `6000.3.24f1 (4e7b9b5b6244)`;
- Edit Mode test run exited 0;
- all ten Play Mode tests passed;
- Hotbox lifecycle, exposure/elimination, restart and composition passed;
- jump/gravity/return-to-ground passed;
- visual-only startup displacement regression passed;
- real 1280x720 Unity PNG capture succeeded;
- exact-revision visual evidence validation passed before and after Player export;
- Linux Player build completed successfully;
- overall Unity Build Automation result was SUCCESS;
- downloaded artifact contained the Linux player, PNG and JSON manifest;
- manifest revision matched the exact validated SHA;
- manifest Unity version was `6000.3.24f1`;
- manifest dimensions were 1280x720;
- the PNG was manually inspected.

Visual inspection:
the engineering-prototype arena correctly showed the clear zone, future multiplayer
spawn markers, fictional haze visualization, neon/lounge accents and local-player
engineering beacon with no missing-texture checkerboards. The primitive presentation is
accepted for this engineering milestone and is explicitly not final art.

## Merge and closeout

PR #7 was marked ready only after Build #16 and artifact inspection.

Validated PR head:
`d36d71bba58d63fed5b9c3ab64377452511c2168`

Squash merge commit on `main`:
`2855362b73fc96e312dcfecca667800a58f48628`

The squash merge used expected-head protection and its 30-file content delta matched
the validated PR delta.

Post-merge GitHub static workflow #82:
**PASS**

This active task is complete. The implementation is merged to `main` and the task lock
may be released only after this closeout commit itself passes static validation.

## Next step

None for this closed task.

After the closeout commit passes static validation, create a new ACTIVE_TASK entry for
the next Party Night milestone. Do not reopen or redesign this completed Hotbox
engineering prototype unless a verified regression requires it.
