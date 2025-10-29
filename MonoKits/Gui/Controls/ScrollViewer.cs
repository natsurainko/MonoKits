using CommunityToolkit.Mvvm.ComponentModel;
using MonoKits.Components;
using MonoKits.Gui.Controls.Primitives;
using MonoKits.Gui.Input;
using MonoKits.Overrides;

namespace MonoKits.Gui.Controls;

public partial class ScrollViewer : ContentControl
{
    private CanvasPanel? _canvasPanel;
    private ScrollContentPresenter? _scrollContentPresenter;
    private ScrollBar? _verticalScrollBar;
    private ScrollBar? _horizontalScrollBar;

    public ScrollViewer()
    {
        Template = new ControlTemplate<ScrollViewer>(static scrollViewer =>
        {
            scrollViewer._verticalScrollBar = new ScrollBar
            {
                Orientation = Orientation.Vertical,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Stretch,
                Width = 16,
                Visible = false
            };
            scrollViewer._horizontalScrollBar = new ScrollBar
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Bottom,
                Height = 16,
                Visible = false,
            };
            scrollViewer._scrollContentPresenter = new ScrollContentPresenter
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                CanVerticallyScroll = scrollViewer.IsScrollableVertically,
                CanHorizontallyScroll = scrollViewer.IsScrollableHorizontally
            };
            scrollViewer._canvasPanel = new CanvasPanel
            {
                Children =
                {
                    scrollViewer._verticalScrollBar,
                    scrollViewer._horizontalScrollBar,
                    scrollViewer._scrollContentPresenter
                }
            };

            if (scrollViewer.IsScrollableHorizontally && scrollViewer.IsScrollableVertically)
            {
                scrollViewer._verticalScrollBar.Margin = new Thickness(0, 0, 0, 16);
                scrollViewer._horizontalScrollBar.Margin = new Thickness(0, 0, 16, 0);
                scrollViewer._scrollContentPresenter.Margin = new Thickness(0, 0, 20, 20);

                scrollViewer._verticalScrollBar.Visible = true;
                scrollViewer._horizontalScrollBar.Visible = true;
            }
            else if (scrollViewer.IsScrollableVertically)
            {
                scrollViewer._scrollContentPresenter.Margin = new Thickness(0, 0, 20, 0);
                scrollViewer._verticalScrollBar.Visible = true;
                scrollViewer._horizontalScrollBar.Visible = false;
            }
            else if (scrollViewer.IsScrollableHorizontally)
            {
                scrollViewer._scrollContentPresenter.Margin = new Thickness(0, 0, 0, 20);
                scrollViewer._horizontalScrollBar.Visible = true;
                scrollViewer._verticalScrollBar.Visible = false;
            }
            else
            {
                scrollViewer._scrollContentPresenter.Margin = new Thickness(0, 0, 0, 0);
                scrollViewer._horizontalScrollBar!.Visible = false;
                scrollViewer._verticalScrollBar!.Visible = false;
            }

            if (scrollViewer.ContentTemplate != null && scrollViewer.Content != null)
            {
                scrollViewer._scrollContentPresenter.Content = scrollViewer.ContentTemplate.ContentConstructor(scrollViewer, scrollViewer.Content);
            }

            if (scrollViewer.Content is UIElement uIElement)
            {
                if (uIElement.Parent != null && uIElement.Parent != scrollViewer)
                    throw new InvalidOperationException();

                scrollViewer._scrollContentPresenter.Content = uIElement;
            }

            scrollViewer.UpdateVisualResources();

            return scrollViewer._canvasPanel;
        });
    }
}

public partial class ScrollViewer
{
    public double VerticalOffset
    {
        get => _scrollContentPresenter?.VerticalOffset ?? 0;
        set
        {
            _scrollContentPresenter?.VerticalOffset = value;
            _verticalScrollBar!.Value = value;
        }
    }

    public double HorizontalOffset
    {
        get => _scrollContentPresenter?.HorizontalOffset ?? 0;
        set
        {
            _scrollContentPresenter?.HorizontalOffset = value;
            _horizontalScrollBar!.Value = value;
        }
    }

    public double ScrollableWidth => _scrollContentPresenter?.ScrollableWidth ?? 0;

    public double ScrollableHeight => _scrollContentPresenter?.ScrollableHeight ?? 0;

    [ObservableProperty]
    public partial bool IsScrollableVertically { get; set; }

    [ObservableProperty]
    public partial bool IsScrollableHorizontally { get; set; }

