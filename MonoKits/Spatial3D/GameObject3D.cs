using Microsoft.Xna.Framework;

namespace MonoKits.Spatial3D;

public abstract partial class GameObject3D
{
    protected Matrix _worldMatrix;

    /// <summary>
    /// angles = (pitch, yaw, roll)
    /// </summary>
    public virtual Vector3 Rotation
    {
        get => field;
        set
        {
            if (field != value)
            {
                field = value;
                UpdateWorldMatrix();
            }
        }
    } = Vector3.Zero;

    public virtual Vector3 Scale
    {
        get => field;
        set
        {
            if (field != value)
            {
                field = value;
                UpdateWorldMatrix();
            }
        }
    } = Vector3.One;

    public virtual Vector3 Position
    {
        get => field;
        set
        {
            if (field != value)
            {
                field = value;
                UpdateWorldMatrix();
            }
        }
    } = Vector3.Zero;

    public GameObject3D()
    {
        UpdateWorldMatrix();
    }

    public virtual void Update(GameTime gameTime) { }

    public virtual void Draw(GameTime gameTime, Matrix view, Matrix projection) { }
}

public partial class GameObject3D
{
    protected virtual void UpdateWorldMatrix()
    {
        _worldMatrix = Matrix.CreateScale(Scale) *
            Matrix.CreateRotationX(Rotation.X) *
            Matrix.CreateRotationY(Rotation.Y) *
            Matrix.CreateRotationZ(Rotation.Z) *
            Matrix.CreateTranslation(Position);
    }
}