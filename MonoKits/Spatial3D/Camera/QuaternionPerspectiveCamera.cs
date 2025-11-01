using Microsoft.Xna.Framework;
using MonoGame.Extended.ViewportAdapters;
using MonoKits.Extensions;

namespace MonoKits.Spatial3D.Camera;

public class QuaternionPerspectiveCamera(ViewportAdapter viewportAdapter) : GameObject3D, ICamera
{
    private readonly ViewportAdapter _viewportAdapter = viewportAdapter;
    private GameObject3D? _targetObject;

    private Vector3 _positionNow = Vector3.Zero;

    private Quaternion _quaternionTarget = Quaternion.Identity;
    private Quaternion _quaternionNow = Quaternion.Identity;

    private bool _useSmoothLerp = false;

    private Vector3 _movementAccumulator = Vector3.Zero;
    private Vector3 _rotationAccumulator = Vector3.Zero;

    private CameraMode oldCameraMode = CameraMode.Free;

    public float FieldOfView { get; set; } = MathHelper.ToRadians(45);
    public float AspectRatio => _viewportAdapter.Viewport.AspectRatio;
    public float NearPlane { get; set; } = 0.1f;
    public float FarPlane { get; set; } = 1000f;
    public CameraMode CameraMode { get; set; } = CameraMode.Free;
    public float BaseTargetDistance { get; set; } = 0.0f;

    private Vector3 ForwardTarget => Vector3.Transform(Vector3.Forward, _quaternionTarget);
    private Vector3 RightTarget => Vector3.Transform(Vector3.Right, _quaternionTarget);
    private Vector3 UpTarget => Vector3.Transform(Vector3.Up, _quaternionTarget);
    private Vector3 ForwardNow => Vector3.Transform(Vector3.Forward, _quaternionNow);
    private Vector3 RightNow => Vector3.Transform(Vector3.Right, _quaternionNow);
    private Vector3 UpNow => Vector3.Transform(Vector3.Up, _quaternionNow);


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
        _quaternionTarget = rotation * _quaternionTarget;
    }
    public void RotateByEuler(float pitch, float yaw, float roll)
    {
        RotateByAxis(Vector3.Up, yaw);
        RotateByAxis(RightTarget, pitch);
        RotateByAxis(ForwardTarget, roll);
    }
    public void RotateToEluer(float pitch, float yaw, float roll)
    {
        _quaternionTarget = Quaternion.Identity;
        RotateByEuler(pitch, yaw, roll);
    }

    public void Target(GameObject3D? target) => _targetObject = target;

    public void GetProjectionMatrix(out Matrix matrix) => matrix = Matrix.CreatePerspectiveFieldOfView(FieldOfView, AspectRatio, NearPlane, FarPlane);
    public void GetViewMatrix(out Matrix matrix) 
    {
        //切换人称检测
        if (oldCameraMode != CameraMode)
        {
            _useSmoothLerp = true;
            oldCameraMode = CameraMode;
        }

        UpdateRotation();
        UpdatePosition();

        matrix = Matrix.CreateLookAt(_positionNow, _positionNow + ForwardNow, UpNow);
    } 

    private void UpdateRotation()
    {
        //更新最终朝向
        if (CameraMode == CameraMode.FirstPerson && _targetObject != null)
        {
            Rotation = _targetObject.Rotation;
            RotateToEluer(Rotation.X, Rotation.Y, Rotation.Z);
        }
        else if (_rotationAccumulator != Vector3.Zero)
        {
            base.Rotate(_rotationAccumulator);
            //_targetObject?.Rotation = Rotation;
            _rotationAccumulator = Vector3.Zero;
            RotateToEluer(Rotation.X, Rotation.Y, Rotation.Z);
        }
        
        //旋转插值，更新当前的虚拟朝向
        if (_quaternionNow != _quaternionTarget)
        {
            if (_useSmoothLerp)
                _quaternionNow = Quaternion.Slerp(_quaternionNow, _quaternionTarget, 0.1f);
            else if ((_quaternionNow - _quaternionTarget).Length() > 0.001f)
                _quaternionNow = Quaternion.Slerp(_quaternionNow, _quaternionTarget, 0.5f);
            else
                _quaternionNow = _quaternionTarget;
        }
    }
    private void UpdatePosition()
    {
        //更新最终位置
        if (CameraMode == CameraMode.FirstPerson && _targetObject != null)
        {
            Position = _targetObject.Position;
        }
        else if (CameraMode == CameraMode.ThirdPerson && _targetObject != null)
        {
            Position = _targetObject.Position - ForwardNow * BaseTargetDistance;
        }
        else
        {
            if (_movementAccumulator != Vector3.Zero)
            {
                Position += ForwardTarget * _movementAccumulator.X;
                Position += RightTarget * _movementAccumulator.Z;
                Position += UpTarget * _movementAccumulator.Y;

                _movementAccumulator = Vector3.Zero;
            }
        }

        //切换人称时，移动插值，更新当前虚拟位置
        if (!_useSmoothLerp || (_positionNow - Position).Length() < 0.1f || CameraMode == CameraMode.Free)
        {
            _positionNow = Position;
            _useSmoothLerp = false;
            return;
        }

        _positionNow = Vector3.Lerp(_positionNow, Position, 0.1f);
    }

    public void Reset()
    {
        _positionNow = Position = Vector3.Zero;
        Rotation = Vector3.Zero;
        _quaternionNow = _quaternionTarget = Quaternion.Identity;
    }
}