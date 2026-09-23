# ACTIVE TASK

## Active task

**Server-authoritative network movement + ownership bridge**

Single active implementation task for Party Night.

Repository:
`UglyGameFace/The-420-Lobby-Party-Night`

Base:
`main`

Base head:
`81b6b443280baaada3a3a06da08b54db06f127a4`

Working branch:
`multiplayer/server-authoritative-movement`

State:
**IMPLEMENTED — STATIC PASS; UNITY REVALIDATION PENDING AFTER BUILD #22 TEST CONTRACT FIX**

## Prior validated checkpoint

The canonical network-player prefab + server-owned spawn contract is complete and merged.

Validated player-spawn head:
`502388737615d4d0e62ce866dc9a315c8e52cb91`

Player-spawn squash merge:
`38bbcd3903e1d89178db33eff548bed94bec9a40`

Player-spawn closeout:
`81b6b443280baaada3a3a06da08b54db06f127a4`

Build #21 passed Edit Mode, all 14 Play Mode tests, canonical player-prefab import and
registration, exact-revision visual validation, Linux Player export and artifact
inspection. Post-merge static #122 and closeout static #123 passed.

Do not redesign the completed session/bootstrap or player-prefab registration contracts.

## Outcome

Make the canonical network player capable of server-authoritative CharacterController
movement without introducing a second movement implementation.

Clients may submit movement intent only:
- desired planar world direction;
- jump request;
- monotonically increasing intent sequence.

Clients never submit an authoritative position, rotation, velocity, score, elimination
or other privileged state.

The dedicated server validates ownership and intent shape, advances the existing
`PartyNightCharacterMotor`, and publishes authoritative pose state.

## Architecture

The existing `PartyNightCharacterMotor` remains the one movement implementation.

The canonical network prefab intentionally evolves from identity-only into:
- NGO `NetworkObject`;
- `PartyNightNetworkPlayer` identity;
- `CharacterController`;
- existing `PartyNightCharacterMotor`;
- one gameplay-owned network movement bridge.

`PartyNight.Gameplay` may depend directly on NGO for the movement bridge because it
already depends on `PartyNight.Networking`. `PartyNight.Networking` remains independent
of gameplay/input.

Authority:
- owner client can invoke the movement-intent RPC;
- RPC target is the server;
- NGO owner permission is required;
- server also verifies sender id equals `OwnerClientId`;
- server rejects malformed, duplicate and stale/out-of-order intent sequences;
- server clamps movement magnitude;
- jump is consumed as a one-shot intent;
- stale input decays to zero rather than moving forever after packet loss;
- only server-written NetworkVariables carry authoritative pose to clients.

No host path is introduced.

## Scope

Implement:
- `PartyNightNetworkMovement` on the canonical network player;
- owner-only NGO RPC carrying direction/jump/sequence, never position;
- explicit server-side intent validation;
- server-side `PartyNightCharacterMotor` simulation;
- server-write-only authoritative position and yaw NetworkVariables;
- client-side application of replicated authoritative pose;
- CharacterController enabled only on authority while spawned;
- stale-input timeout;
- duplicate/out-of-order sequence rejection;
- canonical prefab upgrade using the existing motor;
- Play Mode server-only movement validation;
- Play Mode invalid/non-owner/duplicate intent rejection;
- static anti-cheat/authority guards;
- architecture/status documentation.

## Explicitly out of scope

Do not implement yet:
- client prediction;
- reconciliation;
- interpolation/smoothing beyond direct authoritative pose application;
- camera ownership/rebinding;
- local network-owner input wiring;
- remote character visuals/animation;
- networked knockback;
- server-owned Hotbox round replication;
- lobby/matchmaking/reconnect;
- WebSocket/WebGL validation;
- persistence/Discord/production hosting;
- final character art.

Those follow only after the authoritative movement kernel is validated.

## Quota rule

Unity DevOps free quota is already around 75%.

Do not request a Unity Cloud build until:
1. the full implementation is complete;
2. static CI is green;
3. the final diff is cleaned;
4. the exact head is frozen.

Use GitHub/static validation for intermediate mistakes.

## Validation plan

