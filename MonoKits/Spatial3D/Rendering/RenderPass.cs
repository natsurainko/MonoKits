using Microsoft.Xna.Framework.Graphics;

namespace MonoKits.Spatial3D.Rendering;

public abstract class RenderPass : IDisposable
{
    protected GraphicsDevice? _graphicsDevice;
    protected RenderContext? _context;

    public abstract string Name { get; }
    public bool Enabled { get; set; } = true;

    public virtual void Initialize(GraphicsDevice graphicsDevice, RenderContext renderContext)
    {
        _graphicsDevice = graphicsDevice;
        _context = renderContext;
    }

    public abstract void Execute(RenderTarget2D? input, out RenderTarget2D? output);

    public virtual void Dispose() { }
}
