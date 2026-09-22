# ACTIVE TASK

## Active task / outcome

Capture and source-control Party Night's **authoritative Unity project settings, deterministic package lock, and real URP configuration** using Unity 6000.3.24f1.

Outcome:
- source-control Unity's authoritative `Packages/packages-lock.json`;
- source-control authoritative Unity-generated project settings;
- source-control Party Night's real URP Asset + Universal Renderer;
- source-control/register Party Night URP Global Settings and default volume profile;
- assign URP as the project default render pipeline;
- keep every quality level inheriting the project render pipeline;
- preserve Force Text serialization;
- remove the temporary capture/bootstrap implementation;
- validate a successful Linux Player export on the exact final head.

## Scope

Included:
- Party Night repository only;
- package lock;
- Editor/Project/Graphics/Quality settings;
- Party Night URP pipeline asset;
- Universal Renderer asset;
- URP Global Settings;
- default volume profile;
- editor-side URP/project-settings validation;
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

**VALIDATION — FINAL COMMITTED STATE**

Repository:
`UglyGameFace/The-420-Lobby-Party-Night`

Base/default branch:
`main`

Base head:
`0e18327a9cee65b6251fd6d3cde467656f447a10`

Working branch:
`foundation/authoritative-settings-urp`

PR:
#4, draft.

## Capture build evidence

Unity Build Automation build #4 ran on exact capture head:

`9f6ade2d800547ebdea16e4cdaa3e4119efd8e1c`

Validated:
- correct branch checked out;
- exact capture commit matched;
- Unity `6000.3.24f1 (4e7b9b5b6244)` launched;
- package resolution succeeded;
- Unity generated `Packages/packages-lock.json`;
- Edit Mode tests completed with exit code 0;
- pre-export bootstrap generated/imported Party Night URP assets;
- foundation validator passed;
- Linux Player build completed with `Result: Success`;
- Player export finished successfully;
- post-build capture reported 16 authoritative settings files;
- UBA build #4 ended `Finished: SUCCESS`.

## Artifact verification

The downloaded build #4 artifact was inspected directly.

Capture directory:
`PartyNightAuthoritativeSettings/`

Manifest:
`SHA256SUMS.txt`

Verified:
- all 16 captured files were present;
- all 16 SHA-256 entries matched the captured bytes;
- the archive imported into Git was itself SHA-256 verified before extraction;
- every authoritative Git blob was compared against the corresponding captured file's Git blob SHA-1 and matched exactly.

Authoritative captured assets:
- `Assets/PartyNight/Settings/PartyNightURP.asset`;
- `Assets/PartyNight/Settings/PartyNightUniversalRenderer.asset`;
- `Assets/PartyNight/Settings/PartyNightURPGlobalSettings.asset`;
- `Assets/PartyNight/Settings/PartyNightDefaultVolumeProfile.asset`;
- their `.meta` files and Settings folder metadata;
- `Packages/packages-lock.json`;
- `ProjectSettings/EditorSettings.asset`;
- `ProjectSettings/ProjectSettings.asset`;
- `ProjectSettings/GraphicsSettings.asset`;
- `ProjectSettings/QualitySettings.asset`.

The captured `ProjectVersion.txt` and `EditorBuildSettings.asset` were byte-compared with the already committed authoritative copies during import and matched, so duplicate replacements were not retained.

## Permanent ownership

The temporary settings generator and artifact-capture callback are removed in the final committed state.

`ProjectFoundationValidator` now validates rather than mutates:
- exact Unity version;
- exact resolved direct package versions;
- imported build scene;
- imported Party Night URP asset;
- authoritative Universal Renderer ownership;
- project default render pipeline;
- registered Party Night URP Global Settings;
- default volume profile import;
- Force Text serialization;
- Party Night product name;
- absence of quality-level render-pipeline overrides.

Edit Mode regression coverage calls the same authoritative validator.

Static CI now requires:
- the deterministic package lock;
- exact direct package pins in manifest and lock;
- authoritative settings files;
- preserved Unity GUID relationships between URP, renderer, global settings, volume profile, GraphicsSettings and metadata;
- Force Text;
- Party Night product name;
- inherited render pipeline at every quality level;
- no temporary capture/import plumbing;
- existing scene, metadata, assembly, stale-artifact and conflict checks.

## Known intentional follow-ups

Not changed inside this task:
- `companyName` is still Unity's default and must be intentionally set before real platform/store packaging;
- `activeInputHandler` remains at the captured baseline until the dedicated Input Action/controller/touch task;
- platform-specific quality tuning comes after the shared URP foundation is validated.

These are tracked follow-ups, not claimed as complete.

## Cleanup

The final task result must not contain:
- `AuthoritativeSettingsBootstrap.cs`;
- its metadata;
- the artifact-capture callback;
- base64 transport chunks;
- the one-shot import workflow;
- duplicate URP assets;
- root-level temporary renderer/global-settings/volume-profile assets;
- backup/temp copies;
- generated Unity working directories.

## Remaining validation before merge

1. GitHub static CI must pass on the exact final implementation head.
2. The complete PR diff must contain only this task's intended files.
3. Unity Build Automation must run against that exact final head.
4. C# compilation must pass.
5. Edit Mode tests must pass.
6. pre-export authoritative settings validation must pass.
7. Linux Player export must succeed.
8. cleanup/stale-artifact review must pass.
9. only then may PR #4 be marked ready and merged.

## Next step

Run static CI on the exact final implementation head. If green, run one final Unity Build Automation build against `foundation/authoritative-settings-urp`. Do not replay the capture build.
