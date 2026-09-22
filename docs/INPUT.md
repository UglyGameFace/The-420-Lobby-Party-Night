# Cross-platform input foundation

Party Night uses Unity Input System actions as the boundary between devices and gameplay.

Authoritative asset:

`Assets/PartyNight/Input/PartyNightInputActions.inputactions`

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

Gameplay code consumes these actions rather than physical device identities.

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

On-screen controls create a virtual input device from these control paths, so touch widgets and physical gamepads feed the same action bindings.

This task defines the contract, not the final touch HUD. Visual sizing, safe areas and ergonomics require the later mobile UI task and real-device testing.

## Runtime API

`PartyNightInputReader` owns a cloned action asset instance and exposes a `PartyNightInputFrame` containing Move, Look and one-frame button presses. Future movement and gameplay code consumes that logical frame rather than reading devices directly.

## Validation

Static CI verifies the action map, actions, bindings, control schemes, assembly references and Input System-only project setting.

Unity Edit Mode tests verify the asset imports, the permanent input validator passes, generic keyboard/mouse/gamepad bindings resolve against synthetic devices, and the runtime reader consumes the imported asset.

Pre-export validation runs the committed input validator before every UBA Player build.
