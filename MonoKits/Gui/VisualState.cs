namespace MonoKits.Gui;

public record struct VisualState(string Name)
{
    public static readonly VisualState Normal = new("Normal");
    public static readonly VisualState MouseOver = new("MouseOver");
    public static readonly VisualState Pressed = new("Pressed");
}
