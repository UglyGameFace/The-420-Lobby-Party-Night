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
**INVESTIGATION / IMPLEMENTATION**

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

## Next step

Implement the local round state, prototype visualization/HUD, deterministic screenshot capture/export path, static validation, and Play Mode tests on this branch.
