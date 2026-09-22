# ACTIVE TASK

## Active task

**Cross-platform input foundation**

Single active implementation task for Party Night.

Repository:
`UglyGameFace/The-420-Lobby-Party-Night`

Base:
`main`

Base head:
`9448f082f8781917cf6f939f0d49c773ffe34cfd`

Working branch:
`foundation/cross-platform-input`

State:
**INVESTIGATION / IMPLEMENTATION**

## Outcome

Establish one shared action-oriented input foundation for:

`Move, Look, Jump, Interact, Grab, Dash, UseItem, Emote`

The gameplay layer must not depend on physical key/button names.

The foundation must support:
- keyboard + mouse;
- native gamepad/controller through Unity Input System's generic Gamepad bindings;
- mobile touch UI through Unity Input System on-screen controls feeding the same logical actions;
- controller use on mobile where the OS/Input System exposes the hardware;
- browser input without a separate gameplay implementation.

## Constraints

- Unity remains pinned to `6000.3.24f1`.
- Input System remains pinned to `1.20.0`.
- Closed consoles remain unsupported.
- Do not create player movement or Hotbox Havoc gameplay in this task.
- Do not add a competing input framework.
- Do not hardcode keyboard controls inside gameplay code.
- Do not require desktop Unity from the owner.
- Mobile touch must not be a desktop UI merely shrunk onto a phone.
- Web gamepad behavior is browser/OS dependent and must not be over-promised until runtime validation.
- No obsolete, temporary, duplicate, compatibility or backup implementation may remain when the task closes.

## Design direction

Use a source-controlled `.inputactions` asset as the authoritative binding definition.

The action asset will contain one gameplay action map with the eight required actions.

Physical input direction:
- keyboard/mouse bindings;
- generic `<Gamepad>` bindings so supported Xbox-style, PlayStation-style and generic controllers can flow through the Input System abstraction.

Mobile touch direction:
- Unity Input System on-screen controls will target the same Gamepad control paths used by the gameplay action map;
- this lets touch sticks/buttons drive the same actions without creating touch-only gameplay branches;
- visual touch HUD/prefab work belongs to the later UI/local-player task unless required for validation here.

Runtime ownership:
- input names/contracts live in a dedicated Party Night input assembly;
- gameplay consumers receive action values/events, not device-specific paths.

## Investigation notes

Current authoritative project state:
- `com.unity.inputsystem@1.20.0` is already pinned and resolved;
- `ProjectSettings.asset` still has `activeInputHandler: 0`, so this task must intentionally switch the project to the new Input System rather than leave the package installed but inactive;
- existing gameplay architecture already specifies action-oriented input and the same eight action names;
- foundation scene currently contains no gameplay/input MonoBehaviour ownership.

Official Unity documentation confirms that:
- `.inputactions` files are JSON-based InputActionAssets imported by Unity;
- InputActionAssets contain action maps and control schemes;
- Input System on-screen controls create virtual input devices from their configured control paths, so an on-screen stick/button targeting Gamepad controls can feed the same Gamepad bindings used by physical controllers.

## Validation plan

Before merge:
1. static CI validates the input asset structure, required actions, binding groups, generic Gamepad paths and Input System-only project setting;
2. Unity imports the `.inputactions` asset successfully;
3. C# compiles;
4. Edit Mode tests validate the imported InputActionAsset, action map, control schemes and required bindings;
5. pre-export validation checks the committed input foundation;
6. Linux Player export succeeds on the exact final PR head;
7. complete diff and cleanup are reviewed;
8. PR merges only after exact-head validation;
9. merged `main` is rechecked.

## Out of scope

- character movement implementation;
- camera controller;
- mobile HUD artwork/layout;
- safe-area UI implementation;
- rebinding/settings UI;
- multiplayer input transport;
- authoritative movement validation;
- Hotbox Havoc mechanics;
- character art;
- Discord/backend work.

## Next step

Implement the dedicated input assembly, authoritative action asset, validation and Edit Mode regression coverage on this branch.