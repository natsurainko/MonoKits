using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Minesweeper.Game3D;
using MonoKits.Components;
using MonoKits.Gui;
using MonoKits.Gui.Input;
using MonoKits.Overrides;
using MonoKits.Spatial3D;
using MonoKits.Spatial3D.Objects;

namespace Minesweeper.Layers;

public partial class Game3DLayer : UIElement
{
    float _moveSpeed = 10f;
    double _mouseSensitivity = 0.5f;
    float _smoothing = 0.5f;

    private readonly Game _game;
    private readonly SceneManager _sceneManager;

    private ModelObject3D? _ground;
    private ModelObject3D? _board;
    private Player? _player;

    private Point _screenCenter;

    public ICamera Camera => _sceneManager.Camera;

    public Game3DLayer(Game game)
    {
        _game = game;
        _sceneManager = new(game.GraphicsDevice);
        _screenCenter = new Point(
            _game.GraphicsDevice.Viewport.Width / 2,
            _game.GraphicsDevice.Viewport.Height / 2
        );
    }

    protected override void OnLoaded()
    {
        GuiComponent.KeyboardInputManager.RegisterOnFocused(this);
        GuiComponent.MouseInputManager.Register(this);

        _ground = ModelObject3D.LoadContent(_game.Content, "Models/Test");
        _board = ModelObject3D.LoadContent(_game.Content, "Models/Board");
        _player = new Player(_game.Content.Load<Model>("Models/Block"));

        _ground.EnableDefaultLighting();
        _board.EnableDefaultLighting();
        _player.EnableDefaultLighting();

        _sceneManager.AddObject(_ground);
        _sceneManager.AddObject(_board);
        _sceneManager.AddObject(_player);

        _game.Window.ClientSizeChanged += OnClientSizeChanged;

        Camera.CameraMode = CameraMode.ThirdPerson;
        Camera.TargetDistance = 25;
        Camera.Target(_player);
    }

    protected override void OnUnloaded()
    {
        GuiComponent.KeyboardInputManager.Unregister(this);
        GuiComponent.MouseInputManager.Unregister(this);

        _game.Window.ClientSizeChanged -= OnClientSizeChanged;
    }

    private void OnClientSizeChanged(object? sender, System.EventArgs e)
    {
        _screenCenter = new Point(
            _game.GraphicsDevice.Viewport.Width / 2,
            _game.GraphicsDevice.Viewport.Height / 2
        );
    }

    public override void Update(GameTime gameTime)
    {
        _sceneManager.Update(gameTime);

        if (_playerMovementDirection != Vector3.Zero && _player != null)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            Vector3 scaledMovement = _playerMovementDirection * _moveSpeed * deltaTime;
            _player.Move(scaledMovement);
        }

        if (_cameraMovementDirection != Vector3.Zero)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            Vector3 scaledMovement = _cameraMovementDirection * _moveSpeed * deltaTime;
            Camera.Move(scaledMovement);
        }
    }

    protected override void DrawOverride(GameTime gameTime)
    {
        _sceneManager?.Draw(gameTime);
    }
}

public partial class Game3DLayer : IFocusableElement, IMouseInputReceiver, IKeyboardInputReceiver
{
    private Vector3 _playerMovementDirection = Vector3.Zero;
    private Vector3 _cameraMovementDirection = Vector3.Zero;

    public bool Focusable => true;

    void IKeyboardInputReceiver.OnKeyPressed(in KeyboardEventArgs e)
    {
        if (e.Key == Keys.A)
            _playerMovementDirection.Z = -1;
        else if (e.Key == Keys.D)
            _playerMovementDirection.Z = 1;
        else if (e.Key == Keys.S)
            _playerMovementDirection.X = -1;
        else if (e.Key == Keys.W)
            _playerMovementDirection.X = 1;
        else if (e.Key == Keys.Space)
            _playerMovementDirection.Y = 1;
        else if (e.Key == Keys.LeftShift)
            _playerMovementDirection.Y = -1;

        if (e.Key == Keys.Left)
            _cameraMovementDirection.Z = -1;
        else if (e.Key == Keys.Right)
            _cameraMovementDirection.Z = 1;
        else if (e.Key == Keys.Up)
            _cameraMovementDirection.X = 1;
        else if (e.Key == Keys.Down)
            _cameraMovementDirection.X = -1;
        else if (e.Key == Keys.Add)
            _cameraMovementDirection.Y = 1;
        else if (e.Key == Keys.Subtract)
            _cameraMovementDirection.Y = -1;

        else if (e.Key == Keys.F5)
            Camera.CameraMode = (CameraMode)(((int)Camera.CameraMode + 1) % 3);
    }

    void IKeyboardInputReceiver.OnKeyReleased(in KeyboardEventArgs e)
    {
        if (e.Key == Keys.A || e.Key == Keys.D)
            _playerMovementDirection.Z = 0;
        else if (e.Key == Keys.S || e.Key == Keys.W)
            _playerMovementDirection.X = 0;
        else if (e.Key == Keys.Space || e.Key == Keys.LeftShift)
            _playerMovementDirection.Y = 0;

        if (e.Key == Keys.Left || e.Key == Keys.Right)
            _cameraMovementDirection.Z = 0;
        else if (e.Key == Keys.Down || e.Key == Keys.Up)
            _cameraMovementDirection.X = 0;
        else if (e.Key == Keys.Add || e.Key == Keys.Subtract)
            _cameraMovementDirection.Y = 0;
    }

    void IMouseInputReceiver.OnMouseMove(in MouseEventArgs e)
    {
        if (!FocusManager.IsFocused(this)) return;

        double deltaX = (e.CurrentState.X - _screenCenter.X) * _mouseSensitivity / _screenCenter.X;
        double deltaY = (e.CurrentState.Y - _screenCenter.Y) * _mouseSensitivity / _screenCenter.Y;

        if (deltaX == 0 && deltaY == 0) return;

        if (Camera.CameraMode == CameraMode.ThirdPerson)
            Camera?.Rotate(new((float)-deltaY, (float)-deltaX, 0));
        else if (Camera.CameraMode == CameraMode.FirstPerson)
            _player?.Rotate(new((float)-deltaY, (float)-deltaX, 0));
        else
            Camera?.Rotate(new((float)-deltaY, (float)-deltaX, 0));

        Mouse.SetPosition(_screenCenter.X, _screenCenter.Y);
    }

    void IFocusableElement.OnGotFocus()
    {
        _game.IsMouseVisible = false;
        Mouse.SetPosition(_screenCenter.X, _screenCenter.Y);
    }

    void IFocusableElement.OnLostFocus() 
    { 
        _game.IsMouseVisible = true;
    }
}