using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonoKits.Spatial3D.Objects;

public partial class SpriteObject3D : GameObject3D
{
    private readonly static short[] _indices = [0, 1, 2, 0, 2, 3];
    private readonly VertexPositionTexture[] _vertices =
    [
        new(Vector3.Zero, Vector2.UnitY),
        new(Vector3.Zero, Vector2.One),
        new(Vector3.Zero, Vector2.UnitX),
        new(Vector3.Zero, Vector2.Zero),
    ];

    public Texture2D Texture { get; }

    public BasicEffect Effect { get; }

    public BillboardMode Billboard { get; set; } = BillboardMode.None;

    public Color Tint { get; set; } = Color.White;

    public Rectangle? SourceRectangle
    {
        get => field;
        set
        {
            if (field != value)
            {
                field = value;
                OnSourceRectangleChanged();
            }
        }
    }

    public Vector2 Size
    {
        get => field;
        set
        {
            if (field != value)
            {
                field = value;
                OnSizeChanged();
            }
        }
    }

    public SpriteObject3D(Texture2D texture, GraphicsDevice graphicsDevice, Vector2? size = null)
    {
        Texture = texture;
        Size = size ?? new Vector2(texture.Width, texture.Height);
        Effect = new BasicEffect(graphicsDevice)
        {
            World = _worldMatrix,
            Texture = texture,
            TextureEnabled = true,
            VertexColorEnabled = false,
            LightingEnabled = false
        };
    }

    public override void Draw(GraphicsDevice graphicsDevice, Effect sharedEffect, Matrix view, Matrix projection)
    {
        if (Billboard != BillboardMode.None)
        {
            Matrix invView = Matrix.Invert(view);
            Vector3 cameraPosition = invView.Translation;

            Matrix billboard = Billboard == BillboardMode.CameraBillboard
                ? Matrix.CreateBillboard(Position, cameraPosition, Vector3.Up, null)
                : Matrix.CreateConstrainedBillboard(Position, cameraPosition, Vector3.Up, null, null);

            billboard = Matrix.CreateScale(Scale) * Matrix.CreateRotationY(MathHelper.Pi) * billboard;

            Effect.World = billboard;
        }
        else Effect.World = _worldMatrix;

        Effect.View = view;
        Effect.Projection = projection;
        Effect.DiffuseColor = Tint.ToVector3();
        Effect.Alpha = Tint.A / 255f;

        var oldBlendState = graphicsDevice.BlendState;
        var oldDepthStencilState = graphicsDevice.DepthStencilState;
        var oldRasterizerState = graphicsDevice.RasterizerState;

        graphicsDevice.BlendState = BlendState.AlphaBlend;
        graphicsDevice.DepthStencilState = DepthStencilState.DepthRead;
        graphicsDevice.RasterizerState = RasterizerState.CullNone;

        for (int i = 0; i < Effect.CurrentTechnique.Passes.Count; i++)
        {
            EffectPass effectPass = Effect.CurrentTechnique.Passes[i];
            effectPass.Apply();
            graphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, _vertices, 0, 4, _indices, 0, 2);
        }

        graphicsDevice.BlendState = oldBlendState;
        graphicsDevice.DepthStencilState = oldDepthStencilState;
        graphicsDevice.RasterizerState = oldRasterizerState;
    }
}

public partial class SpriteObject3D
{
    protected virtual void OnSizeChanged()
    {
        float halfWidth = Size.X * 0.5f;
        float halfHeight = Size.Y * 0.5f;

        _vertices[0].Position = new Vector3(-halfWidth, -halfHeight, 0);
        _vertices[1].Position = new Vector3(halfWidth, -halfHeight, 0);
        _vertices[2].Position = new Vector3(halfWidth, halfHeight, 0);
        _vertices[3].Position = new Vector3(-halfWidth, halfHeight, 0);
    }

    protected virtual void OnSourceRectangleChanged()
    {
        if (!SourceRectangle.HasValue)
        {
            _vertices[0].TextureCoordinate = Vector2.UnitY;
            _vertices[1].TextureCoordinate = Vector2.One;
            _vertices[2].TextureCoordinate = Vector2.UnitX;
            _vertices[3].TextureCoordinate = Vector2.Zero;
            return;
        }

        Rectangle src = SourceRectangle.Value;
        Vector2 uvMin = new(src.X / (float)Texture.Width, src.Y / (float)Texture.Height);
        Vector2 uvMax = new((src.X + src.Width) / (float)Texture.Width, (src.Y + src.Height) / (float)Texture.Height);

        _vertices[0].TextureCoordinate = new Vector2(uvMin.X, uvMax.Y);
        _vertices[1].TextureCoordinate = new Vector2(uvMax.X, uvMax.Y);
        _vertices[2].TextureCoordinate = new Vector2(uvMax.X, uvMin.Y);
        _vertices[3].TextureCoordinate = new Vector2(uvMin.X, uvMin.Y);
    }
}

public partial class SpriteObject3D
{
    public static SpriteObject3D LoadFromContent(GraphicsDevice graphicsDevice, string texturelPath) 
        => new(Texture2D.FromFile(graphicsDevice, texturelPath), graphicsDevice);

    public static SpriteObject3D LoadFromContent(GraphicsDevice graphicsDevice, Texture2D texture2D)
        => new(texture2D, graphicsDevice);
}

public partial class SpriteObject3D
{
    public enum BillboardMode
    {
        None = 0,
        CameraBillboard = 1,
        CylindricalBillboard = 2,
    }
}