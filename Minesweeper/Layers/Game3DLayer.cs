using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Minesweeper.Scenes;
using Minesweeper.Scenes.Contents;
using MonoKits.Components;
using MonoKits.Gui;
using MonoKits.Gui.Input;
using MonoKits.Overrides;
using MonoKits.Spatial3D;
using MonoKits.Spatial3D.Camera;
using MonoKits.Spatial3D.Objects.Physics;
using MonoKits.Spatial3D.Physics;
using System.Collections.Generic;

namespace Minesweeper.Layers;

public partial class Game3DLayer : UIElement
{
    readonly float _moveSpeed = 10;
    readonly double _mouseSensitivity = 0.5f;

    private readonly Game _game;
    private readonly SceneManager _sceneManager;
    private readonly PhysicsSystem _physicsSystem;

    private BodyModelObject3D? Player;
    private BodyModelObject3D? Plane;

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

        MainSceneContent mainSceneContent = new(_game.Content);
        MainScene mainScene = new(mainSceneContent, _game.Content, _physicsSystem);
        mainScene.Load(_sceneManager);

        Player = mainScene.Player;
        Plane = mainScene.Plane;

        Camera.Position = new(0, 2, 25);
        Camera.Rotation = Vector3.Zero;
        Camera.CameraMode = CameraMode.Free;
        Camera.BaseTargetDistance = 25;
        Camera.SetTarget(Player);
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
        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        if (deltaTime <= 0) return;

        Vector3 playerMovementDirection = Vector3.Zero;
        Vector3 cameraMovementDirection = Vector3.Zero; 
        Vector3 planeMovementDirection = Vector3.Zero;

        Vector3 planeRotationDirection = Vector3.Zero;

        float cameraRoll = 0;
        float playerYaw = 0;

        foreach (var item in _pressingKeys)
        {
            if (Camera.Target != null && Camera.Target == Player) 
            {
                playerMovementDirection.X += item switch
                {
                    Keys.W => 1,
                    Keys.S => -1,
                    _ => 0
                };
                playerMovementDirection.Y += item switch
                {
                    Keys.Space => 1,
                    Keys.LeftShift => -1,
                    _ => 0
                };
                playerMovementDirection.Z += item switch
                {
                    Keys.A => -1,
                    Keys.D => 1,
                    _ => 0
                };
                playerYaw += item switch
                {
                    Keys.Z => 1,
                    Keys.C => -1,
                    _ => 0
                };
            }

            if (Camera.Target != null && Camera.Target == Plane)
            {
                planeMovementDirection.X += item switch
                {
                    Keys.W => 1,
                    Keys.S => -0.5f,
                    _ => 0
                };
                planeMovementDirection.Y += item switch
                {
                    Keys.Space => 1,
                    Keys.LeftShift => -1,
                    _ => 0
                };
                planeRotationDirection.Y += item switch
                {
                    Keys.A => 1,
                    Keys.D => -1,
                    _ => 0
                };

                if (Camera.CameraMode == CameraMode.ThirdPerson)
                {
                    planeRotationDirection.X += item switch
                    {
                        Keys.Up => 1,
                        Keys.Down => -1,
                        _ => 0
                    };
                    planeRotationDirection.Z += item switch
                    {
                        Keys.Left => 1,
                        Keys.Right => -1,
                        _ => 0
                    };
                }
            }

            if (Camera.CameraMode == CameraMode.Free)
            {
                cameraMovementDirection.X += item switch
                {
                    Keys.Up => 1,
                    Keys.Down => -1,
                    _ => 0
                };
                cameraMovementDirection.Y += item switch
                {
                    Keys.Add => 1,
                    Keys.Subtract => -1,
                    _ => 0
                };
                cameraMovementDirection.Z += item switch
                {
                    Keys.Left => -1,
                    Keys.Right => 1,
                    _ => 0
                };
                cameraRoll += item switch
                {
                    Keys.NumPad4 => -1,
                    Keys.NumPad6 => 1,
                    _ => 0
                };
            }
        }

        _sceneManager.Update(gameTime);
        _physicsSystem.Update(gameTime);

        if (playerMovementDirection != Vector3.Zero)
            Player?.Move(playerMovementDirection * _moveSpeed);
        if (playerYaw != 0.0f)
            Player?.Rotate(new(0, -playerYaw * _moveSpeed * deltaTime, 0));
        if (planeMovementDirection != Vector3.Zero)
            Plane?.Move(planeMovementDirection * _moveSpeed);
        if (planeRotationDirection != Vector3.Zero)
            Plane?.Rotate(planeRotationDirection * _moveSpeed * deltaTime);
        if (cameraMovementDirection != Vector3.Zero)
            Camera.Move(cameraMovementDirection * _moveSpeed * deltaTime);
        if (cameraRoll != 0.0f)
            Camera?.Rotate(new(0, 0, cameraRoll * _moveSpeed * deltaTime * 0.1f));
    }

    protected override void DrawOverride(GameTime gameTime)
    {
        _sceneManager?.Draw();
    }
}

public partial class Game3DLayer : IFocusableElement, IMouseInputReceiver, IKeyboardInputReceiver
{
    private readonly List<Keys> _pressingKeys = [];

    public bool Focusable => true;

    void IKeyboardInputReceiver.OnKeyPressed(in KeyboardEventArgs e)
    {
        if (!_pressingKeys.Contains(e.Key)) 
            _pressingKeys.Add(e.Key);

        if (e.Key == Keys.F5)
            Camera.CameraMode = (CameraMode)(((int)Camera.CameraMode + 1) % 3);

        if (e.Key == Keys.F6)
        {
            if (Camera.Target == null ||Camera.Target != Player) Camera.SetTarget(Player);
            else if (Camera.Target == null || Camera.Target != Plane) Camera.SetTarget(Plane);
        }
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
        {
            if (Camera.Target == Player)
                Player?.Rotate(new((float)-deltaY, (float)-deltaX, 0));

            if (Camera.Target == Plane)
                Plane?.Rotate(new((float)-deltaY * 10f, 0, (float)-deltaX * 10f));
        }
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