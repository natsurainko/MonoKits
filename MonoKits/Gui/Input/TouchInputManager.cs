using MonoGame.Extended.Input.InputListeners;

namespace MonoKits.Gui.Input;

public class TouchInputManager
{
    private readonly TouchListener _touchListener;
    private readonly List<ITouchInputReceiver> _receivers = [];

    public TouchInputManager(TouchListener touchListener)
    {
        _touchListener = touchListener;
        _touchListener.TouchCancelled += OnTouchCancelled;
        _touchListener.TouchStarted += OnTouchStarted;
        _touchListener.TouchEnded += OnTouchEnded;
        _touchListener.TouchMoved += OnTouchMoved;
    }

    public void Register(ITouchInputReceiver receiver)
    {
        if (!_receivers.Contains(receiver))
            _receivers.Add(receiver);
    }

    public void Unregister(ITouchInputReceiver receiver) => _receivers.Remove(receiver);


    private void OnTouchMoved(object? sender, TouchEventArgs e)
    {
        for (int i = 0; i < _receivers.Count; i++)
            _receivers[i].OnTouchMoved(e);
    }

    private void OnTouchEnded(object? sender, TouchEventArgs e)
    {
        for (int i = 0; i < _receivers.Count; i++)
            _receivers[i].OnTouchEnded(e);
    }

    private void OnTouchStarted(object? sender, TouchEventArgs e)
    {
        for (int i = 0; i < _receivers.Count; i++)
            _receivers[i].OnTouchStarted(e);
    }

    private void OnTouchCancelled(object? sender, TouchEventArgs e)
    {
        for (int i = 0; i < _receivers.Count; i++)
            _receivers[i].OnTouchCancelled(e);
    }
}
