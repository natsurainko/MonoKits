using CommunityToolkit.Mvvm.ComponentModel;

namespace MonoKits.Gui.Controls;

public partial class ContentControl : Control
{
    [ObservableProperty]
    public partial object? Content { get; set; }

    [ObservableProperty]
    public partial IDataTemplate? ContentTemplate { get; set; }

    public ContentControl()
    {
        Template = new ControlTemplate<ContentControl>(static control =>
        {
            if (control.ContentTemplate != null && control.Content != null)
                control.Content = control.ContentTemplate.ContentConstructor(control, control.Content);

            if (control.Content is UIElement uIElement)
            {
                if (uIElement.Parent != null && uIElement.Parent != control)
                    throw new InvalidOperationException();

                return uIElement;
            }

            return null;
        });
    }

    partial void OnContentChanged(object? value) => _templateApplied = false;

    partial void OnContentTemplateChanged(IDataTemplate? value) => _templateApplied = false;
}