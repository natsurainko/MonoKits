using Microsoft.Xna.Framework;

namespace MonoKits.Gui.Documents;

public class TextCursor(List<TextLine> textLines)
{
    private bool _cachedCursorPositionInvalidated = true;
    private Vector2 _cachedCursorPosition;
    private Rectangle _cachedTextBounds;

    public int LinePosition 
    { 
        get => field;
        private set
        {
            if (field != value)
            {
                field = value;
                _cachedCursorPositionInvalidated = true;
            }
        }
    } = 0;

    public int ColumnPosition
    {
        get => field;
        private set
        {
            if (field != value)
            {
                field = value;
                _cachedCursorPositionInvalidated = true;
            }
        }
    } = 0;

    public int DesiredColumnPosition { get; private set; } = 0;

    public List<TextLine> Lines { get; } = textLines;

    public void Insert(char c)
    {
        if (c == '\n')
        {
            InsertNewLine();
            return;
        }

        LinePosition = Math.Clamp(LinePosition, 0, Lines.Count - 1);
        var line = Lines[LinePosition];

        ColumnPosition = Math.Clamp(ColumnPosition, 0, line.Line.Length);

        line.Insert(ColumnPosition, c);
        ColumnPosition++;
        DesiredColumnPosition++;
        
        _cachedCursorPositionInvalidated = true;
    }

    public void Delete()
    {
        LinePosition = Math.Clamp(LinePosition, 0, Lines.Count - 1);
        TextLine currentLine = Lines[LinePosition];
        ColumnPosition = Math.Clamp(ColumnPosition, 0, currentLine.Line.Length);

        if (ColumnPosition == 0)
        {
            if (LinePosition == 0) return;

            Lines.RemoveAt(LinePosition);
            LinePosition--;

            TextLine previousLine = Lines[LinePosition];
            
            ColumnPosition = previousLine.Line.Length;
            int measuredLines = previousLine.GetMeasuredLines();
            DesiredColumnPosition = measuredLines > 0
                ? previousLine.GetMeasuredLineLength(measuredLines - 1)
                : 0;
            
            previousLine.Line += currentLine.Line;
        }
        else
        {
            currentLine.Delete(ColumnPosition - 1);
            ColumnPosition--;
            DesiredColumnPosition = Math.Max(0, DesiredColumnPosition - 1);
        }
    }

    public Vector2 GetCursorPosition(Rectangle bounds)
    {
        if (!_cachedCursorPositionInvalidated && _cachedTextBounds == bounds)
            return _cachedCursorPosition;

        Vector2 basePosition = Vector2.Zero;

        for (int i = 0; i < LinePosition; i++)
        {
            int measuredLines = Lines[i].GetMeasuredLines();
            basePosition.Y += Math.Max(Lines[i].FontFamily.LineSpacing, measuredLines * Lines[i].FontFamily.LineSpacing);
        }

        if (Lines[LinePosition].GetCursorPosition(ColumnPosition, ref basePosition))
        {
            _cachedCursorPosition = basePosition;
            _cachedTextBounds = bounds;
            _cachedCursorPositionInvalidated = false;
        }
        
        return _cachedCursorPosition;
    }

    private void InsertNewLine()
    {
        LinePosition = Math.Clamp(LinePosition, 0, Lines.Count - 1);
        var line = Lines[LinePosition];
        ColumnPosition = Math.Clamp(ColumnPosition, 0, line.Line.Length);

        string leftPart = line.Line[..ColumnPosition];
        string rightPart = line.Line[ColumnPosition..];

        line.Line = leftPart;
        var newLine = new TextLine
        {
            FontFamily = line.FontFamily,
            TextWrapping = line.TextWrapping,
            Line = rightPart
        };
        Lines.Insert(LinePosition + 1, newLine);

        LinePosition++;
        ColumnPosition = 0;
        DesiredColumnPosition = 0;
    }

    public void MoveLeft()
    {
        if (ColumnPosition > 0)
        {
            ColumnPosition--;

            TextLine currentLine = Lines[LinePosition];
            (int _, int totalLengthIndex) = currentLine.FindLineIndex(ColumnPosition);

            DesiredColumnPosition = ColumnPosition - totalLengthIndex;
        }
        else if (LinePosition > 0)
        {
            LinePosition--;
            TextLine previousLine = Lines[LinePosition];

            ColumnPosition = previousLine.Line.Length;
            DesiredColumnPosition = previousLine.GetMeasuredLineLength(previousLine.GetMeasuredLines() - 1);
        }
    }

    public void MoveRight()
    {
        if (ColumnPosition < Lines[LinePosition].Line.Length)
        {
            ColumnPosition++;
            DesiredColumnPosition++;
        }
        else if (LinePosition < Lines.Count - 1)
        {
            LinePosition++;
            ColumnPosition = 0;
            DesiredColumnPosition = 0;
        }
    }

    public void MoveUp()
    {
        TextLine currentLine = Lines[LinePosition];
        (int lineIndex, int totalLengthIndex) = currentLine.FindLineIndex(ColumnPosition);

        if (lineIndex >= 1)
        {
            int previousLineLength = currentLine.GetMeasuredLineLength(lineIndex - 1);
            totalLengthIndex -= previousLineLength;

            ColumnPosition = Math.Clamp(totalLengthIndex + DesiredColumnPosition, totalLengthIndex, totalLengthIndex + previousLineLength);
        }
        else if (LinePosition > 0)
        {
            LinePosition--;
            TextLine previousLine = Lines[LinePosition];
            int measuredLines = previousLine.GetMeasuredLines();
            int startOfLastLineIndex = measuredLines > 1 
                ? previousLine.Line.Length - previousLine.GetMeasuredLineLength(measuredLines - 1)
                : 0;

            ColumnPosition = Math.Clamp(startOfLastLineIndex + DesiredColumnPosition, startOfLastLineIndex, previousLine.Line.Length);
        }
    }

    public void MoveDown()
    {
        TextLine currentLine = Lines[LinePosition];
        (int lineIndex, int totalLengthIndex) = currentLine.FindLineIndex(ColumnPosition);

        if (lineIndex < currentLine.GetMeasuredLines() - 1)
        {
            totalLengthIndex += currentLine.GetMeasuredLineLength(lineIndex);

            ColumnPosition = DesiredColumnPosition == 0
                ? totalLengthIndex
                : Math.Clamp(totalLengthIndex + DesiredColumnPosition, totalLengthIndex, totalLengthIndex + currentLine.GetMeasuredLineLength(lineIndex + 1));
        }
        else if (LinePosition < Lines.Count - 1)
        {
            LinePosition++;
            TextLine nextLine = Lines[LinePosition];
            int measuredLines = nextLine.GetMeasuredLines();
            int endOfFirstLineIndex = measuredLines >= 1
                ? nextLine.GetMeasuredLineLength(0)
                : 0;

            ColumnPosition = Math.Clamp(DesiredColumnPosition, 0, endOfFirstLineIndex);
        }
    }
    
    public void MoveEnd()
    {
        LinePosition = Lines.Count - 1;
        ColumnPosition = 0;
        DesiredColumnPosition = 0;
    }
}
