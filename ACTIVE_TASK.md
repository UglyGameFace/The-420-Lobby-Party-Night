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

**IMPLEMENTATION**

Working branch:
`foundation/first-buildable-scene`

## Root cause

Unity Build Automation build #1 validated the previous foundation successfully through package resolution, script compilation, Edit Mode tests, and the Party Night pre-export validator.

The Player export then failed because no build scene existed:

`ERROR: There were no scenes configured to build!`

This task fixes that exact blocker and nothing unrelated.

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

## Validation plan

Required before merge:
- static validator confirms scene and metadata exist;
- static validator confirms `EditorBuildSettings.asset` enables exactly the intended foundation scene;
- Unity imports the scene;
- Party Night scripts compile;
- Edit Mode regression test confirms the scene asset exists and is enabled;
- pre-export validator confirms the scene is configured;
- Linux Player export proceeds beyond the previous "no scenes configured" failure;
- exact-head GitHub static workflow passes;
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

## Next step

Wait for the exact-head GitHub static check to pass, then point the existing Unity Build Automation target at `foundation/first-buildable-scene` and run the cloud build.
