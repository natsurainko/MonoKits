using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoKits.Components;
using MonoKits.Gui.Controls;
using System.ComponentModel;

namespace MonoKits.Gui;

/// <summary>
/// Provides the base class for all visual elements that participate in layout, rendering, and event handling within a
/// user interface. UIElement defines the core layout methods and lifecycle hooks for derived elements.
/// </summary>
/// <remarks>UIElement serves as the foundational class for building user interface components that require layout
/// measurement, arrangement, and visual invalidation. Derived classes should override the layout methods, such as
/// MeasureOverride and ArrangeOverride, to implement custom sizing and positioning logic. UIElement also provides hooks
/// for loaded and unloaded events, enabling elements to respond to changes in their visual tree presence. Thread safety
/// is not guaranteed; layout methods should be called from the UI thread associated with the element's
/// SynchronizationContext.</remarks>
public abstract partial class UIElement
{
    protected Size? _lastMeasureSize;
    protected Rectangle? _lastArrangeRect;

    protected bool _isLoaded = false;
    protected bool _invalidated = false;

    /// <summary>
    /// Measures the element using the specified available size and updates the DesiredSize property based on the
    /// element's visibility and layout constraints.
    /// </summary>
    /// <remarks>If the element is not visible, the DesiredSize is set to zero. Otherwise, the measurement is
    /// performed using the available size minus the element's margins. This method should be called during the layout
    /// process to ensure that the element's DesiredSize reflects its current layout requirements.</remarks>
    /// <param name="availableSize">The available space that the parent can allocate to the element. The size may be infinite to indicate that the
    /// element can be as large as it wants.</param>
    public void Measure(Size availableSize)
    {
        _lastMeasureSize = availableSize;

        Size availableContentSize = new
        (
            Math.Max(0, availableSize.Width - Margin.Left - Margin.Right),
            Math.Max(0, availableSize.Height - Margin.Top - Margin.Bottom)
        );

        DesiredSize = MeasureOverride(!Visible ? new Size(0, 0) : availableContentSize);
    }

    /// <summary>
    /// Positions and sizes the element and its child elements within the specified final rectangle.
    /// </summary>
    /// <remarks>This method should be called during the layout process to update the element's bounds and
    /// trigger any necessary loading logic. It updates the ActualWidth and ActualHeight properties based on the
    /// arranged bounds.</remarks>
    /// <param name="finalRect">The final area within the parent that this element should use to arrange itself and its children.</param>
    public void Arrange(Rectangle finalRect)
    {
        _lastArrangeRect = finalRect;
        Bounds = ArrangeOverride(finalRect);

        ActualWidth = Math.Max(0, Bounds.Width);
        ActualHeight = Math.Max(0, Bounds.Height);

        if (!_isLoaded)
        {
            _isLoaded = true;
            OnLoaded();
        }
    }

    /// <summary>
    /// Provides the measurement logic for the layout system to determine the desired size of the element based on the
    /// available space and the element's sizing properties.
    /// </summary>
    /// <remarks>Override this method in a derived class to customize how the element calculates its desired
    /// size during the measure pass of layout. The returned size should not exceed the provided availableSize. The
    /// default implementation considers the element's Width, Height, Padding, and alignment properties.</remarks>
    /// <param name="availableSize">The maximum size that the element can occupy, as determined by the layout system. This value may be infinite to
    /// indicate no constraint in a particular dimension.</param>
    /// <returns>A Size structure representing the element's desired width and height, which will be used by the layout system to
    /// arrange the element.</returns>
    protected virtual Size MeasureOverride(Size availableSize)
    {
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

        return new(desiredWidth, desiredHeight);
    }

