using MonoKits.Overrides;
using System.Collections.Concurrent;
using System.Xml.Linq;
using MouseListener = MonoKits.Overrides.MouseListener;

namespace MonoKits.Gui.Input;

public class MouseInputManager
{
    private readonly MouseListener _mouseListener;

    private readonly List<IMouseInputReceiver> _receivers = [];
    private readonly ConcurrentDictionary<IMouseInputReceiver, bool> _receiversBoundsState = [];

    public MouseInputManager(MouseListener mouseListener)
    {
        _mouseListener = mouseListener;

        _mouseListener.MouseDown += OnMouseDown;
        _mouseListener.MouseUp += OnMouseRelease;
        _mouseListener.MouseMoved += OnMouseMove;
        _mouseListener.MouseClicked += OnMouseClick;
        _mouseListener.MouseDoubleClicked += OnMouseDoubleClick;
        _mouseListener.MouseWheelMoved += OnMouseWheelMoved;
        _mouseListener.MouseDrag += OnMouseDrag;
        _mouseListener.MouseDragStart += OnMouseDragStart;
        _mouseListener.MouseDragEnd += OnMouseDragEnd;
    }

    public void Register<TUIElement>(TUIElement receiver) where TUIElement : UIElement, IMouseInputReceiver
    {
        if (!_receivers.Contains(receiver))
            _receivers.Add(receiver);
    }

    public void Unregister<TUIElement>(TUIElement receiver) where TUIElement : UIElement, IMouseInputReceiver
    {
        if (_receivers.Contains(receiver))
            _receivers.Remove(receiver);
    }

    private void OnMouseWheelMoved(object? sender, in MouseEventArgs e)
    {
        for (int i = 0; i < _receivers.Count; i++)
        {
            UIElement element = (UIElement)_receivers[i];

            if (!element.Bounds.Contains(e.Position) || !element.Visible || !element.Enabled)
                continue;

            _receivers[i].OnMouseWheelMoved(e);
        }
    }

    private void OnMouseDoubleClick(object? sender, in MouseEventArgs e)
    {
        for (int i = 0; i < _receivers.Count; i++)
        {
            UIElement element = (UIElement)_receivers[i];

            if (!element.Bounds.Contains(e.Position) || !element.Visible || !element.Enabled)
                continue;

            _receivers[i].OnMouseDoubleClick(e);
        }
    }

    private void OnMouseClick(object? sender, in MouseEventArgs e)
    {
        for (int i = 0; i < _receivers.Count; i++)
        {
            UIElement element = (UIElement)_receivers[i];

            if (!element.Bounds.Contains(e.Position) || !element.Visible || !element.Enabled)
                continue;

            _receivers[i].OnMouseClick(e);
        }
    }

    private void OnMouseMove(object? sender, in MouseEventArgs e)
    {
        for (int i = 0; i < _receivers.Count; i++)
        {
            IMouseInputReceiver receiver = _receivers[i];
            UIElement element = (UIElement)_receivers[i];

            if (!element.Bounds.Contains(e.Position) || !element.Visible || !element.Enabled)
            {
                if (_receiversBoundsState.TryRemove(receiver, out _))
                    receiver.OnMouseLeave(e);

                continue;
            }

            if (_receiversBoundsState.TryAdd(receiver, true))
                receiver.OnMouseEnter(e);

            receiver.OnMouseMove(e);
        }
    }

    private void OnMouseRelease(object? sender, in MouseEventArgs e)
    {
        for (int i = 0; i < _receivers.Count; i++)
        {
            UIElement element = (UIElement)_receivers[i];

            if (!element.Bounds.Contains(e.Position) || !element.Visible || !element.Enabled)
                continue;

            _receivers[i].OnMouseRelease(e);
        }
    }

    private void OnMouseDown(object? sender, in MouseEventArgs e)
    {
        bool focused = false;

        for (int i = 0; i < _receivers.Count; i++)
        {
            UIElement element = (UIElement)_receivers[i];

            if (!element.Bounds.Contains(e.Position) || !element.Visible || !element.Enabled) 
                continue;

            if (!focused && element is IFocusableElement focusableElement && focusableElement.Focusable)
            {
                FocusManager.SetFocus(focusableElement);
                focused = true;
            }

            _receivers[i].OnMouseDown(e);
        }
    }

    private void OnMouseDragEnd(object? sender, in MouseEventArgs e)
    {
        for (int i = 0; i < _receivers.Count; i++)
        {
            UIElement element = (UIElement)_receivers[i];

            if (!element.Visible || !element.Enabled) continue;
            _receivers[i].OnMouseDragEnd(e);
        }
    }

    private void OnMouseDragStart(object? sender, in MouseEventArgs e)
    {
        for (int i = 0; i < _receivers.Count; i++)
        {
            UIElement element = (UIElement)_receivers[i];

            if (!element.Bounds.Contains(e.Position) || !element.Visible || !element.Enabled)
                continue;

            _receivers[i].OnMouseDragStart(e);
        }
    }

    private void OnMouseDrag(object? sender, in MouseEventArgs e)
    {
        for (int i = 0; i < _receivers.Count; i++)
        {
            UIElement element = (UIElement)_receivers[i];

            if (!element.Visible || !element.Enabled) continue;
            _receivers[i].OnMouseDragEnd(e);
        }
    }
}
