// From MonoGame.Extended.Input.InputListeners.KeyboardListener
// Fix Performance Issues

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Input;
using MonoGame.Extended.Input.InputListeners;
using MonoGame.Extended.ViewportAdapters;

namespace MonoKits.Overrides;

public delegate void MouseEventHandler(object sender, in MouseEventArgs e);

/// <summary>
///     Handles mouse input.
/// </summary>
/// <remarks>
///     Due to nature of the listener, even when game is not in focus, listener will continue to be updated.
///     To avoid that, manual pause of Update() method is required whenever game loses focus.
///     To avoid having to do it manually, register listener to <see cref="InputListenerComponent" />
/// </remarks>
public class MouseListener(MouseListenerSettings settings) : InputListener
{
    private MouseState _currentState;
    private bool _dragging;
    private GameTime? _gameTime;
    private bool _hasDoubleClicked;
    private MouseEventArgs? _mouseDownArgs;
    private MouseEventArgs? _previousClickArgs;
    private MouseState _previousState;

    public MouseListener() : this(new MouseListenerSettings()) { }

    public MouseListener(ViewportAdapter viewportAdapter) : this(new MouseListenerSettings())
    {
        ViewportAdapter = viewportAdapter;
    }

    public ViewportAdapter ViewportAdapter { get; } = settings.ViewportAdapter;

    public int DoubleClickMilliseconds { get; } = settings.DoubleClickMilliseconds;

    public int DragThreshold { get; } = settings.DragThreshold;

    /// <summary>
    ///     Returns true if the mouse has moved between the current and previous frames.
    /// </summary>
    /// <value><c>true</c> if the mouse has moved; otherwise, <c>false</c>.</value>
    public bool HasMouseMoved => (_previousState.X != _currentState.X) || (_previousState.Y != _currentState.Y);

    public event MouseEventHandler? MouseDown;
    public event MouseEventHandler? MouseUp;
    public event MouseEventHandler? MouseClicked;
    public event MouseEventHandler? MouseDoubleClicked;
    public event MouseEventHandler? MouseMoved;
    public event MouseEventHandler? MouseWheelMoved;
    public event MouseEventHandler? MouseDragStart;
    public event MouseEventHandler? MouseDrag;
    public event MouseEventHandler? MouseDragEnd;

    private void CheckButtonPressed(MouseButton button)
    {
        ButtonState currentButton = GetButtonState(_currentState, button);
        ButtonState previousButton = GetButtonState(_previousState, button);

        if (currentButton != ButtonState.Pressed || previousButton != ButtonState.Released) return;

        MouseEventArgs e = new(ViewportAdapter, _gameTime!.TotalGameTime, _previousState, _currentState, button);
        this.MouseDown?.Invoke(this, e);
        _mouseDownArgs = e;

        if (_previousClickArgs != null)
        {
            if ((e.Time - _previousClickArgs.Value.Time).TotalMilliseconds <= (double)DoubleClickMilliseconds)
            {
                this.MouseDoubleClicked?.Invoke(this, e);
                _hasDoubleClicked = true;
            }

            _previousClickArgs = null;
        }
    }

    private void CheckButtonReleased(MouseButton button)
    {
        ButtonState currentButton = GetButtonState(_currentState, button);
        ButtonState previousButton = GetButtonState(_previousState, button);

        if (currentButton != ButtonState.Released || previousButton != ButtonState.Pressed) return;

        MouseEventArgs e = new(ViewportAdapter, _gameTime!.TotalGameTime, _previousState, _currentState, button);

        if (_mouseDownArgs?.Button == e.Button)
        {
            if (DistanceBetween(e.Position, _mouseDownArgs.Value.Position) < DragThreshold)
            {
                if (!_hasDoubleClicked)
                {
                    this.MouseClicked?.Invoke(this, e);
                }
            }
            else
            {
                this.MouseDragEnd?.Invoke(this, e);
                _dragging = false;
            }
        }

        this.MouseUp?.Invoke(this, e);
        _hasDoubleClicked = false;
        _previousClickArgs = e;
    }

    private void CheckMouseDragged(MouseButton button)
    {
        ButtonState currentButton = GetButtonState(_currentState, button);
        ButtonState previousButton = GetButtonState(_previousState, button);

        if (currentButton != ButtonState.Pressed || previousButton != ButtonState.Pressed) return;

        MouseEventArgs e = new(ViewportAdapter, _gameTime!.TotalGameTime, _previousState, _currentState, button);

        if (_mouseDownArgs?.Button == e.Button)
        {
            if (_dragging)
            {
                this.MouseDrag?.Invoke(this, e);
            }
            else if (DistanceBetween(e.Position, _mouseDownArgs.Value.Position) > DragThreshold)
            {
                _dragging = true;
                this.MouseDragStart?.Invoke(this, e);
            }
        }
    }

    public override void Update(GameTime gameTime)
    {
        _gameTime = gameTime;
        _currentState = Mouse.GetState();

        CheckButtonPressed(MouseButton.Left);
        CheckButtonPressed(MouseButton.Middle);
        CheckButtonPressed(MouseButton.Right);
        CheckButtonPressed(MouseButton.XButton1);
        CheckButtonPressed(MouseButton.XButton2);

        CheckButtonReleased(MouseButton.Left);
        CheckButtonReleased(MouseButton.Middle);
        CheckButtonReleased(MouseButton.Right);
        CheckButtonReleased(MouseButton.XButton1);
        CheckButtonReleased(MouseButton.XButton2);

        if (HasMouseMoved)
        {
            this.MouseMoved?.Invoke(this, new MouseEventArgs(ViewportAdapter, gameTime.TotalGameTime, _previousState, _currentState));

            CheckMouseDragged(MouseButton.Left);
            CheckMouseDragged(MouseButton.Middle);
            CheckMouseDragged(MouseButton.Right);
            CheckMouseDragged(MouseButton.XButton1);
            CheckMouseDragged(MouseButton.XButton2);
        }

        if (_previousState.ScrollWheelValue != _currentState.ScrollWheelValue)
        {
            this.MouseWheelMoved?.Invoke(this, new MouseEventArgs(ViewportAdapter, gameTime.TotalGameTime, _previousState, _currentState));
        }

        _previousState = _currentState;
    }

    private static int DistanceBetween(Point a, Point b) => Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);

    private static ButtonState GetButtonState(MouseState state, MouseButton button) => button switch
    {
        MouseButton.Left => state.LeftButton,
        MouseButton.Middle => state.MiddleButton,
        MouseButton.Right => state.RightButton,
        MouseButton.XButton1 => state.XButton1,
        MouseButton.XButton2 => state.XButton2,
        _ => ButtonState.Released
    };
}