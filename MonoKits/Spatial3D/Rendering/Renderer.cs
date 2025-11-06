using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonoKits.Spatial3D.Rendering;

public partial class Renderer(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch)
{
    public RenderPipeline RenderPipeline { get; } = new(graphicsDevice);

    public Rectangle? RenderBounds { get; set; }

    public void Update(GameTime gameTime)
    {
        RenderPipeline.Update(gameTime);
    }

    public void Draw(GameTime gameTime)
    {
        RenderPipeline.Execute(out RenderTarget2D? output);

        graphicsDevice.SetRenderTarget(null);

        if (output == null) return;

        if (RenderBounds == null)
            spriteBatch.Draw(output, Vector2.Zero, Color.White);
        else spriteBatch.Draw(output, RenderBounds.Value, Color.White);
    }
}