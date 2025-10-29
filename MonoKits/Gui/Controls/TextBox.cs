using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoKits.Components;
using MonoKits.Extensions;
using MonoKits.Gui.Documents;
using MonoKits.Gui.Input;
using MonoKits.Gui.Media;
using MonoKits.Native;
using MonoKits.Overrides;
using MouseEventArgs = MonoKits.Overrides.MouseEventArgs;

namespace MonoKits.Gui.Controls;

public partial class TextBox : Control
{
    protected Border? _border;
    protected TextBlock? _textBlock;
    protected TextCursor? _textCursor;
    protected ScrollViewer? _scrollViewer;

    public TextBox()
    {
        IsScrollableHorizontally = false;
        IsScrollableVertically = false;

        Width = 128;
        Height = 32;

        Template = new ControlTemplate<TextBox>(static textBox =>
        {
            textBox._textBlock = new TextBlock 
            {
                FontFamily = textBox.FontFamily,
                Foreground = textBox.Foreground,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                TextWrapping = textBox.TextWrapping,
                Text = textBox.Text
            };
            textBox._scrollViewer = new ScrollViewer
            {
                Content = textBox._textBlock,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                IsScrollableHorizontally = textBox.IsScrollableHorizontally,
                IsScrollableVertically = textBox.IsScrollableVertically,
            };
            textBox._border = new Border
            {
                Child = textBox._scrollViewer,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Padding = new Thickness(8)
            };

            textBox._textCursor = new(textBox._textBlock.InLines);
            textBox.UpdateVisualResources();

            return textBox._border;
        });

        GotFocus += OnGotFocus;
        LostFocus += OnLostFocus;
    }
}

public partial class TextBox
{
    [ObservableProperty]
    public partial bool IsScrollableHorizontally { get; set; }

    [ObservableProperty]
    public partial bool IsScrollableVertically { get; set; }

    [ObservableProperty]
    public partial string Text { get; set; } = string.Empty;

    [ObservableProperty]
    public partial IFontFamily? FontFamily { get; set; }

    [ObservableProperty]
    public partial Color? Foreground { get; set; } = Color.Black;

    [ObservableProperty]
    public partial TextWrapping TextWrapping { get; set; } = TextWrapping.NoWrap;

    public override bool Focusable => true;

    public override Dictionary<string, object> Resources { get; } = TextBoxDefaultResources;

    partial void OnTextChanged(string value) => _textBlock?.Text = value;

    partial void OnFontFamilyChanged(IFontFamily? value) => _textBlock?.FontFamily = value;

    partial void OnForegroundChanged(Color? value) => _textBlock?.Foreground = value;

    partial void OnIsScrollableHorizontallyChanged(bool value) => _scrollViewer?.IsScrollableHorizontally = value;

    partial void OnIsScrollableVerticallyChanged(bool value) => _scrollViewer?.IsScrollableVertically = value;

    protected override void OnLoaded()
    {
        GuiComponent.MouseInputManager.Register(this);
        GuiComponent.KeyboardInputManager.RegisterOnFocused(this);
    }

    protected override void OnUnloaded()
    {
        GuiComponent.MouseInputManager.Unregister(this);
        GuiComponent.KeyboardInputManager.Unregister(this);
    }
}

public partial class TextBox : IMouseInputReceiver
{
    void IMouseInputReceiver.OnMouseDown(in MouseEventArgs e) => VisualState = VisualState.Pressed;

    void IMouseInputReceiver.OnMouseRelease(in MouseEventArgs e) => VisualState = VisualState.MouseOver;

    void IMouseInputReceiver.OnMouseEnter(in MouseEventArgs e) => VisualState = VisualState.MouseOver;

    void IMouseInputReceiver.OnMouseLeave(in MouseEventArgs e) => VisualState = VisualState.Normal;
}

public partial class TextBox : ITextInputReceiver, IKeyboardInputReceiver
{
    private static readonly TimeSpan _cursorBlinkInterval = TimeSpan.FromMilliseconds(530);
    private TimeSpan _cursorBlinkTimer = TimeSpan.Zero;
    private bool _cursorVisible = true;

    void IKeyboardInputReceiver.OnKeyPressed(in KeyboardEventArgs e)
    {
        if (_textCursor == null || FontFamily == null) return;

        if (e.Key == Keys.Up) _textCursor.MoveUp();
        else if (e.Key == Keys.Down) _textCursor.MoveDown();
        else if (e.Key == Keys.Left) _textCursor.MoveLeft();
        else if (e.Key == Keys.Right) _textCursor.MoveRight();
        else if (e.Key == Keys.PageDown) _textCursor.MoveEnd();

        ResetCursorBlink();

        Vector2 position = _textCursor.GetCursorPosition(_textBlock!.Bounds);
        Rectangle cursorRectangle = new()
        {
            X = (int)(_textBlock!.Bounds.X + position.X),
            Y = (int)(_textBlock!.Bounds.Y + position.Y),
            Width = 3,
            Height = (int)FontFamily.LineSpacing,
        };

        if (!_scrollViewer!.Bounds.Contains(cursorRectangle))
        {
            if (_scrollViewer!.IsScrollableVertically)
                _scrollViewer!.ScrollToVerticalOffset(_scrollViewer.VerticalOffset - (Bounds.Y + Padding.Top + _border!.Padding.Top - (_textBlock.Bounds.Y + position.Y)));
        }
    }

