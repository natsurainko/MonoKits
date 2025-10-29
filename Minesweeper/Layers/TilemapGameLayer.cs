using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.Tiled.Renderers;
using MonoKits.Components;
using MonoKits.Gui;
using MonoKits.Gui.Input;
using MonoKits.Overrides;

namespace Minesweeper.Layers;

public partial class TilemapGameLayer : UIElement
{
    private readonly Game _game;
    private TiledMapRenderer? _tiledMapRenderer;
    //private BoxingViewportAdapter? _viewportAdapter;

    private OrthographicCamera? _camera;
    private Vector2 _cameraPosition;

    private Vector2 _movementXDirection = Vector2.Zero;
    private Vector2 _movementYDirection = Vector2.Zero;

    private readonly float _speed = 200f;

    public TilemapGameLayer(Game game)
    {
        _game = game;
    }

    protected override void OnLoaded()
    {
        //_viewportAdapter = new BoxingViewportAdapter(_game.Window, SpriteBatch!.GraphicsDevice, 800, 600);
        _camera = new OrthographicCamera(SpriteBatch!.GraphicsDevice);
        _cameraPosition = _camera.Origin;
        _camera.LookAt(_cameraPosition);
        _camera.ZoomIn(1.5f);

        GuiComponent.MouseInputManager.Register(this);
        GuiComponent.KeyboardInputManager.Register(this);

        _tiledMapRenderer = new TiledMapRenderer(SpriteBatch!.GraphicsDevice, Contents.TiledMap);
    }

    protected override void OnUnloaded()
    {
        GuiComponent.MouseInputManager.Unregister(this);
        GuiComponent.KeyboardInputManager.Unregister(this);
    }

    protected override void DrawOverride(GameTime gameTime)
    {
        if (_camera != null)
            _tiledMapRenderer?.Draw(_camera.GetViewMatrix());
    }

    public override void Update(GameTime gameTime)
    {
        _tiledMapRenderer?.Update(gameTime);

        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        _cameraPosition += _speed * (_movementXDirection + _movementYDirection) * deltaTime;
        _camera?.LookAt(_cameraPosition);
    }
}

public partial class TilemapGameLayer : IFocusableElement, IMouseInputReceiver, IKeyboardInputReceiver
{
    public bool Focusable => true;

    void IKeyboardInputReceiver.OnKeyPressed(in KeyboardEventArgs e)
    {
        if (e.Key == Keys.Left)
            _movementXDirection = -Vector2.UnitX;
        else if (e.Key == Keys.Right)
            _movementXDirection = Vector2.UnitX;
        else if (e.Key == Keys.Down)
            _movementYDirection = Vector2.UnitY;
        else if (e.Key == Keys.Up)
            _movementYDirection = -Vector2.UnitY;
    }

    void IKeyboardInputReceiver.OnKeyReleased(in KeyboardEventArgs e) 
    {
        if (e.Key == Keys.Left || e.Key == Keys.Right)
            _movementXDirection = Vector2.Zero;
        else if (e.Key == Keys.Down || e.Key == Keys.Up)
            _movementYDirection = Vector2.Zero;
    }
}