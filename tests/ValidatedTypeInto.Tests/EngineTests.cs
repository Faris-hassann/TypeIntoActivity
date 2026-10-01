using Vodafone.Robotics.ValidatedTypeInto.Automation;
using Vodafone.Robotics.ValidatedTypeInto.Engine;
using Vodafone.Robotics.ValidatedTypeInto.Logging;
using Vodafone.Robotics.ValidatedTypeInto.Models;

namespace ValidatedTypeInto.Tests;

public sealed class EngineTests
{
    private static ValidatedTypeIntoConfiguration Config() => new()
    {
        ActivityName = "Enter Customer ID",
        TimeoutMilliseconds = 10_000,
        NumberOfRetries = 0,
        RetryInterval = TimeSpan.Zero,
        ValueToBeWritten = "ABC-123",
        TargetSelector = "secret-selector",
        AlterIfDisabled = true,
        ClickBeforeTyping = true,
        EmptyField = true,
        SimulateType = true
    };

    public static IEnumerable<object[]> FlagCombinations()
    {
        foreach (var alter in new[] { false, true })
        foreach (var click in new[] { false, true })
        foreach (var empty in new[] { false, true })
        foreach (var simulate in new[] { false, true })
            yield return new object[] { alter, click, empty, simulate };
    }

    [Fact]
    public void Success_uses_same_selector_and_timeout_for_three_fresh_resolutions()
    {
        var work = new FakeTarget();
        var adapter = new FakeAdapter().Add("secret-selector",
            new FakeTarget(), work, new FakeTarget { Text = "  ABC-123  " });
        var logger = new RecordingLogger();

        new ValidatedTypeIntoEngine(adapter, logger).Execute(Config());

        Assert.Equal(3, adapter.Calls.Count);
        Assert.All(adapter.Calls, call =>
        {
            Assert.Equal("secret-selector", call.Selector);
            Assert.Equal(10_000, call.Timeout);
        });
        Assert.Equal(("ABC-123", true, true, true, true), work.LastType);
        Assert.Contains(logger.Events, item => item.Level == ValidationLogLevel.Information && item.Message.Contains("Attempt 1/1"));
        Assert.Contains(logger.Events, item => item.Message.Contains("PreCondition passed"));
        Assert.Contains(logger.Events, item => item.Message.Contains("DoWork completed"));
        Assert.Contains(logger.Events, item => item.Message.Contains("PostCondition passed"));
    }

    [Theory]
    [MemberData(nameof(FlagCombinations))]
    public void Every_type_into_flag_combination_is_forwarded(bool alter, bool click, bool empty, bool simulate)
    {
        var work = new FakeTarget();
        var adapter = new FakeAdapter().Add("secret-selector",
            new FakeTarget(), work, new FakeTarget { Text = "ABC-123" });

        new ValidatedTypeIntoEngine(adapter, new RecordingLogger()).Execute(Config() with
        {
            AlterIfDisabled = alter,
            ClickBeforeTyping = click,
            EmptyField = empty,
            SimulateType = simulate
        });

        Assert.Equal(("ABC-123", alter, click, empty, simulate), work.LastType);
    }

    [Fact]
    public void Missing_precondition_logs_error_and_never_types()
    {
        var logger = new RecordingLogger();
        var adapter = new FakeAdapter().Add("secret-selector", (IUiTarget?)null);

        var exception = Assert.Throws<ValidatedTypeIntoException>(() =>
            new ValidatedTypeIntoEngine(adapter, logger).Execute(Config()));

        Assert.Equal(ValidationStage.PreCondition, exception.Stage);
        Assert.Contains(logger.Events, item => item.Level == ValidationLogLevel.Error && item.Message.Contains("PreCondition failed"));
        Assert.Single(adapter.Calls);
    }

    [Fact]
    public void Missing_do_work_target_is_classified_and_logged()
    {
        var logger = new RecordingLogger();
        var adapter = new FakeAdapter().Add("secret-selector", new FakeTarget(), null);

        var exception = Assert.Throws<ValidatedTypeIntoException>(() =>
            new ValidatedTypeIntoEngine(adapter, logger).Execute(Config()));

        Assert.Equal(ValidationStage.DoWork, exception.Stage);
        Assert.Contains(logger.Events, item => item.Level == ValidationLogLevel.Error && item.Message.Contains("DoWork failed"));
    }

