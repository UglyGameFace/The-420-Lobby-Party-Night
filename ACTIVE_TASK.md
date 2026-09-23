# ACTIVE TASK

## Task

**Server-authoritative network movement + ownership bridge**

## Status

**CLOSED — VALIDATED, MERGED, POST-MERGE GREEN**

## Scope

The canonical NGO network player now supports server-authoritative
CharacterController movement without introducing a second movement implementation.

Clients submit movement intent only:
- desired planar world direction;
- one-shot jump request;
- monotonic sequence.

Clients do not submit authoritative position, rotation, velocity, grounding,
collision, exposure, elimination, score, or other privileged state.

## Architecture

The existing `PartyNightCharacterMotor` remains the single movement implementation.

The canonical network player contains:
- one NGO `NetworkObject`;
- one `PartyNightNetworkPlayer`;
- one `CharacterController`;
- one existing `PartyNightCharacterMotor`;
- one `PartyNightNetworkMovement`.

Authority contract:
- movement RPC targets the server;
- NGO owner invocation permission is required;
- server independently verifies sender equals `OwnerClientId`;
- malformed, duplicate and stale/out-of-order intents are rejected;
- accepted planar movement is clamped to unit magnitude;
- jump is consumed once;
- stale intent expires after 0.25 seconds;
- only the server advances `PartyNightCharacterMotor`;
- authoritative position/yaw NetworkVariables are server-write-only;
- non-server clients disable CharacterController and apply replicated pose;
- no `StartHost()` path exists.

## Changes

Implemented:
- `Assets/PartyNight/Gameplay/Runtime/PartyNightNetworkMovement.cs`;
- canonical network prefab evolution for authoritative movement;
- gameplay assembly NGO reference;
- editor/prefab validation updates;
- Play Mode authoritative movement coverage;
- stale previous-milestone prefab assertion correction;
- static authority regression guards;
- validator mutation hardening for stale/inverted assertions;
- architecture/readme documentation.

No local input/camera ownership wiring, prediction/reconciliation, remote character
presentation, or server-owned Hotbox rules were added in this task.

## Findings

Unity Build #22 validated the movement runtime itself but exposed one stale
previous-milestone regression assertion:
`NetworkingRuntimeTests.FoundationConfiguresCanonicalNetworkPlayerPrefab`
still expected no CharacterController/motor.

That stale expectation was corrected without changing movement runtime behavior.

Zero-quota mutation testing then exposed a static-validator blind spot: the validator
proved component assertion tokens existed but did not prove whether they used
`Is.Null` versus `Is.Not.Null`.

The validator now explicitly requires:
- CharacterController: `Is.Not.Null`;
- PartyNightCharacterMotor: `Is.Not.Null`;
- PartyNightNetworkMovement: `Is.Not.Null`;
- PartyNightLocalPlayerController: `Is.Null`.

## Validation

Final validated PR head:
`1f4a7b8902cb751f16f2a6647679cdb841d92c77`

Before Unity Build #23:
- GitHub static #140: **PASS**;
- license-free shadow runtime: **84/84 PASS**;
- shadow compile: **0 warnings, 0 errors**;
- mutation guards: **9/9 PASS**;
- full PR scope audit: **PASS**.

Unity Build Automation Build #23:
- branch: `multiplayer/server-authoritative-movement`;
- exact revision: `1f4a7b8902cb751f16f2a6647679cdb841d92c77`;
- Unity: `6000.3.24f1 (4e7b9b5b6244)`;
- Edit Mode: **PASS / exit 0**;
- Play Mode: **17/17 PASS**;
- corrected canonical network prefab test: **PASS**;
- all three authoritative movement tests: **PASS**;
- exact-revision visual validation: **PASS**;
- StandaloneLinux64 Player export: **PASS**;
- Build Automation final result: **SUCCESS**.

Build #23 artifact inspection:
- ZIP integrity: **PASS**;
- visual manifest exact revision: **PASS**;
- visual manifest Unity version: **PASS**;
- visual capture: valid **1280x720 RGB PNG**, manually inspected;
- Linux executable: valid **x86-64 ELF**;
- `UnityPlayer.so`: valid **x86-64 ELF shared object**;
- Party Night runtime assemblies, NGO runtime and Unity Transport present.

The generated visual text says `Build result: Unknown` because it is emitted during
post-processing before the overall Build Automation result is finalized. The
authoritative build run ended in SUCCESS and exact-revision visual validation passed,
so this is a non-blocking artifact-label quirk, not a build failure.

## Cleanup

Final PR scope audit confirmed:
- no unrelated runtime subsystem changes;
- no cross-project runtime contamination;
- no duplicate local movement implementation;
- no owner-writable authoritative pose;
- no client-authoritative position RPC;
- no host startup path;
- no stale/backup/temp artifact introduced on main.

The disposable license-free shadow/GameCI probe remains outside main on its isolated
CI branch and is not part of the shipped project.

## Conflicts / blockers

None for the completed task.

## Git state

PR #10:
**MERGED**

Squash merge on main:
`b67656d19fcbd8a955a829e7c51694137631b4be`

Post-merge GitHub static workflow #141:
**PASS**

## Backlog

Queued player-facing progression:
1. #11 — owned network player input + camera handoff;
2. #12 — two-client ownership + replicated movement proof;
3. #13 — server-owned Hotbox Havoc multiplayer round.

Important constraint for #11:
the current local Hotbox prototype must not be rebound to a network player as
client-authoritative game logic. Network sessions suspend that local authority until
Hotbox rules migrate to the dedicated server in #13.

## Next step

After this closeout-only commit passes its final static workflow, activate #11 as the
new Single Active Task:

**owned network player input + camera handoff**