    protected override void OnLoaded()
    {
        GuiComponent.MouseInputManager.Register(this);

        _verticalScrollBar?.ViewportSize = _scrollContentPresenter!.ActualHeight;
        _verticalScrollBar?.Maximum = _scrollContentPresenter!.ScrollableHeight;
        _verticalScrollBar?.ValueChanged += VerticalScrollBar_ValueChanged;

        _horizontalScrollBar?.ViewportSize = _scrollContentPresenter!.ActualWidth;
        _horizontalScrollBar?.Maximum = _scrollContentPresenter!.ScrollableWidth;
        _horizontalScrollBar?.ValueChanged += HorizontalScrollBar_ValueChanged;

        _scrollContentPresenter?.PropertyChanged += ScrollViewer_PropertyChanged;
    }

    private void ScrollViewer_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == "ScrollableHeight")
        {
            _verticalScrollBar?.ViewportSize = _scrollContentPresenter!.ActualHeight;
            _verticalScrollBar?.Maximum = _scrollContentPresenter!.ScrollableHeight;
        }
        else if (e.PropertyName == "ScrollableWidth")
        {
            _horizontalScrollBar?.ViewportSize = _scrollContentPresenter!.ActualWidth;
            _horizontalScrollBar?.Maximum = _scrollContentPresenter!.ScrollableWidth;
        }
    }

    protected override void OnUnloaded()
    {
        GuiComponent.MouseInputManager.Unregister(this);

        _verticalScrollBar?.ValueChanged -= VerticalScrollBar_ValueChanged;
        _horizontalScrollBar?.ValueChanged -= HorizontalScrollBar_ValueChanged;

        _scrollContentPresenter?.PropertyChanged -= ScrollViewer_PropertyChanged;
    }

    partial void OnIsScrollableVerticallyChanged(bool value)
    {
        _scrollContentPresenter?.CanVerticallyScroll = value;
        UpdateScrollBarsVisibility();
    }

    partial void OnIsScrollableHorizontallyChanged(bool value)
    {
        _scrollContentPresenter?.CanHorizontallyScroll = value;
        UpdateScrollBarsVisibility();
    }

    public void ScrollToHorizontalOffset(double offset) => HorizontalOffset = Math.Clamp(offset, 0, ScrollableWidth);

    public void ScrollToVerticalOffset(double offset) => VerticalOffset = Math.Clamp(offset, 0, ScrollableHeight);

    private void VerticalScrollBar_ValueChanged(object? sender, double e) => VerticalOffset = e;

    private void HorizontalScrollBar_ValueChanged(object? sender, double e) => HorizontalOffset = e;

    private void UpdateScrollBarsVisibility()
    {
        if (_verticalScrollBar == null || _horizontalScrollBar == null || _scrollContentPresenter == null) return;

        if (IsScrollableHorizontally && IsScrollableVertically)
        {
            _verticalScrollBar.Margin = new Thickness(0, 0, 0, 16);
            _horizontalScrollBar.Margin = new Thickness(0, 0, 16, 0);
            _scrollContentPresenter.Margin = new Thickness(0, 0, 20, 20);

            _verticalScrollBar.Visible = true;
            _horizontalScrollBar.Visible = true;
        }
        else if (IsScrollableVertically)
        {
            _scrollContentPresenter.Margin = new Thickness(0, 0, 20, 0);
            _verticalScrollBar.Visible = true;
            _horizontalScrollBar.Visible = false;
        }
        else if (IsScrollableHorizontally)
        {
            _scrollContentPresenter.Margin = new Thickness(0, 0, 0, 20);
            _horizontalScrollBar.Visible = true;
            _verticalScrollBar.Visible = false;
        }
        else
        {
            _scrollContentPresenter.Margin = new Thickness(0, 0, 0, 0);
            _horizontalScrollBar!.Visible = false;
            _verticalScrollBar!.Visible = false;
        }
    }
}

public partial class ScrollViewer : IMouseInputReceiver
{
    void IMouseInputReceiver.OnMouseWheelMoved(in MouseEventArgs e)
    {
        if (_templateContent == null) return;

        if (IsScrollableVertically || (IsScrollableVertically && IsScrollableHorizontally))
            ScrollToVerticalOffset(VerticalOffset - (e.ScrollWheelDelta / 6));
        else if (IsScrollableHorizontally)
            ScrollToHorizontalOffset(HorizontalOffset - (e.ScrollWheelDelta / 6));
    }
}