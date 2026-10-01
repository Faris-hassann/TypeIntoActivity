# Validated Type Into UiPath Library

This repository builds `Vodafone.Robotics.ValidatedTypeInto.Activities.1.0.1.nupkg`, a UiPath Windows package containing exactly one activity: **Validated Type Into**. The previous `1.0.0` package remains available in the local feed for existing workflows.

## Behavior

Each attempt performs the following complete sequence:

1. Confirm that `in_strTargetSelector` exists using `in_intTimeout`.
2. Resolve the same selector again and type `in_strValueToBeWritten` using the four Type Into options.
3. Resolve the same selector again and compare its trimmed text/value with the trimmed requested value using case-sensitive ordinal equality.

Failures are logged, the activity waits for `in_tspRetryInterval`, and the complete sequence is attempted again. `in_intNumberOfRetries` is the number of additional attempts, so the maximum is `1 + in_intNumberOfRetries`. The final failure is thrown with its validation stage.

The activity generates professional UiPath Robot-native logs with the display name, attempt count, validation stage, retry timing, and Type Into settings. It does not require `IAP.Logging`, expose message arguments, or include selector XML or typed text in generated logs.

## Build and test

For the simplest build, double-click `BuildArtifacts.exe`. The launcher builds and tests the complete solution, creates the package under `artifacts/packages`, and opens that folder when successful. It requires the .NET 10 SDK or newer; see [BuildIt.md](BuildIt.md) for exit codes, automation mode, troubleshooting, and launcher rebuild instructions.

For a manual build:

```powershell
dotnet restore ValidatedTypeInto.slnx --configfile NuGet.config
dotnet build ValidatedTypeInto.slnx -c Release --no-restore
dotnet test tests/ValidatedTypeInto.Tests/ValidatedTypeInto.Tests.csproj -c Release --no-build
```

The package is written to `artifacts/packages`.

## Install in UiPath Studio

1. Add the repository's `artifacts/packages` folder under **Manage Packages > Settings**.
2. Install `Vodafone.Robotics.ValidatedTypeInto.Activities` in a modern **Windows** project.
3. Find **Validated Type Into** under **UI Validation**.
4. Supply one complete target selector and the value to write.

## CRM workflow example

For `CRM_SearchPage_EnterCustomerID.xaml`, use the complete Customer ID edit-field selector as `in_strTargetSelector`. Bind the customer ID to `in_strValueToBeWritten`, choose one per-stage `in_intTimeout`, and retain the workflow's retry count and retry interval.

The original XAML is retained unchanged as the behavioral reference.
