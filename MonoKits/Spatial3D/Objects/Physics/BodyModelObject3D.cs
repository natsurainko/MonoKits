using BepuPhysics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoKits.Extensions;
using MonoKits.Spatial3D.Physics.Interfaces;
using System.Numerics;

namespace MonoKits.Spatial3D.Objects.Physics;

public partial class BodyModelObject3D(Model model) : ModelObject3D(model), IPhysicsBody
{
    private bool _loaded = false;
    private BodyReference _bodyReference;

    public bool IsKinematic => false;

    public float Mass { get; set; } = 20f;

    public BodyDescription BodyDescription { get; set; }

    public Quaternion Orientation
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
    } = Quaternion.Identity;

    public override Microsoft.Xna.Framework.Vector3 Rotation
    {
        get => throw new InvalidOperationException("You can't use Rotation in a IPhysicsBody");
        set => throw new InvalidOperationException("You can't use Rotation in a IPhysicsBody");
    }

    void IPhysicsBody.OnLoaded(BodyReference bodyReference)
    {
        _bodyReference = bodyReference;
        _loaded = true;
    }

    void IPhysicsBody.OnUpdate(RigidPose pose)
    {
        Position = pose.Position;
        Orientation = pose.Orientation;
    }
}

public partial class BodyModelObject3D
{
    public override void Move(Microsoft.Xna.Framework.Vector3 offset)
    {
        if (!_loaded) return;

        _bodyReference.Awake = true;

        //线加速度增量运动
        Vector3 forward = Vector3.Normalize(Vector3.Transform(new Vector3(0, 0, -1), Orientation));
        Vector3 right = Vector3.Normalize(Vector3.Transform(new Vector3(1, 0, 0), Orientation));
        Vector3 impulse = Vector3.Zero;

        impulse += forward * offset.X;
        impulse += new Vector3(0, 1, 0) * offset.Y;
        impulse += right * offset.Z;

        _bodyReference.ApplyLinearImpulse(impulse);
    }

    public override void Rotate(Microsoft.Xna.Framework.Vector3 angles)
    {
        if (!_loaded) return;

        _bodyReference.Awake = true;

        //角速度增量运动
        //Quaternion deltaRotation = Quaternion.CreateFromYawPitchRoll(angles.Y, angles.X, angles.Z);
        //Orientation = Quaternion.Normalize(deltaRotation * Orientation);
        //_bodyReference.Pose.Orientation = Orientation;

        //角加速度增量运动
        Vector3 anglesTrans = Vector3.Transform(Vector3Extensions.ToNumerics(angles), Orientation);
        _bodyReference.ApplyAngularImpulse(anglesTrans);
    }

    protected override void UpdateWorldMatrix()
    {
        _worldMatrix = Microsoft.Xna.Framework.Matrix.CreateScale(Scale) *
            Microsoft.Xna.Framework.Matrix.CreateFromQuaternion(Orientation) *
            Microsoft.Xna.Framework.Matrix.CreateTranslation(Position);

        OnWorldMatrixChanged();
    }

    public override Microsoft.Xna.Framework.Quaternion GetOrientationForCamera() => Orientation;
}

public partial class BodyModelObject3D : ICollidable
{
    public bool IsCollidable { get; set; } = true;

    public int CollisionLayer { get; set; } = 1;

    public int CollisionMask { get; set; } = ~0;
}

public partial class BodyModelObject3D : IAccelerationAware
{
    public bool AffectedByGravity { get; set; } = true;

    public Vector3 CustomAcceleration { get; set; } = Vector3.Zero;
}

public partial class BodyModelObject3D
{
    public static new BodyModelObject3D LoadFromContent(ContentManager content, string modelPath) => new(content.Load<Model>(modelPath));
}