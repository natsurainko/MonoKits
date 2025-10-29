using Microsoft.Xna.Framework;

namespace MonoKits.Gui.Controls;

public partial class CanvasPanel : Panel
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

        for (int i = 0; i < Children.Count; i++)
        {
            UIElement child = Children[i];
            child.Measure(childAvailableSize);
        }

        double desiredWidth, desiredHeight;

        if (!double.IsNaN(Width))
            desiredWidth = Width;
        else 
            desiredWidth = Padding.Left + Padding.Right;

        if (!double.IsNaN(Height))
            desiredHeight = Height;
        else
            desiredHeight = Padding.Top + Padding.Bottom;

        desiredWidth = Math.Min(desiredWidth, availableSize.Width);
        desiredHeight = Math.Min(desiredHeight, availableSize.Height);

        return new Size(desiredWidth, desiredHeight);
    }

    protected override Rectangle ArrangeOverride(Rectangle finalRect)
    {
        Rectangle bounds = base.ArrangeOverride(finalRect);

        for (int i = Children.Count - 1; i >= 0; i--)
        {
            UIElement child = Children[i];
            child?.Arrange(bounds + Padding + child.Margin);
        }

        return bounds;
    }
}