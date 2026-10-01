namespace Vodafone.Robotics.ValidatedTypeInto.Models;

public sealed record ValidatedTypeIntoConfiguration
{
    public string ActivityName { get; init; } = "Validated Type Into";
    public int TimeoutMilliseconds { get; init; } = 10_000;
    public int NumberOfRetries { get; init; } = 2;
    public TimeSpan RetryInterval { get; init; } = TimeSpan.FromSeconds(1);
    public string ValueToBeWritten { get; init; } = string.Empty;
    public string TargetSelector { get; init; } = string.Empty;
    public bool AlterIfDisabled { get; init; } = true;
    public bool ClickBeforeTyping { get; init; } = true;
    public bool EmptyField { get; init; } = true;
    public bool SimulateType { get; init; } = true;
}

public enum ValidationStage
{
    Configuration,
    PreCondition,
    DoWork,
    PostCondition
}

public sealed class ValidatedTypeIntoException : Exception
{
    public ValidationStage Stage { get; }
    public int Attempt { get; }

    public ValidatedTypeIntoException(string message, ValidationStage stage, int attempt, Exception? innerException = null)
        : base(message, innerException)
    {
        Stage = stage;
        Attempt = attempt;
    }
}
