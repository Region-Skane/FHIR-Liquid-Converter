# Copilot Instructions (C#/.NET)

These instructions apply to all code changes in this repository. Configuration is centralized in `/Directory.Build.props`, `/stylecop.json`, and `/CustomAnalysisRules.ruleset`.

## Language
- Use **English** in:
  - code comments
  - XML documentation summaries (`/// <summary>...</summary>`)
  - exception messages
  - log messages
- Prefer clear, concise sentences.

## Default Project Settings
All projects in this repository use the following default settings (enforced via `Directory.Build.props`):

```xml
<PropertyGroup>
  <TargetFramework>net8.0</TargetFramework>
  <ImplicitUsings>enable</ImplicitUsings>
  <Nullable>disable</Nullable>
  <Company>Service Well AB</Company>
  <Product>Service Well Health</Product>
  <PackageLicenseExpression>Apache-2.0</PackageLicenseExpression>
</PropertyGroup>
```

These settings ensure consistency across all projects and should be maintained in new projects created within this repository.

Do **not** introduce features that require a higher target framework unless explicitly requested.

## StyleCop and Code Analysis
All projects use **StyleCop.Analyzers (v1.1.118)** with custom analysis rules:

- **StyleCop configuration**: `stylecop.json`
  - XML headers are optional (`xmlHeader: false`)
  - System using directives come first (`systemUsingDirectivesFirst: true`)
  - Using directives placed outside namespace (`usingDirectivesPlacement: outsideNamespace`)
  - 4-space indentation (no tabs)

- **Code analysis ruleset**: `CustomAnalysisRules.ruleset`
  - Enforced at build time for all projects
  - Do not suppress analyzer warnings without justification

## C# coding conventions
- Never use **Tuples** (`(a, b)`, `ValueTuple`, returning tuples, deconstruction).
  - Use a dedicated type (record/class) instead.
  - Use `TryXxx` patterns (`bool Try...`) with `out` parameters when appropriate.
- Keep code readable and consistent with existing patterns in the repo.
- Prefer explicit types when clarity improves (implicit usings are enabled, but still keep readability high).
- Follow StyleCop rules automatically enforced by analyzers.

## Changes & output expectations
When making changes:
- Keep diffs minimal and consistent with surrounding code style.
- Update or add tests where it makes sense (follow existing test framework/patterns).
- Do not reformat unrelated files.
- Do not suppress StyleCop or code analysis warnings without explaining the reason.
- If a requested change conflicts with these instructions, explain the conflict and propose an alternative that follows the rules.

## Build & fix policy (mandatory)
After making code changes:
- Always run a build/compile check (or equivalent validation) to verify no analyzer warnings or errors.
- Ensure StyleCop violations are resolved.
- If the change affects multiple files, ensure all references compile.
- If you introduce new types/methods, update all call sites.
- If a build error is likely, proactively fix it before finishing.

Before you finish:
- Summarize what changed
- Confirm that the solution compiles without warnings or StyleCop violations
- If compilation cannot be verified, clearly say so and list what to check