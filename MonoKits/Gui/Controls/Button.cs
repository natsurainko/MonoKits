using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MonoKits.Components;
using MonoKits.Extensions;
using MonoKits.Gui.Input;
using MonoKits.Overrides;

namespace MonoKits.Gui.Controls;

public partial class Button : ContentControl
{
    protected Border? _border;

    public Button()
    {
        Template = new ControlTemplate<Button>(static button =>
        {
            button._border = new Border 
            {
                Padding = new Thickness(16, 12)
            };

            if (button.ContentTemplate != null && button.Content != null)
            {
                button._border.Child = button.ContentTemplate.ContentConstructor(button, button.Content);
            }

            if (button.Content is UIElement uIElement)
            {
                if (uIElement.Parent != null && uIElement.Parent != button)
                    throw new InvalidOperationException();

                button._border.Child = uIElement;
            }

            button.UpdateVisualResources();

            return button._border;
        });
    }
}

public partial class Button
{
    public event EventHandler? Clicked;

    public override bool Focusable => true;

    public override Dictionary<string, object> Resources { get; } = ButtonDefaultResources;
}

public partial class Button : IMouseInputReceiver
{
    protected override void OnLoaded() => GuiComponent.MouseInputManager.Register(this);

    protected override void OnUnloaded() => GuiComponent.MouseInputManager.Unregister(this);

    protected virtual void OnMouseClick(in MouseEventArgs e) => this.Clicked?.Invoke(this, EventArgs.Empty);

    protected virtual void OnMouseDown(in MouseEventArgs e) => VisualState = VisualState.Pressed;

    protected virtual void OnMouseRelease(in MouseEventArgs e) => VisualState = VisualState.MouseOver;

    protected virtual void OnMouseEnter(in MouseEventArgs e) => VisualState = VisualState.MouseOver;

    protected virtual void OnMouseLeave(in MouseEventArgs e) => VisualState = VisualState.Normal;

    void IMouseInputReceiver.OnMouseClick(in MouseEventArgs e) => OnMouseClick(e);

    void IMouseInputReceiver.OnMouseDown(in MouseEventArgs e) => OnMouseDown(e);

    void IMouseInputReceiver.OnMouseRelease(in MouseEventArgs e) => OnMouseRelease(e);

    void IMouseInputReceiver.OnMouseEnter(in MouseEventArgs e) => OnMouseEnter(e);

    void IMouseInputReceiver.OnMouseLeave(in MouseEventArgs e) => OnMouseLeave(e);
}

public partial class Button
{
    protected override void UpdateVisualResources()
    {
        if (VisualState == VisualState.Normal)
        {
            _border?.Background = (Color)Resources["Button.Normal.Background"];
            _border?.Resources["Border.TopLeft"] = ((Color)Resources["Button.Normal.Border.TopLeft"]).WithOpacity(0.75);
            _border?.Resources["Border.BottomRight"] = (Color)Resources["Button.Normal.Border.BottomRight"];
        }
        else if (VisualState == VisualState.MouseOver)
        {
            _border?.Background = (Color)Resources["Button.MouseOver.Background"];
            _border?.Resources["Border.TopLeft"] = ((Color)Resources["Button.MouseOver.Border.TopLeft"]).WithOpacity(0.75);
            _border?.Resources["Border.BottomRight"] = (Color)Resources["Button.MouseOver.Border.BottomRight"];
        }
        else if (VisualState == VisualState.Pressed)
        {
            _border?.Background = (Color)Resources["Button.Pressed.Background"];
            _border?.Resources["Border.TopLeft"] = ((Color)Resources["Button.Pressed.Border.TopLeft"]);
            _border?.Resources["Border.BottomRight"] = (Color)Resources["Button.Pressed.Border.BottomRight"];
        }
    }

    public static Dictionary<string, object> ButtonDefaultResources => new()
    {
        { "Button.MouseOver.Background", ColorHelper.FromHex("#5f5f5f") },
        { "Button.MouseOver.Border.TopLeft", ColorHelper.FromHex("#787878") },
        { "Button.MouseOver.Border.BottomRight", ColorHelper.FromHex("#3e3e3e") },
        { "Button.Normal.Background", ColorHelper.FromHex("#4a4a4a") },
        { "Button.Normal.Border.TopLeft", ColorHelper.FromHex("#787878") },
        { "Button.Normal.Border.BottomRight", ColorHelper.FromHex("#303030") },
        { "Button.Pressed.Background", ColorHelper.FromHex("#5F5F5F") },
        { "Button.Pressed.Border.TopLeft", ColorHelper.FromHex("#303030") },
        { "Button.Pressed.Border.BottomRight", Color.Transparent },
    };
}