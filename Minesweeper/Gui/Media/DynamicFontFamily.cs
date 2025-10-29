using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoKits.Gui.Media;

namespace Minesweeper.Gui.Media;

internal class DynamicFontFamily(DynamicSpriteFont spriteFont) : IFontFamily
{
    private readonly DynamicSpriteFont _spriteFont = spriteFont;

    public float LineSpacing => _spriteFont.LineHeight;

    public float FontSize => _spriteFont.FontSize;

    public virtual void Draw(SpriteBatch spriteBatch, string text, Vector2 position, Color color)
        => _spriteFont.DrawText(spriteBatch, text, position, color);

    public Vector2 MeasureString(string text)
    {
        Bounds bounds = _spriteFont.TextBounds(text, Vector2.Zero);
        return new Vector2(bounds.X2, bounds.Y2);
    }
}
