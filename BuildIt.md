# BuildIt

## One-click build

Double-click `BuildArtifacts.exe` in the repository root. It automatically:

1. Locates the repository and reads the package ID and version.
2. Finds a compatible .NET SDK.
3. Creates `artifacts/packages` when needed.
4. Restores and builds the UiPath package first.
5. Restores and builds the complete solution.
6. Runs the activity and artifact-builder tests.
7. Verifies and opens the generated package folder.

The console remains open so the result can be read. The executable may be moved anywhere inside this repository. It never deletes older package versions.

For automation, run:

```powershell
.\BuildArtifacts.exe --non-interactive
```

Exit code `0` means the complete pipeline succeeded. Other documented exit codes identify invalid arguments, missing repository or SDK, build failure, missing package, invalid configuration, or cancellation.

## Prerequisite

Install the .NET 10 SDK or newer. The launcher itself is a self-contained Windows x64 executable, but compiling the solution requires the SDK. Verify it with:

```powershell
dotnet --info
```

## Manual fallback

```powershell
dotnet restore src/ValidatedTypeInto.Packaging/ValidatedTypeInto.Packaging.csproj --configfile NuGet.config
dotnet build src/ValidatedTypeInto.Packaging/ValidatedTypeInto.Packaging.csproj -c Release --no-restore
dotnet restore ValidatedTypeInto.slnx --configfile NuGet.config
dotnet build ValidatedTypeInto.slnx -c Release --no-restore
dotnet test tests/ValidatedTypeInto.Tests/ValidatedTypeInto.Tests.csproj -c Release --no-build --no-restore
dotnet test tests/ValidatedTypeInto.ArtifactBuilder.Tests/ValidatedTypeInto.ArtifactBuilder.Tests.csproj -c Release --no-build --no-restore
```

The packaging project is built first because the consumer smoke project restores the generated package from `artifacts/packages`.

## Rebuild the launcher

```powershell
dotnet publish tools/ValidatedTypeInto.ArtifactBuilder/ValidatedTypeInto.ArtifactBuilder.csproj -c Release -r win-x64 --self-contained true -o artifacts/builder
Copy-Item artifacts/builder/BuildArtifacts.exe .\BuildArtifacts.exe -Force
```

## UiPath Studio

After a successful build, add `artifacts/packages` as a local source in **Manage Packages > Settings**, then install `Vodafone.Robotics.ValidatedTypeInto.Activities`.
