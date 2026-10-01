using Vodafone.Robotics.ValidatedTypeInto.Automation;
using Vodafone.Robotics.ValidatedTypeInto.Logging;

namespace ValidatedTypeInto.Tests;

internal sealed class FakeAdapter : IUiAutomationAdapter
{
    private readonly Dictionary<string, Queue<IUiTarget?>> _targets = new(StringComparer.Ordinal);
    public List<(string Selector, int Timeout)> Calls { get; } = new();

    public FakeAdapter Add(string selector, params IUiTarget?[] targets)
    {
        _targets[selector] = new Queue<IUiTarget?>(targets);
        return this;
    }

    public IUiTarget? Resolve(string selector, int timeoutMilliseconds)
    {
        Calls.Add((selector, timeoutMilliseconds));
        if (!_targets.TryGetValue(selector, out var queue) || queue.Count == 0) return null;
        return queue.Count == 1 ? queue.Peek() : queue.Dequeue();
    }
}

internal sealed class FakeTarget : IUiTarget
{
    public bool IsEnabled { get; set; } = true;
    public string? Text { get; set; }
    public int TypeCount { get; private set; }
    public (string Value, bool Alter, bool Click, bool Empty, bool Simulate)? LastType { get; private set; }
    public Action<FakeTarget, string>? OnType { get; set; }

    public string? GetText() => Text;

    public void TypeInto(string value, bool alterIfDisabled, bool clickBeforeTyping, bool emptyField, bool simulateType)
    {
        TypeCount++;
        LastType = (value, alterIfDisabled, clickBeforeTyping, emptyField, simulateType);
        if (!alterIfDisabled && !IsEnabled) throw new InvalidOperationException("disabled");
        if (OnType is not null) OnType(this, value);
        else Text = emptyField ? value : (Text ?? string.Empty) + value;
    }

    public void Dispose() { }
}

internal sealed class RecordingLogger : IValidationLogger
{
    public List<(ValidationLogLevel Level, string Message)> Events { get; } = new();
    public void Log(ValidationLogLevel level, string message) => Events.Add((level, message));
}
