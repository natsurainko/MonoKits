using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonoKits.Gui.Media;

public interface IFontFamily
{
    float LineSpacing { get; }

    float FontSize { get; }

    Vector2 MeasureString(string text);

    void Draw(SpriteBatch spriteBatch, string text, Vector2 position, Color color);
}