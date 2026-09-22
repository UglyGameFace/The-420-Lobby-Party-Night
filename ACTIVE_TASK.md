# ACTIVE TASK

## Active task / outcome

Bootstrap **The 420 Lobby: Party Night** as an isolated production repository and establish the architecture baseline required before gameplay implementation.

Outcome achieved for this task:
- repository boundaries are explicit;
- Unity/source-control hygiene is established;
- supported/unsupported platform assumptions are documented;
- client/server/service ownership is defined;
- the Unity/networking baseline is documented without claiming unimplemented runtime behavior.

## Scope

Included:
- standalone Party Night repository only;
- repository initialization;
- Unity-oriented .gitignore;
- architecture and platform documentation;
- cross-platform constraints;
- authoritative multiplayer boundaries;
- PR validation and merge.

Excluded:
- Hotbox Havoc gameplay code;
- movement/input implementation;
- Unity scenes/prefabs/assets;
- Discord/Dank Shield integration;
- production hosting/backend;
- additional minigames;
- modifications to any other repository.

## Status

**COMPLETE — POST-MERGE VERIFIED**

This bootstrap task is closed. No Unity gameplay/runtime/build claim is implied by completion of this repository-foundation task.

## Findings / root cause

This was a new-project bootstrap rather than a bug fix.

Initial inspection on 2026-09-21 confirmed:
- repository: `UglyGameFace/The-420-Lobby-Party-Night`;
- repository was empty;
- no pre-existing Party Night code, branches, Unity project, tests, scenes, packages, or build configuration existed;
- GitHub reports the repository as public.

Technology research confirmed:
- Unity 6.3 is the current LTS family and is supported through December 2027;
- Unity supports dedicated-server build profiles;
- Unity Web cannot rely on unrestricted direct IP sockets, so browser-compatible networking is a first-order architectural constraint.

## Execution path

No runtime execution path exists yet.

The planned ownership boundary is:

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

Baseline decisions:
- Unity 6.3 LTS engine family;
- URP rendering direction;
- Unity Input System with abstract actions;
- authoritative dedicated-server multiplayer;
- Netcode for GameObjects + Unity Transport as the initial networking candidate;
- browser transport compatibility must be proven before final network-stack lock;
- separate ownership for client, game server, persistence, and Discord integration;
- 12–16 player initial Hotbox Havoc target;
- closed consoles explicitly out of scope.

Deferred to later validated tasks:
- exact Unity 6000.3 patch;
- exact package manifest;
- final transport configuration;
- hosting provider;
- persistence technology;
- account/authentication implementation.

## Changes

Merged repository foundation:
- `README.md` seed on `main`;
- `.gitignore`;
- `ACTIVE_TASK.md`;
- `docs/ARCHITECTURE.md`;
- `docs/PLATFORM_SUPPORT.md`;
- `.github/PULL_REQUEST_TEMPLATE.md`.

No gameplay, external integration, binary asset, generated Unity folder, or other-project code was introduced.

## Validation

Completed:
- repository identity and initial empty state verified;
- focused branch `bootstrap/project-foundation` created from the seed `main`;
- Unity LTS/support, dedicated-server capability, and Web networking constraints checked against current Unity documentation;
- exact PR head validated as `a38ec7f5c2aad49c943ad5894ac89c884df814ac`;
- PR patch inspected on that exact head;
- changed files limited to the five expected bootstrap text files beyond the seed README;
- no binary/generated/secret/conflict/unrelated-project files found in the PR diff;
- PR was mergeable;
- no CI/status checks or workflow runs existed on the validated head because CI has not yet been created;
- PR #1 marked ready only after exact-head inspection;
- merge performed with expected-head protection;
- PR #1 confirmed merged;
- squash merge commit confirmed as `80fe640c1f3d50651d46a27a0674c9a99ba32544`;
- merged `main` compared against the seed and confirmed to contain only the expected five bootstrap additions.

Not applicable to this bootstrap scope:
- Unity compilation;
- Edit Mode/Play Mode tests;
- desktop/mobile/Web builds;
- multiplayer runtime tests.

Those require the actual Unity project, which intentionally does not exist yet.

External note:
- Qodo posted that its review is paused because its subscription is inactive. This did not block the documentation-only bootstrap but means Qodo supplied no automated review evidence.

## Cleanup

- No temporary/debug code exists.
- No Unity-generated directories are tracked.
- No obsolete or duplicate Party Night implementation existed to remove.
- No other repository was modified.

## Conflicts

No duplicate runtime ownership exists yet.

The architecture reserves authoritative match-state ownership for the dedicated game server.

## Blockers / risks

Remaining project risks, not blockers for this completed task:
- exact Unity patch/package versions still require real project resolution;
- browser networking must be proven end-to-end before transport selection is final;
- mobile/Web performance remains unvalidated;
- repository visibility is currently public, so all future changes must remain secret-safe.

## Backlog

Not active:
- create and validate the real Unity project;
- shared Hotbox Havoc gameplay foundation;
- abstract input/player controller;
- camera;
- authoritative networking;
- lobby/ready/round lifecycle;
- spawn/elimination/results;
- smoke/fog mechanic;
- touch controls and safe-area UI;
- Web/mobile builds;
- reconnect handling;
- performance profiling/tiers;
- persistence/accounts/cosmetics/stats;
- Discord integration API;
- additional minigames from the master specification.

## Git state

Repository: `UglyGameFace/The-420-Lobby-Party-Night`

Base/default branch: `main`

Initial seed commit: `2935d4f352650d731262c24a69e84cb7136d88bf`

Task branch: `bootstrap/project-foundation`

Validated PR head: `a38ec7f5c2aad49c943ad5894ac89c884df814ac`

PR: #1

PR status: merged

Squash merge commit: `80fe640c1f3d50651d46a27a0674c9a99ba32544`

Current `main` head after this bookkeeping update is verified externally after commit creation; embedding a commit's own SHA inside itself is self-referential.

## Next step

Create the actual Unity project in a new focused task, pin and validate the Unity/package versions together, establish the URP/Input System foundation, and add the first real compile/build validation without starting Hotbox Havoc gameplay prematurely.
