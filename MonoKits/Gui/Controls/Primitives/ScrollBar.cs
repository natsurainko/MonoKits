using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Xna.Framework;
using MonoKits.Components;
using MonoKits.Gui.Input;
using MonoKits.Overrides;

namespace MonoKits.Gui.Controls.Primitives;

public partial class ScrollBar : Control
{
    private const double MinimumThumbSize = 20.0;

    protected Border? _track;
    protected Thumb? _thumb;
    protected CanvasPanel? _canvasPanel;

    public ScrollBar()
    {
        HorizontalAlignment = HorizontalAlignment.Stretch;
        VerticalAlignment = VerticalAlignment.Stretch;

        Template = new ControlTemplate<ScrollBar>(static scrollBar =>
        {
            scrollBar._thumb = new Thumb();
            scrollBar._track = new Border
            {
                Padding = new Thickness(4),
            };
            scrollBar._canvasPanel = new CanvasPanel
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Children =
                {
                    scrollBar._track,
                    scrollBar._thumb
                }
            };

            if (scrollBar.Orientation == Orientation.Vertical)
            {
                scrollBar._thumb.HorizontalAlignment = HorizontalAlignment.Stretch;
                scrollBar._track.HorizontalAlignment = HorizontalAlignment.Center;
                scrollBar._track.VerticalAlignment = VerticalAlignment.Stretch;
            }
            else
            {
                scrollBar._thumb.VerticalAlignment = VerticalAlignment.Stretch;
                scrollBar._track.VerticalAlignment = VerticalAlignment.Center;
                scrollBar._track.HorizontalAlignment = HorizontalAlignment.Stretch;
            }

            (scrollBar._track.Resources["Border.TopLeft"], scrollBar._track.Resources["Border.BottomRight"]) = (scrollBar._track.Resources["Border.BottomRight"], scrollBar._track.Resources["Border.TopLeft"]);

            scrollBar.UpdateVisualResources();
            
            return scrollBar._canvasPanel;
        });
    }
}

public partial class ScrollBar
{
    public event EventHandler<double>? ValueChanged;

    [ObservableProperty]
    public partial double Minimum { get; set; } = 0;

    [ObservableProperty]
    public partial double Maximum { get; set; } = 100;

    [ObservableProperty]
    public partial double Value { get; set; } = 0;

    [ObservableProperty]
    public partial double ViewportSize { get; set; } = 10;

    [ObservableProperty]
    public partial Orientation Orientation { get; set; } = Orientation.Vertical;

    partial void OnValueChanged(double value)
    {
        UpdateThumb();
        ValueChanged?.Invoke(this, value);
    }

    partial void OnMinimumChanged(double value) => UpdateThumb();

    partial void OnMaximumChanged(double value) => UpdateThumb();

    partial void OnViewportSizeChanged(double value) => UpdateThumb();

    protected override void OnLoaded()
    {
        _thumb!.DragStarted += OnThumbDragStarted;
        _thumb!.DragDelta += OnThumbDragDelta;
        _thumb!.DragEnded += OnThumbDragEnded;

        GuiComponent.MouseInputManager.Register(this);
    }

    protected override void OnUnloaded()
    {
        _thumb!.DragStarted -= OnThumbDragStarted;
        _thumb!.DragDelta -= OnThumbDragDelta;
        _thumb!.DragEnded -= OnThumbDragEnded;

        GuiComponent.MouseInputManager.Unregister(this);
    }

    private void OnThumbDragStarted(object? sender, in MouseEventArgs e) { }

    private void OnThumbDragEnded(object? sender, in MouseEventArgs e) { }

    private void OnThumbDragDelta(object? sender, in MouseEventArgs e)
    {
        if (_track == null || _thumb == null) return;

        if (Orientation == Orientation.Vertical)
        {
            double offset = (e.Position.Y - this.Bounds.Y) - (_thumb.Margin.Top + _thumb.Height / 2);
            double verticalOffset = Math.Clamp(_thumb.Margin.Top + offset, 0, this.ActualHeight - _thumb!.Height);

            _thumb!.Margin = new Thickness(0, verticalOffset, 0, 0);

            double trackRange = this.ActualHeight - _thumb.Height;
            Value = trackRange > 0 ? (verticalOffset / trackRange) * Maximum : 0;
        }
        else
        {
            double offset = (e.Position.X - this.Bounds.X) - (_thumb.Margin.Left + _thumb.Width / 2);
            double horizontalOffset = Math.Clamp(_thumb.Margin.Left + offset, 0, this.ActualWidth - _thumb!.Width);

            _thumb!.Margin = new Thickness(horizontalOffset, 0, 0, 0);

            double trackRange = this.ActualWidth - _thumb.Width;
            Value = trackRange > 0 ? (horizontalOffset / trackRange) * Maximum : 0;
        }
    }
}

public partial class ScrollBar : IMouseInputReceiver
{
    void IMouseInputReceiver.OnMouseDown(in MouseEventArgs e)
    {
        if (_thumb == null || _track == null) return;
        if (_thumb.Bounds.Contains(e.Position)) return;

        if (Orientation == Orientation.Vertical)
        {
            double offset = (e.Position.Y - this.Bounds.Y) - (_thumb.Margin.Top + _thumb.Height / 2);
            double verticalOffset = Math.Clamp(_thumb.Margin.Top + offset, 0, this.ActualHeight - _thumb!.Height);

            _thumb!.Margin = new Thickness(0, verticalOffset, 0, 0);

            double trackRange = this.ActualHeight - _thumb.Height;
            Value = trackRange > 0 ? (verticalOffset / trackRange) * Maximum : 0;
        }
        else
        {
            double offset = (e.Position.X - this.Bounds.X) - (_thumb.Margin.Left + _thumb.Width / 2);
            double horizontalOffset = Math.Clamp(_thumb.Margin.Left + offset, 0, this.ActualWidth - _thumb!.Width);

            _thumb!.Margin = new Thickness(horizontalOffset, 0, 0, 0);

            double trackRange = this.ActualWidth - _thumb.Width;
            Value = trackRange > 0 ? (horizontalOffset / trackRange) * Maximum : 0;
        }
    }

    private void UpdateThumb()
    {
        if (_thumb == null) return;

        if (Orientation == Orientation.Vertical)
        {
            double idealThumbHeight = this.ActualHeight * (ViewportSize / (Maximum - Minimum + ViewportSize));
            double thumbHeight = Math.Max(MinimumThumbSize, Math.Clamp(idealThumbHeight, 0, this.ActualHeight));

            double verticalOffset = 0;
            if (Maximum > 0 && this.ActualHeight > thumbHeight)
            {
                double trackRange = this.ActualHeight - thumbHeight;
                verticalOffset = Math.Clamp((Value / Maximum) * trackRange, 0, trackRange);
            }

            _thumb.Height = thumbHeight;
            _thumb.Margin = new Thickness(0, verticalOffset, 0, 0);
        }
        else
        {
            double idealThumbWidth = this.ActualWidth * (ViewportSize / (Maximum - Minimum + ViewportSize));
            double thumbWidth = Math.Max(MinimumThumbSize, Math.Clamp(idealThumbWidth, 0, this.ActualWidth));

            double horizontalOffset = 0;
            if (Maximum > 0 && this.ActualWidth > thumbWidth)
            {
                double trackRange = this.ActualWidth - thumbWidth;
                horizontalOffset = Math.Clamp((Value / Maximum) * trackRange, 0, trackRange);
            }

            _thumb.Width = thumbWidth;
            _thumb.Margin = new Thickness(horizontalOffset, 0, 0, 0);
        }
    }
}