    /// <summary>
    /// Arranges the element and its content within the specified final rectangle, determining the element's size and
    /// position based on alignment and sizing properties.
    /// </summary>
    /// <remarks>Override this method to customize how the element's size and position are determined during
    /// the layout arrange pass. The returned rectangle reflects the element's alignment, desired size, and any explicit
    /// width or height settings. Typically called by the layout system; not intended to be called directly.</remarks>
    /// <param name="finalRect">The final area within the parent that the element should use to arrange itself and its children.</param>
    /// <returns>A Rectangle that represents the actual size and position of the arranged element within the finalRect.</returns>
    protected virtual Rectangle ArrangeOverride(Rectangle finalRect)
    {
        int width, height;

        if (!double.IsNaN(Width))
            width = (int)Math.Min(Width, finalRect.Width);
        else if (HorizontalAlignment == HorizontalAlignment.Stretch)
            width = finalRect.Width;
        else
            width = (int)Math.Min(DesiredSize.Width, finalRect.Width);

        if (!double.IsNaN(Height))
            height = (int)Math.Min(Height, finalRect.Height);
        else if (VerticalAlignment == VerticalAlignment.Stretch)
            height = finalRect.Height;
        else
            height = (int)Math.Min(DesiredSize.Height, finalRect.Height);

        int x = HorizontalAlignment switch
        {
            HorizontalAlignment.Left => finalRect.X,
            HorizontalAlignment.Center => finalRect.X + (finalRect.Width - width) / 2,
            HorizontalAlignment.Right => finalRect.X + finalRect.Width - width,
            _ => finalRect.X,
        };

        int y = VerticalAlignment switch
        {
            VerticalAlignment.Top => finalRect.Y,
            VerticalAlignment.Center => finalRect.Y + (finalRect.Height - height) / 2,
            VerticalAlignment.Bottom => finalRect.Y + finalRect.Height - height,
            _ => finalRect.Y,
        };

        return new(x, y, width, height);
    }

    /// <summary>
    /// Updates the layout of the visual tree based on the specified root size.
    /// </summary>
    /// <remarks>This method must be called from the correct synchronization context, typically the UI thread,
    /// to ensure thread safety during layout operations.</remarks>
    /// <param name="rootSize">The size of the root element to use for layout calculations. Specifies the width and height available for
    /// measuring and arranging child elements.</param>
    /// <exception cref="Exception">Thrown if the current synchronization context does not match the required context for layout updates.</exception>
    public void UpdateLayout(Size rootSize)
    {
        if (SynchronizationContext.Current != SynchronizationContext) throw new Exception();

        Measure(rootSize);
        Arrange(new Rectangle(0, 0, (int)rootSize.Width, (int)rootSize.Height));
    }

    public void UpdateLayout(Size size, Rectangle rect)
    {
        if (SynchronizationContext.Current != SynchronizationContext) throw new Exception();

        Measure(size);
        Arrange(rect);
    }

    /// <summary>
    /// Invalidates the visual representation of the element and causes a layout update to be scheduled.
    /// </summary>
    /// <remarks>Call this method when the visual appearance of the element has changed and needs to be
    /// redrawn. If the element has a parent, the invalidation may propagate up the visual tree to ensure that layout
    /// and rendering are updated appropriately.</remarks>
    public void InvalidateVisual()
    {
        if (Parent != null)
        {
            if (this.Parent is CanvasPanel && _lastMeasureSize != null && _lastArrangeRect != null)
            {
                _invalidated = true;
                return;
            }

            _lastMeasureSize = null;
            _lastArrangeRect = null;

            Parent.InvalidateVisual();
        }
        else if (_lastMeasureSize != null)
        {
            _invalidated = true;
        }
    }

    protected virtual void OnLoaded() { }

    protected virtual void OnUnloaded() { }
}

public partial class UIElement : DependencyObject
{
    [ObservableProperty]
    public partial HorizontalAlignment HorizontalAlignment { get; set; } = HorizontalAlignment.Stretch;
    [ObservableProperty]
    public partial VerticalAlignment VerticalAlignment { get; set; } = VerticalAlignment.Stretch;

    [ObservableProperty]
    public partial Thickness Margin { get; set; }
    [ObservableProperty]
    public partial Thickness Padding { get; set; }

    [ObservableProperty]
    public partial double Width { get; set; } = double.NaN;
    [ObservableProperty]
    public partial double Height { get; set; } = double.NaN;

    public double ActualWidth { get; protected set; }
    public double ActualHeight { get; protected set; }

    public Rectangle Bounds { get; protected set; }
    public Size DesiredSize { get; protected set; }

    public Color? Background { get; set; }

    [ObservableProperty]
    public partial bool ClipToBounds { get; set; } = true;

    public UIElement? Parent
    {
        get => field;
        internal set
        {
            if (_isLoaded)
            {
                OnUnloaded();
                _isLoaded = false;
            }

            field = value;
        }
    }

    protected VisualState VisualState
    {
        get => field;
        set
        {
            if (field != value)
            {
                field = value;
                UpdateVisualResources();
            }
        }
    } = VisualState.Normal;

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
        this.InvalidateVisual();
    }
}

public partial class UIElement : IUpdateable, IDrawable
{
    private static RasterizerState? _scissorRasterizerState;
    protected static RasterizerState ScissorRasterizerState
    {
        get
        {
            _scissorRasterizerState ??= new RasterizerState
            {
                CullMode = CullMode.CullCounterClockwiseFace,
                ScissorTestEnable = true
            };

            return _scissorRasterizerState;
        }
    }

