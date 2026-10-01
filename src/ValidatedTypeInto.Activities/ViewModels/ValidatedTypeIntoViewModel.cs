using System.Activities.DesignViewModels;

namespace Vodafone.Robotics.ValidatedTypeInto.ViewModels;

public sealed class ValidatedTypeIntoViewModel : DesignPropertiesViewModel
{
    public DesignInArgument<int> in_intTimeout { get; set; } = null!;
    public DesignInArgument<int> in_intNumberOfRetries { get; set; } = null!;
    public DesignInArgument<TimeSpan> in_tspRetryInterval { get; set; } = null!;
    public DesignInArgument<string> in_strValueToBeWritten { get; set; } = null!;
    public DesignInArgument<string> in_strTargetSelector { get; set; } = null!;
    public DesignInArgument<bool> in_boolAlterIfDisabled { get; set; } = null!;
    public DesignInArgument<bool> in_boolClickBeforeTyping { get; set; } = null!;
    public DesignInArgument<bool> in_boolEmptyField { get; set; } = null!;
    public DesignInArgument<bool> in_boolSimulateType { get; set; } = null!;

    public ValidatedTypeIntoViewModel(IDesignServices services) : base(services) { }

    protected override void InitializeModel()
    {
        base.InitializeModel();
        var order = 0;
        Configure(in_intTimeout, "Timeout (ms)", "Per-stage timeout used independently for the precondition, Type Into target, and postcondition read-back. Example: 10000.", false, ref order);
        Configure(in_intNumberOfRetries, "Number Of Retries", "Additional full attempts after the first. Example: 2.", false, ref order);
        Configure(in_tspRetryInterval, "Retry Interval", "Delay between attempts. Example: 00:00:01.", false, ref order);
        Configure(in_strValueToBeWritten, "Value To Be Written", "Required text to type and verify. Example: customerId.", true, ref order, true);
        Configure(in_strTargetSelector, "Target Selector", "Required complete selector used for precondition existence, Type Into, and postcondition read-back. Example: <wnd .../><ctrl role='editable text'/>.", true, ref order, true);
        Configure(in_boolAlterIfDisabled, "Alter If Disabled", "Attempt typing when the element reports disabled. Example: True.", false, ref order);
        Configure(in_boolClickBeforeTyping, "Click Before Typing", "Click and focus before entering text. Example: True.", false, ref order);
        Configure(in_boolEmptyField, "Empty Field", "Clear existing text before typing. Example: True.", false, ref order);
        Configure(in_boolSimulateType, "Simulate Type", "Use simulated input instead of window messages. Example: True.", false, ref order);
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
