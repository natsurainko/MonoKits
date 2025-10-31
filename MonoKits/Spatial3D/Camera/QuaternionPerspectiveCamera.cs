using Microsoft.Xna.Framework;
using MonoGame.Extended.ViewportAdapters;

namespace MonoKits.Spatial3D.Camera;

public class QuaternionPerspectiveCamera(ViewportAdapter viewportAdapter) : GameObject3D, ICamera
{
    private readonly ViewportAdapter _viewportAdapter = viewportAdapter;
    private GameObject3D? _targetObject;

    private Quaternion _rotationFromWorldToCamera = Quaternion.Identity;

    private Vector3 _movementAccumulator = Vector3.Zero;
    private Vector3 _rotationAccumulator = Vector3.Zero;

    public float FieldOfView { get; set; } = MathHelper.ToRadians(45);
    public float AspectRatio => _viewportAdapter.Viewport.AspectRatio;
    public float NearPlane { get; set; } = 0.1f;
    public float FarPlane { get; set; } = 1000f;
    public CameraMode CameraMode { get; set; } = CameraMode.Free;
    public float BaseTargetDistance { get; set; } = 0.0f;

    public Vector3 Forward => Vector3.Transform(Vector3.Forward, _rotationFromWorldToCamera);
    public Vector3 Right => Vector3.Transform(Vector3.Right, _rotationFromWorldToCamera);
    public Vector3 Up => Vector3.Transform(Vector3.Up, _rotationFromWorldToCamera);

    /// <param name="offset">ypr = (forward, up, right)</param>
    public override void Move(Vector3 offset)
    {
        if (CameraMode != CameraMode.Free)
        {
            _targetObject?.Move(offset);
            return;
        }
        _movementAccumulator += offset;
    }

    /// <param name="angles">ypr = (pitch, yaw, roll)</param>
    public override void Rotate(Vector3 angles) => _rotationAccumulator += new Vector3(angles.X, angles.Y, angles.Z);
    public void RotateByAxis(Vector3 axis, float angle)
    {
        Quaternion rotation = Quaternion.CreateFromAxisAngle(axis, angle);
        _rotationFromWorldToCamera = rotation * _rotationFromWorldToCamera;
    }
    public void RotateByEuler(float yaw, float pitch, float roll)
    {
        RotateByAxis(Vector3.Up, yaw);
        RotateByAxis(Right, pitch);
        RotateByAxis(Forward, roll);
    }
    public void RotateToEluer(float yaw, float pitch, float roll)
    {
        _rotationFromWorldToCamera = Quaternion.Identity;
        RotateByEuler(yaw, pitch, roll);
    }

    public void Target(GameObject3D? target) => _targetObject = target;

    public void GetProjectionMatrix(out Matrix matrix) => matrix = Matrix.CreatePerspectiveFieldOfView(FieldOfView, AspectRatio, NearPlane, FarPlane);
    public void GetViewMatrix(out Matrix matrix) 
    {
        if (_rotationAccumulator != Vector3.Zero)
        {
            base.Rotate(_rotationAccumulator);

            //RotateByEuler(_rotationAccumulator.Y, _rotationAccumulator.X, _rotationAccumulator.Z);
            RotateToEluer(Rotation.Y, Rotation.X, Rotation.Z);

            _rotationAccumulator = Vector3.Zero;
        }

        Vector3 targetPosition;

        if (CameraMode == CameraMode.FirstPerson && _targetObject != null)
        {
            Position = _targetObject.Position;
            targetPosition = _targetObject.Position + Forward;
        }
        else if (CameraMode == CameraMode.ThirdPerson && _targetObject != null)
        {
            Position = _targetObject.Position - Forward * BaseTargetDistance;
            targetPosition = _targetObject.Position;
        }
        else
        {
            if (_movementAccumulator != Vector3.Zero)
            {
                Position += Forward * _movementAccumulator.X;
                Position += Right * _movementAccumulator.Z;
                Position += Up * _movementAccumulator.Y;

                _movementAccumulator = Vector3.Zero;
            }

            targetPosition = Position + Forward;

        }

        matrix = Matrix.CreateLookAt(Position, targetPosition, Up);
    } 

    public void Reset()
    {
        Position = Vector3.Zero;
        Rotation = Vector3.Zero;
        _rotationFromWorldToCamera = Quaternion.Identity;
    }
}