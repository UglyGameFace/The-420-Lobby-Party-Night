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
