# ACTIVE TASK

## Active task / outcome

Bootstrap **The 420 Lobby: Party Night** as an isolated production repository and establish the architecture baseline required before gameplay implementation.

Outcome for this task:
- repository boundaries are explicit;
- source-control hygiene exists;
- supported/unsupported platform assumptions are documented;
- client/server/service ownership is defined;
- the Unity/networking baseline is documented without pretending unvalidated runtime behavior exists;
- the repository is ready for the next task to create and validate the actual Unity project.

## Scope

Included:
- standalone Party Night repository only;
- repository initialization;
- Unity-oriented .gitignore;
- architecture documentation;
- platform support documentation;
- cross-platform constraints;
- authoritative multiplayer boundaries;
- active-task tracking;
- source-control validation of this bootstrap branch.

Excluded:
- Hotbox Havoc gameplay code;
- player movement implementation;
- scenes, prefabs, materials, art, audio, or third-party assets;
- Discord/Dank Shield integration;
- matchmaking or persistent backend implementation;
- production hosting;
- additional minigames;
- modifications to any other repository.

## Status

**IMPLEMENTATION / VALIDATION**

Repository bootstrap files are being established on `bootstrap/project-foundation`.

No Unity gameplay or runtime code exists yet. No Unity build has been claimed or validated.

## Findings / root cause

This is a new project, not a bug investigation.

Repository inspection on 2026-09-21 confirmed:
- repository: `UglyGameFace/The-420-Lobby-Party-Night`;
- repository was empty when bootstrap began;
- no pre-existing branches, files, Unity project, packages, tests, scenes, prefabs, or build settings existed;
- the repository is currently reported by GitHub as public;
- no unrelated user work existed to preserve inside this repository.

Technology research confirmed:
- Unity 6.3 is the current LTS family and is supported through December 2027;
- Unity supports dedicated-server build profiles;
- Unity Web cannot rely on ordinary direct IP socket access and requires browser-compatible networking such as WebSockets/WebRTC or supported Unity web networking;
- therefore browser networking must remain a design constraint before a transport is locked into production.

## Execution path

No runtime execution path exists yet.

Planned ownership boundary:

```text
Input device
  -> client input abstraction
  -> client gameplay intent
  -> network transport
  -> authoritative game server
  -> shared simulation / round state
  -> replicated state
  -> client presentation / HUD

Persistent services and Discord integration remain outside the match simulation.
```

## Architecture

Current baseline decisions:
- Engine family: Unity 6.3 LTS.
- Rendering direction: Universal Render Pipeline (URP), mobile/Web-conscious.
- Input direction: Unity Input System with abstract gameplay actions.
- Multiplayer model: authoritative dedicated server.
- Initial high-level networking candidate: Netcode for GameObjects with Unity Transport.
- Web transport compatibility must be proven before networking adoption is considered final.
- Client, game server, persistent services, and Discord integration have separate ownership.
- Initial multiplayer target: 12–16 players per Hotbox Havoc match.
- Closed-console platforms are out of scope.

Not yet locked:
- exact Unity 6000.3 patch;
- exact package manifest/version set;
- final transport configuration;
- hosting provider;
- persistence technology;
- authentication/account-linking implementation.

Those choices require validation in their own active task rather than being smuggled into this one.

## Changes

- Seeded the repository with a project README on `main`.
- Created focused branch `bootstrap/project-foundation`.
- Added Unity repository hygiene.
- Added this active-task record.
- Added architecture and platform documentation.

## Validation

Completed:
- GitHub repository identity verified.
- Empty initial state verified before edits.
- Bootstrap branch created from the initial `main` seed.
- Current Unity LTS/support information checked against Unity documentation.
- Dedicated-server capability checked against Unity documentation.
- Web networking constraints checked against Unity documentation.

Still required before this task can be marked complete:
- inspect final branch file list;
- inspect final branch diff against `main`;
- verify no secret-bearing/generated/binary junk was introduced;
- verify final branch head exactly;
- open a focused PR;
- inspect PR diff/state on its exact head.

Not applicable yet:
- Unity compilation;
- Edit Mode/Play Mode tests;
- desktop/mobile/Web builds;
- multiplayer runtime tests.

There is no Unity project to run, so claiming those checks would be fiction wearing a lab coat.

## Cleanup

No pre-existing Party Night code existed.

No temporary/debug code has been added.

No Unity-generated folders are tracked.

## Conflicts

No duplicate runtime ownership exists because runtime systems have not been implemented.

Architecture explicitly reserves one authoritative owner for match state: the dedicated game server.

## Blockers / risks

- Exact Unity 6000.3 patch remains intentionally unpinned until the real Unity project is created and package resolution can be validated together.
- Web networking is a hard compatibility constraint; transport adoption is provisional until a Web client can connect to the dedicated-server path.
- The repository is currently public. No secrets may ever be committed, regardless of future visibility changes.
- No Unity Editor/runtime validation is possible until the project files exist.

## Backlog

Product backlog only; not active:
- Hotbox Havoc shared gameplay foundation;
- player movement/input;
- camera;
- network identity;
- lobby/ready flow;
- round lifecycle;
- spawning/elimination/results;
- smoke/fog mechanic;
- touch controls and safe-area UI;
- Web/mobile build validation;
- reconnect behavior;
- performance tiers/profiling;
- persistence/accounts/cosmetics/stats;
- Discord integration API;
- additional minigames listed in the master specification.

## Git state

Repository: `UglyGameFace/The-420-Lobby-Party-Night`

Base branch: `main`

Base seed commit: `2935d4f352650d731262c24a69e84cb7136d88bf`

Working branch: `bootstrap/project-foundation`

Head SHA: update after final bootstrap commit.

PR: not yet opened.

Merge status: not applicable.

## Next step

Finish the bootstrap files, inspect the exact branch diff/head, then open and validate the focused repository-foundation PR.
