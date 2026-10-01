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
            Log(configuration, ValidationLogLevel.Information, attempt, maxAttempts,
                "Attempt started: validating the target and requested operation.");
            try
            {
                ExecuteAttempt(configuration, attempt, maxAttempts);
                return;
            }
            catch (ValidatedTypeIntoException ex)
            {
                lastFailure = ex;
                if (attempt == maxAttempts)
                {
                    Log(configuration, ValidationLogLevel.Error, attempt, maxAttempts,
                        $"Validation failed after {maxAttempts} attempt(s); final stage: {ex.Stage}.");
                    throw new ValidatedTypeIntoException(
                        $"Validated Type Into failed after {maxAttempts} attempt(s) during {ex.Stage}: {ex.Message}",
                        ex.Stage,
                        attempt,
                        ex);
                }
                Log(configuration, ValidationLogLevel.Information, attempt, maxAttempts,
                    $"Retry scheduled after {ex.Stage} failure; attempt {attempt + 1}/{maxAttempts} will begin in {configuration.RetryInterval.TotalMilliseconds:0} ms.");
                _delay(configuration.RetryInterval);
            }
            catch (Exception ex)
            {
                Log(configuration, ValidationLogLevel.Error, attempt, maxAttempts,
                    $"Unexpected {ex.GetType().Name} encountered; execution cannot continue safely.");
                throw;
            }
        }

        throw lastFailure!;
    }

    private void ExecuteAttempt(ValidatedTypeIntoConfiguration configuration, int attempt, int maxAttempts)
    {
        using (var precondition = _automation.Resolve(configuration.TargetSelector, configuration.TimeoutMilliseconds))
        {
            if (precondition is null)
            {
                const string message = "PreCondition failed: the target element was not found within the configured timeout.";
                Log(configuration, ValidationLogLevel.Error, attempt, maxAttempts, message);
                throw new ValidatedTypeIntoException(message, ValidationStage.PreCondition, attempt);
            }
            Log(configuration, ValidationLogLevel.Information, attempt, maxAttempts,
                "PreCondition passed: the target element exists and is available for the operation.");
        }

        using (var target = _automation.Resolve(configuration.TargetSelector, configuration.TimeoutMilliseconds))
        {
            if (target is null)
            {
                const string message = "DoWork failed: the target element was not found within the configured timeout.";
                Log(configuration, ValidationLogLevel.Error, attempt, maxAttempts, message);
                throw new ValidatedTypeIntoException(message, ValidationStage.DoWork, attempt);
            }
            try
            {
                target.TypeInto(configuration.ValueToBeWritten, configuration.AlterIfDisabled,
                    configuration.ClickBeforeTyping, configuration.EmptyField, configuration.SimulateType);
                var inputMethod = configuration.SimulateType ? "Simulated" : "WindowMessages";
                Log(configuration, ValidationLogLevel.Information, attempt, maxAttempts,
                    $"DoWork completed: Type Into executed with InputMethod={inputMethod}, " +
                    $"ClickBeforeTyping={configuration.ClickBeforeTyping}, EmptyField={configuration.EmptyField}, " +
                    $"AlterIfDisabled={configuration.AlterIfDisabled}.");
            }
            catch (Exception ex)
            {
                const string message = "DoWork failed: the Type Into operation could not be completed.";
                Log(configuration, ValidationLogLevel.Error, attempt, maxAttempts, message);
                throw new ValidatedTypeIntoException(message, ValidationStage.DoWork, attempt, ex);
            }
        }

        using var postcondition = _automation.Resolve(configuration.TargetSelector, configuration.TimeoutMilliseconds);
        var actual = postcondition?.GetText();
        if (postcondition is not null && string.Equals(actual?.Trim(), configuration.ValueToBeWritten.Trim(), StringComparison.Ordinal))
        {
            Log(configuration, ValidationLogLevel.Information, attempt, maxAttempts,
                $"PostCondition passed: the target value matches the expected value ({configuration.ValueToBeWritten.Trim().Length} characters).");
            return;
        }

        var failedMessage = postcondition is null
            ? "PostCondition failed: the target element was not found within the configured timeout."
            : $"PostCondition failed: the target value does not match the expected value " +
              $"(expected length {configuration.ValueToBeWritten.Trim().Length}, actual length {actual?.Trim().Length ?? 0}).";
        Log(configuration, ValidationLogLevel.Error, attempt, maxAttempts, failedMessage);
        throw new ValidatedTypeIntoException(failedMessage, ValidationStage.PostCondition, attempt);
    }

    private static void Validate(ValidatedTypeIntoConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        Require(configuration.ValueToBeWritten, nameof(configuration.ValueToBeWritten));
        Require(configuration.TargetSelector, nameof(configuration.TargetSelector));
        if (configuration.TimeoutMilliseconds < 0) throw Configuration("Timeout cannot be negative.");
        if (configuration.NumberOfRetries < 0) throw Configuration("Number of retries cannot be negative.");
        if (configuration.RetryInterval < TimeSpan.Zero) throw Configuration("Retry interval cannot be negative.");
    }

    private static void Require(string? value, string name)
    {
        if (string.IsNullOrWhiteSpace(value)) throw Configuration($"{name} is required.");
    }

    private static ValidatedTypeIntoException Configuration(string message)
        => new(message, ValidationStage.Configuration, 0);

    private void Log(ValidatedTypeIntoConfiguration configuration, ValidationLogLevel level,
        int attempt, int maxAttempts, string message)
        => _logger.Log(level, $"{configuration.ActivityName} | Attempt {attempt}/{maxAttempts} | {message}");
}
