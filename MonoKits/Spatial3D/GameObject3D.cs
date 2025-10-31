using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonoKits.Spatial3D;

public abstract partial class GameObject3D
{
    protected Matrix _worldMatrix;

    public Matrix WorldMatrix => _worldMatrix;

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

    public virtual void Draw(GraphicsDevice graphicsDevice, Effect sharedEffect, Matrix view, Matrix projection) { }

    /// <summary>
    /// Move the object in its relative orientation
    /// </summary>
    /// <param name="offset">offset == (forward, up, right)</param>
    public virtual void Move(Vector3 offset)
    {
        Quaternion orientation = Quaternion.CreateFromYawPitchRoll(Rotation.Y, Rotation.X, Rotation.Z);

        // Add translation
        Vector3 forward = Vector3.Transform(Vector3.Forward, orientation);
        Vector3 right = Vector3.Transform(Vector3.Right, orientation);

        Position += forward * offset.X;
        Position += Vector3.Up * offset.Y;
        Position += right * offset.Z;
    }

    /// <summary>
    /// angles = (pitch, yaw, roll)
    /// </summary>
    /// <param name="angles"></param>
    public virtual void Rotate(Vector3 angles)
    {
        float pitch = Rotation.X + angles.X;
        float yaw = Rotation.Y + angles.Y;
        float roll = Rotation.Z + angles.Z;

        pitch = Math.Clamp(pitch, -MathHelper.PiOver2, MathHelper.PiOver2);
        yaw = MathHelper.WrapAngle(yaw);
        roll = MathHelper.WrapAngle(roll);

        Rotation = new(pitch, yaw, roll);
    }
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

        OnWorldMatrixChanged();
    }

    protected virtual void OnWorldMatrixChanged() { }
}