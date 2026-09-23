# ACTIVE TASK

## Task

**Owned network player input + camera handoff**

GitHub issue:
#11

Pull request:
#14 — `Add owned network player input and camera handoff`

## Status

**IMPLEMENTED — STATIC + OWNER-SHADOW + MUTATION PASS; FINAL EXACT-HEAD FREEZE PENDING**

## Base

Main closeout head:
`36886705326be69ec5cebb056968896a1bc690b0`

Prior completed milestone:
server-authoritative network movement + ownership bridge.

## Player-visible success condition

A real client connects to the dedicated server, NGO assigns its local PlayerObject,
the existing Party Night orbit camera follows that owned player, and the existing
keyboard/gamepad input path drives the owned player through the validated
server-authoritative movement bridge.

There must never be two competing local-control paths.

## Architecture

Canonical ownership source:
`NetworkManager.LocalClient.PlayerObject`

Lifecycle:
- use NGO `NetworkManager.OnConnectionEvent`;
- use explicit Party Night `ModeChanged` session notifications;
- do not poll for ownership;
- do not discover ownership through tags, names, scene-wide searches, or a second
  registry;
- `PartyNightNetworkPlayer.NetworkDespawned` provides deterministic binding cleanup.

Input:
- keep the existing project-wide `PartyNightInputReader`;
- `PartyNightLocalPlayerController` remains its sole runtime owner;
- do not add another Input Actions asset, generated wrapper, or `PlayerInput` graph;
- outside a network session the local controller advances the standalone motor;
- during a network session automatic standalone motor control is disabled;
- the owner bridge consumes the same `PartyNightInputFrame`;
- Look remains local;
- Move is converted through the orbit-camera planar basis;
- Jump remains one-shot intent;
- only desired direction, jump and monotonic sequence are submitted.

Authority:
- `PartyNightNetworkMovement` remains the movement-authority boundary;
- the client does not submit position, rotation, velocity, grounded state, collision
  result, elimination state, winner state, or restart state;
- the network prefab still does not contain `PartyNightLocalPlayerController`;
- prediction/reconciliation remains out of scope.

Session suppression:
- any active client or dedicated-server session disables standalone automatic motor
  control;
- the standalone CharacterController is disabled so its collider cannot interfere with
  authoritative network players;
- the local prototype round object is inactive during network sessions;
- owned-player despawn releases the binding but does not restore local authority while
  the session remains active;
- disconnect/shutdown restores the standalone controller, collider, camera target and
  local prototype state.

Disconnect cleanup uses Party Night's own Client mode as the lifecycle truth. It does
not require NGO `IsClient` to remain true at the instant `ClientDisconnected` is
delivered.

## Implemented

- added `PartyNightNetworkOwnerBridge`;
- added one `ModeChanged` event to `PartyNightNetworkBootstrap`;
- bootstrap now listens to NGO `OnConnectionEvent`;
- bootstrap clears Client mode on local client disconnect;
- network player exposes deterministic `NetworkDespawned`;
- local controller exposes its existing logical input frame without creating another
  input reader;
- local controller can suspend automatic standalone motor control;
- orbit camera can retarget without resetting current yaw/pitch;
- foundation composition creates exactly one owner bridge;
- owner bridge binds only NGO `LocalClient.PlayerObject`;
- owner binding validates spawned/local ownership and LocalClientId;
- Move is camera-relative and clamped;
- Look remains local;
- successful movement-intent submission advances a uint sequence;
- standalone motor, collider and local prototype round authority are suppressed for
  active network sessions;
- standalone mode restores cleanly on disconnect/shutdown;
- Play Mode coverage added for standalone baseline, dedicated-server
  suppression/restoration, camera-relative input, and network-player despawn lifecycle;
- static guards added for ownership, input uniqueness, session suppression, host
  exclusion and client-authority boundaries;
- architecture and README documentation updated.

## Zero-Unity validation

Runtime implementation head before documentation-only commits:
`0e31c65d509df767d6185cd693a1c265d6870395`

