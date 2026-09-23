# ACTIVE TASK

## Task

**Owned network player input + camera handoff**

GitHub issue:
#11

## Status

**IN PROGRESS — IMPLEMENTATION**

## Base

Main closeout head:
`36886705326be69ec5cebb056968896a1bc690b0`

Prior completed milestone:
server-authoritative network movement + ownership bridge.

## Player-visible success condition

A real client connects to the dedicated server, NGO assigns its local PlayerObject,
the existing Party Night orbit camera follows that owned player, and the existing
keyboard/gamepad input path drives the owned player through the already-validated
server-authoritative movement bridge.

There must never be two competing local-control paths.

## Architecture

Canonical ownership source:
`NetworkManager.LocalClient.PlayerObject`

Use NGO `NetworkManager.OnConnectionEvent` for connection/disconnection lifecycle.
Do not poll for ownership and do not discover the local player through tags, names,
scene-wide searches, or a second player registry.

Input:
- keep the existing project-wide `PartyNightInputReader`;
- do not add another Input Actions asset, generated wrapper, or `PlayerInput` graph;
- the standalone local controller remains the one owner of the input reader;
- during network ownership, it stops advancing the standalone local motor;
- the network owner bridge consumes that same input frame;
- Look remains local and immediately updates the orbit camera;
- Move becomes camera-relative planar desired direction;
- Jump is a one-shot movement intent;
- only desired direction, jump and monotonic sequence are submitted.

Authority:
- existing `PartyNightNetworkMovement` remains the movement authority boundary;
- client never submits position, rotation, velocity, grounded state, collision state,
  exposure, elimination, winner state, or restart state;
- network prefab must not gain `PartyNightLocalPlayerController`;
- prediction/reconciliation remains out of scope.

Session handoff:
- any active network session suppresses the standalone local motor-control path;
- dedicated-server mode also suppresses the standalone prototype rig as gameplay
  authority;
- local Hotbox round gameplay/HUD/visual authority is suspended during network sessions;
- a bound owned PlayerObject retargets the one existing orbit camera;
- player despawn releases the owned-player binding without creating a local-authority
  fallback while the network session remains active;
- local disconnect/shutdown restores standalone prototype mode cleanly.

## Scope

Implement:
- network session-mode lifecycle notification;
- one gameplay-owned network owner input/camera bridge;
- shared existing input-reader handoff;
- camera retarget support;
- local prototype suppression/restoration;
- bound-player despawn cleanup;
- camera-relative Move + local Look + one-shot Jump submission;
- monotonic sequence diagnostics;
- Play Mode coverage for session suppression/restoration and input mapping;
- static guards for canonical `LocalClient.PlayerObject` ownership and forbidden
  duplicate input/host/client-authority paths;
- documentation.

## Explicitly out of scope

Do not implement:
- two-client replication proof (#12);
- client prediction/reconciliation;
- smoothing redesign;
- server-owned Hotbox rules (#13);
- networked Interact/Grab/Dash/UseItem/Emote;
- final character art/animation;
- matchmaking/lobby/reconnect;
- persistence/Discord/production hosting.

## Validation plan

Before any Unity Build Automation run:
1. implementation complete;
2. static CI green;
3. exact diff cleaned;
4. no duplicate input reader or local controller on network prefab;
5. no host path;
6. no local Hotbox authority during network session;
7. deterministic shadow/static checks where useful;
8. exact head frozen.

Unity validation must then prove:
- existing tests remain green;
- new owner-handoff tests pass;
- default local prototype still works when no session is active;
- dedicated server suppresses standalone local gameplay authority;
- camera-relative mapping and local Look behavior remain correct;
- project imports/compiles under Unity `6000.3.24f1`;
- visual validation remains intact;
- Linux Player export succeeds.

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
not created yet.

## Next step

Implement the ownership/session handoff without duplicating the input reader or creating
client-authoritative gameplay state.