Before merge:
1. static CI passes on exact head;
2. Unity compiles exact head in 6000.3.24f1;
3. Edit Mode passes;
4. all existing 14 Play Mode tests remain green;
5. authoritative-movement tests pass;
6. server-owned spawned player advances only through `PartyNightCharacterMotor`;
7. malformed/non-owner/duplicate intent is rejected;
8. authoritative pose variables are server-write only;
9. no client-authoritative position RPC exists;
10. no `StartHost` path exists;
11. exact-revision visual validation passes;
12. Linux Player exports successfully;
13. artifact inspection + exact diff/cleanup pass;
14. expected-head merge + post-merge static pass;
15. ACTIVE_TASK closes on main.

## Implemented

- added `PartyNightNetworkMovement` in the gameplay assembly;
- canonical network player now intentionally carries one CharacterController;
- canonical network player reuses the existing `PartyNightCharacterMotor`;
- canonical network player carries one network movement bridge;
- network prefab still does not carry `PartyNightLocalPlayerController`;
- owner submission surface carries desired world direction, jump and sequence only;
- NGO RPC targets the server and requires owner invoke permission;
- server independently verifies RPC sender equals `OwnerClientId`;
- server rejects non-finite, duplicate and out-of-order movement intents;
- server clamps accepted planar direction to unit magnitude;
- stale intent expires after 0.25 seconds;
- jump intent is consumed once by the authoritative simulation step;
- only the server advances `PartyNightCharacterMotor`;
- authoritative position and yaw are server-write-only NetworkVariables;
- non-server instances disable CharacterController and apply replicated pose directly;
- Play Mode coverage spawns the real canonical prefab under a dedicated server;
- Play Mode coverage proves authoritative movement advances the existing motor;
- Play Mode coverage proves wrong-owner, malformed, duplicate and stale intents are rejected;
- editor/static validation updated for the intentional prefab evolution;
- no host path, owner-writable pose or client-authoritative position RPC exists;
- local movement, local controller, orbit camera and Hotbox rules remain unchanged.

GitHub static workflow #133:
**PASS**

## Build #22 findings

Unity Build Automation Build #22 correctly checked out:

`multiplayer/server-authoritative-movement`

at exact frozen revision:

`b4b6fa11bd979101321b3d19487f558187af5db6`

Unity version:
`6000.3.24f1 (4e7b9b5b6244)`

Confirmed:
- Edit Mode exited 0;
- NGO RPC/NetworkVariable code compiled and Play Mode executed;
- all three new authoritative-movement tests passed;
- `CanonicalNetworkPlayerUsesExistingCharacterMotor` passed;
- `ServerMovesSpawnedPlayerThroughCanonicalMotor` passed;
- `ServerRejectsInvalidDuplicateAndStaleMovementIntent` passed;
- Hotbox and local-player tests passed;
- the real Unity visual capture succeeded;
- exactly one Play Mode test failed;
- Linux Player export did not run because Play Mode exited 2.

Sole failure:
`NetworkingRuntimeTests.FoundationConfiguresCanonicalNetworkPlayerPrefab`

The old player-spawn milestone test still asserted that the canonical network prefab
must have no `CharacterController` or `PartyNightCharacterMotor`. That expectation was
intentionally superseded by this movement milestone, which upgrades the same canonical
prefab to contain one CharacterController, the existing PartyNightCharacterMotor and
one PartyNightNetworkMovement bridge.

This was a stale regression test, not a movement/runtime failure.

Correction:
- update the old canonical-prefab test to require CharacterController;
- require the existing PartyNightCharacterMotor;
- require PartyNightNetworkMovement;
- continue forbidding PartyNightLocalPlayerController on the network prefab;
- add static guards for those evolved assertions.

No movement implementation, authority rule, Hotbox logic, camera logic, transport,
package version or prefab hash was changed in response to Build #22.

## Next step

Pass GitHub static CI on the Build #22 stale-test correction, inspect the exact delta,
and freeze the new head. Then spend one Unity Build Automation run on that frozen SHA.
The expected Play Mode count remains 17, with all 17 required to pass before visual
validation and Linux export can count.
