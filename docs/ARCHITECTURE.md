# Architecture Baseline

## Purpose

This document defines the initial technical boundaries for **The 420 Lobby: Party Night**. It prevents early convenience decisions from quietly coupling the game to one device class, one player's machine, Discord availability, or another project.

This is an architecture baseline, not proof that unimplemented systems already work.

## System boundaries

### Game client

Responsibilities:
- render the world and UI;
- collect abstract player actions;
- perform client-side presentation and safe prediction where appropriate;
- send player intentions to the authoritative server;
- receive replicated authoritative state;
- expose platform-specific adapters without leaking device-specific input into gameplay logic.

The client does **not** authoritatively own scores, wins, inventory, eliminations, round state, privileged commands, or persistent progression.

### Authoritative game server

Responsibilities:
- player/session state required by the match;
- round lifecycle and timers;
- spawn/elimination state;
- scoring and win conditions;
- validated pickups and item effects;
- authoritative gameplay events;
- important movement/physics validation;
- disconnect/timeout cleanup;
- abuse-resistant request handling.

A player's device is not the permanent authority for the match.

### Persistent services

Future responsibility:
- accounts;
- identity links;
- cosmetics;
- statistics;
- achievements;
- seasonal data;
- durable match results.

Persistent authoritative state must not live only in a client build.

### Discord integration

Discord is an external integration surface, not the game runtime.

Future Party Night integration can consume a stable API/event boundary for lobby creation, join tokens, status, results, moderation, account linking, leaderboards, and announcements.

An active match must not depend on Discord remaining available.

## Unity baseline

Engine family: **Unity 6.3 LTS**.

Rendering direction: **URP** with mobile/Web-safe content rules.

Input direction: **Unity Input System** using action-oriented gameplay APIs.

Candidate multiplayer stack:
- Netcode for GameObjects;
- Unity Transport;
- native dedicated Unity server build.

Package selection remains provisional until the actual Unity project resolves and compiles the chosen versions.

## Input contract

Gameplay systems consume actions, not device keys.

Initial action vocabulary:
- Move
- Look
- Jump
- Interact
- Grab
- Dash
- UseItem
- Emote

Adapters may map those actions from:
- keyboard/mouse;
- controller;
- touch/virtual controls.

## Multiplayer authority

Clients send intent. The server determines authoritative outcomes.

The server must validate as applicable:
- movement plausibility;
- action eligibility;
- cooldowns;
- item ownership;
- collisions/knockback outcomes;
- elimination conditions;
- scores;
- round transition requests.

Network handlers must be designed for duplicate, late, malformed, reordered, or abusive requests rather than assuming a perfect localhost connection.

## Web constraint

Browser builds cannot be designed around unrestricted direct IP sockets.

The networking implementation must preserve a browser-compatible route to the dedicated server. WebSocket/WSS is the initial compatibility direction to validate. No production transport decision is final until a Unity Web client has actually connected through the chosen path.

Browser clients never host authoritative matches.

## Mobile constraint

The foundation must account for:
- touch-first input;
- safe areas;
- varying aspect ratios;
- smaller screens;
- background/interruption behavior;
- thermal limits;
- memory limits;
- Wi-Fi/cellular instability.

Desktop UI is not to be merely shrunk onto a phone.

## Scene and gameplay ownership

The first vertical slice is Hotbox Havoc.

Shared systems should emerge from demonstrated requirements and then be reused. Minigames must not each create competing versions of:
- movement;
- player identity;
- lobby state;
- round lifecycle;
- score state;
- spawning;
- elimination;
- results;
- networking;
- global UI infrastructure.

## Performance principles

The supported baseline assumes ordinary community hardware, phones, and browsers rather than high-end PCs.

Track:
- CPU frame time;
- GPU frame time;
- managed allocations / GC;
- physics cost;
- draw calls;
- particle/transparency cost;
- texture/audio memory;
- network frequency and bandwidth;
- scene/load memory;
- mobile thermals;
- browser memory pressure.

Hotbox Havoc smoke must remain readable and performant; expensive layered transparency is not a license to turn low-end phones into hand warmers.

## Dependency rule

Before adding a package or SDK:
1. verify active support;
2. verify Unity compatibility;
3. verify license;
4. verify Web compatibility;
5. verify mobile compatibility;
6. verify dedicated-server compatibility where relevant;
7. prefer a native/simple solution when it satisfies the requirement.

No abandoned or redundant framework is adopted merely because a tutorial used it.

## Security boundary

Anything shipped in a client is considered recoverable by players.

Never ship:
- private API keys;
- database credentials;
- Discord bot tokens;
- signing secrets;
- unrestricted administrative credentials.

Privileged operations belong behind authenticated server/service boundaries.

## Current non-decisions

