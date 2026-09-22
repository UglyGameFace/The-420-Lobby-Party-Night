# The 420 Lobby: Party Night

A cross-platform online multiplayer cartoon party game for The 420 Lobby community.

## Project boundaries

This repository is the sole source repository for **The 420 Lobby: Party Night**.

It must remain isolated from Dank Shield, ThePlugConnect, ThePlugBridge, Minecraft, Cobbleverse, Outbreak, SniperPlug, and all other projects. External integrations must use explicit service or API boundaries rather than sharing game-runtime ownership.

## Current milestone

The first playable vertical slice is **Hotbox Havoc**, targeting 12–16 players.

Current development priority:

1. Repository and architecture baseline
2. Shared Unity foundation
3. Input and local movement
4. Local Hotbox Havoc prototype
5. Authoritative multiplayer
6. Lobby and round lifecycle
7. Cross-platform controls
8. Web/mobile compatibility
9. Performance validation
10. Results and packaging

Additional minigames remain backlog items until the shared Hotbox Havoc foundation is validated.

## Unity foundation

Validated project pin:

- Unity 6000.3.24f1
- URP 17.3.0
- Input System 1.20.0
- Netcode for GameObjects 2.13.2
- Unity Transport 2.7.4
- Unity Test Framework 1.6.0

Unity Build Automation has opened the project on the pinned editor, resolved the package graph, compiled Party Night scripts, passed the Edit Mode tests, passed the pre-export foundation validator, imported the real Party Night foundation scene, and successfully exported a Linux Player build.

The first real source-controlled scene now lives at `Assets/PartyNight/Scenes/PartyNightFoundation.unity` and is owned by `ProjectSettings/EditorBuildSettings.asset`.

See `docs/UNITY_SETUP.md`, `docs/UNITY_CLOUD_VALIDATION.md`, and `docs/DEPENDENCIES.md`.

## Supported platform families

- Windows
- macOS
- Linux
- Android
- iPhone / iPad
- Web on supported desktop and mobile browsers
- Chromebook through Web where technically viable
- Steam Deck through the Linux/PC build where practical

Closed consoles are explicitly out of scope.

## Engineering rules

- Unity is the game engine.
- Cross-platform design starts at the foundation.
- Important multiplayer state is server-authoritative.
- Dedicated game servers are separate from clients, persistent services, and Discord integration.
- Input is action-based, not keyboard-hardcoded.
- Web and mobile are first-class constraints.
- One implementation task is active at a time.
- `ACTIVE_TASK.md` is the source of truth for current engineering state.
- Never commit secrets or Unity-generated folders such as `Library/`, `Temp/`, `Logs/`, or `Obj/`.
- Unity `.meta` files are source-controlled and must not be casually regenerated.
- Superseded, obsolete, duplicate, temporary, debug, backup, and abandoned code/artifacts must be removed from the affected area before a task can close. Git history is the backup.

## Status

Repository bootstrap, the shared Unity project foundation, and the first real buildable Unity scene are complete and post-merge verified.

No Hotbox Havoc gameplay implementation has been validated yet.

The authoritative Unity project settings, deterministic package lock, URP activation, and cross-platform Input Action foundation are complete. The current focused task is the first local player movement + third-person camera runtime foundation with Play Mode validation.
