# Validated Type Into

This UiPath Windows package contains one activity: **Validated Type Into** under **UI Validation**.

The activity checks a precondition selector, types into a separate target selector, reads a postcondition selector, and compares the trimmed read-back with the trimmed requested value using case-sensitive ordinal equality. A failed stage retries the complete sequence; `in_intNumberOfRetries` counts additional attempts.

Add the folder containing this package as a UiPath Studio package source, install `Vodafone.Robotics.ValidatedTypeInto.Activities`, and provide all three complete selectors plus `in_strValueToBeWritten`.
