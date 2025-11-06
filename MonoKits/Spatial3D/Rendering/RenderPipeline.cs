using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonoKits.Spatial3D.Rendering;

public class RenderPipeline(GraphicsDevice graphicsDevice) : IDisposable
{
    private readonly List<RenderPass> _renderPasses = [];

    public RenderContext RenderContext { get; private set; } = new();

    public void Execute(out RenderTarget2D? output)
    {
        RenderTarget2D? previousOutput = null;

        for (int i = 0; i < _renderPasses.Count; i++)
            _renderPasses[i].Execute(previousOutput, out previousOutput);

        RenderContext.FrameSharedData.Clear();
        output = previousOutput;
    }

    public void AddPass(RenderPass pass)
    {
        _renderPasses.Add(pass);
        pass.Initialize(graphicsDevice, RenderContext);
    }

    public void AddPass<TRenderPass>()
        where TRenderPass : RenderPass, new() => AddPass(new TRenderPass());

    public void AddPass<TRenderPass>(TRenderPass pass)
        where TRenderPass : RenderPass
    {
        _renderPasses.Add(pass);
        pass.Initialize(graphicsDevice, RenderContext);
    }

    public void AddPass<TRenderPass>(Func<TRenderPass> createPass)
        where TRenderPass : RenderPass
    {
        var pass = createPass();
        _renderPasses.Add(pass);
        pass.Initialize(graphicsDevice, RenderContext);
    }

    public void RemovePass(RenderPass pass)
    {
        if (_renderPasses.Remove(pass))
            pass.Dispose();
    }

    public void Update(GameTime gameTime) => RenderContext.Update(gameTime);

    public void Dispose() { }
}
