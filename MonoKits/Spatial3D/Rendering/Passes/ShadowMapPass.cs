using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonoKits.Spatial3D.Rendering.Passes;

public class ShadowMapPass(Effect effect) : RenderPass
{
    private readonly Effect _depthEffect = effect;
    private RenderTarget2D? _shadowMap;
    private DepthStencilState? _depthStencilState;

    public int ShadowMapSize { get; set; } = 4096;

    public override string Name => nameof(ShadowMapPass);

    public override void Initialize(GraphicsDevice graphicsDevice, RenderContext renderContext)
    {
        base.Initialize(graphicsDevice, renderContext);

        _shadowMap = new RenderTarget2D(
            graphicsDevice,
            ShadowMapSize,
            ShadowMapSize,
            true,
            SurfaceFormat.Single,
            DepthFormat.Depth24Stencil8,
            0,
            RenderTargetUsage.PreserveContents
        );

        _depthStencilState = new DepthStencilState
        {
            DepthBufferEnable = true,
            DepthBufferWriteEnable = true,
            DepthBufferFunction = CompareFunction.LessEqual
        };
    }

    public override void Execute(RenderTarget2D? input, out RenderTarget2D? output)
    {
        if (_graphicsDevice == null || _context == null)
            throw new InvalidOperationException("RenderPass not initialized");
        if (_shadowMap == null || _depthEffect == null)
            throw new InvalidOperationException("Shadow resources not initialized");

        Matrix lightViewMatrix = _context.LightViewMatrix;
        Matrix lightProjectionMatrix = _context.LightProjectionMatrix;

        _graphicsDevice.SetRenderTarget(_shadowMap);
        _graphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.White, 1.0f, 0);
        _graphicsDevice.DepthStencilState = _depthStencilState;
        _graphicsDevice.RasterizerState = RasterizerState.CullClockwise;
        output = input;

        if (_context.SceneManager == null) return;

        for (int i = 0; i < _context.SceneManager.SceneObjects.Count; i++)
        {
            GameObject3D sceneObject = _context.SceneManager.SceneObjects[i];
            _depthEffect.Parameters["World"]?.SetValue(sceneObject.WorldMatrix);
            _depthEffect.Parameters["View"]?.SetValue(lightViewMatrix);
            _depthEffect.Parameters["Projection"]?.SetValue(lightProjectionMatrix);

            sceneObject.Draw(
                _graphicsDevice,
                _depthEffect,
                lightViewMatrix,
                lightProjectionMatrix
            );
        }

        _context.FrameSharedData["ShadowMap"] = _shadowMap;
    }

    public override void Dispose()
    {
        _shadowMap?.Dispose();
        _depthStencilState?.Dispose();
    }
}