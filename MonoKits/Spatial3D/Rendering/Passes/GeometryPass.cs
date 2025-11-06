using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonoKits.Spatial3D.Rendering.Passes;

public class GeometryPass(Effect effect) : RenderPass
{
    private readonly Effect? _lightingEffect = effect;
    private RenderTarget2D? _geometryTarget;
    private DepthStencilState? _depthStencilState;
    private SamplerState? _shadowSamplerState;

    public override string Name => nameof(GeometryPass);

    public override void Initialize(GraphicsDevice graphicsDevice, RenderContext renderContext)
    {
        base.Initialize(graphicsDevice, renderContext);
        var parameters = graphicsDevice.PresentationParameters;

        _geometryTarget = new RenderTarget2D(
            _graphicsDevice,
            parameters.BackBufferWidth * 2,
            parameters.BackBufferHeight * 2,
            false,
            SurfaceFormat.HdrBlendable,
            DepthFormat.Depth24Stencil8,
            4,
            RenderTargetUsage.DiscardContents
        );

        _depthStencilState = new DepthStencilState
        {
            DepthBufferEnable = true,
            DepthBufferWriteEnable = true,
            DepthBufferFunction = CompareFunction.LessEqual
        };

        _shadowSamplerState = new SamplerState
        {
            Filter = TextureFilter.Point,
            AddressU = TextureAddressMode.Clamp,
            AddressV = TextureAddressMode.Clamp
        };
    }

    public override void Execute(RenderTarget2D? input, out RenderTarget2D? output)
    {
        if (_graphicsDevice == null || _context == null)
            throw new InvalidOperationException();
        if (_geometryTarget == null || _lightingEffect == null)
            throw new InvalidOperationException();

        Matrix viewMatrix = _context.ViewMatrix;
        Matrix projectionMatrix = _context.ProjectionMatrix;

        RenderTarget2D? shadowMap = null;

        if (_context.FrameSharedData.TryGetValue("ShadowMap", out var shadowMapObj))
            shadowMap = shadowMapObj as RenderTarget2D;

        if (shadowMap != null)
        {
            _lightingEffect.Parameters["ShadowMap"]?.SetValue(shadowMap);
            _lightingEffect.Parameters["LightViewProjection"]?.SetValue(_context.LightViewProjectionMatrix);
            _lightingEffect.Parameters["ShadowMapSize"]?.SetValue((float)shadowMap.Width);
            _graphicsDevice.SamplerStates[1] = _shadowSamplerState;
        }

        if (_context.GlobalLight != null)
        {
            _lightingEffect.Parameters["LightDirection"]?.SetValue(_context.GlobalLight.GetLightDirection());
            _lightingEffect.Parameters["LightColor"]?.SetValue(_context.GlobalLight.Color.ToVector3());
            _lightingEffect.Parameters["AmbientColor"]?.SetValue(_context.GlobalLight.AmbientColor.ToVector3());

            _lightingEffect.Parameters["LightPosition"]?.SetValue(_context.GlobalLight.Position);
        }

        _graphicsDevice.SetRenderTarget(_geometryTarget);
        _graphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.CornflowerBlue, 1.0f, 0);
        _graphicsDevice.DepthStencilState = _depthStencilState;
        _graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
        output = _geometryTarget;

        if (_context.SceneManager == null) return;

        for (int i = 0; i < _context.SceneManager.SceneObjects.Count; i++)
        {
            GameObject3D sceneObject = _context.SceneManager.SceneObjects[i];
            _lightingEffect.Parameters["World"]?.SetValue(sceneObject.WorldMatrix);
            _lightingEffect.Parameters["View"]?.SetValue(_context.ViewMatrix);
            _lightingEffect.Parameters["Projection"]?.SetValue(_context.ProjectionMatrix);

            sceneObject.Draw(
                _graphicsDevice,
                _lightingEffect,
                viewMatrix,
                projectionMatrix
            );
        }
    }
    public override void Dispose()
    {
        _lightingEffect?.Dispose();
        _geometryTarget?.Dispose();
        _depthStencilState?.Dispose();
        _shadowSamplerState?.Dispose();
    }
}