using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonoKits.Spatial3D.Objects;

public partial class LineObject3D : GameObject3D
{
    private readonly VertexPositionColor[] _vertices = new VertexPositionColor[2];

    public Color Color { get; set; } = Color.Red;

    public LineObject3D(Vector3 position, Vector3 rotation, float length)
    {
        Position = position;
        Rotation = rotation;
        Scale = new Vector3(1, 1, length);

        UpdateVertices();
    }

    public override void Draw(GraphicsDevice graphicsDevice, Effect sharedEffect, Matrix view, Matrix projection)
    {
        if (sharedEffect is IEffectMatrices effectMatrices)
        {
            effectMatrices.World = _worldMatrix;
            effectMatrices.View = view;
            effectMatrices.Projection = projection;
        }

        for (int i = 0; i < sharedEffect.CurrentTechnique.Passes.Count; i++)
        {
            EffectPass effectPass = sharedEffect.CurrentTechnique.Passes[i];
            effectPass.Apply();
            graphicsDevice.DrawUserPrimitives(PrimitiveType.LineList, _vertices, 0, 1);
        }
    }
}

public partial class LineObject3D
{
    public void UpdateVertices()
    {
        _vertices[0] = new VertexPositionColor(Vector3.Zero, Color);
        _vertices[1] = new VertexPositionColor(Vector3.Forward, Color);
    }
}