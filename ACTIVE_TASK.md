# ACTIVE TASK

## Status

**CLOSED — LOCAL PLAYER MOVEMENT + CAMERA FOUNDATION**

No implementation task is currently active.

Repository:
`UglyGameFace/The-420-Lobby-Party-Night`

Merged PR:
#6 — `Add local player movement and camera foundation`

Validated PR head:
`2d40914d0919fd2ac8ffa796c550f5ba4ba8d7a9`

Squash merge on `main`:
`f395f75ee868a03a9eb83b3e15026de41e2169dc`

## Completed scope

Party Night now has its first real runtime local-player foundation.

Implemented:
- one canonical `CharacterController` movement motor;
- explicit gravity and jump behavior;
- camera-relative movement;
- third-person orbit camera;
- `LateUpdate` camera follow behavior;
- pointer-delta vs Gamepad/touch-stick rate look semantics;
- project-wide Unity Input System actions;
- one foundation-scene composition root;
- invisible engineering ground/collision surface;
- local runtime input/controller hookup;
- dedicated gameplay runtime assembly;
- permanent editor/pre-export gameplay validation;
- Edit Mode regression coverage;
- Play Mode runtime coverage.

## Runtime architecture

The local player runtime consumes `PartyNightInputFrame`.

Gameplay code does not query physical Keyboard, Mouse, Gamepad, or Touchscreen controls directly.

The movement stack is:
- `PartyNightInputReader` -> logical input frame;
- `PartyNightLocalPlayerController` -> camera-relative intent;
- `PartyNightCharacterMotor` -> collision-constrained movement;
- `PartyNightOrbitCamera` -> third-person camera state;
- `FoundationSceneComposition` -> current foundation-scene composition only.

This does not make the client authoritative for multiplayer. The future networking task must validate/drive the same movement contract instead of introducing a second unrelated controller.

Discord remains outside input, movement, camera, physics, scene lifecycle, and active-match runtime ownership.

## Final validation

GitHub static workflow #50 passed on exact PR head:

`2d40914d0919fd2ac8ffa796c550f5ba4ba8d7a9`

Unity Build Automation build #8 validated:
- exact branch/head checkout;
- Unity import and C# compilation;
- Edit Mode tests;
- gameplay/pre-export validators;
- Linux Player export.

Build #8 did not execute Play Mode tests, so it was not accepted as final runtime validation.

Unity Build Automation build #9 validated the same exact head and is the authoritative runtime validation build.

Build #9 confirmed:
- branch `foundation/local-player-movement-camera`;
- exact revision `2d40914d0919fd2ac8ffa796c550f5ba4ba8d7a9`;
- Unity `6000.3.24f1 (4e7b9b5b6244)`;
- Edit Mode launched with `-testPlatform editmode`;
- Edit Mode completed with exit code 0;
- Play Mode launched as a separate Unity run;
- Play Mode used `-testPlatform playmode`;
- Play Mode completed with exit code 0;
- committed runtime foundation scene was loaded during the Play Mode suite;
- gameplay foundation validation passed;
- pre-export foundation/gameplay validation passed;
- final Linux Player contains `PartyNight.Gameplay.dll`;
- Linux Player build completed with `Result: Success`;
- Unity reported `Finished exporting player successfully`;
- overall UBA build ended `Finished: SUCCESS`.

## Post-merge verification

The squash-merge commit on `main` has the exact same Git tree as the exact Unity-validated PR head:

`4ef6fb01830242c17f84f75fbce56215882bfbd9`

Therefore no implementation content changed during merge.

GitHub post-merge static workflow #51 passed on merge commit:

`f395f75ee868a03a9eb83b3e15026de41e2169dc`

## Cleanup

No temporary movement generator, duplicate controller, Rigidbody fallback, Resources copy of the Input Action Asset, generated input wrapper, compatibility shim, placeholder character model, backup file, or abandoned task-only code remains in the merged implementation.

## Intentionally deferred

Separate future tasks:
- final player character model and animation;
- camera collision/occlusion;
- dash gameplay;
- grab/interact/use-item gameplay;
- final mobile touch HUD and safe-area layout;
- controller glyph/UI presentation;
- rebinding/settings UI;
- authoritative multiplayer movement;
- prediction/reconciliation;
- Hotbox Havoc rules and round gameplay;
- matchmaking/lobby lifecycle;
- Discord integration.

## Next task candidate

**Local Hotbox Havoc prototype**

The next implementation should use the validated movement/camera/input foundation to establish the first actual minigame loop locally before networking expands it.

Do not begin another implementation until it becomes the single active task.
