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
- source-control/PR validation.

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

**VALIDATION**

Bootstrap implementation is present on `bootstrap/project-foundation` and PR #1 is open.

No Unity gameplay/runtime code exists yet. No Unity build has been claimed or validated.

## Findings / root cause

This is a new project, not a bug investigation.

Repository inspection on 2026-09-21 confirmed:
- repository: `UglyGameFace/The-420-Lobby-Party-Night`;
- repository was empty when bootstrap began;
- no pre-existing branches, files, Unity project, packages, tests, scenes, prefabs, or build settings existed;
- GitHub reports the repository as public;
- no unrelated user work existed inside this repository.

Technology research confirmed:
- Unity 6.3 is the current LTS family and is supported through December 2027;
- Unity supports dedicated-server build profiles;
- Unity Web cannot rely on ordinary direct IP socket access and requires browser-compatible networking such as WebSockets/WebRTC or supported Unity web networking;
- browser networking is therefore a first-order constraint before production transport is locked.

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
- Web transport compatibility must be proven before networking adoption is final.
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

## Changes

- Seeded `main` with the isolated Party Night README.
- Created `bootstrap/project-foundation`.
- Added Unity repository hygiene.
- Added this active-task record.
- Added architecture and platform documentation.
- Added a validation-focused pull-request template.
- Opened draft PR #1 for the bootstrap diff.

## Validation

Completed before this bookkeeping update:
- GitHub repository identity verified.
- Empty initial state verified before edits.
- Bootstrap branch created from the initial `main` seed.
- Current Unity LTS/support information checked against Unity documentation.
- Dedicated-server capability checked against Unity documentation.
- Web networking constraints checked against Unity documentation.
- Branch diff inspected against `main`.
- Diff contained only the five expected added text files.
- Branch was 5 commits ahead and 0 behind `main`.
- No generated Unity folders, binary assets, secrets, or unrelated project files appeared in that diff.
- PR #1 opened against `main`.
- Pre-bookkeeping PR head verified as `8a7a0d47e6771c0bfc7f22c2b3aa3394a64e9aa5`.

Required after this bookkeeping update:
- verify the new exact PR head;
- inspect PR patch/file list on that exact head;
- inspect available CI/status checks;
- mark PR ready only if validation remains clean;
- merge with expected-head protection;
- verify merged `main`.

Not applicable yet:
- Unity compilation;
- Edit Mode/Play Mode tests;
- desktop/mobile/Web builds;
- multiplayer runtime tests.

Those checks require an actual Unity project and belong to the next active task.

## Cleanup

No pre-existing Party Night code existed.

No temporary/debug code has been added.

No Unity-generated folders are tracked.

## Conflicts

No duplicate runtime ownership exists because runtime systems have not been implemented.

Architecture reserves authoritative match ownership for the dedicated game server.

## Blockers / risks

- Exact Unity 6000.3 patch remains intentionally unpinned until the real Unity project is created and package resolution can be validated together.
- Web networking is a hard compatibility constraint; transport adoption remains provisional until a Web client can connect to the dedicated-server path.
- The repository is currently public. No secrets may be committed regardless of future visibility changes.
- Unity Editor/runtime validation cannot occur until project files exist.

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

Last exact pre-bookkeeping head: `8a7a0d47e6771c0bfc7f22c2b3aa3394a64e9aa5`

Current exact head: verified from PR metadata after this file update. It cannot be embedded in the commit that defines that same SHA without creating a self-referential hash.

PR: #1, draft during exact-head validation.

Merge status: not merged.

## Next step

Validate PR #1 on its new exact head, then mark ready and merge only if the final patch/status checks remain clean.
