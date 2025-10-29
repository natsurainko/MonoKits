using Microsoft.Xna.Framework;

namespace MonoKits.Gui;

public record struct Thickness(double Left, double Top, double Right, double Bottom)
{
    public Thickness(double uniform) : this(uniform, uniform, uniform, uniform) { }

    public Thickness(double leftRight, double topBottom) : this(leftRight, topBottom, leftRight, topBottom) { }

    public static Rectangle operator+ (Rectangle rectangle, Thickness thickness)
    {
        rectangle.X += (int)thickness.Left;
        rectangle.Y += (int)thickness.Top;

        rectangle.Width -= (int)(thickness.Left + thickness.Right);
        rectangle.Height -= (int)(thickness.Top + thickness.Bottom);

        return rectangle;
    }
}
