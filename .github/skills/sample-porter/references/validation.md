# Synchronization and validation

Read this reference before running synchronization or builds. Preserve evidence
in the task's external checkpoint, not in sample folders.

## Synchronize without unrelated changes

After implementing the sample and its WPF README, run:

```powershell
python tools\sample_sync.py
```

This command copies readmes and regenerates metadata, sample attributes, and
platform tables of contents across the repository. It is not target-sample-only.
Do not skip it because the generated files already look correct.

Before running, capture Git status, the binary-capable diff, and exact bytes and
hashes of all modified or untracked files the scripts can touch. Record the
current `HEAD` as the baseline for tracked files that are clean. Preserve the
index separately from working-tree content; do not stage synchronization output
as a way to save it.

After running:

1. Capture output, exit status, and the complete changed-path list before cleanup.
2. Inspect diagnostics as well as the wrapper exit code. `sample_sync.py` does
   not propagate its subprocess failures, and its child scripts can print errors
   without failing. A zero exit code alone is not proof of synchronization.
3. Verify the target sample's readmes, metadata, sample attributes, image casing,
   and entries in each platform's table of contents.
4. Keep target-sample changes and only the required registration/documentation
   changes outside it.
5. Undo this run's changes to other samples using their exact pre-sync contents.
   Use recorded `HEAD` content only for individually identified paths that were
   clean before synchronization; use saved bytes for pre-existing edits. Delete
   only exact paths proven to have been newly created by this run.
6. Check that pre-existing changes and index contents are preserved and no
   unrelated synchronization diff remains.

Never perform a directory-wide restore or wildcard cleanup. If the snapshot is
insufficient or files changed concurrently, stop instead of guessing. Diagnose
target-sample errors before proceeding. Record unrelated script errors and their
effect; do not claim a clean full sync when the wrapper masked a failure.

## Choose the SDK and toolchain

Read `src\Directory.Packages.props` and the current project target frameworks.
Use the repository's SDK pin by default; do not change it to make a sample build.
Record the base branch from task context rather than guessing from a feature
branch name.

Development-line work may require an unreleased SDK. When the task or branch
requirements call for a daily build, use the authorized package source available
to the contributor. Do not assume access to Esri-internal feeds, create feed
credentials, or hardcode a release number that will become stale.

For a configured source named `Artifactory`, the discovery command is:

```powershell
dotnet package search Esri.ArcGISRuntime `
    --source Artifactory `
    --prerelease `
    --exact-match `
    --format json
```

Use the release line specified by the task, otherwise the version in
`ArcGISMapsSDKVersion`. For `<major>.<minor>.<patch>-daily<digits>` candidates,
filter to that exact release line and choose the largest numeric daily suffix,
not the lexicographically greatest string. Record the query, chosen version,
source, and repository pin. If the required source is unavailable or no matching
version can restore, report the blocker; never silently use a stale daily.

Pass a selected daily only as `-p:ArcGISMapsSDKVersion=<selected-version>` to
every restore, build, run, and deployment command, including fallbacks. Never
persist it in `Directory.Packages.props`, another tracked file, or a commit.
Keep any pre-existing user changes to package configuration intact.

## Build each viewer

On Windows, build WPF, WinUI, and MAUI Windows in that order. The following
commands match the current projects; re-read their target frameworks before use
on another branch:

```powershell
dotnet build src\WPF\WPF.Viewer\ArcGIS.WPF.Viewer.Net.csproj
dotnet build src\WinUI\ArcGIS.WinUI.Viewer\ArcGIS.WinUI.Viewer.csproj -p:Platform=x64
dotnet build src\MAUI\Maui.Samples\ArcGIS.Samples.Maui.csproj -f net10.0-windows10.0.19041.0
```

Add the build-only SDK override above when required. WinUI must select a supported
architecture, not AnyCPU; use x64 for these validation commands.

Record each command, exit code, and relevant warnings/errors. Classify failures
as source, dependency, toolchain, platform, or environment failures before
changing code. Establish a baseline without task-owned changes when needed and
safe; do not remove user work or fix unrelated samples to force a green build.

On non-Windows hosts, build supported MAUI targets when possible and explicitly
report WPF, WinUI, and MAUI Windows as unvalidated. Missing workloads, private
packages, or platform tooling are blockers, not successful validation.

### Conditional Windows fallbacks

Use these only when the original failure supports the diagnosis, not as blanket
build settings:

| Failure | Response |
| --- | --- |
| WinUI XAML compilation fails from a long worktree path | Check XAML diagnostics first. If path length is implicated, build through a task-owned short directory junction; record both results and remove only that junction afterward. |
| MAUI copy failures caused by paths exceeding a tool's path limit | Measure the failing path, then retry with `-p:UseArtifactsOutput=true -p:ArtifactsPath=<task-owned-short-directory>`. This isolates output by project. |
| Windows App Runtime registration failure (`REGDB_E_CLASSNOTREG`) before app startup | If the diagnostics identify missing registration, try a build-only `-p:WindowsAppSDKSelfContained=true` override and run that output. |

Never share one `BaseIntermediateOutputPath` across the viewer and catalog
generator projects: their assets can collide. Do not claim that a generic XAML
compiler exit code proves either a path problem or a source problem.

Use only task-owned temporary directories. Do not remove a junction's target,
reuse someone else's output directory, or terminate unrelated viewer processes.
Record the exact successful fallback and retain the selected SDK override.

## Exercise runtime behavior

When a desktop viewer is available, build the updated binaries and launch the
target sample. After a change, rebuild and relaunch rather than testing a stale
process. Stop only the process started for this task.

Exercise the initial state and each control that changes SDK state, including
clear/reset behavior. Confirm visible map or scene changes, not only checkbox,
radio-button, or slider state. Check loading failures, repeated events, and
required data access as relevant to the lesson.

For an authorized connected Android device, the current MAUI deployment command
is:

```powershell
dotnet build src\MAUI\Maui.Samples\ArcGIS.Samples.Maui.csproj -t:Run -f net10.0-android
```

Use the same SDK override if selected. Report iOS, Android, and Mac Catalyst
validation separately; a Windows build does not establish mobile behavior.
Use the viewer's API-key prompt or existing local configuration, never a key in
sample source or command output.

## Final audit and handoff

Review the final diff against the initial snapshot. Require:

- Equivalent WPF, WinUI, and MAUI behavior and correct names, namespaces, category,
  geo-view, scene mode, and offline-data handling.
- The generated five-file baseline on each platform, with any necessary extra
  support files justified and all placeholder screenshot hashes unchanged.
- Synchronized README, metadata, and named sample attributes, plus exactly one
  featured-sample entry in the intended category.
- A second semantic README-to-code review after synchronization: every user
  interaction and workflow step maps to the final code and initial state.
- No unrelated edits, generated catalog edits, redundant project entries, secrets,
  or persisted daily SDK pins.

In the final response, state the sample and platforms changed, generator command
and inputs, placeholder retention, synchronization outcome, build results, any SDK
override, runtime interactions exercised, and intentional deviations. Keep full
logs in task artifacts. Identify skipped checks and unresolved failures plainly.
Say "compiles" when only builds were run; say runtime behavior was verified only
for viewers and interactions actually exercised.

Do not commit, push, or open a pull request unless requested. When a PR is
requested, follow the repository's `.github\pull_request_template.md`, retain its
sections, and leave unperformed checks unchecked. Use a draft PR and title it
`New Sample: <Canonical title>` for a new sample. Follow the user's branch and
approval requirements, and inspect the outgoing diff for secrets before publishing.
