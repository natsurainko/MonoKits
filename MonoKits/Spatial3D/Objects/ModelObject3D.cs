using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace MonoKits.Spatial3D.Objects;

public partial class ModelObject3D(Model model) : GameObject3D()
{
    public Model Model { get; } = model;

    public override void Draw(GraphicsDevice graphicsDevice, Effect sharedEffect, Matrix view, Matrix projection)
    {
        foreach (ModelMesh mesh in Model.Meshes)
        {
            foreach (IEffectMatrices effect in mesh.Effects.OfType<IEffectMatrices>())
            {
                effect.World = _worldMatrix;
                effect.View = view;
                effect.Projection = projection;
            }

            mesh.Draw();
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