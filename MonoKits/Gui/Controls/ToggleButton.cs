using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MonoKits.Extensions;
using MonoKits.Overrides;

namespace MonoKits.Gui.Controls;

public partial class ToggleButton : Button
{
    public bool IsChecked { get; set; } = false;

    public override Dictionary<string, object> Resources { get; } = ToggleButtonDefaultResources;
}

public partial class ToggleButton
{
    protected override void OnMouseClick(in MouseEventArgs e)
    {
        IsChecked = !IsChecked;
        base.OnMouseClick(e);
    }

    protected override void OnMouseDown(in MouseEventArgs e) => VisualState = IsChecked ? VisualState_Checked_Pressed : VisualState.Pressed;

    protected override void OnMouseRelease(in MouseEventArgs e) => VisualState = IsChecked ? VisualState_Checked_MouseOver : VisualState.MouseOver;

    protected override void OnMouseEnter(in MouseEventArgs e) => VisualState = IsChecked ? VisualState_Checked_MouseOver : VisualState.MouseOver;

    protected override void OnMouseLeave(in MouseEventArgs e) => VisualState = IsChecked ? VisualState_Checked_Normal : VisualState.Normal;
}

public partial class ToggleButton
{
    protected static readonly VisualState VisualState_Checked_Normal = new("Checked.Normal");
    protected static readonly VisualState VisualState_Checked_Pressed = new("Checked.Pressed");
    protected static readonly VisualState VisualState_Checked_MouseOver = new("Checked.MouseOver");

    protected override void UpdateVisualResources()
    {
        if (VisualState == VisualState.Normal)
        {
            _border?.Background = (Color)Resources["ToggleButton.Normal.Background"];
            _border?.Resources["Border.TopLeft"] = ((Color)Resources["ToggleButton.Normal.Border.TopLeft"]).WithOpacity(0.75);
            _border?.Resources["Border.BottomRight"] = (Color)Resources["ToggleButton.Normal.Border.BottomRight"];
        }
        else if (VisualState == VisualState.MouseOver)
        {
            _border?.Background = (Color)Resources["ToggleButton.MouseOver.Background"];
            _border?.Resources["Border.TopLeft"] = ((Color)Resources["ToggleButton.MouseOver.Border.TopLeft"]).WithOpacity(0.75);
            _border?.Resources["Border.BottomRight"] = (Color)Resources["ToggleButton.MouseOver.Border.BottomRight"];
        }
        else if (VisualState == VisualState.Pressed)
        {
            _border?.Background = (Color)Resources["ToggleButton.Pressed.Background"];
            _border?.Resources["Border.TopLeft"] = ((Color)Resources["ToggleButton.Pressed.Border.TopLeft"]);
            _border?.Resources["Border.BottomRight"] = (Color)Resources["ToggleButton.Pressed.Border.BottomRight"];
        }
        else if (VisualState == VisualState_Checked_Normal)
        {
            _border?.Background = (Color)Resources["ToggleButton.Checked.Background"];
            _border?.Resources["Border.TopLeft"] = ((Color)Resources["ToggleButton.Checked.Border.TopLeft"]).WithOpacity(0.75);
            _border?.Resources["Border.BottomRight"] = (Color)Resources["ToggleButton.Checked.Border.BottomRight"];
        }
        else if (VisualState == VisualState_Checked_MouseOver)
        {
            _border?.Background = (Color)Resources["ToggleButton.Checked.MouseOver.Background"];
            _border?.Resources["Border.TopLeft"] = ((Color)Resources["ToggleButton.Checked.MouseOver.Border.TopLeft"]).WithOpacity(0.75);
            _border?.Resources["Border.BottomRight"] = (Color)Resources["ToggleButton.Checked.MouseOver.Border.BottomRight"];
        }
        else if (VisualState == VisualState_Checked_Pressed)
        {
            _border?.Background = (Color)Resources["ToggleButton.Checked.Pressed.Background"];
            _border?.Resources["Border.TopLeft"] = ((Color)Resources["ToggleButton.Checked.Pressed.Border.TopLeft"]);
            _border?.Resources["Border.BottomRight"] = (Color)Resources["ToggleButton.Checked.Pressed.Border.BottomRight"];
        }
    }

    public static Dictionary<string, object> ToggleButtonDefaultResources => new()
    {
        { "ToggleButton.Normal.Background", ColorHelper.FromHex("#4a4a4a") },
        { "ToggleButton.Normal.Border.TopLeft", ColorHelper.FromHex("#787878") },
        { "ToggleButton.Normal.Border.BottomRight", ColorHelper.FromHex("#303030") },
        { "ToggleButton.Pressed.Background", ColorHelper.FromHex("#5F5F5F") },
        { "ToggleButton.Pressed.Border.TopLeft", ColorHelper.FromHex("#303030") },
        { "ToggleButton.Pressed.Border.BottomRight", Color.Transparent },
        { "ToggleButton.MouseOver.Background", ColorHelper.FromHex("#5f5f5f") },
        { "ToggleButton.MouseOver.Border.TopLeft", ColorHelper.FromHex("#787878") },
        { "ToggleButton.MouseOver.Border.BottomRight", ColorHelper.FromHex("#3e3e3e") },
        { "ToggleButton.Checked.Background", ColorHelper.FromHex("#39CB60") },
        { "ToggleButton.Checked.Border.TopLeft", ColorHelper.FromHex("#39CB60") },
        { "ToggleButton.Checked.Border.BottomRight", ColorHelper.FromHex("#29A942") },
        { "ToggleButton.Checked.Pressed.Background", ColorHelper.FromHex("#3BD364") },
        { "ToggleButton.Checked.Pressed.Border.TopLeft", ColorHelper.FromHex("#29A942") },
        { "ToggleButton.Checked.Pressed.Border.BottomRight", Color.Transparent },
        { "ToggleButton.Checked.MouseOver.Background", ColorHelper.FromHex("#3BD364") },
        { "ToggleButton.Checked.MouseOver.Border.TopLeft", ColorHelper.FromHex("#3BD364") },
        { "ToggleButton.Checked.MouseOver.Border.BottomRight", ColorHelper.FromHex("#2CBA48") },
    };
}