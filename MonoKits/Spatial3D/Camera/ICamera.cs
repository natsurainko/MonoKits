using Microsoft.Xna.Framework;

namespace MonoKits.Spatial3D.Camera;

public interface ICamera
{
    Vector3 Position { get; set; }
    Vector3 Rotation { get; set; }

    CameraMode CameraMode { get; set; }
    float BaseTargetDistance { get; set; }

    float FieldOfView { get; }
    float AspectRatio { get; }
    float NearPlane { get; }
    float FarPlane { get; }

    GameObject3D? Target { get; }

    void GetViewMatrix(out Matrix matrix);
    void GetProjectionMatrix(out Matrix matrix);

    void Move(Vector3 offset);
    void Rotate(Vector3 angles);

    void SetTarget(GameObject3D? target);
}
