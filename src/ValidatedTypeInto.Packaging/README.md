# Validated Type Into

This UiPath Windows package contains one activity: **Validated Type Into** under **UI Validation**.

The activity uses one target selector for its precondition existence check, Type Into action, and postcondition read-back. It compares the trimmed read-back with the trimmed requested value using case-sensitive ordinal equality. One timeout applies independently to each stage, and a failed stage retries the complete sequence; `in_intNumberOfRetries` counts additional attempts.

Add the folder containing this package as a UiPath Studio package source, install `Vodafone.Robotics.ValidatedTypeInto.Activities`, and provide `in_strTargetSelector` plus `in_strValueToBeWritten`. Logging is generated automatically and never includes the selector or typed value.