    public override void Update(GameTime gameTime)
    {
        if (IsFocused)
        {
            _cursorBlinkTimer += gameTime.ElapsedGameTime;
            if (_cursorBlinkTimer >= _cursorBlinkInterval)
            {
                _cursorVisible = !_cursorVisible;
                _cursorBlinkTimer = TimeSpan.Zero;
            }
        }

        base.Update(gameTime);
    }

    protected override void DrawOverride(GameTime gameTime)
    {
        base.DrawOverride(gameTime);

        if (IsFocused && _cursorVisible && _textCursor != null)
        {
            Vector2 position = _textBlock!.Bounds.Location.ToVector2() + 
                _textCursor.GetCursorPosition(_textBlock!.Bounds);

            SDL.Rectangle rectangle = new()
            {
                X = Bounds.X,
                Y = Bounds.Y,
                Width = (int)ActualWidth,
                Height = (int)ActualHeight
            };

            if (_scrollViewer!.Bounds.Contains(position))
            {
                Rectangle cursorRectangle = new()
                {
                    X = (int)position.X,
                    Y = (int)position.Y,
                    Width = 3,
                    Height = (int)((FontFamily?.LineSpacing ?? 0)),
                };

                rectangle.X = cursorRectangle.X;
                rectangle.Y = cursorRectangle.Y;
                rectangle.Width = cursorRectangle.Width;
                rectangle.Height = cursorRectangle.Height;

                SpriteBatch?.Draw(GuiComponent.ControlDefaultTexture, cursorRectangle, Foreground ?? Color.Black);
            }

            SDL.SetTextInputRect(ref rectangle);
        }
    }

    private void OnGotFocus(object? sender, EventArgs e)
    {
        if (Bounds != Rectangle.Empty)
        {
            SDL.Rectangle rectangle = new()
            {
                X = Bounds.X,
                Y = Bounds.Y,
                Width = Bounds.Width,
                Height = Bounds.Height
            };

            SDL.SetTextInputRect(ref rectangle);
        }

        SDL.StartTextInput();
        ResetCursorBlink();
    }

    private void OnLostFocus(object? sender, EventArgs e) => SDL.StopTextInput();

    void ITextInputReceiver.OnTextInput(TextInputEventArgs eventArgs)
    {
        if (_textCursor == null) return;

        char character = eventArgs.Character;

        if (char.IsControl(character))
        {
            if (character == '\b')
                _textCursor.Delete();
            else if (character == '\r' || character == '\n')
                _textCursor.Insert('\n');
        }
        else _textCursor.Insert(character);

        _textBlock?.InvalidateVisual();
        ResetCursorBlink();
    }

    private void ResetCursorBlink()
    {
        _cursorVisible = true;
        _cursorBlinkTimer = TimeSpan.Zero;
    }
}

public partial class TextBox
{
    protected override void UpdateVisualResources()
    {
        if (VisualState == VisualState.Normal)
        {
            _border?.Background = (Color)Resources["TextBox.Normal.Background"];
            _border?.Resources["Border.TopLeft"] = ((Color)Resources["TextBox.Normal.Border.TopLeft"]).WithOpacity(0.75);
            _border?.Resources["Border.BottomRight"] = (Color)Resources["TextBox.Normal.Border.BottomRight"];
        }
        else if (VisualState == VisualState.MouseOver)
        {
            _border?.Background = (Color)Resources["TextBox.MouseOver.Background"];
            _border?.Resources["Border.TopLeft"] = ((Color)Resources["TextBox.MouseOver.Border.TopLeft"]).WithOpacity(0.75);
            _border?.Resources["Border.BottomRight"] = (Color)Resources["TextBox.MouseOver.Border.BottomRight"];
        }
        else if (VisualState == VisualState.Pressed)
        {
            _border?.Background = (Color)Resources["TextBox.Pressed.Background"];
            _border?.Resources["Border.TopLeft"] = ((Color)Resources["TextBox.Pressed.Border.TopLeft"]);
            _border?.Resources["Border.BottomRight"] = (Color)Resources["TextBox.Pressed.Border.BottomRight"];
        }
    }

    public static Dictionary<string, object> TextBoxDefaultResources => new()
    {
        { "TextBox.MouseOver.Background", ColorHelper.FromHex("#5f5f5f") },
        { "TextBox.MouseOver.Border.BottomRight", ColorHelper.FromHex("#787878") },
        { "TextBox.MouseOver.Border.TopLeft", ColorHelper.FromHex("#3e3e3e") },
        { "TextBox.Normal.Background", ColorHelper.FromHex("#4a4a4a") },
        { "TextBox.Normal.Border.BottomRight", ColorHelper.FromHex("#787878") },
        { "TextBox.Normal.Border.TopLeft", ColorHelper.FromHex("#303030") },
        { "TextBox.Pressed.Background", ColorHelper.FromHex("#5F5F5F") },
        { "TextBox.Pressed.Border.BottomRight", ColorHelper.FromHex("#303030") },
        { "TextBox.Pressed.Border.TopLeft", Color.Transparent },
    };
}