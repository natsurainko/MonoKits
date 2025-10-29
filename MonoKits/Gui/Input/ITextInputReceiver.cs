using Microsoft.Xna.Framework;

namespace MonoKits.Gui.Input;

public interface ITextInputReceiver
{
    void OnTextInput(TextInputEventArgs eventArgs) { }
}
