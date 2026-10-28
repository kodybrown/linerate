# LineRate Agent Notes

These instructions apply to every task in this repository.

## Project Identity

- Project name: `LineRate`
- Project type: `.NET console CLI`
- Base directory: `~\Projects\linerate`
- Integration checkout: `~\Projects\linerate`
- Task worktree root: `~\Projects\linerate\worktrees`
- Integration branch: `main`
- Task branch prefix: `feature/`, `bugfix/`, `hotfix/`, or `release/`,
  according to the work. Use descriptive names such as `feature/<short-name>`.
- Do not include `codex` or `ai` identifiers anywhere in new branch names.
- Worktree mode: `direct-local-unless-requested`
- Root Markdown policy: `canonical-restricted`

<!-- powercode-agent-modules:start -->
## Shared Agent Modules

At task start, after reading this root file and before inspecting or changing
other repository files, read these files once in order:

1. `AGENTS.Workflow.md`
2. `AGENTS.CodeStyle.md`
3. `AGENTS.Documentation.md`
4. `AGENTS.BuildArtifacts.md`
5. `AGENTS.DotNetVerification.md`

Reuse the loaded modules on follow-up prompts in the same task. Re-read them
only after the checkout, branch, module selection, or module contents change,
or after an interrupted module load.

This root `AGENTS.md` takes precedence over shared modules. If a selected
module is missing or unreadable, stop before other repository work and report
the problem.
<!-- powercode-agent-modules:end -->

## Project Workflow

- The integration checkout is user-owned. Preserve unrelated changes and stop
  when they overlap the task or make the task base ambiguous.
- This repository normally allows direct local edits unless the user requests
  an isolated worktree.
- Commit only after explicit user approval.
- Do not push, publish releases, or change GitHub repository state unless the
  user explicitly requests it.

## Read Documentation Conditionally

- Read `docs/WORKFLOW.md` when changing repository workflow, verification,
  release, or PowerCode synchronization guidance.
- Read `README.md` when changing command-line behavior or user-facing examples.

## Project Documentation

- This repository uses the `canonical-restricted` root Markdown layout from
  `AGENTS.Documentation.md`, with `LICENSE` as the existing license filename.
- Authoritative user documentation lives in `README.md`.
- Repository workflow documentation lives in `docs/WORKFLOW.md`.
- The docs index lives in `docs/README.md`.
- Root `AGENTS.*.md` modules are synchronized PowerCode policy files and should
  not be edited independently.

## Project Constraints

- `linerate` is a pipe-friendly CLI and should remain usable in stdin/stdout
  pipelines.
- Preserve quiet pipe-filter behavior when downstream output closes.
- The embedded `src\LineRate\PowerCode` files are copied or adapted from
  `~\Projects\PowerCode\local\src\PowerCode`.
- Keep LineRate-specific CLI behavior, including `-multiplier [decimal]`, when
  synchronizing the embedded PowerCode argument binder.

## Project Verification

- Documentation-only changes: `git diff --check`
- Source or tooling changes: `dotnet build LineRate.slnx`
- Format verification: `dotnet format -v d LineRate.slnx --verify-no-changes`
- CLI smoke: `dotnet run --project src\LineRate\LineRate.csproj -- -help`
- Filter smoke: `@('ERROR one','OK') | dotnet run --project src\LineRate\LineRate.csproj -- -tail -filter ERROR -multiplier 2.5`
- All changes: `git diff --check`

## Canonical Source

PowerCode's canonical source is
`~\Projects\PowerCode\local`. The root `AGENTS.md` is an adapted
project file; selected `AGENTS.*.md` files are exact synchronized copies.
Audit synchronized modules against PowerCode before replacing anything.
