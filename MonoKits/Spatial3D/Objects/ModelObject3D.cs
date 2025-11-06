using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace MonoKits.Spatial3D.Objects;

public partial class ModelObject3D(Model model) : GameObject3D()
{
    public Model Model { get; } = model;

    public override void Draw(GraphicsDevice graphicsDevice, Effect sharedEffect, Matrix view, Matrix projection)
    {
        for (int i = 0; i < Model.Meshes.Count; i++)
        {
            ModelMesh mesh = Model.Meshes[i];

            for (int j = 0; j < mesh.MeshParts.Count; j++)
            {
                ModelMeshPart modelMeshPart = mesh.MeshParts[j];
                Effect effect = sharedEffect;

                if (effect is IEffectMatrices effectMatrices)
                {
                    effectMatrices.World = _worldMatrix;
                    effectMatrices.View = view;
                    effectMatrices.Projection = projection;
                }

                if (modelMeshPart.PrimitiveCount <= 0) continue;

                graphicsDevice.SetVertexBuffer(modelMeshPart.VertexBuffer);
                graphicsDevice.Indices = modelMeshPart.IndexBuffer;

                for (int k = 0; k < effect.CurrentTechnique.Passes.Count; k++)
                {
                    effect.CurrentTechnique.Passes[k].Apply();
                    graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, modelMeshPart.VertexOffset, modelMeshPart.StartIndex, modelMeshPart.PrimitiveCount);
                }
            }
        }
    }

    public void EnableDefaultLighting()
    {
        foreach (ModelMesh mesh in Model.Meshes)
        {
            foreach (BasicEffect effect in mesh.Effects.OfType<BasicEffect>())
            {
                effect.EnableDefaultLighting();
                effect.PreferPerPixelLighting = true;
            }
        }
    }
}

public partial class ModelObject3D
{
    public static ModelObject3D LoadFromContent(ContentManager content, string modelPath) => new(content.Load<Model>(modelPath));
}