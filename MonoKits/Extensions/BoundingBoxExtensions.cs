using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonoKits.Extensions;

internal class BoundingBoxExtensions
{
    public static void DrawBoundingBox(BoundingBox boundingBox, GraphicsDevice graphicsDevice, Effect sharedEffect, Matrix view, Matrix worldMatrix, Matrix projection)
    {
        if (sharedEffect is IEffectMatrices effectMatrices)
        {
            effectMatrices.World = worldMatrix;
            effectMatrices.View = view;
            effectMatrices.Projection = projection;
        }

        Color color = Color.White;
        Vector3[] corners = boundingBox.GetCorners();
        VertexPositionColor[] vertices =
        [
            new VertexPositionColor(corners[0], color),
            new VertexPositionColor(corners[1], color),
            new VertexPositionColor(corners[1], color),
            new VertexPositionColor(corners[2], color),
            new VertexPositionColor(corners[2], color),
            new VertexPositionColor(corners[3], color),
            new VertexPositionColor(corners[3], color),
            new VertexPositionColor(corners[0], color),

            new VertexPositionColor(corners[4], color),
            new VertexPositionColor(corners[5], color),
            new VertexPositionColor(corners[5], color),
            new VertexPositionColor(corners[6], color),
            new VertexPositionColor(corners[6], color),
            new VertexPositionColor(corners[7], color),
            new VertexPositionColor(corners[7], color),
            new VertexPositionColor(corners[4], color),

            new VertexPositionColor(corners[0], color),
            new VertexPositionColor(corners[4], color),
            new VertexPositionColor(corners[1], color),
            new VertexPositionColor(corners[5], color),
            new VertexPositionColor(corners[2], color),
            new VertexPositionColor(corners[6], color),
            new VertexPositionColor(corners[3], color),
            new VertexPositionColor(corners[7], color),
        ];

        for (int i = 0; i < sharedEffect.CurrentTechnique.Passes.Count; i++)
        {
            EffectPass effectPass = sharedEffect.CurrentTechnique.Passes[i];
            effectPass.Apply();
            graphicsDevice.DrawUserPrimitives(PrimitiveType.LineList, vertices, 0, 12);
        }
    }
}
