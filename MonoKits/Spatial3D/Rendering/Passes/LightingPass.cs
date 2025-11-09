using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonoKits.Spatial3D.Rendering.Passes;

public class LightingPass(Effect effect) : RenderPass
{
    private readonly Effect _lightingEffect = effect;

    private RenderTarget2D? _lightingOutput;
    private readonly short[] _quadIndices = [0, 1, 2, 2, 1, 3];
    private VertexPositionTexture[]? _fullScreenQuad;

    public override string Name => nameof(LightingPass);

    public override void Initialize(GraphicsDevice graphicsDevice, RenderContext renderContext)
    {
        base.Initialize(graphicsDevice, renderContext);

        var pp = graphicsDevice.PresentationParameters;
        _lightingOutput = new RenderTarget2D(
            graphicsDevice,
            pp.BackBufferWidth,
            pp.BackBufferHeight,
            false,
            SurfaceFormat.Color,
            DepthFormat.None,
            0,
            RenderTargetUsage.DiscardContents
        );

        _fullScreenQuad =
        [
            new(new Vector3(-1,  1, 0), new Vector2(0, 0)),
            new(new Vector3( 1,  1, 0), new Vector2(1, 0)),
            new(new Vector3(-1, -1, 0), new Vector2(0, 1)),
            new(new Vector3( 1, -1, 0), new Vector2(1, 1))
        ];
    }

    public override void Execute(RenderTarget2D? input, out RenderTarget2D? output)
    {
        if (_graphicsDevice == null || _context == null)
            throw new InvalidOperationException("LightingPass not initialized");

        var positionRT = _context.FrameSharedData["GBuffer_Position"] as RenderTarget2D
            ?? throw new InvalidOperationException("G-Buffer not available");
        var normalRT = _context.FrameSharedData["GBuffer_Normal"] as RenderTarget2D
            ?? throw new InvalidOperationException("G-Buffer not available");
        var albedoRT = _context.FrameSharedData["GBuffer_Albedo"] as RenderTarget2D 
            ?? throw new InvalidOperationException("G-Buffer not available");
        //var depthRT = _context.FrameSharedData["GBuffer_Depth"] as RenderTarget2D;

        _lightingEffect.Parameters["PositionTexture"]?.SetValue(positionRT);
        _lightingEffect.Parameters["NormalTexture"]?.SetValue(normalRT);
        _lightingEffect.Parameters["AlbedoTexture"]?.SetValue(albedoRT);

        RenderTarget2D? shadowMap = null;
        if (_context.FrameSharedData.TryGetValue("ShadowMap", out var shadowMapObj))
            shadowMap = shadowMapObj as RenderTarget2D;

        if (shadowMap != null)
        {
            _lightingEffect.Parameters["ShadowMap"]?.SetValue(shadowMap);
            _lightingEffect.Parameters["ShadowMapSize"]?.SetValue((float)shadowMap.Width);
            _lightingEffect.Parameters["LightViewProjection"]?.SetValue(_context.LightViewProjectionMatrix);
        }

        if (_context.GlobalLight != null)
        {
            _lightingEffect.Parameters["LightDirection"]?.SetValue(_context.GlobalLight.GetLightDirection());
            _lightingEffect.Parameters["LightColor"]?.SetValue(_context.GlobalLight.Color.ToVector3());
            _lightingEffect.Parameters["AmbientColor"]?.SetValue(_context.GlobalLight.AmbientColor.ToVector3());
            _lightingEffect.Parameters["ShadowIntensity"]?.SetValue(_context.GlobalLight.ShadowIntensity);
        }

        if (_context.Camera != null)
            _lightingEffect.Parameters["CameraPosition"]?.SetValue(_context.Camera.Position);

        _graphicsDevice.SetRenderTarget(_lightingOutput);
        _graphicsDevice.Clear(Color.Black);
        _graphicsDevice.DepthStencilState = DepthStencilState.None;
        _graphicsDevice.RasterizerState = RasterizerState.CullNone;

        for (int i = 0; i < _lightingEffect.CurrentTechnique.Passes.Count; i++)
        {
            _lightingEffect.CurrentTechnique.Passes[i].Apply();
            _graphicsDevice.DrawUserIndexedPrimitives(
                PrimitiveType.TriangleList,
                _fullScreenQuad,
                0,
                4,
                _quadIndices,
                0,
                2
            );
        }

        output = _lightingOutput;
    }

    public override void Dispose()
    {
        _lightingOutput?.Dispose();
    }
}
