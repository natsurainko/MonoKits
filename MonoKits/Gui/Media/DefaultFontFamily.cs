using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonoKits.Gui.Media;

public class DefaultFontFamily : IFontFamily
{
    private float _scale = 1.0f;
    private readonly SpriteFont _spriteFont;

    public float LineSpacing
    {
        get => field;
        private set => field = value;
    }

    public float FontSize
    {
        get => field;
        set
        {
            if (field != value)
            {
                field = value;
                _scale = value / _spriteFont.LineSpacing;

                LineSpacing = _spriteFont.LineSpacing * _scale;
            }
        }
    }

    public DefaultFontFamily(SpriteFont spriteFont, float fontSize = default)
    {
        _spriteFont = spriteFont;

        FontSize = fontSize == default
            ? spriteFont.LineSpacing
            : fontSize;
    }

    public virtual void Draw(SpriteBatch spriteBatch, string text, Vector2 position, Color color) 
        => spriteBatch.DrawString(_spriteFont, text, position, color, 0.0f, Vector2.Zero, _scale, SpriteEffects.None, 0.5f);

    public Vector2 MeasureString(string text) => _spriteFont.MeasureString(text) * _scale;
}
