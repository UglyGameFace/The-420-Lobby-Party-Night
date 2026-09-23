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

The workflow is isolated from normal development.

It can be started in two ways:
- manual dispatch where GitHub exposes the branch workflow;
- a push that changes only `.github/gameci-trigger` on `ci/gameci-unity-probe`.

The sentinel trigger exists so the probe can be launched without merging the workflow
to `main` and without touching PR #10.

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

## Unity Personal licensing status

This hosted real-Unity route is currently **BLOCKED for this project workflow**.

The project owner is phone-only and does not have access to a desktop machine running
Unity Hub.

Unity's current official licensing documentation states that Unity Personal activation
must be performed through Unity Hub. Manual .alf/.ulf activation and command-line
activation do not support Unity Personal.

Therefore:
- do not ask the project owner to retrieve a Windows/macOS/Linux Unity license file;
- do not ask the project owner to use a desktop-only Unity Hub flow;
- do not use undocumented browser/devtools workarounds to bypass Unity's Personal
  activation flow;
- do not trigger the hosted GameCI Unity job while those constraints remain.

Keep this workflow as a parked experiment only.

The license-free shadow runtime is the active zero-Unity-cost validation path.

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

Normal commits on the probe branch do not launch Unity. Only the explicit sentinel path
(or an available manual dispatch) does.

Do not run it for every commit.

Freeze a meaningful implementation head first, then run one validation.

Artifacts use one-day retention and the full Linux Player is not uploaded, minimizing
GitHub Actions storage use.


## Self-hosted runner note

GitHub self-hosted runners do not consume GitHub-hosted Actions minutes, and a PC with
Unity already activated could run the editor directly.

Do not attach the user's everyday Windows PC as a general self-hosted runner to this
public repository as the default solution. GitHub warns that public-repository fork/PR
workflows can create a code-execution risk for self-hosted machines.

The hosted GameCI probe is therefore the safer first route for this public repository.
If a self-hosted runner is ever introduced, it must be isolated and locked down rather
than using the user's normal desktop as an unrestricted runner.
