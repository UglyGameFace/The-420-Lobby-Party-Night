# ACTIVE TASK

## Active task

**Authoritative multiplayer foundation**

Single active implementation task for Party Night.

Repository:
`UglyGameFace/The-420-Lobby-Party-Night`

Base:
`main`

Base head:
`36388a8f83e5411bb3948794477d29ab6d1c680c`

Working branch:
`multiplayer/authoritative-foundation`

State:
**IMPLEMENTED — STATIC PASS; UNITY REVALIDATION PENDING AFTER BUILD #18 NGO ROOT FIX**

## Prior validated checkpoint

The local Hotbox Havoc engineering prototype is complete and merged.

Validated Hotbox head:
`d36d71bba58d63fed5b9c3ab64377452511c2168`

Hotbox squash merge:
`2855362b73fc96e312dcfecca667800a58f48628`

Hotbox closeout:
`36388a8f83e5411bb3948794477d29ab6d1c680c`

Build #16 passed Edit Mode, all ten Play Mode tests, exact-revision visual
validation, Linux Player export, artifact inspection, post-merge static #82,
and closeout static #83.

Do not redesign or replace the completed local gameplay foundation.

## Outcome

Establish Party Night's first canonical client-server networking foundation on the
already-pinned Netcode for GameObjects and Unity Transport packages.

This task must prove:
- exactly one Party Night network session bootstrap is composed in the foundation scene;
- the bootstrap owns one `NetworkManager` and one `UnityTransport`;
- Party Night exposes explicit dedicated-server and client start paths;
- Party Night does not expose or use a host-authoritative gameplay path;
- a dedicated server session can start and stop successfully in Play Mode;
- a normal client build does not silently become authoritative;
- dedicated server builds have an explicit server-only autostart path;
- the networking assembly remains independent from Discord and physical device input;
- existing CharacterController movement remains the one movement implementation.

## Architecture

Add a dedicated `PartyNight.Networking` runtime assembly.

Dependency direction:
- `PartyNight.Networking` may depend on NGO and Unity Transport;
- `PartyNight.Gameplay` may reference `PartyNight.Networking` only to compose/use the
  shared network bootstrap;
- `PartyNight.Networking` must not depend on gameplay, input, Discord, persistent
  services, or platform-specific UI.

The bootstrap owns session lifecycle only. It does not own Hotbox gameplay rules.

Production topology:
- dedicated server = authoritative;
- clients = non-authoritative;
- browser/mobile/desktop clients never become authoritative hosts.

## Scope

Implement:
- `PartyNightNetworkMode` with explicit None / DedicatedServer / Client roles;
- `PartyNightNetworkBootstrap` with instance-owned NGO/Transport references;
- deterministic client/server endpoint configuration;
- `StartDedicatedServer`, `StartClient`, and `Shutdown` lifecycle;
- `UNITY_SERVER` dedicated-server autostart support;
- foundation scene composition of exactly one bootstrap;
- Play Mode coverage for composition, non-authoritative default state, forbidden host
  surface, and real server start/stop;
- static validation for assembly boundaries and topology rules;
- architecture documentation for the new ownership boundary.

## Explicitly out of scope

Do not implement yet:
- network player prefab/spawning;
- movement replication/prediction/reconciliation;
- networked knockback;
- server-owned Hotbox round replication;
- lobby/matchmaking;
- reconnect handling;
- WSS/WebGL transport validation;
- persistent accounts/data;
- Discord integration;
- production hosting/provider selection;
- final environment art.

Those are subsequent tasks after this network session foundation is validated.

## Validation plan

Before merge:
1. static CI passes on exact head;
2. Unity compiles exact head in 6000.3.24f1;
3. Edit Mode passes;
4. existing local Hotbox/character Play Mode tests remain green;
5. networking Play Mode tests prove one canonical bootstrap;
6. networking Play Mode tests prove default client scene is not authoritative;
7. networking Play Mode test starts a server-only NGO session and shuts it down cleanly;
8. no Party Night runtime code calls `StartHost`;
9. Linux Player exports successfully;
10. full diff/cleanup review removes duplicate/temp/obsolete code;
11. exact-head merge protection and post-merge static validation pass;
12. ACTIVE_TASK closes on main.

## Implemented

- added isolated `PartyNight.Networking` runtime assembly;
- added explicit None / DedicatedServer / Client roles;
- added one canonical `PartyNightNetworkBootstrap`;
- bootstrap owns one NGO `NetworkManager` and one `UnityTransport`;
- added dedicated-server and client startup paths;
- added `UNITY_SERVER` server-only autostart;
- intentionally exposed no host-authority startup path;
- composed exactly one bootstrap into the existing foundation scene;
- preserved the existing CharacterController movement implementation;
- added Play Mode coverage for composition, non-authoritative default state and real
  server-only start/shutdown;
- added static topology/dependency guards;
- updated architecture/status documentation.

GitHub static workflow #93:
**PASS**

## Build #18 findings

Unity Build Automation Build #18 correctly checked out:

`multiplayer/authoritative-foundation`

at exact revision:

`f06923de242f502269afc185c2e198e71cf95a9c`

Unity version:
`6000.3.24f1 (4e7b9b5b6244)`

Confirmed:
- Edit Mode completed with exit code 0;
- the new `PartyNight.Networking` assembly compiled and was processed by NGO ILPP;
- NGO and Unity Transport resolved correctly;
- Play Mode exited with code 2 before Party Night test result reporting;
- Linux Player export did not run because the Play Mode gate failed.

Root cause:
`FoundationSceneComposition` created the Party Night network bootstrap under the
`Foundation Runtime` child hierarchy. NGO explicitly rejects a nested
`NetworkManager` and logged:

`Party Night Network Bootstrap is nested under Foundation Scene Composition. NetworkManager cannot be nested.`

That invalid topology then caused `PartyNightNetworkBootstrap.Initialize()` to hit a
null `NetworkConfig` path.

Correction:
- create the network bootstrap as a scene-root GameObject;
- explicitly move that root object into the loaded foundation scene;
- keep gameplay objects under the existing `Foundation Runtime` child root;
- destroy the root-level network object explicitly if composition throws;
- add Play Mode assertions proving the bootstrap has no parent and belongs to the
  foundation scene;
- add static validation rejecting any future parent assignment for the network object.

No Hotbox rules, movement, input, camera, package versions, or authority model changed.

## Next step

Pass GitHub static CI on the exact Build #18 NGO scene-root fix head and review the
complete delta. Freeze that SHA. Then run one new Unity Build Automation validation
build. Edit Mode, all existing Hotbox/movement tests and all networking Play Mode tests
must pass before exact-revision visual validation and Linux Player export can count.
