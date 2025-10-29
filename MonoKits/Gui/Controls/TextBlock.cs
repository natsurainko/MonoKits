using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Xna.Framework;
using MonoKits.Gui.Documents;
using MonoKits.Gui.Media;
using MonoKits.Native;
using System.Diagnostics;

namespace MonoKits.Gui.Controls;

public partial class TextBlock : UIElement
{
    private NativeArray<int>? _array;

    protected override Size MeasureOverride(Size availableSize)
    {
        if (FontFamily == null || Text == null)
        {
            return new Size
            (
                Math.Min(!double.IsNaN(Width) ? Width : Padding.Left + Padding.Right, availableSize.Width),
                Math.Min(!double.IsNaN(Height) ? Height : Padding.Top + Padding.Bottom, availableSize.Height)
            );
        }

        double availableTextWidth = availableSize.Width - Padding.Left - Padding.Right;
        double availableTextHeight = availableSize.Height - Padding.Top - Padding.Bottom;

        var availableTextSize = new Size(availableTextWidth, availableTextHeight);
        var desiredTextSize = new Size();

        _array?.Dispose();
        _array = new NativeArray<int>((uint)InLines.Count);
        Span<int> span = _array.Value.AsSpan();

        for (int i = 0; i < InLines.Count; i++)
        {
            TextLine textLine = InLines[i];
            Size lineSize = textLine.Measure(availableTextSize);

            span[i] = i == 0
                ? textLine.GetMeasuredLines()
                : span[i - 1] + textLine.GetMeasuredLines();

            desiredTextSize.Width = Math.Max(desiredTextSize.Width, lineSize.Width);
            desiredTextSize.Height += lineSize.Height;
        }

        double desiredWidth = Padding.Left + desiredTextSize.Width + Padding.Right;
        double desiredHeight = Padding.Top + desiredTextSize.Height + Padding.Bottom;

        if (!double.IsNaN(Width)) desiredWidth = Width;
        if (!double.IsNaN(Height)) desiredHeight = Height;

        desiredWidth = Math.Min(desiredWidth, availableSize.Width);
        desiredHeight = Math.Min(desiredHeight, availableSize.Height);

        return new Size(desiredWidth, desiredHeight);
    }
}

public partial class TextBlock
{
    [ObservableProperty]
    public partial IFontFamily? FontFamily { get; set; }

    [ObservableProperty]
    public partial Color? Foreground { get; set; } = Color.Black;

    [ObservableProperty]
    public partial TextWrapping TextWrapping { get; set; } = TextWrapping.NoWrap;

    [ObservableProperty]
    public partial TextTrimming TextTrimming { get; set; } = TextTrimming.None;

    [ObservableProperty]
    public partial string? Text { get; set; }

    public List<TextLine> InLines { get; } = [];

    partial void OnTextChanged(string? value)
    {
        InLines.Clear();

        if (value != null && FontFamily != null)
        {
            InLines.AddRange(value.Split('\n').Select(l => new TextLine
            {
                FontFamily = FontFamily,
                TextWrapping = TextWrapping,
                Line = l
            }));
        }
    }

    partial void OnFontFamilyChanged(IFontFamily? value)
    {
        foreach (var line in InLines)
            line.FontFamily = value!;
    }

    partial void OnTextWrappingChanged(TextWrapping value)
    {
        foreach (var line in InLines)
            line.TextWrapping = value;
    }
}

public partial class TextBlock
{
    protected override void DrawOverride(GameTime gameTime)
    {
        base.DrawOverride(gameTime);

        if (SpriteBatch == null || FontFamily == null || Text == null) return;

        if (TextWrapping == TextWrapping.NoWrap)
            NowrapDrawing();
        else WrapDrawing();
    }

    private void BaseDrawing()
    {
        Rectangle textBounds = new
        (
            Bounds.X + (int)Padding.Left,
            Bounds.Y + (int)Padding.Top,
            Math.Max(0, Bounds.Width - (int)(Padding.Left + Padding.Right)),
            Math.Max(0, Bounds.Height - (int)(Padding.Top + Padding.Bottom))
        );

        if (textBounds.Width <= 0 || textBounds.Height <= 0) return;

        bool drawn = false;
        //int startIndex = 0;
        //int endIndex = 0;

        Vector2 location = textBounds.Location.ToVector2();

        for (int i = 0; i < InLines.Count; i++)
        {
            TextLine textLine = InLines[i];
            bool drawResult = textLine.Draw(SpriteBatch!, Foreground ?? Color.Black, this.Parent!.Bounds, ref location);

            if (!drawn && drawResult)
            {
                //startIndex = i;
                drawn = true;
            }
            else if (drawn && !drawResult)
            {
                //endIndex = i - 1;
                break;
            }
        }
    }

