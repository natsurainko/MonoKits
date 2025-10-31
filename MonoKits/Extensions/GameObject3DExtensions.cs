using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoKits.Spatial3D;
using MonoKits.Spatial3D.Collision;

namespace MonoKits.Extensions;

public static class GameObject3DExtensions
{
    private readonly static VertexPositionColor[] _debugRotationLineVertices = [new(Vector3.Zero, Color.Red), new(Vector3.Forward * 2, Color.Red)];

    public static void DrawRotation(this GameObject3D gameObject3D, GraphicsDevice graphicsDevice, Effect sharedEffect, Matrix view, Matrix projection)
    {
        if (sharedEffect is IEffectMatrices effectMatrices)
        {
            effectMatrices.World = gameObject3D.WorldMatrix;
            effectMatrices.View = view;
            effectMatrices.Projection = projection;
        }

        for (int i = 0; i < sharedEffect.CurrentTechnique.Passes.Count; i++)
        {
            EffectPass effectPass = sharedEffect.CurrentTechnique.Passes[i];
            effectPass.Apply();
            graphicsDevice.DrawUserPrimitives(PrimitiveType.LineList, _debugRotationLineVertices, 0, 1);
        }
    }

    public static void DrawBoundingBox(this ICollidable collidable, GraphicsDevice graphicsDevice, Effect sharedEffect, Matrix view, Matrix worldMatrix, Matrix projection)
        => BoundingBoxExtensions.DrawBoundingBox(collidable.BoundingBox, graphicsDevice, sharedEffect, view, worldMatrix, projection);

    public static void DrawBoundingBox<TGameObject3D>(this TGameObject3D gameObject3D, GraphicsDevice graphicsDevice, Effect sharedEffect, Matrix view, Matrix projection)
        where TGameObject3D : GameObject3D, ICollidable => BoundingBoxExtensions.DrawBoundingBox(gameObject3D.BoundingBox, graphicsDevice, sharedEffect, view, gameObject3D.WorldMatrix, projection);
}
