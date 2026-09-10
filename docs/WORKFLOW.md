# LineRate Workflow

LineRate is a small .NET console CLI in a direct local checkout at
`~\Projects\linerate`.

## Verification

Use these checks after source or tooling changes:

```powershell
dotnet build LineRate.slnx
dotnet format -v d LineRate.slnx --verify-no-changes
git diff --check
dotnet run --project src\LineRate\LineRate.csproj -- -help
@('ERROR one','OK') | dotnet run --project src\LineRate\LineRate.csproj -- -tail -filter ERROR -multiplier 2.5
```

`dotnet test LineRate.slnx` currently performs restore only because there is no
test project in the solution.

## PowerCode Synchronization

The embedded files under `src\LineRate\PowerCode` come from
`~\Projects\PowerCode\local\src\PowerCode`.

The current embedded argument binder intentionally differs from canonical
PowerCode by keeping `double` binding support for LineRate's
`-multiplier [decimal]` option. Preserve that behavior when refreshing the
embedded binder.

`CODEFORMAT.md` is intentionally absent. Formatting guidance is owned by
`.editorconfig`, `AGENTS.CodeStyle.md`, and nearby code.
