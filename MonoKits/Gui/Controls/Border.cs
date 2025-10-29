using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MonoKits.Components;

namespace MonoKits.Gui.Controls;

public partial class Border : UIElement
{
    protected override Size MeasureOverride(Size availableSize)
    {
        Size childAvailableSize = new
        (
            Math.Max(0, availableSize.Width - Padding.Left - Padding.Right),
            Math.Max(0, availableSize.Height - Padding.Top - Padding.Bottom)
        );

        if (!double.IsNaN(Width))
            childAvailableSize.Width = Math.Min(Width - Padding.Left - Padding.Right, childAvailableSize.Width);

        if (!double.IsNaN(Height))
            childAvailableSize.Height = Math.Min(Height - Padding.Top - Padding.Bottom, childAvailableSize.Height);

        Child?.Measure(childAvailableSize);

        double desiredWidth, desiredHeight;

        if (!double.IsNaN(Width))
            desiredWidth = Width;
        else
        {
            desiredWidth = Padding.Left + Padding.Right;

            if (Child != null)
                desiredWidth += Child.DesiredSize.Width + Child.Margin.Left + Child.Margin.Right;
        }

        if (!double.IsNaN(Height))
            desiredHeight = Height;
        else
        {
            desiredHeight = Padding.Top + Padding.Bottom;

            if (Child != null)
                desiredHeight += Child.DesiredSize.Height + Child.Margin.Top + Child.Margin.Bottom;
        }

        desiredWidth = Math.Min(desiredWidth, availableSize.Width);
        desiredHeight = Math.Min(desiredHeight, availableSize.Height);

        return new Size(desiredWidth, desiredHeight);
    }

    protected override Rectangle ArrangeOverride(Rectangle finalRect)
    {
        Rectangle bounds = base.ArrangeOverride(finalRect);
        Child?.Arrange(bounds + Padding + Child.Margin);

        return bounds;
    }
}

public partial class Border
{
    [ObservableProperty]
    public partial UIElement? Child { get; set; }

    public override Dictionary<string, object> Resources { get; } = BorderDefaultResources;

    partial void OnChildChanged(UIElement? oldValue, UIElement? newValue)
    {
        oldValue?.Parent = null;

        if (newValue?.Parent != null)
            throw new InvalidOperationException();

        newValue?.Parent = this;
    }
}

public partial class Border
{
    protected override void DrawOverride(GameTime gameTime)
    {
        base.DrawOverride(gameTime);

        SpriteBatch?.Draw(GuiComponent.ControlDefaultTexture, new Rectangle
        {
            X = this.Bounds.X,
            Y = this.Bounds.Y + this.Bounds.Height - 4,
            Width = this.Bounds.Width,
            Height = 4
        }, (Color)Resources["Border.BottomRight"]);
        SpriteBatch?.Draw(GuiComponent.ControlDefaultTexture, new Rectangle
        {
            X = this.Bounds.X + this.Bounds.Width - 4,
            Y = this.Bounds.Y,
            Width = 4,
            Height = this.Bounds.Height - 4
        }, (Color)Resources["Border.BottomRight"]);

        SpriteBatch?.Draw(GuiComponent.ControlDefaultTexture, new Rectangle
        {
            X = this.Bounds.X,
            Y = this.Bounds.Y,
            Width = this.Bounds.Width,
            Height = 4
        }, (Color)Resources["Border.TopLeft"]);
        SpriteBatch?.Draw(GuiComponent.ControlDefaultTexture, new Rectangle
        {
            X = this.Bounds.X,
            Y = this.Bounds.Y + 4,
            Width = 4,
            Height = this.Bounds.Height - 4
        }, (Color)Resources["Border.TopLeft"]);

        Child?.Draw(gameTime);
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        Child?.Update(gameTime);
    }

    public static Dictionary<string, object> BorderDefaultResources => new()
    {
        { "Border.TopLeft", ColorHelper.FromHex("#787878") },
        { "Border.BottomRight", ColorHelper.FromHex("#303030") },
    };
}
