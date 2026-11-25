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

## Check README and metadata formatting

After synchronization, check every target sample folder, including new files
that are still untracked. Substitute each actual folder for `$sampleDirectory`:

```powershell
python tools\CI\README_Metadata_StyleCheck\README_style_checker.py -s $sampleDirectory
python tools\CI\README_Metadata_StyleCheck\metadata_style_checker.py -s $sampleDirectory
mdl --style tools\CI\README_Metadata_StyleCheck\style.rb (Join-Path $sampleDirectory 'readme.md')
```

The last command is Ruby markdownlint, using the same style configuration as
CI. If `mdl` is unavailable, record that check as skipped; do not claim the full
Markdown lint check passed because the Python checks passed.

Always verify that all three READMEs end with exactly one newline (`MD047`).
For this check only, normalize CRLF to LF and require
`data.endswith(b"\n") and not data.endswith(b"\n\n")`. Check the actual bytes,
not just the editor's appearance. Neither the Python checkers nor
`git diff --check` detects the missing-final-newline case, and an ordinary Git
diff does not include untracked sample files.

Correct the WPF source and propagate it through the repository's copy/sync tools,
rather than editing the two copies independently. For a newline-only follow-up,
the target-sample mode of `tools\readme_copy\readme_copy.py` can copy the fix
without rewriting unrelated samples; the initial port still requires the full
`sample_sync.py` run. Record commands, exit codes, and skipped checks.

Before publishing, repeat the final-newline check against the staged or outgoing
committed README blobs. Do not rely solely on working-tree line endings.

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

For an intended unpackaged Windows launch, pair self-contained deployment with
`-p:WindowsPackageType=None` when needed. A failure to locate
`ms-appx:///Microsoft.UI.Xaml/Themes/themeresources.xaml` after a self-contained
build can indicate a packaged/unpackaged resource-resolution mismatch. Verify
the deployment model and rebuild with both properties instead of changing sample
source. Do not apply `WindowsPackageType=None` to a deployment intended to remain
packaged. Record this as an environment/deployment fallback.

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

Record a per-platform validation matrix with the target OS/API/architecture,
build result, initial-rendering result, interaction coverage, and evidence or
reason for skipping. Distinguish passed, failed, partial, and not run. A launch
or screenshot does not establish that all controls work, and equivalent source
does not prove platform-specific event wiring works.

Keep agent-run checks separate from user-reported testing. A PR checkbox changed
by someone else is not evidence that the agent performed that validation; do
not overwrite that person's sign-off or silently relabel it as agent-run.

### Android devices and emulators

Check `adb devices -l` and existing AVDs before declaring Android runtime
validation unavailable. Resolve tools from the configured Android SDK/toolchain;
if multiple SDK installations exist, the first available `adb` does not prove
that no emulator is installed elsewhere. Use `emulator -list-avds` from the
resolved emulator installation.

When authorized, use an existing AVD with read-only storage and snapshots
disabled, such as `-read-only -no-snapshot`, on a verified unused emulator port.
Record its process and serial. Do not create, wipe, or reconfigure an AVD merely
to obtain a passing result. Target every device operation explicitly and stop
only the emulator started for this task.

For an authorized device or emulator, set `$serial` to its recorded serial and
deploy explicitly to that target:

```powershell
dotnet build src\MAUI\Maui.Samples\ArcGIS.Samples.Maui.csproj -t:Run -f net10.0-android -p:Device=$serial
```

Add the same SDK override if selected. Exercise the sample controls and, where
relevant, portrait/landscape layout and state preservation across rotation.
Restore any changed device settings and shut down task-owned runtime helpers.
Report emulator, physical-device, architecture, and Android-version coverage
accurately; one emulator does not establish behavior on all Android devices.

Report iOS, Android, and Mac Catalyst validation separately; a Windows build does
not establish mobile behavior. Use the viewer's API-key prompt or existing local
configuration, never a key in sample source or command output.

## Screenshots and repository assets

Distinguish three requests: capture screenshots as artifacts, attach images to a
PR, and replace the sample's repository images. One does not imply the others.
Resolve the destination and requested platforms when unclear, and do not add
Android or other captures to a Windows-only request.

