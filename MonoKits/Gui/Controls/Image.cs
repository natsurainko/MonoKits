using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonoKits.Gui.Controls;

public partial class Image : UIElement
{
    protected override Size MeasureOverride(Size availableSize)
    {
        double desiredWidth, desiredHeight;

        if (!double.IsNaN(Width))
            desiredWidth = Width;
        else 
            desiredWidth = Padding.Left + (Texture?.Width ?? 0) + Padding.Right;

        if (!double.IsNaN(Height))
            desiredHeight = Height;
        else
            desiredHeight = Padding.Top + (Texture?.Height ?? 0) + Padding.Bottom;

        desiredWidth = Math.Min(desiredWidth, availableSize.Width);
        desiredHeight = Math.Min(desiredHeight, availableSize.Height);

        return new Size(desiredWidth, desiredHeight);
    }
}

public partial class Image : UIElement
{
    [ObservableProperty]
    public partial Texture2D? Texture { get; set; }
}

public partial class Image
{
    protected override void DrawOverride(GameTime gameTime)
    {
        base.DrawOverride(gameTime);
        SpriteBatch?.Draw(Texture, Bounds, Color.White);
    }
}