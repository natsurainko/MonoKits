using Microsoft.Xna.Framework;
using Minesweeper.Gui.Media;
using MonoGame.Extended;
using MonoKits.Components;
using MonoKits.Gui;
using MonoKits.Gui.Controls;
using MonoKits.Gui.Input;
using System;
using Thickness = MonoKits.Gui.Thickness;

namespace Minesweeper.Layers;

internal partial class DialogLayer : Control
{
    private Game _game;

    private ToggleButton? _startButton;
    private Button? _exitButton;

    public DialogLayer(Game game)
    {
        _game = game;

        Background = Color.LightGray * 0.5f;
        Visible = false;

        Template = new ControlTemplate<DialogLayer>(static dialog =>
        {
            dialog._startButton = new()
            {
                Content = new TextBlock
                {
                    FontFamily = new DynamicFontFamily(Contents.Unifont),
                    Foreground = Color.White,
                    Text = "扫雷 Click",
                    TextWrapping = TextWrapping.NoWrap,
                }
            };
            dialog._exitButton = new()
            {
                Content = new TextBlock
                {
                    FontFamily = new DynamicFontFamily(Contents.Unifont),
                    Foreground = Color.White,
                    Text = "Exit",
                    TextWrapping = TextWrapping.NoWrap,
                }
            };

            return new Border
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Padding = new Thickness(16),
                Background = ColorHelper.FromHex("#4a4a4a"),
                Child = new StackPanel
                {
                    Spacing = 16,
                    Children =
                    {
                        new Image()
                        {
                            Texture = Contents.Title,
                        },
                        new TextBlock
                        {
                            FontFamily = new DynamicFontFamily(Contents.FontSystem.GetFont(24)),
                            Foreground = Color.White,
                            TextWrapping =  TextWrapping.NoWrap,
                            Text = "扫雷 Minesweeper",
                        },
                        new StackPanel
                        {
                            Orientation = Orientation.Horizontal,
                            Children =
                            {
                                new TextBlock()
                                {
                                    VerticalAlignment = VerticalAlignment.Center,
                                    FontFamily = new DynamicFontFamily(Contents.Unifont),
                                    Foreground = Color.White,
                                    TextWrapping =  TextWrapping.NoWrap,
                                    Text = "扫雷 Minesweeper Description",
                                },
                            }
                        },
                        new TextBox
                        {
                            Width = 200,
                            FontFamily = new DynamicFontFamily(Contents.Unifont),
                            Foreground = Color.White,
                            TextWrapping =  TextWrapping.NoWrap,
                        },
                        new TextBox
                        {
                            Width = double.NaN,
                            Height = 200,
                            FontFamily = new DynamicFontFamily(Contents.Unifont),
                            Foreground = Color.White,
                            //Text = File.ReadAllText(@"E:\Nuget\packages\microsoft.windowsappsdk\1.7.250909003\lib\net6.0-windows10.0.22621.0\Microsoft.WinUI\Themes\generic.xaml"),
                            IsScrollableVertically = true,
                            IsScrollableHorizontally = false,
                            TextWrapping = TextWrapping.Wrap
                        },
                        new StackPanel
                        {
                            HorizontalAlignment = HorizontalAlignment.Right,
                            VerticalAlignment = VerticalAlignment.Center,
                            Orientation = Orientation.Horizontal,
                            Spacing = 16,
                            Children =
                            {
                                dialog._startButton,
                                dialog._exitButton
                            }
                        }
                    }
                }
            };
        });
    }
}

internal partial class DialogLayer : IMouseInputReceiver
{
    private int clickCount = 0;

    public override bool Focusable => true;

    protected override void OnLoaded()
    {
        GuiComponent.MouseInputManager.Register(this);

        _startButton?.Clicked += StartButton_Click;
        _exitButton?.Clicked += ExitButton_Clicked;
    }

    protected override void OnUnloaded()
    {
        GuiComponent.MouseInputManager.Unregister(this);

        _startButton?.Clicked -= StartButton_Click;
        _exitButton?.Clicked -= ExitButton_Clicked;
    }

    private void ExitButton_Clicked(object? sender, EventArgs e) => _game.Exit();

    public void StartButton_Click(object? sender, EventArgs args)
    {
        clickCount++;
        ((sender as Button)!.Content as TextBlock)!.Text = $"Click Count: {clickCount}";
    }
}