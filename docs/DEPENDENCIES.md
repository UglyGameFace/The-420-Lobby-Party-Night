# Unity Foundation Dependencies

This file records the explicit top-level dependencies selected for the first Party Night Unity project foundation.

## Editor

| Component | Pin | Reason |
| --- | --- | --- |
| Unity Editor | 6000.3.24f1 | Unity 6.3 LTS patch released 2026-09-10; chosen instead of a moving "latest" target. |
| Editor changeset | 4e7b9b5b6244 | Recorded in `ProjectSettings/ProjectVersion.txt` for exact editor identification. |

Release notes:
- https://unity.com/releases/editor/whats-new/6000.3.24f1

## Packages

| Package | Pin | Purpose |
| --- | --- | --- |
| `com.unity.render-pipelines.universal` | 17.3.0 | Cross-platform render pipeline for mobile, Web, and desktop scaling. |
| `com.unity.inputsystem` | 1.20.0 | Device-independent keyboard, mouse, controller, and touch input foundation. |
| `com.unity.netcode.gameobjects` | 2.13.2 | Initial high-level networking candidate for GameObject/MonoBehaviour gameplay. |
| `com.unity.transport` | 2.7.4 | Explicitly pinned underlying Unity transport dependency. |
| `com.unity.test-framework` | 1.6.0 | Edit Mode/Play Mode testing foundation. |

These are top-level pins. Unity will create/update `Packages/packages-lock.json` only after the real editor resolves the dependency graph. Do not hand-author a fake lock file.

## Compatibility evidence

- Unity 6000.3.24f1 release notes update Netcode for GameObjects to 2.13.2.
- Unity 6000.3.21f1 release notes update Input System to 1.20.0 and Unity Transport to 2.7.4.
- Unity Graphics records URP 17.3.0 as compatible with the Unity 6000.3 line.
- Netcode for GameObjects documents Unity 6 LTS support and Windows/macOS/Linux/iOS/Android runtime support.
- Web transport behavior remains a later validation gate; package presence is not evidence of end-to-end browser connectivity.

## Licensing

The selected runtime packages are official Unity packages intended for Unity-dependent projects. Unity's public package repositories/documentation identify the Input System and Unity Transport under the Unity Companion License; Unity package-specific LICENSE and Third Party Notices files remain authoritative for redistribution obligations.

Before a release ships:
1. retain required Unity/package notices;
2. review the exact resolved package versions;
3. review package-specific third-party notices;
4. record any non-Unity assets separately in an asset license manifest.

## Deliberate exclusions

This task does not add:
- third-party character/model packs;
- Relay/Lobby/Multiplayer Services;
- authentication;
- analytics;
- voice;
- Addressables;
- external backend SDKs.

Dependencies are added only when an active requirement justifies them.
