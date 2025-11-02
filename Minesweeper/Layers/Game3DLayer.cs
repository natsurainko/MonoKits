using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoKits.Components;
using MonoKits.Extensions;
using MonoKits.Gui;
using MonoKits.Gui.Input;
using MonoKits.Overrides;
using MonoKits.Spatial3D;
using MonoKits.Spatial3D.Camera;
using MonoKits.Spatial3D.Objects;
using MonoKits.Spatial3D.Objects.Physics;
using MonoKits.Spatial3D.Physics;
using System.Collections.Generic;

namespace Minesweeper.Layers;

public partial class Game3DLayer : UIElement
{
    float _moveSpeed = 50;
    double _mouseSensitivity = 0.5f;

    private readonly Game _game;
    private readonly SceneManager _sceneManager;
    private readonly PhysicsSystem _physicsSystem;

    private SpriteObject3D? _sprite;
    private SpriteObject3D? _bait;

    private StaticModelObject3D? _ground;
    private StaticModelObject3D? _board;
    private StaticModelObject3D? _house;

    private BodyModelObject3D? _player;

    private Point _screenCenter;

    public ICamera Camera => _sceneManager.Camera;

    public Game3DLayer(Game game)
    {
        _game = game;
        _sceneManager = new(game.GraphicsDevice);
        _sceneManager.DebugMode = true;
        _physicsSystem = new PhysicsSystem();
        _screenCenter = new Point(
            _game.GraphicsDevice.Viewport.Width / 2,
            _game.GraphicsDevice.Viewport.Height / 2
        );
    }

