using System.Activities.DesignViewModels;

namespace Vodafone.Robotics.ValidatedTypeInto.ViewModels;

public sealed class ValidatedTypeIntoViewModel : DesignPropertiesViewModel
{
    public DesignInArgument<int> in_intTimeoutL { get; set; } = null!;
    public DesignInArgument<int> in_intTimeoutS { get; set; } = null!;
    public DesignInArgument<int> in_intNumberOfRetries { get; set; } = null!;
    public DesignInArgument<TimeSpan> in_tspRetryInterval { get; set; } = null!;
    public DesignInArgument<string> in_strValueToBeWritten { get; set; } = null!;
    public DesignInArgument<string> in_strPreConditionSelector { get; set; } = null!;
    public DesignInArgument<string?> in_strPreConditionSuccessMsg { get; set; } = null!;
    public DesignInArgument<string?> in_strPreConditionFailedMsg { get; set; } = null!;
    public DesignInArgument<string> in_strTargetSelector { get; set; } = null!;
    public DesignInArgument<bool> in_boolAlterIfDisabled { get; set; } = null!;
    public DesignInArgument<bool> in_boolClickBeforeTyping { get; set; } = null!;
    public DesignInArgument<bool> in_boolEmptyField { get; set; } = null!;
    public DesignInArgument<bool> in_boolSimulateType { get; set; } = null!;
    public DesignInArgument<string> in_strPostConditionSelector { get; set; } = null!;
    public DesignInArgument<string?> in_strPostConditionSuccessMsg { get; set; } = null!;
    public DesignInArgument<string?> in_strPostConditionFailedMsg { get; set; } = null!;

    public ValidatedTypeIntoViewModel(IDesignServices services) : base(services) { }

    protected override void InitializeModel()
    {
        base.InitializeModel();
        var order = 0;
        Configure(in_intTimeoutL, "Long Timeout (ms)", "Maximum time for the precondition selector. Example: 30000.", false, ref order);
        Configure(in_intTimeoutS, "Short Timeout (ms)", "Maximum time for target and postcondition selectors. Example: 10000.", false, ref order);
        Configure(in_intNumberOfRetries, "Number Of Retries", "Additional full attempts after the first. Example: 2.", false, ref order);
        Configure(in_tspRetryInterval, "Retry Interval", "Delay between attempts. Example: 00:00:01.", false, ref order);
        Configure(in_strValueToBeWritten, "Value To Be Written", "Required text to type and verify. Example: customerId.", true, ref order, true);
        Configure(in_strPreConditionSelector, "PreCondition Selector", "Required complete selector checked before typing. Example: <wnd .../><ctrl .../>.", true, ref order);
        Configure(in_strPreConditionSuccessMsg, "PreCondition Success Message", "Information message for a successful precondition. Example: Search page is displayed.", false, ref order);
        Configure(in_strPreConditionFailedMsg, "PreCondition Failed Message", "Error message for a failed precondition. Example: Search page is not displayed.", false, ref order);
        Configure(in_strTargetSelector, "Target Selector", "Required complete selector of the field to type into. Example: <wnd .../><ctrl role='editable text'/>.", true, ref order, true);
        Configure(in_boolAlterIfDisabled, "Alter If Disabled", "Attempt typing when the element reports disabled. Example: True.", false, ref order);
        Configure(in_boolClickBeforeTyping, "Click Before Typing", "Click and focus before entering text. Example: True.", false, ref order);
        Configure(in_boolEmptyField, "Empty Field", "Clear existing text before typing. Example: True.", false, ref order);
        Configure(in_boolSimulateType, "Simulate Type", "Use simulated input instead of window messages. Example: True.", false, ref order);
        Configure(in_strPostConditionSelector, "PostCondition Selector", "Required complete selector used for read-back. Example: the edited field selector.", true, ref order);
        Configure(in_strPostConditionSuccessMsg, "PostCondition Success Message", "Information message for a matching value. Example: Customer ID entered correctly.", false, ref order);
        Configure(in_strPostConditionFailedMsg, "PostCondition Failed Message", "Error message for a missing or mismatched value. Example: Customer ID was not entered correctly.", false, ref order);
        PersistValuesChangedDuringInit();
    }

    private static void Configure<T>(DesignInArgument<T> property, string displayName, string tooltip,
        bool required, ref int order, bool principal = false)
    {
        property.DisplayName = displayName;
        property.Tooltip = tooltip;
        property.IsRequired = required;
        property.IsPrincipal = principal;
        property.OrderIndex = order++;
    }
}
