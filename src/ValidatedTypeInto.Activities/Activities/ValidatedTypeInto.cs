using System.Activities;
using System.ComponentModel;
using UiPath.Robot.Activities.Api;
using Vodafone.Robotics.ValidatedTypeInto.Automation;
using Vodafone.Robotics.ValidatedTypeInto.Engine;
using Vodafone.Robotics.ValidatedTypeInto.Logging;
using Vodafone.Robotics.ValidatedTypeInto.Models;

namespace Vodafone.Robotics.ValidatedTypeInto.Activities;

[DisplayName("Validated Type Into")]
[Description("Validates a precondition, types a value, and verifies the resulting text with retry and Robot logging.")]
[Category("UI Validation")]
public sealed class ValidatedTypeInto : CodeActivity
{
    public ValidatedTypeInto() => DisplayName = "Validated Type Into";

    [Category("1. General Arguments")]
    [Description("Long selector timeout in milliseconds. Example: 30000.")]
    public InArgument<int> in_intTimeoutL { get; set; } = new(30_000);

    [Category("1. General Arguments")]
    [Description("Short selector timeout in milliseconds. Example: 10000.")]
    public InArgument<int> in_intTimeoutS { get; set; } = new(10_000);

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
    [Category("2. Pre Condition Arguments")]
    [Description("Complete selector whose existence permits Type Into to run.")]
    public InArgument<string> in_strPreConditionSelector { get; set; } = null!;

    [Category("2. Pre Condition Arguments")]
    [Description("Information message logged when the precondition element exists.")]
    public InArgument<string?> in_strPreConditionSuccessMsg { get; set; } = new();

    [Category("2. Pre Condition Arguments")]
    [Description("Error message logged when the precondition element does not exist.")]
    public InArgument<string?> in_strPreConditionFailedMsg { get; set; } = new();

    [RequiredArgument]
    [Category("3. Do Work Arguments")]
    [Description("Complete selector of the element that receives the Type Into action.")]
    public InArgument<string> in_strTargetSelector { get; set; } = null!;

    [Category("3. Do Work Arguments")]
    [Description("Allows an attempt against a disabled target when enabled.")]
    public InArgument<bool> in_boolAlterIfDisabled { get; set; } = new(true);

    [Category("3. Do Work Arguments")]
    [Description("Clicks the target before typing when enabled.")]
    public InArgument<bool> in_boolClickBeforeTyping { get; set; } = new(true);

    [Category("3. Do Work Arguments")]
    [Description("Clears the current field value before typing when enabled.")]
    public InArgument<bool> in_boolEmptyField { get; set; } = new(true);

    [Category("3. Do Work Arguments")]
    [Description("Uses UiPath simulated input when enabled; otherwise uses window messages.")]
    public InArgument<bool> in_boolSimulateType { get; set; } = new(true);

    [RequiredArgument]
    [Category("4. Post Condition Arguments")]
    [Description("Complete selector whose text/value is compared with the value written.")]
    public InArgument<string> in_strPostConditionSelector { get; set; } = null!;

    [Category("4. Post Condition Arguments")]
    [Description("Information message logged when the postcondition value matches.")]
    public InArgument<string?> in_strPostConditionSuccessMsg { get; set; } = new();

    [Category("4. Post Condition Arguments")]
    [Description("Error message logged when the postcondition is missing or mismatched.")]
    public InArgument<string?> in_strPostConditionFailedMsg { get; set; } = new();

    protected override void Execute(CodeActivityContext context)
    {
        var configuration = new ValidatedTypeIntoConfiguration
        {
            TimeoutL = in_intTimeoutL.Get(context),
            TimeoutS = in_intTimeoutS.Get(context),
            NumberOfRetries = in_intNumberOfRetries.Get(context),
            RetryInterval = in_tspRetryInterval.Get(context),
            ValueToBeWritten = in_strValueToBeWritten.Get(context),
            PreConditionSelector = in_strPreConditionSelector.Get(context),
            PreConditionSuccessMessage = in_strPreConditionSuccessMsg.Get(context) ?? string.Empty,
            PreConditionFailedMessage = in_strPreConditionFailedMsg.Get(context) ?? string.Empty,
            TargetSelector = in_strTargetSelector.Get(context),
            AlterIfDisabled = in_boolAlterIfDisabled.Get(context),
            ClickBeforeTyping = in_boolClickBeforeTyping.Get(context),
            EmptyField = in_boolEmptyField.Get(context),
            SimulateType = in_boolSimulateType.Get(context),
            PostConditionSelector = in_strPostConditionSelector.Get(context),
            PostConditionSuccessMessage = in_strPostConditionSuccessMsg.Get(context) ?? string.Empty,
            PostConditionFailedMessage = in_strPostConditionFailedMsg.Get(context) ?? string.Empty
        };

        var logger = new RobotValidationLogger(context.GetExtension<IExecutorRuntime>());
        new ValidatedTypeIntoEngine(ActivityServices.AdapterFactory(), logger).Execute(configuration);
    }

    internal static class ActivityServices
    {
        internal static Func<IUiAutomationAdapter> AdapterFactory { get; set; } = static () => new UiPathAutomationAdapter();
    }
}
