using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Xna.Framework;

namespace MonoKits.Gui.Controls;

public partial class ScrollContentPresenter : ContentControl
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

        if (CanHorizontallyScroll)
            childAvailableSize.Width = double.PositiveInfinity;
        if (CanVerticallyScroll)
            childAvailableSize.Height = double.PositiveInfinity;

        if (!_templateApplied)
        {
            _templateContent?.Parent = null;
            _templateContent = Template.ContentConstructor(this);
            _templateContent?.Parent = this;
            _templateApplied = true;
        }

        _templateContent?.Measure(childAvailableSize);

        double desiredWidth, desiredHeight;

        if (!double.IsNaN(Width))
            desiredWidth = Width;
        else
        {
            desiredWidth = Padding.Left + Padding.Right;

            if (_templateContent != null)
                desiredWidth += _templateContent.DesiredSize.Width + _templateContent.Margin.Left + _templateContent.Margin.Right;
        }

        if (!double.IsNaN(Height))
            desiredHeight = Height;
        else
        {
            desiredHeight = Padding.Top + Padding.Bottom;

            if (_templateContent != null)
                desiredHeight += _templateContent.DesiredSize.Height + _templateContent.Margin.Top + _templateContent.Margin.Bottom;
        }

        desiredWidth = Math.Min(desiredWidth, availableSize.Width);
        desiredHeight = Math.Min(desiredHeight, availableSize.Height);

        return new Size(desiredWidth, desiredHeight);
    }

    protected override Rectangle ArrangeOverride(Rectangle finalRect)
    {
        Rectangle bounds = Base_UIElement_ArrangeOverride(finalRect);

        if (_templateContent != null)
        {
            Rectangle contentBounds = bounds + Padding + _templateContent.Margin;
            contentBounds.X -= (int)HorizontalOffset;
            contentBounds.Y -= (int)VerticalOffset;

            if (CanHorizontallyScroll)
                contentBounds.Width = (int)_templateContent.DesiredSize.Width;
            if (CanVerticallyScroll)
                contentBounds.Height = (int)_templateContent.DesiredSize.Height;

            _templateContent.Arrange(contentBounds);

            ExtentHeight = _templateContent.ActualHeight;
            ExtentWidth = _templateContent.ActualWidth;
            ScrollableWidth = Math.Max(0, _templateContent.ActualWidth - (bounds.Width - Padding.Left - Padding.Right));
            ScrollableHeight = Math.Max(0, _templateContent.ActualHeight - (bounds.Height - Padding.Top - Padding.Bottom));
        }

        return bounds;
    }
}

public partial class ScrollContentPresenter
{
    [ObservableProperty]
    public partial double VerticalOffset { get; set; }

    [ObservableProperty]
    public partial double HorizontalOffset { get; set; }

    [ObservableProperty]
    public partial double ScrollableWidth { get; private set; } = 0;

    [ObservableProperty]
    public partial double ScrollableHeight { get; private set; } = 0;

    [ObservableProperty]
    public partial double ExtentHeight { get; private set; } = 0;

    [ObservableProperty]
    public partial double ExtentWidth { get; private set; } = 0;

    [ObservableProperty]
    public partial bool CanVerticallyScroll { get; set; } = false;

    [ObservableProperty]
    public partial bool CanHorizontallyScroll { get; set; } = false;
}