    public SpriteBatch? SpriteBatch
    {
        get
        {
            if (Parent is null)
                return field;

            return Parent.SpriteBatch;
        }
        internal set => field = value;
    }

    public SynchronizationContext? SynchronizationContext
    {
        get
        {
            if (Parent is null) return field;
            return Parent.SynchronizationContext;
        }
        internal set => field = value;
    }

    public virtual Dictionary<string, object> Resources { get; } = [];

    public event EventHandler<EventArgs>? EnabledChanged;
    public event EventHandler<EventArgs>? VisibleChanged;

    public event EventHandler<EventArgs>? UpdateOrderChanged;
    public event EventHandler<EventArgs>? DrawOrderChanged;

    [ObservableProperty]
    public partial bool Enabled { get; set; } = true;

    public bool Visible 
    {
        get
        {
            if (Parent != null && !Parent.Visible) return false;
            return field;
        }
        set
        {
            if (field == value) return;

            field = value;
            OnPropertyChanged(nameof(Visible));
            VisibleChanged?.Invoke(this, EventArgs.Empty);
        }
    } = true;

    partial void OnEnabledChanged(bool value) => EnabledChanged?.Invoke(this, EventArgs.Empty);

    [ObservableProperty]
    public partial int UpdateOrder { get; set; } = 0;

    [ObservableProperty]
    public partial int DrawOrder { get; set; } = 0;

    partial void OnUpdateOrderChanged(int value) => UpdateOrderChanged?.Invoke(this, EventArgs.Empty);

    partial void OnDrawOrderChanged(int value) => DrawOrderChanged?.Invoke(this, EventArgs.Empty);

    public void Draw(GameTime gameTime)
    {
        if (!Visible) return;

        if (ClipToBounds && SpriteBatch != null)
            DrawWithClipping(gameTime);
        else
            DrawOverride(gameTime);
    }

    /// <summary>
    /// Performs custom drawing logic for the control using the specified game time.
    /// </summary>
    /// <remarks>Override this method in a derived class to implement custom rendering behavior for the
    /// control. The base implementation draws the control's background if one is specified.</remarks>
    /// <param name="gameTime">An object that provides a snapshot of timing values, including elapsed game time and total game time, used for
    /// rendering calculations.</param>
    protected virtual void DrawOverride(GameTime gameTime)
    {
        if (Background.HasValue)
            SpriteBatch?.Draw(GuiComponent.ControlDefaultTexture, this.Bounds, Background.Value);
    }

    protected virtual void DrawWithClipping(GameTime gameTime)
    {
        if (Bounds.Width <= 0 || Bounds.Height <= 0) return;

        var device = SpriteBatch!.GraphicsDevice;
        var originalScissorRect = device.ScissorRectangle;
        var originalRasterizerState = device.RasterizerState;

        SpriteBatch.End();

        device.ScissorRectangle = Rectangle.Intersect(Bounds, originalScissorRect);

        SpriteBatch.Begin
        (
            SpriteSortMode.Deferred,
            BlendState.AlphaBlend,
            SamplerState.PointClamp,
            DepthStencilState.None,
            ScissorRasterizerState
        );

        DrawOverride(gameTime);

        SpriteBatch.End();

        device.ScissorRectangle = originalScissorRect;

        SpriteBatch.Begin(
            SpriteSortMode.Deferred,
            BlendState.AlphaBlend,
            SamplerState.PointClamp,
            DepthStencilState.None,
            originalRasterizerState
        );
    }

    /// <summary>
    /// Performs per-frame update logic for the element, including layout updates if necessary.
    /// </summary>
    /// <remarks>This method is typically called once per frame by the game loop. It checks whether the
    /// element's layout is invalidated and, if so, updates the layout accordingly. Override this method in a derived
    /// class to implement custom update logic.</remarks>
    /// <param name="gameTime">An object that provides a snapshot of timing values for the current update frame.</param>
    public virtual void Update(GameTime gameTime) 
    {
        if (Parent == null && _isLoaded && _invalidated)
        {
            UpdateLayout(new Size(this.ActualWidth, this.ActualHeight));
            _invalidated = false;
        }
        else if (this.Parent is CanvasPanel && _invalidated && _lastMeasureSize != null && _lastArrangeRect != null)
        {
            UpdateLayout(_lastMeasureSize.Value, _lastArrangeRect.Value);
            _invalidated = false;
            return;
        }
    }

    protected virtual void UpdateVisualResources() { }
}
