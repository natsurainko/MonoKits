// From MonoGame.Extended.Input.InputListeners.KeyboardListener
// Fix Performance Issues:
// Use Enum.GetValues<Keys>()
// Do not use Linq

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Input.InputListeners;

namespace MonoKits.Overrides;

public delegate void KeyboardEventHandler(object sender, in KeyboardEventArgs e);

public class KeyboardListener(KeyboardListenerSettings settings) : InputListener
{
    private static readonly Keys[] _keysValues = Enum.GetValues<Keys>();

    private bool _isInitial;
    private TimeSpan _lastPressTime;

    private Keys _previousKey;
    private KeyboardState _previousState;

    public KeyboardListener() : this(new KeyboardListenerSettings()) { }

    public bool RepeatPress { get; } = settings.RepeatPress;
    public int InitialDelay { get; } = settings.InitialDelayMilliseconds;
    public int RepeatDelay { get; } = settings.RepeatDelayMilliseconds;

    public event KeyboardEventHandler? KeyTyped;
    public event KeyboardEventHandler? KeyPressed;
    public event KeyboardEventHandler? KeyReleased;

    public override void Update(GameTime gameTime)
    {
        var currentState = Keyboard.GetState();

        RaisePressedEvents(gameTime, currentState);
        RaiseReleasedEvents(currentState);

        if (RepeatPress)
            RaiseRepeatEvents(gameTime, currentState);

        _previousState = currentState;
    }

    private void RaisePressedEvents(GameTime gameTime, KeyboardState currentState)
    {
        if (currentState.IsKeyDown(Keys.LeftAlt) || currentState.IsKeyDown(Keys.RightAlt)) return;

        for (int i = 0; i < _keysValues.Length; i++)
        {
            Keys key = _keysValues[i];
            if (currentState.IsKeyDown(key) && _previousState.IsKeyUp(key))
            {
                KeyboardEventArgs e = new(key, currentState);
                this.KeyPressed?.Invoke(this, e);

                if (e.Character.HasValue)
                    this.KeyTyped?.Invoke(this, e);

                _previousKey = key;
                _lastPressTime = gameTime.TotalGameTime;
                _isInitial = true;
            }
        }
    }

    private void RaiseReleasedEvents(KeyboardState currentState)
    {
        for (int i = 0; i < _keysValues.Length; i++)
        {
            Keys key = _keysValues[i];

            if (currentState.IsKeyUp(key) && _previousState.IsKeyDown(key))
                this.KeyReleased?.Invoke(this, new KeyboardEventArgs(key, currentState));
        }
    }

    private void RaiseRepeatEvents(GameTime gameTime, KeyboardState currentState)
    {
        var elapsedTime = (gameTime.TotalGameTime - _lastPressTime).TotalMilliseconds;

        if (currentState.IsKeyDown(_previousKey) &&
            (_isInitial && elapsedTime > InitialDelay || !_isInitial && elapsedTime > RepeatDelay))
        {
            var args = new KeyboardEventArgs(_previousKey, currentState);

            KeyPressed?.Invoke(this, args);

            if (args.Character.HasValue)
                KeyTyped?.Invoke(this, args);

            _lastPressTime = gameTime.TotalGameTime;
            _isInitial = false;
        }
    }
}