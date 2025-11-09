using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonoKits.Spatial3D.Rendering.Passes;

public class GBufferPass(Effect effect) : RenderPass
{
    private readonly Effect _gbufferEffect = effect;

    private RenderTarget2D? _positionRT;
    private RenderTarget2D? _normalRT;
    private RenderTarget2D? _albedoRT;
    private RenderTarget2D? _depthRT;

    private DepthStencilState? _depthState;

    public override string Name => nameof(GBufferPass);

    public override void Initialize(GraphicsDevice graphicsDevice, RenderContext renderContext)
    {
        base.Initialize(graphicsDevice, renderContext);

        var pp = graphicsDevice.PresentationParameters;
        int w = pp.BackBufferWidth;
        int h = pp.BackBufferHeight;

        _positionRT = new RenderTarget2D(graphicsDevice, w, h, false, SurfaceFormat.Vector4, DepthFormat.Depth24Stencil8, 0, RenderTargetUsage.DiscardContents);
        _normalRT = new RenderTarget2D(graphicsDevice, w, h, false, SurfaceFormat.Vector4, DepthFormat.None, 0, RenderTargetUsage.DiscardContents);
        _albedoRT = new RenderTarget2D(graphicsDevice, w, h, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.DiscardContents);
        _depthRT = new RenderTarget2D(graphicsDevice, w, h, false, SurfaceFormat.Single, DepthFormat.None, 0, RenderTargetUsage.DiscardContents);

        _depthState = new DepthStencilState
        {
            DepthBufferEnable = true,
            DepthBufferWriteEnable = true,
            DepthBufferFunction = CompareFunction.LessEqual
        };
    }

    public override void Execute(RenderTarget2D? input, out RenderTarget2D? output)
    {
        if (_graphicsDevice == null || _context == null)
            throw new InvalidOperationException("GBufferPass not initialized");
        if (_positionRT == null || _normalRT == null || _albedoRT == null || _depthRT == null)
            throw new InvalidOperationException("G-Buffer render targets not created");

        _graphicsDevice.SetRenderTargets(_positionRT, _normalRT, _albedoRT, _depthRT);
        _graphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.White, 1.0f, 0);
        _graphicsDevice.DepthStencilState = _depthState;
        _graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
        output = input;

        if (_context.SceneManager == null) return;

        for (int i = 0; i < _context.SceneManager.SceneObjects.Count; i++)
        {
            GameObject3D sceneObject = _context.SceneManager.SceneObjects[i];
            _gbufferEffect.Parameters["World"]?.SetValue(sceneObject.WorldMatrix);
            _gbufferEffect.Parameters["View"]?.SetValue(_context.ViewMatrix);
            _gbufferEffect.Parameters["Projection"]?.SetValue(_context.ProjectionMatrix);

            _gbufferEffect.Parameters["DiffuseColor"]?.SetValue(Color.White.ToVector4());
            _gbufferEffect.Parameters["SpecularPower"]?.SetValue(32.0f);

            sceneObject.Draw(
                _graphicsDevice,
                _gbufferEffect,
                _context.ViewMatrix,
                _context.ProjectionMatrix
            );
        }

        _context.FrameSharedData["GBuffer_Position"] = _positionRT;
        _context.FrameSharedData["GBuffer_Normal"] = _normalRT;
        _context.FrameSharedData["GBuffer_Albedo"] = _albedoRT;
        _context.FrameSharedData["GBuffer_Depth"] = _depthRT;
    }

    public override void Dispose()
    {
        _positionRT?.Dispose();
        _normalRT?.Dispose();
        _albedoRT?.Dispose();
        _depthRT?.Dispose();
        _depthState?.Dispose();
    }
}