    protected override void OnLoaded()
    {
        GuiComponent.KeyboardInputManager.RegisterOnFocused(this);
        GuiComponent.MouseInputManager.Register(this);

        _game.Window.ClientSizeChanged += OnClientSizeChanged;

        _ground = StaticModelObject3D.LoadFromContent(_game.Content, "Models/Starvalley");
        _board = StaticModelObject3D.LoadFromContent(_game.Content, "Models/Board");
        _house = StaticModelObject3D.LoadFromContent(_game.Content, "Models/House");

        _sprite = SpriteObject3D.LoadFromContent(_game.GraphicsDevice, "Content/Images/title.png");
        _bait = SpriteObject3D.LoadFromContent(_game.GraphicsDevice, "Content/Images/bait.png");

        _player = BodyModelObject3D.LoadFromContent(_game.Content, "Models/Block");

        _sprite.Position = new(10, 10, 10);
        _sprite.Size = new(4.8f, 1.2f);
        _sprite.Billboard = SpriteObject3D.BillboardMode.CameraBillboard;

        _bait.Position = new(0, 10, 0);
        _bait.Size = new(4f, 1f);
        _bait.Billboard = SpriteObject3D.BillboardMode.CylindricalBillboard;

        _house.Position = new(0, 3.25f, 10);
        _board.Position = new(0, 0.5f, 0);
        _player.Position = new(0, 4, 10);

        _ground.EnableDefaultLighting();
        _board.EnableDefaultLighting();
        _house.EnableDefaultLighting();
        _player.EnableDefaultLighting();

        _sceneManager.AddObject(_ground);
        _sceneManager.AddObject(_board);
        _sceneManager.AddObject(_house);
        _sceneManager.AddObject(_player);
        _sceneManager.AddObject(_sprite);
        _sceneManager.AddObject(_bait);

        _ground.InitializeStatic(_physicsSystem, _ground.Model);
        _board.InitializeStatic(_physicsSystem, _board.Model);
        _house.InitializeStatic(_physicsSystem, _house.Model);

        _player.InitializeBody(_physicsSystem, _player.Model);

        _physicsSystem.Add(_ground);
        _physicsSystem.Add(_board);
        _physicsSystem.Add(_house);
        _physicsSystem.Add(_player);

        Camera.Position = new(0, 2, 25);
        Camera.Rotation = Vector3.Zero;
        Camera.CameraMode = CameraMode.Free;
        Camera.BaseTargetDistance = 25;
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
        if (_pressingKeys.Count != 0)
        {
            Vector2 xFlag = Vector2.Zero;
            Vector2 yFlag = Vector2.Zero;
            Vector2 zFlag = Vector2.Zero;
            Vector2 rollFlag = Vector2.Zero;

            if (_pressingKeys.Contains(Keys.A))
                zFlag.Y = 1;
            if (_pressingKeys.Contains(Keys.D))
                zFlag.X = 1;
            if (_pressingKeys.Contains(Keys.S))
                xFlag.Y = 1;
            if (_pressingKeys.Contains(Keys.W))
                xFlag.X = 1;
            if (_pressingKeys.Contains(Keys.Space))
                yFlag.X = 1;
            if (_pressingKeys.Contains(Keys.LeftShift))
                yFlag.Y = 1;

            _playerMovementDirection.Z = zFlag.X - zFlag.Y;
            _playerMovementDirection.X = xFlag.X - xFlag.Y;
            _playerMovementDirection.Y = yFlag.X - yFlag.Y;

            xFlag = Vector2.Zero;
            yFlag = Vector2.Zero;
            zFlag = Vector2.Zero;

            if (_pressingKeys.Contains(Keys.Left))
                zFlag.Y = 1;
            if (_pressingKeys.Contains(Keys.Right))
                zFlag.X = 1;
            if (_pressingKeys.Contains(Keys.Up))
                xFlag.X = 1;
            if (_pressingKeys.Contains(Keys.Down))
                xFlag.Y = 1;
            if (_pressingKeys.Contains(Keys.Add))
                yFlag.X = 1;
            if (_pressingKeys.Contains(Keys.Subtract))
                yFlag.Y = 1;

            if (_pressingKeys.Contains(Keys.NumPad4))
                rollFlag.Y = 1;
            if (_pressingKeys.Contains(Keys.NumPad6))
                rollFlag.X = 1;

            _cameraMovementDirection.Z = zFlag.X - zFlag.Y;
            _cameraMovementDirection.X = xFlag.X - xFlag.Y;
            _cameraMovementDirection.Y = yFlag.X - yFlag.Y;
            _cameraRoll = rollFlag.X - rollFlag.Y;

            Vector2 playerYawFlag = Vector2.Zero;

            if (_pressingKeys.Contains(Keys.Z))
                playerYawFlag.X = 1;
            if (_pressingKeys.Contains(Keys.C))
                playerYawFlag.Y = 1;

            float deltaYaw = 0.01f * (playerYawFlag.Y - playerYawFlag.X);

            if (deltaYaw != 0) _player?.Rotate(new(0, (float)-deltaYaw, 0));
        }
        else
        {
            _playerMovementDirection = Vector3.Zero;
            _cameraMovementDirection = Vector3.Zero;
            _cameraRoll = 0;
        }

        _sceneManager.Update(gameTime);
        _physicsSystem.Update(gameTime);
        
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

        if (_cameraRoll != 0.0f)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            Camera?.Rotate(new(0, 0, (float)_mouseSensitivity * _cameraRoll * _moveSpeed * deltaTime));
        }
    }

    protected override void DrawOverride(GameTime gameTime)
    {
        _sceneManager?.Draw();
    }
}

public partial class Game3DLayer : IFocusableElement, IMouseInputReceiver, IKeyboardInputReceiver
{
    private readonly List<Keys> _pressingKeys = [];

    private Vector3 _playerMovementDirection = Vector3.Zero;
    private Vector3 _cameraMovementDirection = Vector3.Zero;
    float _cameraRoll = 0;

    public bool Focusable => true;

    void IKeyboardInputReceiver.OnKeyPressed(in KeyboardEventArgs e)
    {
        if (!_pressingKeys.Contains(e.Key)) _pressingKeys.Add(e.Key);

        if (e.Key == Keys.F5)
            Camera.CameraMode = (CameraMode)(((int)Camera.CameraMode + 1) % 3);
    }

    void IKeyboardInputReceiver.OnKeyReleased(in KeyboardEventArgs e)
    {
        _pressingKeys.Remove(e.Key);
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