    [Fact]
    public void Type_into_failure_is_classified_and_logged_without_exception_details()
    {
        var logger = new RecordingLogger();
        var work = new FakeTarget { OnType = (_, _) => throw new InvalidOperationException("secret-selector ABC-123") };
        var adapter = new FakeAdapter().Add("secret-selector", new FakeTarget(), work);

        var exception = Assert.Throws<ValidatedTypeIntoException>(() =>
            new ValidatedTypeIntoEngine(adapter, logger).Execute(Config()));

        Assert.Equal(ValidationStage.DoWork, exception.Stage);
        Assert.DoesNotContain(logger.Events, item => item.Message.Contains("secret-selector") || item.Message.Contains("ABC-123"));
    }

    [Fact]
    public void Postcondition_is_trimmed_but_case_sensitive()
    {
        var adapter = new FakeAdapter().Add("secret-selector",
            new FakeTarget(), new FakeTarget(), new FakeTarget { Text = "abc-123" });

        var exception = Assert.Throws<ValidatedTypeIntoException>(() =>
            new ValidatedTypeIntoEngine(adapter, new RecordingLogger()).Execute(Config()));

        Assert.Equal(ValidationStage.PostCondition, exception.Stage);
    }

    [Fact]
    public void Missing_postcondition_target_is_classified_and_logged()
    {
        var logger = new RecordingLogger();
        var adapter = new FakeAdapter().Add("secret-selector", new FakeTarget(), new FakeTarget(), null);

        var exception = Assert.Throws<ValidatedTypeIntoException>(() =>
            new ValidatedTypeIntoEngine(adapter, logger).Execute(Config()));

        Assert.Equal(ValidationStage.PostCondition, exception.Stage);
        Assert.Contains(logger.Events, item => item.Message.Contains("PostCondition failed"));
    }

    [Fact]
    public void Complete_sequence_retries_and_logs_schedule_then_success()
    {
        var delays = new List<TimeSpan>();
        var logger = new RecordingLogger();
        var adapter = new FakeAdapter().Add("secret-selector",
            new FakeTarget(), new FakeTarget(), new FakeTarget { Text = "wrong" },
            new FakeTarget(), new FakeTarget(), new FakeTarget { Text = "ABC-123" });

        new ValidatedTypeIntoEngine(adapter, logger, delays.Add)
            .Execute(Config() with { NumberOfRetries = 1, RetryInterval = TimeSpan.FromSeconds(2) });

        Assert.Equal(6, adapter.Calls.Count);
        Assert.Equal(new[] { TimeSpan.FromSeconds(2) }, delays);
        Assert.Contains(logger.Events, item => item.Level == ValidationLogLevel.Information &&
            item.Message.Contains("attempt 2/2 will begin in 2000 ms"));
    }

    [Fact]
    public void Retry_exhaustion_logs_final_error_with_stage_and_attempt_count()
    {
        var logger = new RecordingLogger();
        var adapter = new FakeAdapter().Add("secret-selector", (IUiTarget?)null, null, null);

        var exception = Assert.Throws<ValidatedTypeIntoException>(() =>
            new ValidatedTypeIntoEngine(adapter, logger).Execute(Config() with { NumberOfRetries = 2 }));

        Assert.Equal(3, exception.Attempt);
        Assert.Contains(logger.Events, item => item.Level == ValidationLogLevel.Error &&
            item.Message.Contains("failed after 3 attempt(s)") && item.Message.Contains("PreCondition"));
    }

    [Theory]
    [InlineData("value")]
    [InlineData("target")]
    public void Blank_required_values_fail_before_UI_access(string field)
    {
        var adapter = new FakeAdapter();
        var config = field == "value"
            ? Config() with { ValueToBeWritten = " " }
            : Config() with { TargetSelector = "" };

        var exception = Assert.Throws<ValidatedTypeIntoException>(() =>
            new ValidatedTypeIntoEngine(adapter, new RecordingLogger()).Execute(config));

        Assert.Equal(ValidationStage.Configuration, exception.Stage);
        Assert.Empty(adapter.Calls);
    }

    [Fact]
    public void Unicode_value_and_selector_are_never_logged()
    {
        const string value = "عميل-١٢٣";
        const string selector = "<wnd title='خاص'/><ctrl id='secret'/>";
        var logger = new RecordingLogger();
        var adapter = new FakeAdapter().Add(selector,
            new FakeTarget(), new FakeTarget(), new FakeTarget { Text = value });

        new ValidatedTypeIntoEngine(adapter, logger).Execute(Config() with
        {
            ValueToBeWritten = value,
            TargetSelector = selector
        });

        Assert.DoesNotContain(logger.Events, item =>
            item.Message.Contains(value, StringComparison.Ordinal) || item.Message.Contains(selector, StringComparison.Ordinal));
        Assert.Contains(logger.Events, item => item.Message.Contains($"{value.Length} characters"));
    }
}