GitHub static workflow #160:
**PASS**

Owner-handoff shadow runtime on exact runtime implementation head:
- actual production source compile: **0 warnings, 0 errors**;
- owner-handoff runtime checks: **56/56 PASS**;
- owner-handoff mutation guards: **11/11 PASS**.

The shadow harness compiles the actual production sources for:
- input-frame/look-mode contract;
- CharacterController motor;
- orbit camera;
- local player controller;
- network movement;
- network owner bridge;
- network mode/bootstrap/player identity.

It uses strict Unity/NGO stubs only for engine plumbing and then exercises session,
ownership, camera, input and disconnect behavior.

## Findings

### Standalone collision interference

Initial handoff suppression disabled standalone motor control and local round authority,
but left the standalone CharacterController enabled.

That inactive local collider could remain at the spawn area on a dedicated server and
interfere physically with real authoritative network players.

Correction:
- disable the standalone CharacterController for every active network session;
- restore it only when returning to no-session standalone mode;
- Play Mode/static/shadow coverage now guards both transitions.

### Disconnect ordering

The first implementation required `NetworkManager.IsClient` to remain true while
processing `ClientDisconnected`.

NGO's normal transport path currently invokes the disconnect callback before shutdown,
but Party Night does not need to depend on that ordering.

A new shadow regression deliberately clears the simulated NGO client role before
`ClientDisconnected`:
- previous source head `1f7cbb5384f4129da8563f1700f51a60ff374722`:
  **FAIL**, proving the regression test detects the dependency;
- corrected source head `0e31c65d509df767d6185cd693a1c265d6870395`:
  **PASS**.

Party Night now clears its Client mode from the disconnect event based on its own
session mode, independent of the transient NGO `IsClient` value.

### Static mutation blind spot

The first owner mutation run found the despawn lifecycle guard used a substring check.
A sabotage rename to `RemovedNetworkDespawned` still contained the substring
`NetworkDespawned`.

The static validator now requires the exact:
- event declaration;
- `OnNetworkDespawn` override;
- event invocation.

The hardened mutation run rejects all 11 deliberate regressions.

## Play Mode validation target

Existing Play Mode result count from Build #23:
17.

New owner-handoff Play Mode tests:
4.

Exact expected Play Mode result count for this milestone:
**21**

Unity validation must prove all 21 pass.

## Explicitly out of scope

Do not implement in this task:
- two-client replication proof (#12);
- client prediction/reconciliation;
- smoothing redesign;
- server-owned round rules (#13);
- networked Interact/Grab/Dash/UseItem/Emote;
- final character art/animation;
- matchmaking/lobby/reconnect;
- persistence/Discord/production hosting.

## Validation plan

Before Unity Build Automation:
1. update this ledger and architecture docs;
2. static CI passes on the final exact head;
3. owner shadow compile/runtime passes on the final exact head;
4. 11/11 owner mutation guards pass on the final exact head;
5. final diff/scope/cleanup audit passes;
6. freeze the exact head.

Unity validation then must prove:
- Unity `6000.3.24f1 (4e7b9b5b6244)`;
- Edit Mode exit 0;
- exactly 21 Play Mode results;
- all 21 Passed;
- all existing 17 regression tests remain green;
- all four new owner-handoff tests pass;
- exact-revision visual validation remains intact;
- Linux Player export succeeds;
- artifact inspection passes.

PR #14 remains draft until those Unity/artifact gates pass.

## Conflicts / blockers

None currently.

## Backlog

After this task closes:
1. #12 — two-client ownership + replicated movement proof;
2. #13 — server-owned Hotbox Havoc multiplayer round.

## Git state

Working branch:
`multiplayer/owned-player-input-camera`

PR:
#14, draft.

## Next step

Freeze the final documentation-complete head, rerun static + owner-shadow + mutation
validation against that exact SHA, audit the final diff, then use one Unity Build
Automation run for the complete milestone.
