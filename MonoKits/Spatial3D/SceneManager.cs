using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.ViewportAdapters;
using MonoKits.Extensions;
using MonoKits.Spatial3D.Camera;
using MonoKits.Spatial3D.Physics.Interfaces;

namespace MonoKits.Spatial3D;

public class SceneManager
{
    private readonly ICamera _camera;
    private readonly BasicEffect _effect;
    private readonly GraphicsDevice _graphicsDevice;

    private Matrix _viewMatrix;
    private Matrix _projectionMatrix;

    private readonly DepthStencilState _depthStencilState;
    private readonly List<GameObject3D> sceneObjects = [];

    public ICamera Camera => _camera;

    public float FieldOfView { get; set; } = MathHelper.ToRadians(45);

    public bool DebugMode { get; set; } = false;

    public SceneManager(GraphicsDevice graphicsDevice, ICamera? camera = default)
    {
        _graphicsDevice = graphicsDevice;
        _camera = camera ?? new QuaternionPerspectiveCamera(new DefaultViewportAdapter(graphicsDevice));

        _depthStencilState = new DepthStencilState
        {
            DepthBufferEnable = true,
            DepthBufferWriteEnable = true,
            DepthBufferFunction = CompareFunction.LessEqual
        };

        _effect = new BasicEffect(_graphicsDevice);
        _effect.VertexColorEnabled = true;
    }

    public void AddObject(GameObject3D obj) => sceneObjects.Add(obj);

    public void RemoveObject(GameObject3D obj) => sceneObjects.Remove(obj);

    public void ClearScene() => sceneObjects.Clear();

    public void Update(GameTime gameTime)
    {
        Camera.GetViewMatrix(out _viewMatrix);
        Camera.GetProjectionMatrix(out _projectionMatrix);

        foreach (var object3D in sceneObjects)
            object3D.Update(gameTime);
    }

    public void Draw()
    {
        var oldDepthStencilState = _graphicsDevice.DepthStencilState;
        var oldRasterizerState = _graphicsDevice.RasterizerState;

        _graphicsDevice.DepthStencilState = _depthStencilState;
        _graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
        _graphicsDevice.Clear(ClearOptions.DepthBuffer, Color.Black, 1.0f, 0);

        foreach (var object3D in sceneObjects)
        {
            object3D.Draw(_graphicsDevice, _effect, _viewMatrix, _projectionMatrix);

            if (DebugMode && object3D is IPhysicsBody)
            {
                //collidable.DrawBoundingBox(_graphicsDevice, _effect, _viewMatrix, object3D.WorldMatrix, _projectionMatrix);
                object3D.DrawRotation(_graphicsDevice, _effect, _viewMatrix, _projectionMatrix);
            }
        }

        _graphicsDevice.DepthStencilState = oldDepthStencilState;
        _graphicsDevice.RasterizerState = oldRasterizerState;
    }
}
