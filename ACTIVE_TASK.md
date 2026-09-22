# ACTIVE TASK

## Active task / outcome

Capture and source-control Party Night's **authoritative Unity project settings, deterministic package lock, and real URP configuration** using Unity 6000.3.24f1.

Outcome:
- capture `Packages/packages-lock.json` from Unity Package Manager;
- capture authoritative Unity-generated project settings;
- create Party Night's real URP Asset + Universal Renderer through Unity APIs;
- create/register Party Night URP Global Settings through Unity APIs;
- assign URP as the project default render pipeline;
- leave quality-level render-pipeline overrides empty so all quality levels inherit the project default;
- preserve Force Text serialization for version-control-safe Unity assets;
- validate a successful Linux Player export on the exact final head;
- remove the temporary capture/bootstrap implementation before task close.

## Scope

Included:
- Party Night repository only;
- `Packages/packages-lock.json`;
- `ProjectSettings/EditorSettings.asset`;
- `ProjectSettings/ProjectSettings.asset`;
- `ProjectSettings/GraphicsSettings.asset`;
- `ProjectSettings/QualitySettings.asset`;
- Party Night URP pipeline asset;
- Party Night Universal Renderer asset;
- Party Night URP Global Settings and default volume profile if Unity creates/requires them;
- editor-side URP/project-settings validation;
- deterministic capture manifest from Unity Build Automation;
- exact-head static + Unity cloud validation.

Excluded:
- Input Action asset implementation;
- player movement;
- controller/touch bindings;
- Hotbox Havoc gameplay;
- character models/art;
- networking implementation;
- Discord/backend integration;
- changes to any other repository.

Native controller support remains a hard requirement for the later input task across supported desktop/mobile/Web environments. Mobile touch and controller will feed the same logical input actions; this settings task does not implement those bindings yet.

## Status

**IMPLEMENTATION — CAPTURE STAGE**

Repository:
`UglyGameFace/The-420-Lobby-Party-Night`

Base/default branch:
`main`

Verified base head:
`0e18327a9cee65b6251fd6d3cde467656f447a10`

Working branch:
`foundation/authoritative-settings-urp`

PR:
not opened yet.

## Why this task uses two Unity passes

The authoritative files do not yet exist in Git and must not be fabricated.

Pass A — capture:
1. GitHub static preflight passes.
2. UBA checks out the capture branch.
3. Package Manager resolves and writes the real lock file.
4. the existing pre-export hook calls a temporary editor bootstrap.
5. Unity APIs create/save the real Party Night URP assets and project settings.
6. the Linux Player build validates the generated configuration.
7. a post-build Unity callback copies the authoritative generated files plus SHA-256 manifest into the downloadable Player artifact.

Pass B — final:
1. the captured files are reviewed and committed unchanged except for intentional path cleanup performed through Unity-safe GUID-preserving moves;
2. the temporary bootstrap/capture code is removed;
3. validators/tests require the committed settings directly;
4. static CI passes on the exact final head;
5. UBA compiles, tests, validates, and exports the Linux Player on that exact final head;
6. only then may the PR merge.

This prevents temporary generation code from becoming permanent architecture.

## URP ownership

Planned source-controlled paths:

- `Assets/PartyNight/Settings/PartyNightURP.asset`
- `Assets/PartyNight/Settings/PartyNightUniversalRenderer.asset`
- `Assets/PartyNight/Settings/PartyNightURPGlobalSettings.asset`
- `Assets/PartyNight/Settings/PartyNightDefaultVolumeProfile.asset` if generated/required by Unity

The bootstrap uses supported Unity/URP APIs rather than hand-writing serialized YAML.

## Project settings ownership

Required captured files:

- `Packages/packages-lock.json`
- `ProjectSettings/ProjectVersion.txt`
- `ProjectSettings/EditorSettings.asset`
- `ProjectSettings/ProjectSettings.asset`
- `ProjectSettings/GraphicsSettings.asset`
- `ProjectSettings/QualitySettings.asset`
- `ProjectSettings/EditorBuildSettings.asset`

The final source of truth remains Git. UBA is the authoritative Unity editor used to generate/validate these files because the project owner is phone-only.

## Validation requirements

Before merge:
- package lock parses and matches the pinned direct package versions;
- Unity metadata GUIDs remain unique;
- Party Night URP asset imports;
- Universal Renderer imports and is the URP asset's default renderer;
- Party Night URP Global Settings are registered;
- GraphicsSettings default render pipeline points to Party Night URP;
- quality-level overrides do not silently select a different render pipeline;
- Force Text serialization is enabled;
- Party Night product name remains correct;
- existing scene/build settings remain valid;
- C# compiles;
- Edit Mode tests pass;
- pre-export validator passes;
- Linux Player export succeeds;
- temporary capture/bootstrap implementation is gone before merge;
- no stale/backup/temp/generated Unity directories remain.

## Cleanup rule

Git history is the backup.

The final task result must not retain:
- the temporary settings bootstrap;
- the artifact capture callback;
- duplicate render-pipeline assets;
- root-level temporary `UniversalRenderer.asset`;
- root-level temporary URP global settings/default volume profile;
- backup/temp copies;
- disabled superseded settings;
- generated Unity working directories.

## Next step

Implement the temporary Unity editor bootstrap + artifact capture path, harden static validation around it, open a draft PR, and run the first UBA capture build.
