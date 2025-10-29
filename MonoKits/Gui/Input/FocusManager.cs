namespace MonoKits.Gui.Input;

public static class FocusManager
{
    private static IFocusableElement? _focusedElement;

    public static IFocusableElement? FocusedElement
    {
        get => _focusedElement;
        private set
        {
            if (_focusedElement == value) return;

            var oldFocus = _focusedElement;
            _focusedElement = value;

            oldFocus?.OnLostFocus();

            if (_focusedElement != null)
            {
                _focusedElement.OnGotFocus();
                GotFocus?.Invoke(null, _focusedElement);
            }
        }
    }

    public static event EventHandler<IFocusableElement>? GotFocus;

    public static bool SetFocus(IFocusableElement? control)
    {
        if (control != null && !control.Focusable)
            return false;

        FocusedElement = control;
        return true;
    }

    public static void ClearFocus() => FocusedElement = null;

    public static bool IsFocused(IFocusableElement control) => _focusedElement == control;
}
