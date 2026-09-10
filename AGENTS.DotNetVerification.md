# Shared Agent Module: .NET Verification

<!-- powercode-agent-module:dotnet-verification:v2026-08-24 -->

- Treat the verification commands declared in the root `AGENTS.md` as the
  authoritative repository interface. Use `make help` when the root Makefile
  is the command reference.
- Run focused tests first when they provide faster feedback, then run the
  applicable documentation-only or source/tooling gate declared by the root
  file.
- Do not bypass a repository wrapper by invoking `dotnet test` directly when
  the project requires tests to run through Make or another coordinator.
- Run `git diff --check` for every change unless the root file declares a
  stricter repository gate that already includes it.
- After build-tooling changes, verify evaluated output paths as required by
  `AGENTS.BuildArtifacts.md`.
- Report the exact commands that ran, what passed, and anything that remains
  unverified. Do not represent interactive, native, deployment, or publishing
  acceptance as automated verification.
