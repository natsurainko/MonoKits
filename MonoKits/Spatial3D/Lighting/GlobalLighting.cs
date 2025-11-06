using Microsoft.Xna.Framework;

namespace MonoKits.Spatial3D.Lighting;

public class GlobalLighting : GameObject3D
{
    private Vector3 _lightDirection;

    private Matrix _lightViewMatrix;
    private Matrix _lightProjectionMatrix;
    private Matrix _lightViewProjectionMatrix;

    public Color Color { get; set; } = Color.White;
    public Color AmbientColor { get; set; } = new Color(0.2f, 0.2f, 0.2f);

    public float FieldOfView { get; set; } = MathHelper.PiOver2;
    public float NearPlane { get; set; } = 1f;
    public float FarPlane { get; set; } = 500f;

    public Matrix GetLightViewMatrix() => _lightViewMatrix;

    public Matrix GetLightProjectionMatrix() => _lightProjectionMatrix;

    public Matrix GetLightViewProjection() => _lightViewProjectionMatrix;

    public Vector3 GetLightDirection() => _lightDirection;

    public void FocusOnSceneCenter(Vector3 sceneCenter, float sceneRadius)
    {
        float lightDistance = sceneRadius * 2f;
        float requiredFov = 2f * (float)Math.Atan(sceneRadius / lightDistance);
        FieldOfView = Math.Min(requiredFov * 1.2f, MathHelper.PiOver2);
        NearPlane = lightDistance - sceneRadius;
        FarPlane = lightDistance + sceneRadius;

        Quaternion orientation = this.GetOrientationForCamera();
        Vector3 forward = Vector3.Transform(Vector3.Forward, orientation);
        Position = sceneCenter - Vector3.Normalize(forward) * lightDistance;
    }

    protected override void OnWorldMatrixChanged()
    {
        Quaternion orientation = this.GetOrientationForCamera();
        Vector3 forward = Vector3.Transform(Vector3.Forward, orientation);
        Vector3 up = Vector3.Transform(Vector3.Up, orientation);
        Vector3 target = Position + forward;

        _lightDirection = forward;
        _lightViewMatrix = Matrix.CreateLookAt(Position, target, up);
        _lightProjectionMatrix = Matrix.CreatePerspectiveFieldOfView(FieldOfView, 1.0f, NearPlane, FarPlane);
        _lightViewProjectionMatrix = _lightViewMatrix * _lightProjectionMatrix;
    }
}