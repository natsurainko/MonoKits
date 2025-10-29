using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoKits.Gui.Media;
using System.Diagnostics;

namespace MonoKits.Gui.Documents;

public class TextLine
{
    bool _invalidated = true;
    Size _cachedMeasuredSize;
    Size _cachedAvailableSize;
    readonly List<string> _lines = [];

    public required string Line 
    { 
        get => field;
        set
        {
            if (field != value)
            {
                field = value;
                _invalidated = true;
            }
        }
    }

    public required TextWrapping TextWrapping
    {
        get => field;
        set
        {
            if (field != value)
            {
                field = value;
                _invalidated = true;
            }
        }
    }

    public required IFontFamily FontFamily
    {
        get => field;
        set
        {
            if (field != value)
            {
                field = value;
                _invalidated = true;
            }
        }
    }

    public Size Measure(Size availableSize)
    {
        if (_cachedAvailableSize == availableSize && !_invalidated)
            return _cachedMeasuredSize;

        _lines.Clear();

        if (TextWrapping == TextWrapping.NoWrap)
        {
            Vector2 size = FontFamily.MeasureString(Line);
            _lines.Add(Line);

            _cachedMeasuredSize = new Size(
                Math.Min(availableSize.Width, size.X),
                Math.Max(size.Y, FontFamily.LineSpacing));
            _cachedAvailableSize = availableSize;
            _invalidated = false;

            return _cachedMeasuredSize;
        }

        int currentIndex = 0;
        float maxWidth = 0;

        while (currentIndex < Line.Length)
        {
            string remainingText = Line[currentIndex..];
            Vector2 remainingSize = FontFamily.MeasureString(remainingText);

            if (remainingSize.X <= availableSize.Width)
            {
                _lines.Add(remainingText);
                maxWidth = Math.Max(maxWidth, remainingSize.X);
                break;
            }

            int lineLength = FindLineLength(remainingText, availableSize.Width);
            if (lineLength == 0) lineLength = 1;

            string line = remainingText[..lineLength];
            Vector2 lineSize = FontFamily.MeasureString(line);

            _lines.Add(line);
            maxWidth = Math.Max(maxWidth, lineSize.X);
            currentIndex += lineLength;
        }

        _cachedMeasuredSize = new Size(
            Math.Min(availableSize.Width, maxWidth),
            Math.Max(_lines.Count * FontFamily.LineSpacing, FontFamily.LineSpacing));
        _cachedAvailableSize = availableSize;
        _invalidated = false;

        return _cachedMeasuredSize;
    }

    public void Draw(SpriteBatch spriteBatch, Color foreground, ref Vector2 location)
    {
        if (_lines.Count == 0)
        {
            location.Y += FontFamily.LineSpacing;
            return;
        }

        foreach (var line in _lines)
        {
            FontFamily.Draw(spriteBatch, line, location, foreground);
            location.Y += FontFamily.LineSpacing;
        }
    }

    public bool Draw(SpriteBatch spriteBatch, Color foreground, Rectangle textBounds, ref Vector2 location)
    {
        float lineHeight = FontFamily.LineSpacing;
        float startY = location.Y;
        float endY = location.Y + (_lines.Count * lineHeight);
        
        bool isVisible = endY > textBounds.Top && startY < textBounds.Bottom;

        if (_lines.Count == 0)
        {
            location.Y += lineHeight;
            return isVisible;
        }

        int firstVisibleLine = 0;
        int lastVisibleLine = _lines.Count - 1;
        
        if (location.Y < textBounds.Top)
        {
            float offset = textBounds.Top - location.Y;
            firstVisibleLine = Math.Min((int)(offset / lineHeight), _lines.Count - 1);
        }
        
        if (location.Y + (_lines.Count * lineHeight) > textBounds.Bottom)
        {
            float visibleHeight = textBounds.Bottom - location.Y;
            lastVisibleLine = Math.Min((int)Math.Ceiling(visibleHeight / lineHeight) - 1, _lines.Count - 1);
            lastVisibleLine = Math.Max(lastVisibleLine, 0);
        }
        
        if (firstVisibleLine > 0)
            location.Y += firstVisibleLine * lineHeight;
        
        for (int i = firstVisibleLine; i <= lastVisibleLine; i++)
        {
            var line = _lines[i];

            if (location.Y >= textBounds.Top && location.Y < textBounds.Bottom)
            {
                if (location.X < textBounds.Right)
                {
                    FontFamily.Draw(spriteBatch, line, location, foreground);
                }
            }

            location.Y += lineHeight;

            if (location.Y >= textBounds.Bottom) break;
        }
        
        int remainingLines = _lines.Count - lastVisibleLine - 1;
        if (remainingLines > 0) 
            location.Y += remainingLines * lineHeight;

        return isVisible;
    }

