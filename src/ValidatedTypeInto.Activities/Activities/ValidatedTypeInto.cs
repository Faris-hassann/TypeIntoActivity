using System.Activities;
using System.ComponentModel;
using UiPath.Robot.Activities.Api;
using Vodafone.Robotics.ValidatedTypeInto.Automation;
using Vodafone.Robotics.ValidatedTypeInto.Engine;
using Vodafone.Robotics.ValidatedTypeInto.Logging;
using Vodafone.Robotics.ValidatedTypeInto.Models;

namespace Vodafone.Robotics.ValidatedTypeInto.Activities;

[DisplayName("Validated Type Into")]
[Description("Uses one selector to validate availability, type a value, and verify the result with retry and dynamic Robot logging.")]
[Category("UI Validation")]
public sealed class ValidatedTypeInto : CodeActivity
{
    public ValidatedTypeInto() => DisplayName = "Validated Type Into";

    [Category("1. General Arguments")]
    [Description("Per-stage selector timeout in milliseconds for precondition, Type Into, and postcondition. Example: 10000.")]
    public InArgument<int> in_intTimeout { get; set; } = new(10_000);

    [Category("1. General Arguments")]
    [Description("Additional attempts after the first attempt. Example: 2 gives three total attempts.")]
    public InArgument<int> in_intNumberOfRetries { get; set; } = new(2);

    [Category("1. General Arguments")]
    [Description("Delay between attempts. Example: 00:00:01.")]
    public InArgument<TimeSpan> in_tspRetryInterval { get; set; } = new(TimeSpan.FromSeconds(1));

    [RequiredArgument]
    [Category("1. General Arguments")]
    [Description("Text to write and the expected postcondition value.")]
    public InArgument<string> in_strValueToBeWritten { get; set; } = null!;

    [RequiredArgument]
    [Category("2. Do Work Arguments")]
    [Description("Complete selector used for the precondition check, Type Into action, and postcondition read-back.")]
    public InArgument<string> in_strTargetSelector { get; set; } = null!;

    [Category("2. Do Work Arguments")]
    [Description("Allows an attempt against a disabled target when enabled.")]
    public InArgument<bool> in_boolAlterIfDisabled { get; set; } = new(true);

    [Category("2. Do Work Arguments")]
    [Description("Clicks the target before typing when enabled.")]
    public InArgument<bool> in_boolClickBeforeTyping { get; set; } = new(true);

    [Category("2. Do Work Arguments")]
    [Description("Clears the current field value before typing when enabled.")]
    public InArgument<bool> in_boolEmptyField { get; set; } = new(true);

    [Category("2. Do Work Arguments")]
    [Description("Uses UiPath simulated input when enabled; otherwise uses window messages.")]
    public InArgument<bool> in_boolSimulateType { get; set; } = new(true);

    protected override void Execute(CodeActivityContext context)
    {
        var configuration = new ValidatedTypeIntoConfiguration
        {
            ActivityName = DisplayName,
            TimeoutMilliseconds = in_intTimeout.Get(context),
            NumberOfRetries = in_intNumberOfRetries.Get(context),
            RetryInterval = in_tspRetryInterval.Get(context),
            ValueToBeWritten = in_strValueToBeWritten.Get(context),
            TargetSelector = in_strTargetSelector.Get(context),
            AlterIfDisabled = in_boolAlterIfDisabled.Get(context),
            ClickBeforeTyping = in_boolClickBeforeTyping.Get(context),
            EmptyField = in_boolEmptyField.Get(context),
            SimulateType = in_boolSimulateType.Get(context)
        };

        var logger = new RobotValidationLogger(context.GetExtension<IExecutorRuntime>());
        new ValidatedTypeIntoEngine(ActivityServices.AdapterFactory(), logger).Execute(configuration);
    }

    internal static class ActivityServices
    {
        internal static Func<IUiAutomationAdapter> AdapterFactory { get; set; } = static () => new UiPathAutomationAdapter();
    }
}
