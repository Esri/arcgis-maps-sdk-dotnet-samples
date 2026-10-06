---
name: sample-porter
description: Port an ArcGIS Maps SDK sample from Swift, Kotlin, Dart/Flutter, Qt/C++, or JavaScript/TypeScript into this repository's WPF, WinUI, and .NET MAUI viewers. Use when implementing a common-samples design, translating a sibling SDK sample to .NET, or resuming a cross-platform sample port.
---

# Sample porter

Implement a sample in the repository, not just a code snippet. Preserve the
canonical design's API lesson and behavior while following this repository's
generator, metadata, and viewer conventions.

## When to use

Use this skill for a new or resumed cross-SDK sample port targeting WPF, WinUI,
and MAUI. If the user requests fewer platforms, clarify the scope before running
the generator, which creates all three. Do not use this skill for unrelated
viewer refactoring or general ArcGIS application development.

## Inputs

Start with a sample design name or directory. Use a supplied `common-samples`
checkout, source implementation, and source revision when available. Ask for a
missing checkout location or inaccessible design instead of assuming a local
machine path. Derive title, category, formal name, constants, and offline item IDs
from the design; do not ask the user to repeat information already present there.

## Reference routing

Read the relevant reference before its phase. Do not load every reference or
every unrelated example at the start.

| When | Read |
| --- | --- |
| Before source discovery, scaffolding, or resuming edits | [Porting workflow](references/porting-workflow.md) |
| Before writing XAML, C#, and the WPF README | [Platform conventions](references/platform-conventions.md) |
| When choosing a prerelease SDK, synchronizing, building, or auditing | [Validation](references/validation.md) |

Use the live generator, project files, and nearby samples to confirm details
that can change between branches. References document the workflow; they do not
replace inspecting the current checkout.

## Workflow

1. **Preflight and resolve.** Inspect repository status and branch context.
   Establish an external task checkpoint and file-ownership baseline. Resolve the
   design, inventory its files, read its specification, and record source
   revisions. Stop on ambiguous inputs or ownership conflicts.
2. **Research.** Derive the generator inputs and required initial state,
   interactions, and API operations. Read nearby samples on all target platforms
   and verify unfamiliar .NET APIs. Select the SDK before relying on prerelease
   APIs. Do not invent API names or replace the lesson with a different feature.
3. **Generate.** For a new sample, run
   `python tools\sample_generator\samplegen.py -n` with the recorded inputs.
   Require the three five-file baselines, record their hashes, and inspect all
   generator changes. For a resumed port, verify the checkpoint instead of
   generating over existing files.
4. **Implement.** Reconcile WPF first, then WinUI and MAUI with equivalent
   behavior. Adapt the canonical README in WPF, retain generated screenshots,
   and register the new sample in `FeaturedSamples.xml`. Review README-to-code
   semantics, not just metadata strings.
5. **Synchronize.** Snapshot affected files, run `python tools\sample_sync.py`,
   and verify target-sample output and diagnostics. Restore only unrelated
   changes proven to have been caused by that run, preserving pre-existing work.
6. **Validate.** Build the target viewers with the selected SDK. Exercise the
   sample's runtime interactions where the host permits. Classify failures before
   changing source; record unsupported or unavailable validation explicitly.
7. **Audit and report.** Check final platform parity, documentation, metadata,
   registration, screenshot hashes, and the complete diff. Report delivered
   files, validation evidence, intentional deviations, and remaining blockers.

Checkpoint each phase before continuing. A failed phase is not complete: resolve
the cause or report where the port is blocked. Resume from verified evidence
rather than rerunning completed destructive operations.

## Rules that apply throughout

- Preserve user work. Never infer ownership from a sample name, overwrite an
  existing destination without evidence, or perform broad cleanup.
- Use the repository generator for new samples and edit its output in place.
  Do not add per-sample project entries or edit generated sample catalogs.
- Keep the generated placeholder screenshots unchanged, including MAUI's
  lowercase filename. A human replaces them after the port.
- Preserve specified values, data attribution, workflow ordering, initial state,
  and reset behavior. Simplify only incidental UI, not required interactions.
- Distinguish `LocalSceneView` from `SceneView` and the view control from the
  scene's viewing mode. Verify support in the targeted SDK.
- Use the WPF README as the source for synchronized readmes, metadata, and
  `[Sample]` attributes. Check their semantics against the final code.
- Do not persist API keys, credentials, or build-only daily SDK overrides.
  Do not assume access to private repositories or package feeds.
- Keep errors visible and validation claims precise. Compiling is not runtime
  verification; incomplete validation must be reported as such.
- Commit, push, or create a pull request only when the user requests it.
