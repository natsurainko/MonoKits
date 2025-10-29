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
        TextBlock positionTextBlock = new()
        {
            FontFamily = new DynamicFontFamily(Contents.Unifont),
            Foreground = Color.White,
            Text = "Position: -",
            TextWrapping = TextWrapping.NoWrap,
        };
        TextBlock directionTextBlock = new()
        {
            FontFamily = new DynamicFontFamily(Contents.Unifont),
            Foreground = Color.White,
            Text = "Directionn: -",
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
                positionTextBlock,
                directionTextBlock
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

            //if (_game3DLayer.Camera != null)
            //{
            //    Vector3 position = _game3DLayer.Camera.Position;
            //    float yaw = _game3DLayer.Camera.Yaw;
            //    float pitch = _game3DLayer.Camera.Pitch;
            //    float roll = _game3DLayer.Camera.Roll;

            //    positionTextBlock.Text = $"Position: {position.X:F2}, {position.Y:F2}, {position.Z:F2}";
            //    directionTextBlock.Text = $"Directionn: Yaw: {yaw:F2}, Pitch: {pitch:F2}, Roll: {roll:F2}";
            //}
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