    public bool GetCursorPosition(int columnIndex, ref Vector2 basePosition)
    {
        if (_invalidated) return false;
        if (_lines.Count == 0) return true;

        columnIndex = Math.Clamp(columnIndex, 0, Line.Length);

        int currentIndex = 0;
        for (int i = 0; i < _lines.Count; i++)
        {
            string layoutLine = _lines[i];

            if (columnIndex <= currentIndex + layoutLine.Length)
            {
                int columnInLine = columnIndex - currentIndex;
                string textBeforeCursor = layoutLine[..columnInLine];
                Vector2 offset = FontFamily.MeasureString(textBeforeCursor);

                if (_lines.Count != 1 && currentIndex + layoutLine.Length != Line.Length && columnIndex == currentIndex + layoutLine.Length)
                {
                    basePosition.Y += (i + 1) * FontFamily.LineSpacing;
                    return true;
                }

                basePosition.X += offset.X;
                basePosition.Y += i * FontFamily.LineSpacing;

                return true;
            }

            currentIndex += layoutLine.Length;
        }

        int lastLineIndex = _lines.Count - 1;
        Vector2 lastLineSize = FontFamily.MeasureString(_lines[lastLineIndex]);
        
        basePosition.X += lastLineSize.X;
        basePosition.Y += lastLineIndex * FontFamily.LineSpacing;
        return true;
    }

    public int GetMeasuredLines() => _lines.Count;

    public int GetMeasuredLineLength(int index) => _lines[index].Length;

    private int FindLineLength(string text, double maxWidth)
    {
        float totalWidth = FontFamily.MeasureString(text).X;
        int estimatedLength = Math.Max(1, (int)(text.Length * (maxWidth / totalWidth)));
        estimatedLength = Math.Min(estimatedLength, text.Length);

        string testLine = text[..estimatedLength];
        float testWidth = FontFamily.MeasureString(testLine).X;

        if (testWidth > maxWidth)
        {
            while (estimatedLength > 0)
            {
                testLine = text[..estimatedLength];
                testWidth = FontFamily.MeasureString(testLine).X;

                if (testWidth <= maxWidth)
                    break;

                estimatedLength--;
            }
        }
        else
        {
            while (estimatedLength < text.Length)
            {
                string nextLine = text[..(estimatedLength + 1)];
                float nextWidth = FontFamily.MeasureString(nextLine).X;

                if (nextWidth > maxWidth)
                    break;

                estimatedLength++;
                //testWidth = nextWidth;
            }
        }

        return estimatedLength;
    }

    public void Insert(int index, char c)
    {
        Line = Line.Insert(index, c.ToString());

        if (_invalidated || _lines.Count == 0 || _lines.Count == 1)
        {
            _invalidated = true;
            return;
        }

        (int lineIndex, int totalLengthIndex) = FindLineIndex(index);
        RelayoutFromLine(lineIndex, totalLengthIndex);
    }

    public void Delete(int index)
    {
        Line = Line.Remove(index, 1);

        if (_invalidated || _lines.Count == 0 || _lines.Count == 1)
        {
            _invalidated = true;
            return;
        }

        (int lineIndex, int totalLengthIndex) = FindLineIndex(index);
        RelayoutFromLine(lineIndex, totalLengthIndex);
    }

    public (int lineIndex, int totalLengthIndex) FindLineIndex(int charIndex)
    {
        int currentIndex = 0;

        for (int i = 0; i < _lines.Count; i++)
        {
            int lineLength = _lines[i].Length;

            if (charIndex >= currentIndex && charIndex < currentIndex + lineLength)
                return (i, currentIndex);

            currentIndex += lineLength;
        }

        if (_lines.Count == 0)
            return (0, 0);

        if (charIndex == Line.Length)
            return (_lines.Count - 1, currentIndex - _lines[^1].Length);

        throw new UnreachableException();
    }

    private void RelayoutFromLine(int lineIndex, int currentIndex)
    {
        int removedLinesCount = _lines.Count - lineIndex;
        _lines.RemoveRange(lineIndex, removedLinesCount);

        float maxWidth = 0;

        foreach (var line in _lines)
            maxWidth = Math.Max(FontFamily.MeasureString(line).X, maxWidth);

        while (currentIndex < Line.Length)
        {
            string remainingText = Line[currentIndex..];
            Vector2 remainingSize = FontFamily.MeasureString(remainingText);

            if (remainingSize.X <= _cachedAvailableSize.Width)
            {
                _lines.Add(remainingText);
                maxWidth = Math.Max(maxWidth, remainingSize.X);
                break;
            }

            int lineLength = FindLineLength(remainingText, _cachedAvailableSize.Width);
            if (lineLength == 0) lineLength = 1;

            string line = remainingText[..lineLength];
            Vector2 lineSize = FontFamily.MeasureString(line);

            _lines.Add(line);
            maxWidth = Math.Max(maxWidth, lineSize.X);
            currentIndex += lineLength;
        }

        _cachedMeasuredSize = new Size(
            Math.Min(_cachedAvailableSize.Width, maxWidth),
            Math.Clamp(_lines.Count * FontFamily.LineSpacing, FontFamily.LineSpacing, _cachedAvailableSize.Height));
    }
}