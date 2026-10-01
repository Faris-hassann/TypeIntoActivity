namespace Vodafone.Robotics.ValidatedTypeInto.Automation;

public interface IUiAutomationAdapter
{
    IUiTarget? Resolve(string selector, int timeoutMilliseconds);
}

public interface IUiTarget : IDisposable
{
    bool IsEnabled { get; }
    string? GetText();
    void TypeInto(string value, bool alterIfDisabled, bool clickBeforeTyping, bool emptyField, bool simulateType);
}