    private void WrapDrawing()
    {
        if (SpriteBatch == null || FontFamily == null || Text == null || InLines.Count == 0 || _array == null)
            return;

        Rectangle textBounds = new
        (
            Bounds.X + (int)Padding.Left,
            Bounds.Y + (int)Padding.Top,
            Math.Max(0, Bounds.Width - (int)(Padding.Left + Padding.Right)),
            Math.Max(0, Bounds.Height - (int)(Padding.Top + Padding.Bottom))
        );

        if (textBounds.Width <= 0 || textBounds.Height <= 0) return;
        if (InLines.Count != _array.Value.Length)
        {
            BaseDrawing();
            return;
        }

        float lineHeight = FontFamily.LineSpacing;
        Vector2 startLocation = textBounds.Location.ToVector2();
        Span<int> span = _array.Value.AsSpan();

        int firstVisibleLogicalLine = 0;
        if (startLocation.Y < this.Parent!.Bounds.Y)
        {
            float offset = this.Parent.Bounds.Y - startLocation.Y;
            firstVisibleLogicalLine = Math.Max(0, (int)(offset / lineHeight));
        }

        int firstVisibleLineIndex = Math.Min(firstVisibleLogicalLine, InLines.Count - 1);
        for (int i = firstVisibleLineIndex; i > 0; i--)
        {
            if (span[i - 1] <= firstVisibleLogicalLine)
            {
                firstVisibleLineIndex = i;
                break;
            }
        }

        //if (firstVisibleLineIndex == 0 && firstVisibleLogicalLine >= span[0])
        //    firstVisibleLineIndex = 1;

        int previousTotalLines = firstVisibleLineIndex > 0 ? span[firstVisibleLineIndex - 1] : 0;
        int skipLinesInCurrent = Math.Max(0, firstVisibleLogicalLine - previousTotalLines);

        Vector2 location = new(
            startLocation.X,
            startLocation.Y + previousTotalLines * lineHeight
        );

        if (skipLinesInCurrent > 0)
            location.Y += skipLinesInCurrent * lineHeight;

        bool drawn = false;
        for (int i = firstVisibleLineIndex; i < InLines.Count; i++)
        {
            TextLine textLine = InLines[i];
            bool drawResult = textLine.Draw(SpriteBatch!, Foreground ?? Color.Black, this.Parent!.Bounds, ref location);

            if (!drawn && drawResult) drawn = true;
            else if (drawn && !drawResult) break;
        }
    }

    private void NowrapDrawing()
    {
        if (SpriteBatch == null || FontFamily == null || Text == null || InLines.Count == 0)
            return;

        Rectangle textBounds = new
        (
            Bounds.X + (int)Padding.Left,
            Bounds.Y + (int)Padding.Top,
            Math.Max(0, Bounds.Width - (int)(Padding.Left + Padding.Right)),
            Math.Max(0, Bounds.Height - (int)(Padding.Top + Padding.Bottom))
        );

        if (textBounds.Width <= 0 || textBounds.Height <= 0) return;

        float lineHeight = FontFamily.LineSpacing;
        Vector2 startLocation = textBounds.Location.ToVector2();

        int firstVisibleLineIndex = 0;
        if (startLocation.Y < this.Parent!.Bounds.Y)
        {
            float offset = this.Parent.Bounds.Y - startLocation.Y;
            firstVisibleLineIndex = Math.Max(0, (int)(offset / lineHeight));
        }

        int maxVisibleLines = (int)Math.Ceiling((this.Parent.Bounds.Bottom - startLocation.Y) / lineHeight) + 1;
        int lastVisibleLineIndex = Math.Min(InLines.Count - 1, firstVisibleLineIndex + maxVisibleLines);

        Vector2 location = new(
            startLocation.X,
            startLocation.Y + firstVisibleLineIndex * lineHeight
        );

        for (int i = firstVisibleLineIndex; i <= lastVisibleLineIndex; i++)
        {
            if (location.Y >= this.Parent.Bounds.Bottom) break;

            if (location.Y + lineHeight >= this.Parent.Bounds.Top)
                InLines[i].Draw(SpriteBatch, Foreground ?? Color.Black, this.Parent.Bounds, ref location);
            else location.Y += lineHeight;
        }
    }
}