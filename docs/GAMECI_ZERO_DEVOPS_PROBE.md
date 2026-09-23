# GameCI zero-DevOps Unity validation probe

This file belongs to the disposable `ci/gameci-unity-probe` branch.

It does not change PR #10 or its frozen validation head.

## Purpose

Run the real Unity editor outside Unity Build Automation so Party Night can validate
Edit Mode, Play Mode and a Linux Player without consuming Unity DevOps build quota.

Frozen project revision:

`4a12c1ff911868e522d7d439cd996d91434440bb`

Required Unity editor:

`6000.3.24f1 (4e7b9b5b6244)`

## Hosted GameCI route

Workflow:

`.github/workflows/gameci-unity-probe.yml`

The workflow is manual-only.

It:
1. checks out the exact frozen PR #10 SHA;
2. proves the exact ProjectVersion.txt editor version and changeset;
3. verifies Unity Personal secrets exist;
4. runs real Unity Edit + Play Mode tests through GameCI;
5. disables GameCI code coverage because coverage is not a validation requirement;
6. builds a real StandaloneLinux64 Player only after tests pass;
7. verifies a real `UnityPlayer.so` and Linux `.x86_64` executable exist;
8. stores hashes, sizes, test text/XML/JSON and visual validation evidence;
9. retains compact proof/failure artifacts for one day only.

The full Linux build is intentionally not uploaded.

## Unity Personal secrets

Current GameCI Personal-license documentation requires a Unity Hub-generated `.ulf`
license plus Unity account credentials.

On Windows, after activating Unity Personal through Unity Hub, the usual license path is:

`C:\ProgramData\Unity\Unity_lic.ulf`

In GitHub:

Repository -> Settings -> Secrets and variables -> Actions

Create:

- `UNITY_LICENSE` = complete contents of `Unity_lic.ulf`
- `UNITY_EMAIL` = Unity account email
- `UNITY_PASSWORD` = Unity account password

Never commit any of these values to the repository.

The first probe already confirmed that none of these three secrets were configured at
that time. It stopped before Unity launched.

## What counts as useful evidence

A hosted GameCI success proves real Unity can:
- import/compile the frozen project;
- run Unity Test Framework Edit + Play Mode tests;
- execute NGO RPC/NetworkVariable IL post-processing;
- execute CharacterController movement tests;
- generate the Party Night visual-validation files;
- export a real Linux Player.

It does not need to become a permanent CI requirement. Unity Build Automation can remain
the occasional official confirmation path while GameCI handles development validation.

## Quota discipline

The workflow is manual-only.

Do not run it for every commit.

Freeze a meaningful implementation head first, then run one validation.

Artifacts use one-day retention and the full Linux Player is not uploaded, minimizing
GitHub Actions storage use.
