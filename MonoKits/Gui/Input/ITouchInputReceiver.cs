using MonoGame.Extended.Input.InputListeners;

namespace MonoKits.Gui.Input;

public interface ITouchInputReceiver
{
    void OnTouchStarted(TouchEventArgs e) { }

    void OnTouchEnded(TouchEventArgs e) { }

    void OnTouchMoved(TouchEventArgs e) { }

    void OnTouchCancelled(TouchEventArgs e) { }
}
