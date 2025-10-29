using MonoKits.Overrides;

namespace MonoKits.Gui.Input;

public interface IKeyboardInputReceiver
{
    //void OnKeyTyped(KeyboardEventArgs e) { }

    void OnKeyReleased(in KeyboardEventArgs e) { }

    void OnKeyPressed(in KeyboardEventArgs e) { }
}