namespace MonoKits.Gui.Input;

public interface IFocusableElement
{
    bool Focusable { get; }

    void OnGotFocus() { }

    void OnLostFocus() { }
}
