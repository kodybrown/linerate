# Shared Agent Module: Build Artifacts

<!-- powercode-agent-module:build-artifacts:v2026-08-24 -->

- The user-level `~/Directory.Build.props` and `~/Directory.Build.targets` own
  .NET output locations. Do not replace, bypass, or override that policy.
- Keep generated `bin`, `obj`, and `publish` output outside source trees under
  `C:\tmp\_dotnet` on Windows and `/tmp/_dotnet` on Linux and macOS.
- Repository-level `Directory.Build.props` files must import the user-level
  props before defining other settings and must not redirect output into source.
- Makefiles and scripts must use the evaluated MSBuild `PublishDir`;
  `install-user` may copy from there into `~/Bin/apps/<Project>`.
- Verify the evaluated `BaseOutputPath`, `BaseIntermediateOutputPath`, and
  `PublishDir` after build-tooling changes. Never add a source-local override
  to work around sandboxing, locks, or tooling.
