# Sample tools

The following tools help manage the sample content for the ArcGIS Maps SDK for .NET:

* [Metadata tools](metadata_tools/readme.md) - tools for managing sample readmes and metadata.
* [Sample generator](sample_generator/readme.md) - adds all the needed files and csproj entries for a new sample, accepting parameters for title, description, formal name, and other properties.
* [Program increment](program_increment.py) - a tool to automate branch creation during program increments.

## Sample porter skill

The repository includes a [sample-porter skill](../.github/skills/sample-porter/SKILL.md)
for porting ArcGIS Maps SDK samples from Swift, Kotlin, Dart/Flutter, Qt/C++, or
JavaScript/TypeScript into WPF, WinUI, and .NET MAUI. In a Copilot client that
supports repository skills, ask it to use `sample-porter` and provide the design:

```text
Use sample-porter to port designs\<FormalName> into the .NET sample viewers.
The common-samples checkout is <absolute path to the checkout>.
```

You can instead provide an explicit design directory. A checkout location already
present in task context, or the optional `COMMON_SAMPLES_ROOT` environment
variable, also avoids repeating the path. The design README and implementation
details supply the title, category, behavior, and other inputs.

For clients supporting custom agents, select the
[sample-porter agent](../.github/agents/sample-porter.agent.md). It reads the same
skill rather than maintaining a second copy of the porting instructions. Neither
entry point requires a personal agent installation, a particular model, or a
specific MCP server. Source-repository access and any required package-feed
credentials must already be available; the skill does not supply them.

The workflow generates the sample scaffold, implements all three viewers,
synchronizes readmes and metadata, and records build/runtime validation. It
preserves existing work and generated placeholder screenshots. SDK versions and
target frameworks come from the current checkout; any required daily SDK is a
build-only override. Publishing changes requires an explicit request.

### Maintaining the skill

Keep discovery metadata, workflow order, and shared safeguards in `SKILL.md`.
Put detailed procedures and conditional guidance in its directly linked
`references` files, and keep the custom agent as a thin entry point. Update the
references when the generator, synchronization scripts, or viewer conventions
change; do not duplicate their templates in the skill.

This organization follows the [Agent Skills specification](https://agentskills.io/specification)
and patterns used in
[VS Code's policy skill](https://github.com/microsoft/vscode/tree/main/.github/skills/policy-and-managed-settings),
[Awesome Copilot's agent architecture skill](https://github.com/github/awesome-copilot/tree/main/skills/agent-architecture),
and [Vercel's React best practices skill](https://github.com/vercel-labs/agent-skills/tree/main/skills/react-best-practices):
a focused entry point with task-specific references loaded when needed.
