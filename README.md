# Validated Type Into UiPath Library

This repository builds `Vodafone.Robotics.ValidatedTypeInto.Activities.1.0.0.nupkg`, a UiPath Windows package containing exactly one activity: **Validated Type Into**.

## Behavior

Each attempt performs the following complete sequence:

1. Confirm that `in_strPreConditionSelector` exists using `in_intTimeoutL`.
2. Type `in_strValueToBeWritten` into `in_strTargetSelector` using `in_intTimeoutS` and the four Type Into options.
3. Read `in_strPostConditionSelector` using `in_intTimeoutS` and compare its trimmed text/value with the trimmed requested value using case-sensitive ordinal equality.

Failures are logged, the activity waits for `in_tspRetryInterval`, and the complete sequence is attempted again. `in_intNumberOfRetries` is the number of additional attempts, so the maximum is `1 + in_intNumberOfRetries`. The final failure is thrown with its validation stage.

The activity uses UiPath Robot-native logging and does not require `IAP.Logging`. Blank message arguments receive safe default messages, and the typed value is never included in library-generated logs.

## Build and test

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
4. Supply complete precondition, target, and postcondition selectors and the value to write.

## CRM workflow example

For `CRM_SearchPage_EnterCustomerID.xaml`, use the search-page element as the precondition, the Customer ID edit field as the target, and the same Customer ID field as the postcondition. Bind the customer ID to `in_strValueToBeWritten`, use the workflow's long/short timeout variables, retry count, and retry interval, and supply the workflow's pre/post success and failure messages.

The original XAML is retained unchanged as the behavioral reference.
