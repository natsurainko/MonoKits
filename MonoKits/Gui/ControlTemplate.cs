using MonoKits.Gui.Controls;

namespace MonoKits.Gui;

public partial interface IControlTemplate
{
    public Func<Control, UIElement?> ContentConstructor { get; }
}

public partial class ControlTemplate<TControl>(Func<TControl, UIElement?> func) 
    : IControlTemplate where TControl : Control
{
    Func<Control, UIElement?> IControlTemplate.ContentConstructor => rootControl =>
    {
        if (rootControl is not TControl control)
            throw new InvalidCastException("Root control can't cast to template target type");

        return ContentConstructor(control);
    };

    public Func<TControl, UIElement?> ContentConstructor { get; } = func;
}