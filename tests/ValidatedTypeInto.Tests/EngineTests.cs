using Vodafone.Robotics.ValidatedTypeInto.Engine;
using Vodafone.Robotics.ValidatedTypeInto.Logging;
using Vodafone.Robotics.ValidatedTypeInto.Models;

namespace ValidatedTypeInto.Tests;

public sealed class EngineTests
{
    private static ValidatedTypeIntoConfiguration Config() => new()
    {
        TimeoutL = 30_000,
        TimeoutS = 10_000,
        NumberOfRetries = 0,
        RetryInterval = TimeSpan.Zero,
        ValueToBeWritten = "ABC-123",
        PreConditionSelector = "pre",
        TargetSelector = "target",
        PostConditionSelector = "post",
        PreConditionSuccessMessage = "pre ok",
        PreConditionFailedMessage = "pre failed",
        PostConditionSuccessMessage = "post ok",
        PostConditionFailedMessage = "post failed"
    };

    [Fact]
    public void Success_uses_stage_timeouts_flags_and_supplied_messages()
    {
        var work = new FakeTarget();
        var adapter = new FakeAdapter()
            .Add("pre", new FakeTarget())
            .Add("target", work)
            .Add("post", new FakeTarget { Text = "  ABC-123  " });
        var logger = new RecordingLogger();

        new ValidatedTypeIntoEngine(adapter, logger).Execute(Config() with
        {
            AlterIfDisabled = false,
            ClickBeforeTyping = false,
            EmptyField = false,
            SimulateType = false
        });

        Assert.Equal(("ABC-123", false, false, false, false), work.LastType);
        Assert.Equal(new[] { 30_000, 10_000, 10_000 }, adapter.Calls.Select(c => c.Timeout));
        Assert.Contains((ValidationLogLevel.Information, "pre ok"), logger.Events);
        Assert.Contains((ValidationLogLevel.Information, "post ok"), logger.Events);
    }

    [Fact]
    public void Missing_precondition_logs_and_never_types()
    {
        var work = new FakeTarget();
        var logger = new RecordingLogger();
        var adapter = new FakeAdapter().Add("pre", (Vodafone.Robotics.ValidatedTypeInto.Automation.IUiTarget?)null).Add("target", work);

        var exception = Assert.Throws<ValidatedTypeIntoException>(() =>
            new ValidatedTypeIntoEngine(adapter, logger).Execute(Config()));

        Assert.Equal(ValidationStage.PreCondition, exception.Stage);
        Assert.Equal(0, work.TypeCount);
        Assert.Contains((ValidationLogLevel.Error, "pre failed"), logger.Events);
    }

    [Fact]
    public void Postcondition_is_trimmed_but_case_sensitive()
    {
        var adapter = new FakeAdapter()
            .Add("pre", new FakeTarget())
            .Add("target", new FakeTarget())
            .Add("post", new FakeTarget { Text = "abc-123" });

        var exception = Assert.Throws<ValidatedTypeIntoException>(() =>
            new ValidatedTypeIntoEngine(adapter, new RecordingLogger()).Execute(Config()));

        Assert.Equal(ValidationStage.PostCondition, exception.Stage);
    }

    [Fact]
    public void Complete_sequence_retries_and_then_succeeds()
    {
        var work1 = new FakeTarget();
        var work2 = new FakeTarget();
        var delays = new List<TimeSpan>();
        var adapter = new FakeAdapter()
            .Add("pre", new FakeTarget(), new FakeTarget())
            .Add("target", work1, work2)
            .Add("post", new FakeTarget { Text = "wrong" }, new FakeTarget { Text = "ABC-123" });

        new ValidatedTypeIntoEngine(adapter, new RecordingLogger(), delays.Add)
            .Execute(Config() with { NumberOfRetries = 1, RetryInterval = TimeSpan.FromSeconds(2) });

        Assert.Equal(1, work1.TypeCount);
        Assert.Equal(1, work2.TypeCount);
        Assert.Equal(new[] { TimeSpan.FromSeconds(2) }, delays);
    }

    [Theory]
    [InlineData("value")]
    [InlineData("pre")]
    [InlineData("target")]
    [InlineData("post")]
    public void Blank_required_values_fail_before_UI_access(string field)
    {
        var adapter = new FakeAdapter();
        var config = field switch
        {
            "value" => Config() with { ValueToBeWritten = " " },
            "pre" => Config() with { PreConditionSelector = "" },
            "target" => Config() with { TargetSelector = "" },
            _ => Config() with { PostConditionSelector = "" }
        };

        var exception = Assert.Throws<ValidatedTypeIntoException>(() =>
            new ValidatedTypeIntoEngine(adapter, new RecordingLogger()).Execute(config));

        Assert.Equal(ValidationStage.Configuration, exception.Stage);
        Assert.Empty(adapter.Calls);
    }

    [Fact]
    public void Unicode_value_is_passed_unchanged_and_not_logged()
    {
        const string value = "عميل-١٢٣";
        var work = new FakeTarget();
        var logger = new RecordingLogger();
        var adapter = new FakeAdapter()
            .Add("pre", new FakeTarget())
            .Add("target", work)
            .Add("post", new FakeTarget { Text = value });

        new ValidatedTypeIntoEngine(adapter, logger).Execute(Config() with { ValueToBeWritten = value });

        Assert.Equal(value, work.LastType?.Value);
        Assert.DoesNotContain(logger.Events, item => item.Message.Contains(value, StringComparison.Ordinal));
    }
}
