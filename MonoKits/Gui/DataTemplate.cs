using MonoKits.Gui.Controls;

namespace MonoKits.Gui;

public partial interface IDataTemplate
{
    public Func<Control, object, UIElement?> ContentConstructor { get; }
}

public class DataTemplate<TControl, TData>(Func<TControl, TData, UIElement> func) : IDataTemplate where TControl : Control
{
    Func<Control, object, UIElement?> IDataTemplate.ContentConstructor => (rootControl, data) => 
    {
        if (rootControl is not TControl control)
            throw new InvalidCastException("Root control can't cast to template target type");

        if (data is not TData t)
            throw new InvalidCastException("Data can't cast to template target data type");

        return ContentConstructor(control, t);
    };

    public Func<TControl, TData, UIElement?> ContentConstructor { get; } = func;
}