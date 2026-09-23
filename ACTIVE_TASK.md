# ACTIVE TASK

## Active task

**Network player prefab + server-owned spawn contract**

Single active implementation task for Party Night.

Repository:
`UglyGameFace/The-420-Lobby-Party-Night`

Base:
`main`

Base head:
`407c547029cb80ca536de44bfd55b4cccd8acec7`

Working branch:
`multiplayer/network-player-spawning`

State:
**CLOSED — MERGED AND POST-MERGE VALIDATED**

## Prior validated checkpoint

The authoritative network-session foundation is complete and merged.

Validated networking head:
`e7c596c9b5a2c06c72e36c9e3f1f793b89ea59c9`

Networking squash merge:
`b0e942ae5c90b8b65192238d1f3f54e5d43eda9f`

Networking closeout:
`407c547029cb80ca536de44bfd55b4cccd8acec7`

Build #20 passed Edit Mode, all 13 Play Mode tests, server-only NGO
startup/shutdown, exact-revision visual validation, Linux Player export and artifact
inspection. Post-merge static #107 and closeout static #108 passed.

Do not redesign or replace the completed session bootstrap.

## Outcome

Give Party Night one canonical NGO player prefab and one explicit server-owned player
spawn/ownership contract that future movement replication can build on.

This task must prove:
- one checked-in Party Night network player prefab exists;
- the prefab root owns exactly one NGO `NetworkObject`;
- the prefab carries a Party Night network-player identity component;
- the persistent network bootstrap receives that prefab through an explicit serialized
  composition reference, not Resources/global lookup;
- `NetworkConfig.PlayerPrefab` is configured before a network session starts;
- the configured prefab has a non-zero NGO prefab hash after Unity import;
- dedicated server startup recognizes the prefab as the player prefab;
- default non-authoritative client scene does not spawn or claim authority by itself;
- no host-authority path is introduced;
- existing local movement remains the sole movement implementation.

## Architecture

`PartyNight.Networking` owns network identity and the NGO player prefab contract.

`PartyNight.Gameplay` owns local presentation/input/movement and supplies the serialized
prefab reference from the foundation scene to the persistent networking bootstrap.

This milestone does not make the network prefab a second movement controller. It is an
identity/spawn shell only.

## Scope

Implement:
- `PartyNightNetworkPlayer` identity component;
- checked-in `PartyNightNetworkPlayer.prefab`;
- serialized prefab reference on `FoundationSceneComposition`;
- bootstrap `ConfigurePlayerPrefab` validation and NGO `PlayerPrefab` assignment;
- Play Mode coverage for prefab configuration, identity, non-zero NGO hash and
  non-authoritative default state;
- server-start coverage proving configured player-prefab registration survives NGO
  initialization;
- static prefab/scene/ownership guards;
- architecture/status documentation.

## Explicitly out of scope

Do not implement yet:
- local input driving the network prefab;
- movement replication;
- prediction/reconciliation;
- network transform smoothing;
- networked knockback;
- remote player visuals/animation;
- server-owned Hotbox round replication;
- lobby/matchmaking/reconnect;
- WebSocket/WebGL transport validation;
- persistent accounts/data;
- Discord integration;
- production hosting/provider selection;
- final character art.

## Validation plan

Before merge:
1. static CI passes on exact head;
2. Unity compiles exact head in 6000.3.24f1;
3. Edit Mode passes;
4. all 13 existing Play Mode tests remain green;
5. new player-prefab tests pass;
6. NGO imports the prefab with non-zero `PrefabIdHash`;
7. `NetworkConfig.PlayerPrefab` references the canonical prefab;
8. dedicated server start/shutdown remains server-only;
9. no Party Night runtime code calls `StartHost`;
10. exact-revision visual validation passes;
11. Linux Player exports successfully;
12. full diff/cleanup review passes;
13. exact-head merge protection and post-merge static validation pass;
14. ACTIVE_TASK closes on main.

## Implemented

- added canonical identity-only `PartyNightNetworkPlayer.prefab`;
- prefab root contains exactly one NGO `NetworkObject`;
- prefab root contains exactly one `PartyNightNetworkPlayer`;
- prefab intentionally contains no CharacterController, local motor, or local input controller;
- foundation scene serializes the canonical prefab reference directly;
- composition supplies the prefab to the persistent network bootstrap;
- bootstrap validates root NetworkObject, Party Night identity and non-zero prefab hash;
- bootstrap assigns `NetworkConfig.PlayerPrefab` before network startup;
- reapplying the same prefab is idempotent across scene loads;
- changing the player prefab while a session is listening is rejected;
- Unity editor validation inspects the imported prefab contract;
- Play Mode coverage verifies canonical prefab configuration and identity-only shape;
- default non-authoritative scene proves configuration does not spawn a player object;
- dedicated-server test proves NGO registers the configured player prefab at startup;
- server-only startup proves no local player is invented without a connected client;
- static validation guards prefab GUID, scene reference, identity, hash and movement isolation;
- architecture/status documentation updated.

GitHub static workflow #120:
**PASS**

## Build #21 final validation

Unity Build Automation Build #21 checked out exact validated revision:

`502388737615d4d0e62ce866dc9a315c8e52cb91`

Confirmed:
- Unity `6000.3.24f1 (4e7b9b5b6244)`;
- Edit Mode exited 0;
- Play Mode exited 0;
- all 14 Party Night Play Mode tests passed;
- all five Hotbox Havoc tests passed;
- all five local-player/movement tests passed;
- all four networking/player-prefab tests passed;
- canonical player prefab configuration passed;
- dedicated-server startup registered the configured player prefab;
- dedicated server remained server-only/non-host;
- default scene remained non-authoritative and spawned no network player;
- real 1280x720 Unity PNG capture succeeded;
- exact-revision visual evidence validation passed before and after Player export;
- Linux Player export completed successfully;
- overall Unity Build Automation result was SUCCESS.

Downloaded Build #21 artifact:
- ZIP integrity passed;
- manifest revision matched the exact validated SHA;
- manifest Unity version was `6000.3.24f1`;
- manifest dimensions were 1280x720;
- Linux runtime payload was present, including `UnityPlayer.so`;
- PNG was manually inspected and showed the expected engineering Hotbox scene with no
  missing-texture corruption or new visual regression.

## Merge and closeout

Validated PR head:
`502388737615d4d0e62ce866dc9a315c8e52cb91`

PR #9 was marked ready only after Build #21 and artifact inspection.

Squash merge commit on `main`:
`38bbcd3903e1d89178db33eff548bed94bec9a40`

The squash merge used expected-head protection and its 15-file content delta matched
the exact validated PR delta.

Post-merge GitHub static workflow #122:
**PASS**

This active task is complete. The canonical network-player prefab + server-owned spawn
contract is merged to `main`. The task lock may be released after this closeout commit
itself passes static validation.

## Next step

None for this closed task.

After the closeout commit passes static validation, create a new ACTIVE_TASK entry for
the next Party Night multiplayer milestone. Do not reopen or redesign this completed
player-spawn foundation unless a verified regression requires it.
