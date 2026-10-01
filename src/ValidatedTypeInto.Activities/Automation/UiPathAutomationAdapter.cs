using UiPath.Core;

namespace Vodafone.Robotics.ValidatedTypeInto.Automation;

internal sealed class UiPathAutomationAdapter : IUiAutomationAdapter
{
    public IUiTarget? Resolve(string selector, int timeoutMilliseconds)
    {
        try
        {
            return new UiPathTarget(new UiElement(new Selector(selector), timeoutMilliseconds));
        }
        catch (SelectorNotFoundException)
        {
            return null;
        }
        catch (FindElementException)
        {
            return null;
        }
    }

    private sealed class UiPathTarget : IUiTarget
    {
        private readonly UiElement _element;

        public UiPathTarget(UiElement element) => _element = element;

        public bool IsEnabled
        {
            get
            {
                try
                {
                    var raw = _element.Get("enabled", true);
                    return raw switch
                    {
                        bool value => value,
                        string value when bool.TryParse(value, out var parsed) => parsed,
                        _ => true
                    };
                }
                catch (ElementOperationException)
                {
                    return true;
                }
            }
        }

        public string? GetText()
        {
            string? attributeText = null;
            try
            {
                attributeText = _element.Get("text", true)?.ToString();
                if (!string.IsNullOrEmpty(attributeText)) return attributeText;
            }
            catch (ElementOperationException) { }

            try { return _element.Get("value", true)?.ToString() ?? attributeText; }
            catch (ElementOperationException) { return attributeText; }
        }

        public void TypeInto(string value, bool alterIfDisabled, bool clickBeforeTyping, bool emptyField, bool simulateType)
        {
            if (!alterIfDisabled && !IsEnabled)
                throw new InvalidOperationException("The target element is disabled and AlterIfDisabled is false.");

            var inputMethod = simulateType ? InputMethod.API : InputMethod.WINDOW_MESSAGES;
            _element.SetFocus();
            if (clickBeforeTyping)
                _element.Click(ClickType.CLICK_SINGLE, MouseButton.BTN_LEFT, inputMethod);
            if (emptyField)
                _element.WriteText("[k(end)d(shift)k(home)u(shift)k(del)]", inputMethod);
            if (value.Length > 0)
                _element.WriteText(value, inputMethod);
        }

        public void Dispose() => _element.Dispose();
    }
}
