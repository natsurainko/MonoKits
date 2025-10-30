using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.ViewportAdapters;

namespace MonoKits.Spatial3D;

public class SceneManager
{
    private readonly GraphicsDevice _graphicsDevice;
    private readonly DefaultViewportAdapter _viewportAdapter;
    private readonly ICamera _camera;

    private readonly DepthStencilState _depthStencilState;
    private readonly List<GameObject3D> sceneObjects = [];

    public ICamera Camera => _camera;

    public float FieldOfView { get; set; } = MathHelper.ToRadians(45);

    public SceneManager(GraphicsDevice graphicsDevice, ICamera? camera = default)
    {
        _graphicsDevice = graphicsDevice;
        _viewportAdapter = new(graphicsDevice);
        _camera = camera ?? new QuaternionCamera(_viewportAdapter);

        _depthStencilState = new DepthStencilState
        {
            DepthBufferEnable = true,
            DepthBufferWriteEnable = true,
            DepthBufferFunction = CompareFunction.LessEqual
        };
    }

    public void AddObject(GameObject3D obj) => sceneObjects.Add(obj);

    public void RemoveObject(GameObject3D obj) => sceneObjects.Remove(obj);

    public void ClearScene() => sceneObjects.Clear();

    public void Update(GameTime gameTime)
    {
        Camera.GetViewMatrix(out viewMatrix);
        Camera.GetProjectionMatrix(out viewProjectionMatrix);

        foreach (var object3D in sceneObjects)
            object3D.Update(gameTime);
    }

    Matrix viewMatrix;
    Matrix viewProjectionMatrix;

    public void Draw(GameTime gameTime)
    {
        var oldDepthStencilState = _graphicsDevice.DepthStencilState;
        var oldRasterizerState = _graphicsDevice.RasterizerState;

        _graphicsDevice.DepthStencilState = _depthStencilState;
        _graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
        _graphicsDevice.Clear(ClearOptions.DepthBuffer, Color.Black, 1.0f, 0);

        foreach (var object3D in sceneObjects)
            object3D.Draw(gameTime, viewMatrix, viewProjectionMatrix);

        _graphicsDevice.DepthStencilState = oldDepthStencilState;
        _graphicsDevice.RasterizerState = oldRasterizerState;
    }
}
