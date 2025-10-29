using MonoKits.Overrides;

namespace MonoKits.Gui.Input;

public interface IMouseInputReceiver
{
    void OnMouseDown(in MouseEventArgs e) { }

    void OnMouseRelease(in MouseEventArgs e) { }

    void OnMouseMove(in MouseEventArgs e) { }

    void OnMouseClick(in MouseEventArgs e) { }

    void OnMouseDoubleClick(in MouseEventArgs e) { }

    void OnMouseWheelMoved(in MouseEventArgs e) { }

    void OnMouseEnter(in MouseEventArgs e) { }

    void OnMouseLeave(in MouseEventArgs e) { }

    void OnMouseDragEnd(in MouseEventArgs e) { }

    void OnMouseDragStart(in MouseEventArgs e) { }

    void OnMouseDrag(in MouseEventArgs e) { }
}