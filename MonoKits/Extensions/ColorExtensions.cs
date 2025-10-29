using Microsoft.Xna.Framework;

namespace MonoKits.Extensions;

public static class ColorExtensions
{
    public static Color WithOpacity(this Color color, double opacity)
    {
        color.A = (byte)(color.A * opacity);
        return color;
    }
}
