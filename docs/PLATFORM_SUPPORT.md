# Platform Support Baseline

## Supported target families

| Target | Status | Foundation requirement |
| --- | --- | --- |
| Windows | Planned | Keyboard/mouse + controller, native client |
| macOS | Planned | Keyboard/mouse + controller, native client |
| Linux | Planned | Keyboard/mouse + controller, native client |
| Android | Planned | Touch, safe areas, hardware diversity |
| iPhone | Planned | Touch, safe areas, iOS lifecycle constraints |
| iPad | Planned | Touch/controller where available, variable aspect ratios |
| Web desktop | Planned | Browser-safe networking and memory/performance limits |
| Web mobile | Viability target | Must be profiled on real browsers/devices before being promised |
| Chromebook | Viability target | Web path where supported by browser/hardware |
| Steam Deck | Compatibility target | Linux/PC build where practical |

## Explicitly unsupported

The project does not target:
- Xbox consoles;
- PlayStation consoles;
- Nintendo Switch;
- Nintendo Switch 2;
- other closed consoles.

No console SDK, certification, storefront, networking, or build work belongs in the current project scope.

## Cross-platform rule

A supported client must ultimately be able to participate in the same authoritative match regardless of whether it originates from desktop, mobile, or Web, subject to proven transport/platform support.

Do not create platform-specific gameplay rules merely to compensate for an architecture that ignored a target earlier.

## Input

Desktop:
- keyboard;
- mouse;
- compatible controller.

Mobile:
- virtual movement control;
- touch look/aim where required;
- context-sensitive action buttons;
- scalable HUD;
- safe-area handling.

Controller:
- Xbox-style layout where the OS exposes it;
- PlayStation-style layout where the OS exposes it;
- generic gamepads.

Gameplay code consumes actions rather than physical key/button identities.

## Web-specific gates

Before Web is marked validated:
- project builds for Unity Web;
- browser client reaches the dedicated-server transport;
- HTTPS/WSS hosting requirements are documented;
- reconnect/disconnect behavior is exercised;
- memory use is measured;
- smoke/fog rendering is profiled;
- input works in supported browsers;
- no required package depends on unsupported native sockets/plugins.

## Mobile-specific gates

Before Android/iOS are marked validated:
- touch controls are usable on real devices;
- safe areas are correct;
- UI is readable at supported resolutions/aspect ratios;
- orientation policy is explicit;
- lifecycle interruption is tested;
- reconnect behavior survives practical mobile network changes where possible;
- thermal and memory behavior are profiled;
- graphics quality can scale down without losing gameplay readability.

## Dedicated server

The game server is a native dedicated-server build, not a Web build and not a random player's client.

Linux is the initial hosting-oriented server target to validate unless evidence requires another deployment target.

## Validation language

Platform states must be reported separately as:
- planned;
- implemented;
- compiled;
- built;
- runtime-tested;
- validated.

Success on one target does not prove another target works.
