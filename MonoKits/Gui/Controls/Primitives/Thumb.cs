using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MonoKits.Components;
using MonoKits.Extensions;
using MonoKits.Gui.Input;
using MonoKits.Overrides;

namespace MonoKits.Gui.Controls.Primitives;

public partial class Thumb : Control
{
    protected Border? _border;

    public Thumb()
    {
        HorizontalAlignment = HorizontalAlignment.Stretch;
        VerticalAlignment = VerticalAlignment.Stretch;

        Template = new ControlTemplate<Thumb>(static thumb =>
        {
            thumb._border = new Border
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch
            };

            thumb.UpdateVisualResources();

            return thumb._border;
        });
    }
}

public partial class Thumb
{
    public bool IsDragging { get; protected set; }

    public override bool Focusable => true;

    public override Dictionary<string, object> Resources { get; } = ThumbDefaultResources;
}

public partial class Thumb : IMouseInputReceiver
{
    protected override void OnLoaded() => GuiComponent.MouseInputManager.Register(this);

    protected override void OnUnloaded() => GuiComponent.MouseInputManager.Unregister(this);

    public event MouseEventHandler? DragStarted;
    public event MouseEventHandler? DragDelta;
    public event MouseEventHandler? DragEnded;

    void IMouseInputReceiver.OnMouseDragStart(in MouseEventArgs e)
    {
        if (!this.Bounds.Contains(e.Position)) return;

        IsDragging = true;
        DragStarted?.Invoke(this, e);
    }

    void IMouseInputReceiver.OnMouseDrag(in MouseEventArgs e)
    {
        if (IsDragging)
            DragDelta?.Invoke(this, e);
    }

    void IMouseInputReceiver.OnMouseDragEnd(in MouseEventArgs e)
    {
        IsDragging = false;
        DragEnded?.Invoke(this, e);
    }

    void IMouseInputReceiver.OnMouseDown(in MouseEventArgs e) => VisualState = VisualState.Pressed;

    void IMouseInputReceiver.OnMouseRelease(in MouseEventArgs e) => VisualState = VisualState.MouseOver;

    void IMouseInputReceiver.OnMouseEnter(in MouseEventArgs e) => VisualState = VisualState.MouseOver;

    void IMouseInputReceiver.OnMouseLeave(in MouseEventArgs e) => VisualState = VisualState.Normal;
}

public partial class Thumb
{
    protected override void UpdateVisualResources()
    {
        if (VisualState == VisualState.Normal)
        {
            _border?.Background = (Color)Resources["Thumb.Normal.Background"];
            _border?.Resources["Border.TopLeft"] = ((Color)Resources["Thumb.Normal.Border.TopLeft"]).WithOpacity(0.75);
            _border?.Resources["Border.BottomRight"] = (Color)Resources["Thumb.Normal.Border.BottomRight"];
        }
        else if (VisualState == VisualState.MouseOver)
        {
            _border?.Background = (Color)Resources["Thumb.MouseOver.Background"];
            _border?.Resources["Border.TopLeft"] = ((Color)Resources["Thumb.MouseOver.Border.TopLeft"]).WithOpacity(0.75);
            _border?.Resources["Border.BottomRight"] = (Color)Resources["Thumb.MouseOver.Border.BottomRight"];
        }
        else if (VisualState == VisualState.Pressed)
        {
            _border?.Background = (Color)Resources["Thumb.Pressed.Background"];
            _border?.Resources["Border.TopLeft"] = ((Color)Resources["Thumb.Pressed.Border.TopLeft"]);
            _border?.Resources["Border.BottomRight"] = (Color)Resources["Thumb.Pressed.Border.BottomRight"];
        }
    }

    public static Dictionary<string, object> ThumbDefaultResources => new()
    {
        { "Thumb.MouseOver.Background", ColorHelper.FromHex("#5f5f5f") },
        { "Thumb.MouseOver.Border.TopLeft", ColorHelper.FromHex("#787878") },
        { "Thumb.MouseOver.Border.BottomRight", ColorHelper.FromHex("#3e3e3e") },
        { "Thumb.Normal.Background", ColorHelper.FromHex("#4a4a4a") },
        { "Thumb.Normal.Border.TopLeft", ColorHelper.FromHex("#787878") },
        { "Thumb.Normal.Border.BottomRight", ColorHelper.FromHex("#303030") },
        { "Thumb.Pressed.Background", ColorHelper.FromHex("#5F5F5F") },
        { "Thumb.Pressed.Border.TopLeft", ColorHelper.FromHex("#303030") },
        { "Thumb.Pressed.Border.BottomRight", Color.Transparent },
    };
}