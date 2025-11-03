using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Minesweeper.Gui.Media;
using MonoGame.Extended;
using MonoKits.Components;
using MonoKits.Gui;
using MonoKits.Gui.Controls;
using MonoKits.Gui.Input;
using MonoKits.Overrides;
using System;
using System.Collections.Generic;
using System.Timers;

using Thickness = MonoKits.Gui.Thickness;

namespace Minesweeper.Layers;

internal partial class UILayer : ContentControl
{
    private readonly Game _game;
    private readonly FramesPerSecondCounterComponent _framesCounter;
    private readonly Timer _timer = new(1000);

    private readonly StackPanel _debugStackPanel;
    private readonly TextBlock _pressingKeysTextBlock;

    private readonly List<Keys> _pressingKeys = [];

    private readonly DialogLayer _dialogLayer;
    private readonly Game3DLayer _game3DLayer;

    public UILayer(Game game, Game3DLayer game3DLayer)
    {
        _game = game;
        _game3DLayer = game3DLayer;
        _framesCounter = new(game);
        _game.Components.Add(_framesCounter);

        TextBlock fpsTextBlock = new()
        {
            FontFamily = new DynamicFontFamily(Contents.Unifont),
            Foreground = Color.White,
            Text = "FPS: -",
            TextWrapping = TextWrapping.NoWrap,
        };
        TextBlock focusedElementTextBlock = new()
        {
            FontFamily = new DynamicFontFamily(Contents.Unifont),
            Foreground = Color.White,
            Text = "Focused UIElement: -",
            TextWrapping = TextWrapping.NoWrap,
        };
        TextBlock cameraModeTextBlock = new()
        {
            FontFamily = new DynamicFontFamily(Contents.Unifont),
            Foreground = Color.White,
            Text = "Camera Mode: -",
            TextWrapping = TextWrapping.NoWrap,
        };
        TextBlock cameraPositionTextBlock = new()
        {
            FontFamily = new DynamicFontFamily(Contents.Unifont),
            Foreground = Color.White,
            Text = "Camera Position: -",
            TextWrapping = TextWrapping.NoWrap,
        };
        TextBlock cameraRotationTextBlock = new()
        {
            FontFamily = new DynamicFontFamily(Contents.Unifont),
            Foreground = Color.White,
            Text = "Camera Rotation: -",
            TextWrapping = TextWrapping.NoWrap,
        };
        TextBlock cameraTargetPositionTextBlock = new()
        {
            FontFamily = new DynamicFontFamily(Contents.Unifont),
            Foreground = Color.White,
            Text = "Camera Target Position: -",
            TextWrapping = TextWrapping.NoWrap,
        };

        _pressingKeysTextBlock = new()
        {
            FontFamily = new DynamicFontFamily(Contents.Unifont),
            Foreground = Color.White,
            Text = "Pressing Keys: ",
            TextWrapping = TextWrapping.NoWrap,
        };

        _debugStackPanel = new StackPanel
        {
            Margin = new Thickness(4),
            Visible = true,
            Children =
            {
                new TextBlock
                {
                    FontFamily = new DynamicFontFamily(Contents.Unifont),
                    Foreground = Color.White,
                    Text = "Minesweeper - Main Screen",
                    TextWrapping =  TextWrapping.NoWrap,
                },
                new TextBlock
                {
                    FontFamily = new DynamicFontFamily(Contents.Unifont),
                    Foreground = Color.LightGray,
                    Text = "DEBUG MODE",
                    TextWrapping =  TextWrapping.NoWrap,
                },
                fpsTextBlock,
                focusedElementTextBlock,
                _pressingKeysTextBlock,
                cameraModeTextBlock,
                cameraPositionTextBlock,
                cameraRotationTextBlock,
                cameraTargetPositionTextBlock
            }
        };

        _dialogLayer = new(game);

        this.Content = new CanvasPanel
        {
            Children =
            {
                _dialogLayer,
                _debugStackPanel
            }
        };

        FocusManager.GotFocus += (sender, e) => focusedElementTextBlock.Text = $"Focused UIElement: {e.GetType().Name}";
        _timer.Elapsed += (s, e) =>
        {
            fpsTextBlock.Text = $"FPS: {_framesCounter.FramesPerSecond:F2}";

            if (_game3DLayer.Camera != null)
            {
                Vector3 position = _game3DLayer.Camera.Position;
                Vector3 rotation = _game3DLayer.Camera.Rotation;

                cameraModeTextBlock.Text = $"Camera Mode: {_game3DLayer.Camera.CameraMode}";
                cameraPositionTextBlock.Text = $"Camera Position: {position}";
                cameraRotationTextBlock.Text = $"Camera Rotation: Yaw: {rotation.Y:F2}, Pitch: {rotation.X:F2}, Roll: {rotation.Z:F2}";
                cameraTargetPositionTextBlock.Text = $"Camera Target Position: {(_game3DLayer.Camera.Target == null ? "-" : _game3DLayer.Camera.Target.Position)}";
            }
        };
    } 

    private void ExitButton_Clicked(object? sender, EventArgs e) => _game.Exit();

    int clickCount = 0;

    public void StartButton_Click(object? sender, EventArgs args)
    {
        clickCount++;
        ((sender as Button)!.Content as TextBlock)!.Text = $"Click Count: {clickCount}";
    }

    protected override void OnLoaded()
    {
        _timer.Start();
        GuiComponent.KeyboardInputManager.Register(this);
    }

    protected override void OnUnloaded()
    {
        _timer.Stop();
        GuiComponent.KeyboardInputManager.Unregister(this);
    }
}

internal partial class UILayer : IKeyboardInputReceiver
{
    void IKeyboardInputReceiver.OnKeyPressed(in KeyboardEventArgs e)
    {
        if (e.Key == Keys.F3)
            _debugStackPanel.Visible = !_debugStackPanel.Visible;
        else if (e.Key == Keys.E)
        {
            _dialogLayer.Visible = !_dialogLayer.Visible;
            FocusManager.SetFocus(_dialogLayer.Visible ? _dialogLayer : _game3DLayer);
        }
        else if (_dialogLayer.Visible && e.Key == Keys.Escape)
        {
            _dialogLayer.Visible = false;
            FocusManager.SetFocus(_game3DLayer);
        }

        if (!_pressingKeys.Contains(e.Key))
        {
            _pressingKeys.Add(e.Key);

            _pressingKeysTextBlock.Visible = true;
            _pressingKeysTextBlock.Text = "Pressing Keys: " + string.Join(',', _pressingKeys);
        }
    }

    void IKeyboardInputReceiver.OnKeyReleased(in KeyboardEventArgs e)
    {
        if (_pressingKeys.Remove(e.Key))
        {
            if (_pressingKeys.Count == 0)
            {
                _pressingKeysTextBlock.Visible = false;
                _pressingKeysTextBlock.Text = "Pressing Keys: " + "NONE";
                return;
            }

            _pressingKeysTextBlock.Visible = true;
            _pressingKeysTextBlock.Text = "Pressing Keys: " + string.Join(',', _pressingKeys);
        }
    }
}