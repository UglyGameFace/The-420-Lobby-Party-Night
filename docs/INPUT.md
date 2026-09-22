# Cross-platform input foundation

Party Night uses Unity Input System actions as the boundary between devices and gameplay.

Authoritative asset:

`Assets/PartyNight/Input/PartyNightInputActions.inputactions`

The asset is assigned as Unity Input System's **project-wide actions** through `ProjectSettings/EditorBuildSettings.asset`. Input System therefore preloads it for Player builds and exposes it through `InputSystem.actions`; Party Night does not duplicate the asset in Resources and does not generate a wrapper class.

## Gameplay action contract

| Action | Type | Keyboard / mouse | Generic gamepad |
| --- | --- | --- | --- |
| Move | Vector2 | WASD | left stick, D-pad |
| Look | Vector2 | mouse delta | right stick |
| Jump | Button | Space | button South |
| Interact | Button | E | button West |
| Grab | Button | F | left shoulder |
| Dash | Button | Left Shift | right shoulder |
| UseItem | Button | left mouse | right trigger |
| Emote | Button | G | D-pad Up |

Gameplay consumes these actions rather than physical device identities.

## Look semantics

`Look` deliberately supports two input quantities.

- Pointer/mouse delta is accumulated motion in pixels for the current update. Party Night labels that frame as `PartyNightLookInputMode.Delta`.
- Physical Gamepad and mobile on-screen sticks are persistent normalized controls. Party Night labels them `PartyNightLookInputMode.Rate`.

The orbit camera applies pointer sensitivity without multiplying by frame time, while rate input is scaled by elapsed time. This prevents mouse sensitivity from changing with FPS and prevents controller/touch rotation speed from changing with FPS.

## Native controllers

The asset binds against Unity's generic `<Gamepad>` layout. When the operating system and Unity Input System expose a controller as a Gamepad, the same actions are used without controller-brand gameplay forks. This includes supported controller use on desktop and mobile.

Web gamepad availability depends on browser, operating system, hardware and browser permission/activation behavior, so Web controller support remains a runtime-validation gate rather than a blanket promise.

Closed consoles remain outside project scope.

## Mobile touch

Touch UI will use Unity Input System on-screen controls targeting the same Gamepad controls:

- movement -> `<Gamepad>/leftStick`
- look -> `<Gamepad>/rightStick`
- jump -> `<Gamepad>/buttonSouth`
- interact -> `<Gamepad>/buttonWest`
- grab -> `<Gamepad>/leftShoulder`
- dash -> `<Gamepad>/rightShoulder`
- use item -> `<Gamepad>/rightTrigger`
- emote -> `<Gamepad>/dpad/up`

On-screen controls create a virtual Gamepad from these paths, so touch widgets and physical gamepads feed the same action bindings.

Final visual sizing, safe areas and ergonomics remain a separate mobile UI task and require real-device validation.

## Runtime API

`PartyNightInputReader.CreateFromProjectWideActions()` clones the preloaded project-wide Action Asset into a local reader instance.

`PartyNightInputFrame` contains:
- Move;
- Look;
- LookMode;
- one-frame button presses for Jump, Interact, Grab, Dash, UseItem and Emote.

Movement/camera code consumes this frame. It does not query Keyboard, Mouse, Gamepad or Touchscreen devices directly.

## Validation

Static CI verifies the action map, actions, bindings, control schemes, project-wide assignment, assembly references and Input System-only project setting.

Unity Edit Mode tests verify the asset imports, project-wide actions resolve, generic keyboard/mouse/gamepad bindings resolve, and the runtime reader can be constructed.

Runtime movement/camera behavior is covered by Party Night's Play Mode tests.

Pre-export validation runs the committed input and gameplay validators before every UBA Player build.
