using Microsoft.Xna.Framework;
using MonoKits.Spatial3D.Camera;
using MonoKits.Spatial3D.Lighting;

namespace MonoKits.Spatial3D.Rendering;

public class RenderContext
{
    public ICamera? Camera { get; set; }
    public GlobalLighting? GlobalLight { get; set; }
    public SceneManager? SceneManager { get; set; }
    public GameTime? GameTime { get; set; }

    public Matrix ViewMatrix;
    public Matrix ProjectionMatrix;
    public Matrix ViewProjectionMatrix;
    public Matrix InverseViewMatrix;
    public Matrix InverseProjectionMatrix;

    public Matrix LightViewMatrix;
    public Matrix LightProjectionMatrix;
    public Matrix LightViewProjectionMatrix;

    public Dictionary<string, object> FrameSharedData = [];

    public void Update(GameTime gameTime)
    {
        GameTime = gameTime;

        if (Camera != null)
        {
            Camera.GetViewMatrix(out ViewMatrix);
            Camera.GetProjectionMatrix(out ProjectionMatrix);

            ViewProjectionMatrix = ViewMatrix * ProjectionMatrix;
            InverseViewMatrix = Matrix.Invert(ViewMatrix);
            InverseProjectionMatrix = Matrix.Invert(ProjectionMatrix);
        }

        if (GlobalLight != null)
        {
            LightViewMatrix = GlobalLight.GetLightViewMatrix();
            LightProjectionMatrix = GlobalLight.GetLightProjectionMatrix();
            LightViewProjectionMatrix = GlobalLight.GetLightViewProjection();
        }
    }
}