These are intentionally deferred:
- final production transport settings;
- cloud/game-server host;
- matchmaking provider;
- account provider;
- database technology;
- Discord integration implementation;
- voice chat;
- monetization;
- analytics provider.

They are not needed to prove the first gameplay foundation and should not become accidental dependencies.


## Local movement and camera foundation

The first local movement implementation uses Unity `CharacterController`.

This is a client runtime foundation, not a declaration that the client becomes authoritative for networked movement.

Responsibilities are separated:
- `PartyNightInputReader` converts Input System actions into a logical `PartyNightInputFrame`;
- `PartyNightLocalPlayerController` converts Move into camera-relative world intent and forwards Jump;
- `PartyNightCharacterMotor` owns one collision-constrained CharacterController movement implementation;
- `PartyNightOrbitCamera` owns local third-person view state and runs its follow update after movement;
- `FoundationSceneComposition` composes the current foundation scene only.

The future authoritative multiplayer task must validate/drive the same movement contract rather than introducing a second unrelated player controller.

The foundation scene intentionally has no placeholder character model. Collision and movement can be validated before final character art exists.

Discord remains outside all of these runtime components.


## Local Hotbox Havoc prototype

The first minigame implementation begins as a local deterministic prototype on top of the validated input/movement/camera stack.

Current prototype state:
- one local player;
- 3 second countdown;
- 20 second active survival window;
- fictional haze progression;
- a clear zone that shrinks from 8.5 m to 3.0 m;
- outside-zone exposure that eliminates after 2.5 continuous seconds;
- recovery when the player returns inside;
- survive-the-timer win state;
- result state followed by automatic restart.

These are tuning constants for proving the loop, not permanent competitive balance.

The prototype does not model realistic drug consumption or intoxication. Haze is a fictional party-game pressure mechanic.

The visible arena is intentionally an engineering/prototype presentation layer. It uses inexpensive geometry, lounge/neon color language, future spawn beacons, and a local-player beacon. It is not final environment or character art.

The local round controller is not authoritative multiplayer state. When networking begins, server authority must own round timers, elimination, win state, spawning, and important movement validation rather than trusting this client-only prototype.


## Authoritative multiplayer foundation

Party Night's first networking layer uses the already-pinned Netcode for GameObjects
and Unity Transport packages.

Ownership is split deliberately:
- `PartyNight.Networking` owns session bootstrap and transport configuration;
- `PartyNight.Gameplay` may compose/use that bootstrap but does not own transport;
- the networking assembly does not depend on gameplay, input, Discord, or persistent services.

The supported runtime roles are:
- `None`: no network session is active;
- `DedicatedServer`: NGO server-only authority;
- `Client`: non-authoritative connected player.

Party Night intentionally exposes no host-authority startup path. A player's game client
must not silently become the authoritative match server.

Dedicated-server builds use the `UNITY_SERVER` path to autostart the server role.
Normal desktop/mobile/Web clients remain non-authoritative unless explicitly started as
clients.

The NGO `NetworkManager` is a root-level persistent session object. Runtime creation
configures `NetworkConfig` and Unity Transport while the object is inactive, then
activates it so NGO can perform its normal singleton/`DontDestroyOnLoad` lifecycle.
Gameplay composition obtains that shared bootstrap but does not parent it or own the
network session lifetime. Re-entering a gameplay scene reuses the single bootstrap
instead of creating a second NetworkManager.

This foundation proves session ownership only. Network player spawning, movement
replication/prediction, server-owned Hotbox round state, lobby/matchmaking, reconnect,
and WebSocket/WSS validation remain later milestones.


## Network player prefab and spawn contract

The canonical Party Night network player is a checked-in NGO prefab at:

`Assets/PartyNight/Networking/Prefabs/PartyNightNetworkPlayer.prefab`

The prefab is intentionally identity-only:
- exactly one root `NetworkObject`;
- exactly one `PartyNightNetworkPlayer`;
- no `CharacterController`;
- no `PartyNightCharacterMotor`;
- no `PartyNightLocalPlayerController`.

The local gameplay rig remains the only movement/input implementation until the
movement-replication milestone deliberately connects local intent to network authority.

`FoundationSceneComposition` owns the serialized prefab reference and supplies it to
the persistent `PartyNightNetworkBootstrap`. The bootstrap validates the prefab and
assigns it to `NetworkConfig.PlayerPrefab` before session startup.

The configuration is idempotent for the same prefab across gameplay scene loads, but a
different player prefab cannot be substituted while the network session is listening.

On NGO startup, `RegisterPlayerPrefab()` registers the configured player prefab in the
runtime network-prefab collection. A server-only session does not create a local player
object by itself; actual player objects are created only for connected clients.

Movement replication, prediction/reconciliation, remote presentation and server-owned
Hotbox state remain subsequent milestones.
