using Microsoft.Xna.Framework;
using MonoGame.Extended.ViewportAdapters;

namespace MonoKits.Spatial3D;

public class PerspectiveCamera(ViewportAdapter viewportAdapter) : GameObject3D, ICamera
{
    private readonly ViewportAdapter _viewportAdapter = viewportAdapter;
    private GameObject3D? _targetObject;

    //private float _yaw;
    //private float _pitch;

    private Vector3 _movementAccumulator = Vector3.Zero;
    private Vector2 _rotationAccumulator = Vector2.Zero;

    public float FieldOfView { get; set; } = MathHelper.ToRadians(45);
    public float AspectRatio => _viewportAdapter.Viewport.AspectRatio;
    public float NearPlane { get; set; } = 0.1f;
    public float FarPlane { get; set; } = 1000f;

    public CameraMode CameraMode { get; set; } = CameraMode.Free;

    public void GetViewMatrix(out Matrix matrix)
    {
        if (_rotationAccumulator != Vector2.Zero)
        {
            float pitch = Rotation.X + _rotationAccumulator.X;
            float yaw = Rotation.Y + _rotationAccumulator.Y;

            pitch = Math.Clamp(pitch, -MathHelper.PiOver2, MathHelper.PiOver2);
            yaw = MathHelper.WrapAngle(yaw);

            // Reset accumulator
            _rotationAccumulator = Vector2.Zero;
            Rotation = new(pitch, yaw, Rotation.Z);
        }

        Quaternion orientation = Quaternion.CreateFromYawPitchRoll(Rotation.Y, Rotation.X, Rotation.Z);
        Vector3 forward = Vector3.Transform(Vector3.Forward, orientation);
        Vector3 right = Vector3.Transform(Vector3.Right, orientation);
        Vector3 up = Vector3.Transform(Vector3.Up, orientation);

        if (_movementAccumulator != Vector3.Zero)
        {
            // Apply changes to Position
            Position += forward * _movementAccumulator.X;
            Position += Vector3.Up * _movementAccumulator.Y;
            Position += right * _movementAccumulator.Z;

            // Reset accumulator
            _movementAccumulator = Vector3.Zero;
        }

        Vector3 position = CameraMode == CameraMode.Free ? Position : Vector3.Zero;

        matrix = Matrix.CreateLookAt(Position, Position + forward, up);
    }

    public void GetProjectionMatrix(out Matrix matrix) => matrix = Matrix.CreatePerspectiveFieldOfView(FieldOfView, AspectRatio, NearPlane, FarPlane);

    /// <summary>
    /// Move the camera in its relative orientation
    /// </summary>
    /// <param name="offset">offset == (forward, up, right)</param>
    public void Move(Vector3 offset) => _movementAccumulator += offset;

    /// <summary>
    /// angles = (pitch, yaw)
    /// </summary>
    /// <param name="vector2"></param>
    public void Rotate(Vector3 angles) => _rotationAccumulator += new Vector2(angles.X, angles.Y);

    public void Target(GameObject3D? target) => _targetObject = target;
}
