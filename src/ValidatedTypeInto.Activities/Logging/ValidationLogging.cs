using System.Diagnostics;
using UiPath.Robot.Activities.Api;

namespace Vodafone.Robotics.ValidatedTypeInto.Logging;

public enum ValidationLogLevel { Information, Error }

public interface IValidationLogger
{
    void Log(ValidationLogLevel level, string message);
}

internal sealed class RobotValidationLogger : IValidationLogger
{
    private readonly IExecutorRuntime? _runtime;
    public RobotValidationLogger(IExecutorRuntime? runtime) => _runtime = runtime;

    public void Log(ValidationLogLevel level, string message) => _runtime?.LogMessage(new LogMessage
    {
        EventType = level == ValidationLogLevel.Error ? TraceEventType.Error : TraceEventType.Information,
        Message = $"[Validated Type Into] {message}"
    });
}
