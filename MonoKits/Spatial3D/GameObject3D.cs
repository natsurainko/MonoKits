using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace MonoKits.Spatial3D;

public partial class GameObject3D
{
    private Matrix _worldMatrix;

    public Model Model { get; }

    /// <summary>
    /// angles = (pitch, yaw, roll)
    /// </summary>
    public Vector3 Rotation
    {
        get => field;
        set
        {
            if (field != value)
            {
                field = value;
                UpdateWorldMatrix();
                UpdateBoundingSphere();
                UpdateBoundingBox();
            }
        }
    } = Vector3.Zero;

    public Vector3 Scale
    {
        get => field;
        set
        {
            if (field != value)
            {
                field = value;
                UpdateWorldMatrix();
                UpdateBoundingSphere();
                UpdateBoundingBox();
            }
        }
    } = Vector3.One;

    public Vector3 Position
    {
        get => field;
        set
        {
            if (field != value)
            {
                field = value;
                UpdateWorldMatrix();
                UpdateBoundingSphere();
                UpdateBoundingBox();
            }
        }
    } = Vector3.Zero;

    public GameObject3D(Model model)
    {
        Model = model;
        UpdateWorldMatrix();
    }

    public BoundingSphere BoundingSphere { get; private set; }

    public BoundingBox BoundingBox { get; private set; }

    public virtual void Update(GameTime gameTime) { }

    public virtual void Draw(GameTime gameTime, Matrix view, Matrix projection)
    {
        foreach (ModelMesh mesh in Model.Meshes)
        {
            foreach (BasicEffect effect in mesh.Effects.Cast<BasicEffect>())
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
            foreach (BasicEffect effect in mesh.Effects.Cast<BasicEffect>())
            {
                effect.EnableDefaultLighting();
                effect.PreferPerPixelLighting = true;
            }
        }
    }
}

public partial class GameObject3D
{
    protected virtual void UpdateBoundingBox()
    {
        BoundingBox = BoundingBox.CreateFromSphere(BoundingSphere.Transform(_worldMatrix));
    }

    protected virtual void UpdateBoundingSphere()
    {
        BoundingSphere sphere = BoundingSphere;
        sphere.Center = Position;
        BoundingSphere = sphere;
    }

    protected virtual void UpdateWorldMatrix()
    {
        _worldMatrix = Matrix.CreateScale(Scale) *
            Matrix.CreateRotationX(Rotation.X) *
            Matrix.CreateRotationY(Rotation.Y) *
            Matrix.CreateRotationZ(Rotation.Z) *
            Matrix.CreateTranslation(Position);
    }

    protected virtual BoundingSphere CreateBoundingSphere()
    {
        BoundingSphere sphere = new()
        {
            Center = Position
        };

        foreach (ModelMesh mesh in Model.Meshes)
        {
            if (sphere.Radius == 0)
                sphere = mesh.BoundingSphere;
            else
                sphere = BoundingSphere.CreateMerged(sphere, mesh.BoundingSphere);
        }

        return sphere;
    }
}

public partial class GameObject3D
{
    public static GameObject3D LoadContent(ContentManager content, string modelPath)
    {
        Model model = content.Load<Model>(modelPath);
        return new GameObject3D(model);
    }
}