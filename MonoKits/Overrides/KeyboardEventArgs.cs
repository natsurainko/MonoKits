using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Input.InputListeners;

namespace MonoKits.Overrides;

public readonly record struct KeyboardEventArgs
{
    public Keys Key { get; }

    public KeyboardModifiers Modifiers { get; }

    public char? Character { get; }

    public KeyboardEventArgs(Keys key, KeyboardState keyboardState)
    {
        Key = key;

        Modifiers = KeyboardModifiers.None;

        if (keyboardState.IsKeyDown(Keys.LeftControl) || keyboardState.IsKeyDown(Keys.RightControl))
            Modifiers |= KeyboardModifiers.Control;

        if (keyboardState.IsKeyDown(Keys.LeftShift) || keyboardState.IsKeyDown(Keys.RightShift))
            Modifiers |= KeyboardModifiers.Shift;

        if (keyboardState.IsKeyDown(Keys.LeftAlt) || keyboardState.IsKeyDown(Keys.RightAlt))
            Modifiers |= KeyboardModifiers.Alt;

        Character = ToChar(Key, Modifiers);
    }

    private static char? ToChar(Keys key, KeyboardModifiers modifiers = KeyboardModifiers.None)
    {
        var isShiftDown = (modifiers & KeyboardModifiers.Shift) == KeyboardModifiers.Shift;
        return KeyCharMapper.ToChar(key, isShiftDown);
    }


    private static class KeyCharMapper
    {
        private static readonly char[] _normalMap = new char[256];
        private static readonly char[] _shiftMap = new char[256];

        static KeyCharMapper()
        {
            for (int i = (int)Keys.A; i <= (int)Keys.Z; i++)
            {
                _normalMap[i] = (char)(i + 32);
                _shiftMap[i] = (char)i;
            }

            for (int i = (int)Keys.D0; i <= (int)Keys.D9; i++)
                _normalMap[i] = (char)i;

            for (int i = (int)Keys.NumPad0; i <= (int)Keys.NumPad9; i++)
                _normalMap[i] = _shiftMap[i] = (char)('0' + (i - (int)Keys.NumPad0));

            _shiftMap[(int)Keys.D0] = ')';
            _shiftMap[(int)Keys.D1] = '!';
            _shiftMap[(int)Keys.D2] = '@';
            _shiftMap[(int)Keys.D3] = '#';
            _shiftMap[(int)Keys.D4] = '$';
            _shiftMap[(int)Keys.D5] = '%';
            _shiftMap[(int)Keys.D6] = '^';
            _shiftMap[(int)Keys.D7] = '&';
            _shiftMap[(int)Keys.D8] = '*';
            _shiftMap[(int)Keys.D9] = '(';

            _normalMap[(int)Keys.Space] = _shiftMap[(int)Keys.Space] = ' ';
            _normalMap[(int)Keys.Tab] = _shiftMap[(int)Keys.Tab] = '\t';
            _normalMap[(int)Keys.Enter] = _shiftMap[(int)Keys.Enter] = (char)13;
            _normalMap[(int)Keys.Back] = _shiftMap[(int)Keys.Back] = (char)8;

            _normalMap[(int)Keys.Add] = _shiftMap[(int)Keys.Add] = '+';
            _normalMap[(int)Keys.Subtract] = _shiftMap[(int)Keys.Subtract] = '-';
            _normalMap[(int)Keys.Decimal] = _shiftMap[(int)Keys.Decimal] = '.';
            _normalMap[(int)Keys.Divide] = _shiftMap[(int)Keys.Divide] = '/';
            _normalMap[(int)Keys.Multiply] = _shiftMap[(int)Keys.Multiply] = '*';

            _normalMap[(int)Keys.OemBackslash] = '\\';
            _normalMap[(int)Keys.OemComma] = ',';
            _normalMap[(int)Keys.OemOpenBrackets] = '[';
            _normalMap[(int)Keys.OemCloseBrackets] = ']';
            _normalMap[(int)Keys.OemPeriod] = '.';
            _normalMap[(int)Keys.OemPipe] = '\\';
            _normalMap[(int)Keys.OemPlus] = '=';
            _normalMap[(int)Keys.OemMinus] = '-';
            _normalMap[(int)Keys.OemQuestion] = '/';
            _normalMap[(int)Keys.OemQuotes] = '\'';
            _normalMap[(int)Keys.OemSemicolon] = ';';
            _normalMap[(int)Keys.OemTilde] = '`';

            _shiftMap[(int)Keys.OemBackslash] = '\\';
            _shiftMap[(int)Keys.OemComma] = '<';
            _shiftMap[(int)Keys.OemOpenBrackets] = '{';
            _shiftMap[(int)Keys.OemCloseBrackets] = '}';
            _shiftMap[(int)Keys.OemPeriod] = '>';
            _shiftMap[(int)Keys.OemPipe] = '|';
            _shiftMap[(int)Keys.OemPlus] = '+';
            _shiftMap[(int)Keys.OemMinus] = '_';
            _shiftMap[(int)Keys.OemQuestion] = '?';
            _shiftMap[(int)Keys.OemQuotes] = '"';
            _shiftMap[(int)Keys.OemSemicolon] = ':';
            _shiftMap[(int)Keys.OemTilde] = '~';
        }

        public static char? ToChar(Keys key, bool isShiftDown)
        {
            int index = (int)key;
            if (index < 0 || index >= _normalMap.Length)
                return null;

            return isShiftDown ? _shiftMap[index] : _normalMap[index];
        }
    }
}