Keep generated placeholders unchanged by default. An explicit replacement
request authorizes using verified .NET runtime captures, not images copied from
the source design. Capture the sample and its controls; retain attribution and
SDK license watermarks rather than editing them out.

For repository replacements, follow the current PR template: WPF, WinUI, and
MAUI Windows images are 800 x 600, and MAUI mobile platforms use the MAUI Windows
screenshot. Preserve the existing JPEG filenames and MAUI lowercase casing.
Convert formats correctly; do not simply rename a PNG to `.jpg`. Verify actual
encoding, dimensions, README/metadata references, and rendering in each viewer's
Description tab before marking the screenshot checklist item complete.

Keep the original generated baseline immutable. Record the replacement request,
source capture, previous hash, and replacement hash separately, then update the
latest checkpoint to the approved contents. Final audits must accept only the
recorded baseline or an explicitly approved replacement, not arbitrary drift.

For PR attachments, use the available media-upload skill or approved host tool,
verify the returned hosted links, and actually update the requested PR. Uploading
an attachment does not replace repository assets, and replacing a repository
asset does not attach it to the PR description. Preserve the latest PR content
when inserting a compact screenshot gallery.

## Final audit and handoff

Review the final diff against the initial snapshot. Require:

- Equivalent WPF, WinUI, and MAUI behavior and correct names, namespaces, category,
  geo-view, scene mode, and offline-data handling.
- The generated five-file baseline on each platform, with any necessary extra
  support files justified and screenshots matching either the generated hashes
  or separately recorded, user-authorized replacement hashes.
- Synchronized README, metadata, and named sample attributes, plus exactly one
  featured-sample entry in the intended category.
- README/metadata check results, explicit Markdown lint skips, and passing
  final-newline checks for all three outgoing READMEs.
- A second semantic README-to-code review after synchronization: every user
  interaction and workflow step maps to the final code and initial state.
- No unrelated edits, generated catalog edits, redundant project entries, secrets,
  or persisted daily SDK pins.

In the final response, state the sample and platforms changed, generator command
and inputs, placeholder retention or approved replacements, synchronization
outcome, build results, any SDK override, runtime interactions exercised, and
intentional deviations. Keep full logs in task artifacts. Identify skipped checks
and unresolved failures plainly.
Say "compiles" when only builds were run; say runtime behavior was verified only
for viewers and interactions actually exercised.

## Publish and maintain the pull request

Do not commit, push, or open a pull request unless requested. When a PR is
requested, follow the repository's `.github\pull_request_template.md`, retain its
sections, and leave unperformed checks unchecked. Create new PRs as drafts and
title a new sample's PR `New Sample: <Canonical title>`. Follow the user's branch
and approval requirements, and inspect the outgoing diff for secrets before
publishing.

Keep the description concise: summarize the change, platform evidence, and
remaining gaps. Include exact validation commands with results when the template
or user requires them; use collapsible detail rather than a full execution
transcript. If the user requests a preview, present the exact branch, base, title,
and body and wait for approval before the first push.

Before updating a PR, read its live state, head revision, title, and body. Preserve
user edits instead of restoring an old proposal or editor artifact. Use the
editing tool's concurrency guard when available. If an edit conflicts, merge only
the requested change into the returned current content and retry with its guard;
do not bypass the conflict with an unguarded overwrite. Do not edit a closed or
merged PR.

After a push, inspect checks for that exact head revision, not an earlier green
commit. Diagnose failures from the failed step's logs; deprecation warnings or a
successful local Python check do not establish the failure's cause. Within the
user-authorized scope, fix only the relevant issue, validate it, and confirm the
resulting CI status when reporting the failure as resolved. Do not change
unrelated workflows merely to silence warnings.

For readiness questions, refresh GitHub state and report failing checks,
conflicts, required reviews, and draft status separately. Green CI does not
replace the platform-validation matrix. Ready for review is not the same as
ready to merge, and a readiness question alone does not authorize changing draft
status or merging.
