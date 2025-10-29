using MonoKits.Overrides;
using KeyboardListener = MonoKits.Overrides.KeyboardListener;

namespace MonoKits.Gui.Input;

public class KeyboardInputManager
{
    private readonly KeyboardListener _keyboardListener;

    private readonly List<IKeyboardInputReceiver> _receivers = [];
    private readonly List<IFocusableElement> _focusedReceivers = [];

    public KeyboardInputManager(KeyboardListener keyboardListener)
    {
        _keyboardListener = keyboardListener;
        _keyboardListener.KeyPressed += OnKeyPressed;
        _keyboardListener.KeyReleased += OnKeyReleased;
        //_keyboardListener.KeyTyped += OnKeyTyped;
    }

    public void Register(IKeyboardInputReceiver receiver)
    {
        if (!_receivers.Contains(receiver))
            _receivers.Add(receiver);
    }

    public void RegisterOnFocused<TElement>(TElement receiver) where TElement : IKeyboardInputReceiver, IFocusableElement
    {
        if (!_focusedReceivers.Contains(receiver))
            _focusedReceivers.Add(receiver);
    }

    public void Unregister(IKeyboardInputReceiver receiver) => _receivers.Remove(receiver);

    public void Unregister<TElement>(TElement receiver) where TElement : IKeyboardInputReceiver, IFocusableElement
    {
        if (_focusedReceivers.Contains(receiver))
            _focusedReceivers.Remove(receiver);
    }

    //private void OnKeyTyped(object? sender, KeyboardEventArgs e)
    //{
    //    if (FocusManager.FocusedElement != null && _focusedReceivers.Contains(FocusManager.FocusedElement))
    //        ((IKeyboardInputReceiver)FocusManager.FocusedElement).OnKeyTyped(e);

    //    for (int i = 0; i < _receivers.Count; i++)
    //        _receivers[i].OnKeyTyped(e);
    //}

    private void OnKeyReleased(object? sender, in KeyboardEventArgs e)
    {
        if (FocusManager.FocusedElement != null && _focusedReceivers.Contains(FocusManager.FocusedElement))
            ((IKeyboardInputReceiver)FocusManager.FocusedElement).OnKeyReleased(e);

        for (int i = 0; i < _receivers.Count; i++)
            _receivers[i].OnKeyReleased(e);
    }

    private void OnKeyPressed(object? sender, in KeyboardEventArgs e)
    {
        if (FocusManager.FocusedElement != null && _focusedReceivers.Contains(FocusManager.FocusedElement))
            ((IKeyboardInputReceiver)FocusManager.FocusedElement).OnKeyPressed(e);

        for (int i = 0; i < _receivers.Count; i++)
            _receivers[i].OnKeyPressed(e);
    }
}
