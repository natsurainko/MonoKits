using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Xna.Framework;
using MonoKits.Gui.Input;

namespace MonoKits.Gui.Controls;

public partial class Control : UIElement
{
    protected bool _templateApplied = false;
    protected UIElement? _templateContent;

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
        _templateContent?.Arrange(bounds + Padding + _templateContent.Margin);

        return bounds;
    }

    protected Rectangle Base_UIElement_ArrangeOverride(Rectangle finalRect) => base.ArrangeOverride(finalRect);
}

public partial class Control : IFocusableElement
{
    [ObservableProperty]
    public virtual partial IControlTemplate Template { get; set; }

    public virtual bool Focusable => false;

    public bool IsFocused => FocusManager.IsFocused(this);

    public event EventHandler? GotFocus;
    public event EventHandler? LostFocus;

    partial void OnTemplateChanged(IControlTemplate value) => _templateApplied = false;

    void IFocusableElement.OnGotFocus() => GotFocus?.Invoke(this, EventArgs.Empty);

    void IFocusableElement.OnLostFocus() => LostFocus?.Invoke(this, EventArgs.Empty);
}

public partial class Control
{
    protected override void DrawOverride(GameTime gameTime)
    {
        base.DrawOverride(gameTime);
        _templateContent?.Draw(gameTime);
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        _templateContent?.Update(gameTime);
    }
}