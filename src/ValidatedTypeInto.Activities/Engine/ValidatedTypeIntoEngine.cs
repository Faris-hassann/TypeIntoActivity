using Vodafone.Robotics.ValidatedTypeInto.Automation;
using Vodafone.Robotics.ValidatedTypeInto.Logging;
using Vodafone.Robotics.ValidatedTypeInto.Models;

namespace Vodafone.Robotics.ValidatedTypeInto.Engine;

public sealed class ValidatedTypeIntoEngine
{
    private readonly IUiAutomationAdapter _automation;
    private readonly IValidationLogger _logger;
    private readonly Action<TimeSpan> _delay;

    public ValidatedTypeIntoEngine(IUiAutomationAdapter automation, IValidationLogger logger, Action<TimeSpan>? delay = null)
    {
        _automation = automation;
        _logger = logger;
        _delay = delay ?? Thread.Sleep;
    }

    public void Execute(ValidatedTypeIntoConfiguration configuration)
    {
        Validate(configuration);
        var maxAttempts = checked(configuration.NumberOfRetries + 1);
        Exception? lastFailure = null;

        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                ExecuteAttempt(configuration, attempt);
                return;
            }
            catch (ValidatedTypeIntoException ex)
            {
                lastFailure = ex;
                if (attempt == maxAttempts)
                    throw new ValidatedTypeIntoException(
                        $"Validated Type Into failed after {maxAttempts} attempt(s) during {ex.Stage}: {ex.Message}",
                        ex.Stage,
                        attempt,
                        ex);
                _delay(configuration.RetryInterval);
            }
            catch (Exception ex)
            {
                _logger.Log(ValidationLogLevel.Error, $"Unexpected failure on attempt {attempt}: {ex.Message}");
                throw;
            }
        }

        throw lastFailure!;
    }

    private void ExecuteAttempt(ValidatedTypeIntoConfiguration configuration, int attempt)
    {
        using (var precondition = _automation.Resolve(configuration.PreConditionSelector, configuration.TimeoutL))
        {
            if (precondition is null)
            {
                var message = MessageOrDefault(configuration.PreConditionFailedMessage, "Precondition failed: the required element was not found.");
                _logger.Log(ValidationLogLevel.Error, message);
                throw new ValidatedTypeIntoException(message, ValidationStage.PreCondition, attempt);
            }
            _logger.Log(ValidationLogLevel.Information,
                MessageOrDefault(configuration.PreConditionSuccessMessage, "Precondition succeeded: the required element exists."));
        }

        using (var target = _automation.Resolve(configuration.TargetSelector, configuration.TimeoutS))
        {
            if (target is null)
                throw new ValidatedTypeIntoException("The Type Into target was not found.", ValidationStage.DoWork, attempt);
            try
            {
                target.TypeInto(configuration.ValueToBeWritten, configuration.AlterIfDisabled,
                    configuration.ClickBeforeTyping, configuration.EmptyField, configuration.SimulateType);
            }
            catch (Exception ex)
            {
                throw new ValidatedTypeIntoException("The Type Into action failed.", ValidationStage.DoWork, attempt, ex);
            }
        }

        using var postcondition = _automation.Resolve(configuration.PostConditionSelector, configuration.TimeoutS);
        var actual = postcondition?.GetText();
        if (postcondition is not null && string.Equals(actual?.Trim(), configuration.ValueToBeWritten.Trim(), StringComparison.Ordinal))
        {
            _logger.Log(ValidationLogLevel.Information,
                MessageOrDefault(configuration.PostConditionSuccessMessage, "Postcondition succeeded: the written value was verified."));
            return;
        }

        var failedMessage = MessageOrDefault(configuration.PostConditionFailedMessage,
            postcondition is null ? "Postcondition failed: the verification element was not found." : "Postcondition failed: the value does not match.");
        _logger.Log(ValidationLogLevel.Error, failedMessage);
        throw new ValidatedTypeIntoException(failedMessage, ValidationStage.PostCondition, attempt);
    }

    private static void Validate(ValidatedTypeIntoConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        Require(configuration.ValueToBeWritten, nameof(configuration.ValueToBeWritten));
        Require(configuration.PreConditionSelector, nameof(configuration.PreConditionSelector));
        Require(configuration.TargetSelector, nameof(configuration.TargetSelector));
        Require(configuration.PostConditionSelector, nameof(configuration.PostConditionSelector));
        if (configuration.TimeoutL < 0) throw Configuration("Long timeout cannot be negative.");
        if (configuration.TimeoutS < 0) throw Configuration("Short timeout cannot be negative.");
        if (configuration.NumberOfRetries < 0) throw Configuration("Number of retries cannot be negative.");
        if (configuration.RetryInterval < TimeSpan.Zero) throw Configuration("Retry interval cannot be negative.");
    }

    private static void Require(string? value, string name)
    {
        if (string.IsNullOrWhiteSpace(value)) throw Configuration($"{name} is required.");
    }

    private static ValidatedTypeIntoException Configuration(string message)
        => new(message, ValidationStage.Configuration, 0);

    private static string MessageOrDefault(string? supplied, string fallback)
        => string.IsNullOrWhiteSpace(supplied) ? fallback : supplied;
}
