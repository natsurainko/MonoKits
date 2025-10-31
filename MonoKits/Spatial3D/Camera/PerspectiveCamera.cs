using Microsoft.Xna.Framework;
using MonoGame.Extended.ViewportAdapters;
using MonoKits.Extensions;

namespace MonoKits.Spatial3D.Camera;

public class PerspectiveCamera(ViewportAdapter viewportAdapter) : GameObject3D, ICamera
{
    private readonly ViewportAdapter _viewportAdapter = viewportAdapter;
    private GameObject3D? _targetObject;

    private Vector3 _movementAccumulator = Vector3.Zero;
    private Vector3 _rotationAccumulator = Vector3.Zero;

    public float FieldOfView { get; set; } = MathHelper.ToRadians(45);
    public float AspectRatio => _viewportAdapter.Viewport.AspectRatio;
    public float NearPlane { get; set; } = 0.1f;
    public float FarPlane { get; set; } = 1000f;

    public CameraMode CameraMode { get; set; } = CameraMode.Free;
    public float BaseTargetDistance { get; set; } = 0.0f;

    public void GetViewMatrix(out Matrix matrix)
    {
        if (_rotationAccumulator != Vector3.Zero)
        {
            base.Rotate(_rotationAccumulator);
            _rotationAccumulator = Vector3.Zero;
        }

        Vector3 rotation = _targetObject != null && CameraMode == CameraMode.FirstPerson
            ? _targetObject.Rotation : this.Rotation;

        Quaternion orientation = QuaternionExtensions.CreateFromRotationVector3(rotation);
        Vector3 forward = Vector3.Transform(Vector3.Forward, orientation);
        Vector3 up = Vector3.Transform(Vector3.Up, orientation);

        Vector3 cameraPosition;
        Vector3 cameraTarget;

        if (CameraMode == CameraMode.FirstPerson && _targetObject != null)
        {
            cameraPosition = _targetObject.Position;
            cameraTarget = _targetObject.Position + forward;
        }
        else if (CameraMode == CameraMode.ThirdPerson && _targetObject != null)
        {
            cameraPosition = _targetObject.Position - forward * BaseTargetDistance;
            cameraTarget = _targetObject.Position;
        }
        else
        {
            Vector3 right = Vector3.Transform(Vector3.Right, orientation);

            if (_movementAccumulator != Vector3.Zero)
            {
                // Apply changes to Position
                Position += forward * _movementAccumulator.X;
                Position += Vector3.Up * _movementAccumulator.Y;
                Position += right * _movementAccumulator.Z;

                // Reset accumulator
                _movementAccumulator = Vector3.Zero;
            }

            cameraPosition = Position;
            cameraTarget = Position + forward;
        }

        matrix = Matrix.CreateLookAt(cameraPosition, cameraTarget, up);
    }

    public void GetProjectionMatrix(out Matrix matrix) => matrix = Matrix.CreatePerspectiveFieldOfView(FieldOfView, AspectRatio, NearPlane, FarPlane);

    /// <summary>
    /// Move the camera in its relative orientation
    /// </summary>
    /// <param name="offset">offset == (forward, up, right)</param>
    public override void Move(Vector3 offset)
    {
        if (CameraMode != CameraMode.Free)
        {
            _targetObject?.Move(offset);
            return;
        }

        _movementAccumulator += offset;
    }

    /// <summary>
    /// angles = (pitch, yaw, roll)
    /// </summary>
    /// <param name="angles"></param>
    public override void Rotate(Vector3 angles) => _rotationAccumulator += new Vector3(angles.X, angles.Y, angles.Z);

    public void Zoom(float value)
    {

    }

    public void Target(GameObject3D? target) => _targetObject = target;
}
