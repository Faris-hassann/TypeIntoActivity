namespace Vodafone.Robotics.ValidatedTypeInto.Models;

public sealed record ValidatedTypeIntoConfiguration
{
    public int TimeoutL { get; init; } = 30_000;
    public int TimeoutS { get; init; } = 10_000;
    public int NumberOfRetries { get; init; } = 2;
    public TimeSpan RetryInterval { get; init; } = TimeSpan.FromSeconds(1);
    public string ValueToBeWritten { get; init; } = string.Empty;
    public string PreConditionSelector { get; init; } = string.Empty;
    public string PreConditionSuccessMessage { get; init; } = string.Empty;
    public string PreConditionFailedMessage { get; init; } = string.Empty;
    public string TargetSelector { get; init; } = string.Empty;
    public bool AlterIfDisabled { get; init; } = true;
    public bool ClickBeforeTyping { get; init; } = true;
    public bool EmptyField { get; init; } = true;
    public bool SimulateType { get; init; } = true;
    public string PostConditionSelector { get; init; } = string.Empty;
    public string PostConditionSuccessMessage { get; init; } = string.Empty;
    public string PostConditionFailedMessage { get; init; } = string.Empty;
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
