using Microsoft.Xna.Framework;

namespace MonoKits.Spatial3D;

public interface ICamera
{
    Vector3 Position { get; set; }
    CameraMode CameraMode { get; set; }
    float TargetDistance { get; set; }

    float FieldOfView { get; }
    float AspectRatio { get; }
    float NearPlane { get; }
    float FarPlane { get; }

    void GetViewMatrix(out Matrix matrix);
    void GetProjectionMatrix(out Matrix matrix);

    void Move(Vector3 offset);
    void Rotate(Vector3 angles);

    void Target(GameObject3D? target);
}
