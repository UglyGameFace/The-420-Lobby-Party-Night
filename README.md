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

Unity Build Automation has opened the project on the pinned editor, resolved the package graph, compiled Party Night scripts, passed Edit Mode and Play Mode tests, passed the pre-export foundation/gameplay validators, imported the real Party Night foundation scene, and successfully exported a Linux Player build.

The first real source-controlled scene lives at `Assets/PartyNight/Scenes/PartyNightFoundation.unity` and is owned by `ProjectSettings/EditorBuildSettings.asset`.

The project now also has:
- one shared cross-platform Input System action layer;
- native generic Gamepad bindings;
- mobile on-screen-control paths that feed the same logical actions;
- a CharacterController-based local movement motor;
- explicit jump/gravity;
- camera-relative movement;
- a third-person orbit camera;
- real Play Mode runtime coverage.

See `docs/UNITY_SETUP.md`, `docs/UNITY_CLOUD_VALIDATION.md`, `docs/DEPENDENCIES.md`, and `docs/INPUT.md`.

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

Repository bootstrap, shared Unity foundation, the first buildable scene, authoritative project settings/URP activation, cross-platform Input Action foundation, local player movement + third-person camera, and the local Hotbox Havoc engineering prototype are complete and post-merge verified.

Hotbox Havoc Build #16 passed Edit Mode, all ten Play Mode tests, exact-revision visual validation, Linux Player export, and artifact inspection.

The authoritative NGO + Unity Transport session foundation and canonical network-player prefab/spawn contract are complete and merged. The current focused implementation task is **server-authoritative network movement + ownership**: owner clients submit direction/jump intent only, while the dedicated server validates ownership, advances the existing CharacterController motor, and publishes server-written pose state. Prediction/reconciliation, owner camera/input wiring, remote player presentation, server-owned Hotbox state, lobby/matchmaking, and WebSocket validation remain subsequent tasks.
