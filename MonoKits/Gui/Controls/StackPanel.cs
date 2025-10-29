using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Xna.Framework;

namespace MonoKits.Gui.Controls;

public partial class StackPanel : Panel
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

        double totalWidth = 0;
        double totalHeight = 0;
        double maxWidth = 0;
        double maxHeight = 0;

        for (int i = 0; i < Children.Count; i++)
        {
            UIElement child = Children[i];
            if (child == null) continue;

            Size childConstraint = Orientation == Orientation.Vertical
                ? new Size(childAvailableSize.Width, double.PositiveInfinity)
                : new Size(double.PositiveInfinity, childAvailableSize.Height);

            child.Measure(childConstraint);

            double childWidth = child.DesiredSize.Width + child.Margin.Left + child.Margin.Right;
            double childHeight = child.DesiredSize.Height + child.Margin.Top + child.Margin.Bottom;

            if (Orientation == Orientation.Vertical)
            {
                totalHeight += childHeight + Spacing;
                maxWidth = Math.Max(maxWidth, childWidth);
            }
            else
            {
                totalWidth += childWidth + Spacing;
                maxHeight = Math.Max(maxHeight, childHeight);
            }
        }

        if (Orientation == Orientation.Vertical)  
            totalHeight -= Spacing;
        else totalWidth -= Spacing;

        double desiredWidth, desiredHeight;

        if (!double.IsNaN(Width))
            desiredWidth = Width;
        else
            desiredWidth = Padding.Left + Padding.Right +
                          (Orientation == Orientation.Horizontal ? totalWidth : maxWidth);

        if (!double.IsNaN(Height))
            desiredHeight = Height;
        else
            desiredHeight = Padding.Top + Padding.Bottom +
                           (Orientation == Orientation.Vertical ? totalHeight : maxHeight);

        desiredWidth = Math.Min(desiredWidth, availableSize.Width);
        desiredHeight = Math.Min(desiredHeight, availableSize.Height);

        return new Size(desiredWidth, desiredHeight);
    }

    protected override Rectangle ArrangeOverride(Rectangle finalRect)
    {
        Rectangle bounds = base.ArrangeOverride(finalRect);
        Rectangle contentRect = bounds + Padding;

        int currentX = contentRect.X;
        int currentY = contentRect.Y;

        if (Orientation == Orientation.Vertical)
            currentY -= (int)Spacing;
        else currentX -= (int)Spacing;

        for (int i = 0; i < Children.Count; i++)
        {
            UIElement child = Children[i];
            int childWidth, childHeight;

            if (Orientation == Orientation.Vertical)
            {
                childWidth = contentRect.Width - (int)(child.Margin.Left + child.Margin.Right);
                childHeight = (int)child.DesiredSize.Height;

                Rectangle childRect = new
                (
                    currentX,
                    currentY + (int)(child.Margin.Top + Spacing),
                    Math.Max(0, childWidth),
                    Math.Max(0, childHeight)
                );

                child.Arrange(childRect);

                currentY += (int)(child.Margin.Top + childHeight + child.Margin.Bottom + Spacing);
            }
            else
            {
                childWidth = (int)child.DesiredSize.Width;
                childHeight = contentRect.Height - (int)(child.Margin.Top + child.Margin.Bottom);

                Rectangle childRect = new
                (
                    currentX + (int)(child.Margin.Left + Spacing),
                    currentY,
                    Math.Max(0, childWidth),
                    Math.Max(0, childHeight)
                );

                child.Arrange(childRect);

                currentX += (int)(child.Margin.Left + childWidth + child.Margin.Right + Spacing);
            }
        }

        return bounds;
    }
}

public partial class StackPanel
{
    [ObservableProperty]
    public partial Orientation Orientation { get; set; } = Orientation.Vertical;

    [ObservableProperty]
    public partial double Spacing { get; set; } = 0;
}