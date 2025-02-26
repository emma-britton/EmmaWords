﻿using System.Drawing.Drawing2D;
using System.Drawing.Text;

namespace Emma.Lib;

public class Gdi : IDisposable
{
    private readonly Dictionary<(Color, float), Pen> PenCache = [];
    private readonly Dictionary<Color, Brush> BrushCache = [];
    private readonly Dictionary<(string, float), Font> FontCache = [];
    private readonly Dictionary<(string, string, bool, float, float), float> SizeCache = [];
    private readonly Dictionary<(string, string, bool, float, float), float> OneLineSizeCache = [];

    private readonly Graphics OriginalGfx;
    private BufferedGraphics BufferedGfx;
    private bool Exit;
    private readonly bool Events;
    private readonly System.Windows.Forms.Timer Timer = new();

    public bool Started { get; set; }
    public bool Visible { get; set; } = true;
    public Color BackColor { get; set; } = Color.FromArgb(255, 70, 70, 70);
    public int TickRate { get; set; } = 60;
    public int MaxFrameRate { get; set; } = 30;
    public int Frame { get; set; } = 0;
    public Rectangle Area { get; private set; }
    public Graphics Gfx { get; private set; }

    public TextFormatFlags TopLeft { get; } = TextFormatFlags.Left | TextFormatFlags.Top | TextFormatFlags.WordBreak;
    public TextFormatFlags TopRight { get; } = TextFormatFlags.Right | TextFormatFlags.Top | TextFormatFlags.WordBreak;
    public TextFormatFlags TopCenter { get; } = TextFormatFlags.HorizontalCenter | TextFormatFlags.Top | TextFormatFlags.WordBreak;
    public TextFormatFlags CenterLeft { get; } = TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.WordBreak;
    public TextFormatFlags CenterRight { get; } = TextFormatFlags.Right | TextFormatFlags.VerticalCenter | TextFormatFlags.WordBreak;
    public TextFormatFlags CenterCenter { get; } = TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.WordBreak;
    public TextFormatFlags BottomLeft { get; } = TextFormatFlags.Left | TextFormatFlags.Bottom | TextFormatFlags.WordBreak;
    public TextFormatFlags BottomRight { get; } = TextFormatFlags.Right | TextFormatFlags.Bottom | TextFormatFlags.WordBreak;
    public TextFormatFlags BottomCenter { get; } = TextFormatFlags.HorizontalCenter | TextFormatFlags.Bottom | TextFormatFlags.WordBreak;


    public Gdi(Form owner, bool events = true)
    {
        OriginalGfx = owner.CreateGraphics();
        BufferedGfx = BufferedGraphicsManager.Current.Allocate(OriginalGfx, owner.DisplayRectangle);
        Gfx = BufferedGfx.Graphics;
        Area = owner.DisplayRectangle;
        owner.Resize += (sender, e) => Resize(owner.DisplayRectangle);
        owner.FormClosing += (sender, e) => Stop();
        owner.Shown += (sender, e) => Start();
        Events = events;
        Timer.Tick += (sender, e) => InternalRender();
    }


    public void Resize(Rectangle displayRectangle)
    {
        try
        {
            Gfx.Dispose();
            BufferedGfx.Dispose();
            BufferedGfx = BufferedGraphicsManager.Current.Allocate(OriginalGfx, displayRectangle);
            Gfx = BufferedGfx.Graphics;
            Area = displayRectangle;
        }
        catch
        {
        }
    }


    public void Start()
    {
        if (Started) return;
        Started = true;

        ThreadPool.QueueUserWorkItem(state => InternalTick());
        InternalRender();
    }


    private void InternalTick()
    {
        var next = DateTime.UtcNow;

        while (!Exit)
        {
            Frame++;
            Tick();

            next = next.AddMicroseconds(1000000 / TickRate);
            int sleepNeeded = (int)(next - DateTime.UtcNow).TotalMilliseconds;

            if (sleepNeeded > 0)
            {
                Thread.Sleep(sleepNeeded);
            }
        }
    }


    private void InternalRender()
    {
        try
        {
            Timer.Stop();
            var next = DateTime.UtcNow;

            if (Visible)
            {
                lock (Gfx)
                {
                    Gfx.SmoothingMode = SmoothingMode.HighQuality;
                    Gfx.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    Gfx.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
                    Gfx.Clear(BackColor);
                }

                Render();

                try
                {
                    lock (Gfx)
                    {
                        BufferedGfx.Render();
                    }
                }
                catch { }
            }

            next = next.AddMicroseconds(1000000 / MaxFrameRate);
            int sleepNeeded = (int)(next - DateTime.UtcNow).TotalMilliseconds;
            if (sleepNeeded < 1) sleepNeeded = 1;

            Timer.Interval = sleepNeeded;

            if (!Timer.Enabled)
            {
                Timer.Start();
            }
        }
        catch
        {
        }
    }


    public void Stop()
    {
        Started = false;
        Exit = true;
    }


    public Font GetFont(string fontName, float size, bool bold)
    {
        if (!FontCache.TryGetValue((fontName + (bold ? "B" : "N"), size), out var font))
        {
            FontCache[(fontName + (bold ? "B" : "N"), size)] = font = new Font(fontName, size, bold ? FontStyle.Bold : FontStyle.Regular);
        }

        return font;
    }


    public Pen GetPen(Color color, float width = 1)
    {
        if (!PenCache.TryGetValue((color, width), out var pen))
        {
            PenCache[(color, width)] = pen = new Pen(color, width);
        }

        return pen;
    }


    public Brush GetBrush(Color color)
    {
        if (!BrushCache.TryGetValue(color, out var brush))
        {
            BrushCache[color] = brush = GetPen(color).Brush;
        }

        return brush;
    }


    public void Dispose()
    {
        foreach (var font in FontCache)
        {
            font.Value.Dispose();
        }

        foreach (var pen in PenCache)
        {
            pen.Value.Dispose();
        }

        foreach (var brush in BrushCache)
        {
            brush.Value.Dispose();
        }

        FontCache.Clear();
        PenCache.Clear();
        BrushCache.Clear();

        GC.SuppressFinalize(this);
    }


    public void FillRoundedRectangle(Color color, RectangleF rectangle, float cornerRadius)
    {
        float quarter = 90;
        var brush = GetBrush(color);

        lock (Gfx)
        {
            Gfx.FillPie(brush, rectangle.Left, rectangle.Top, cornerRadius * 2, cornerRadius * 2, quarter * 2, quarter);
            Gfx.FillPie(brush, rectangle.Right - cornerRadius * 2, rectangle.Top, cornerRadius * 2, cornerRadius * 2, quarter * 3, quarter);
            Gfx.FillPie(brush, rectangle.Left, rectangle.Bottom - cornerRadius * 2, cornerRadius * 2, cornerRadius * 2, quarter, quarter);
            Gfx.FillPie(brush, rectangle.Right - cornerRadius * 2, rectangle.Bottom - cornerRadius * 2, cornerRadius * 2, cornerRadius * 2, 0, quarter);
            Gfx.FillRectangle(brush, rectangle.Left + cornerRadius - 0.5f, rectangle.Top, rectangle.Width - cornerRadius * 2 + 1, cornerRadius + 0.5f);
            Gfx.FillRectangle(brush, rectangle.Left, rectangle.Top + cornerRadius - 0.5f, rectangle.Width, rectangle.Height - cornerRadius * 2 + 1);
            Gfx.FillRectangle(brush, rectangle.Left + cornerRadius - 0.5f, rectangle.Bottom - cornerRadius - 0.5f, rectangle.Width - cornerRadius * 2 + 1, cornerRadius + 0.5f);
        }
    }


    public void DrawRoundedRectangle(Color color, RectangleF rectangle, float cornerRadius)
    {
        float quarter = 90;
        var pen = GetPen(color);

        lock (Gfx)
        {
            Gfx.DrawArc(pen, rectangle.Left, rectangle.Top, cornerRadius * 2, cornerRadius * 2, quarter * 2, quarter);
            Gfx.DrawArc(pen, rectangle.Right - cornerRadius * 2, rectangle.Top, cornerRadius * 2, cornerRadius * 2, quarter * 3, quarter);
            Gfx.DrawArc(pen, rectangle.Left, rectangle.Bottom - cornerRadius * 2, cornerRadius * 2, cornerRadius * 2, quarter, quarter);
            Gfx.DrawArc(pen, rectangle.Right - cornerRadius * 2, rectangle.Bottom - cornerRadius * 2, cornerRadius * 2, cornerRadius * 2, 0, quarter);
            Gfx.DrawLine(pen, rectangle.Left + cornerRadius, rectangle.Top, rectangle.Left + rectangle.Width - cornerRadius, rectangle.Top);
            Gfx.DrawLine(pen, rectangle.Left, rectangle.Top + cornerRadius, rectangle.Left, rectangle.Top + rectangle.Height - cornerRadius);
            Gfx.DrawLine(pen, rectangle.Right, rectangle.Top + cornerRadius, rectangle.Right, rectangle.Top + rectangle.Height - cornerRadius);
            Gfx.DrawLine(pen, rectangle.Left + cornerRadius, rectangle.Bottom, rectangle.Left + rectangle.Width - cornerRadius, rectangle.Bottom);
        }
    }


    public float FitTextOneLine(string text, string fontName, RectangleF rectangle, TextFormatFlags format, float min, float max, bool bold = false)
    {
        var key = (text, fontName, bold, rectangle.Width, rectangle.Height);

        if (SizeCache.TryGetValue(key, out float value)) return value;

        lock (Gfx)
        {
            while (max - min > 0.2)
            {
                float mid = (max + min) / 2;
                var font = GetFont(fontName, mid, bold);

                var size = TextRenderer.MeasureText(text, font, new Size((int)rectangle.Width, (int)rectangle.Height), format);

                if (size.Width > rectangle.Width || size.Height > rectangle.Height || size.Height > font.Height * 1.5f)
                {
                    max = mid;
                }
                else
                {
                    min = mid;
                }
            }
        }

        SizeCache[key] = min;
        return min;
    }


    public float FitText(string text, string fontName, RectangleF rectangle, TextFormatFlags format, float min, float max, bool bold = false)
    {
        var key = (text, fontName, bold, rectangle.Width, rectangle.Height);

        if (OneLineSizeCache.TryGetValue(key, out float value)) return value;

        lock (Gfx)
        {
            while (max - min > 0.2)
            {
                float mid = (max + min) / 2;
                var font = GetFont(fontName, mid, bold);

                var size = TextRenderer.MeasureText(text, font, new Size((int)rectangle.Width, (int)rectangle.Height), format);

                if (size.Width > rectangle.Width || size.Height > rectangle.Height)
                {
                    max = mid;
                }
                else
                {
                    min = mid;
                }
            }
        }

        OneLineSizeCache[key] = min;
        return min;
    }


    public void DrawFitTextOneLine(string? text, string fontName, Color color, RectangleF rectangle, TextFormatFlags format, bool bold = false, float maxSize = 1000)
    {
        if (text == null) return;

        float requiredSize = FitTextOneLine(text, fontName, rectangle, format, 1, maxSize, bold);

        lock (Gfx)
        {
            TextRenderer.DrawText(Gfx, text.Trim(' '), GetFont(fontName, requiredSize, bold),
                new Rectangle((int)rectangle.X, (int)rectangle.Y, (int)rectangle.Width, (int)rectangle.Height), color, format);
        }
    }


    public void DrawFitText(string? text, string fontName, Color color, RectangleF rectangle, TextFormatFlags format, bool bold = false, float maxSize = 1000)
    {
        if (text == null) return;

        float requiredSize = FitText(text, fontName, rectangle, format, 1, maxSize, bold);

        lock (Gfx)
        {
            TextRenderer.DrawText(Gfx, text.Trim(' '), GetFont(fontName, requiredSize, bold),
                new Rectangle((int)rectangle.X, (int)rectangle.Y, (int)rectangle.Width, (int)rectangle.Height), color, format);
        }
    }


    public Size MeasureString(string s, string fontName, float fontSize, Size area, TextFormatFlags format, bool bold)
    {
        return TextRenderer.MeasureText(s, GetFont(fontName, fontSize, bold), area, format);
    }


    public void FillRectangle(Color color, RectangleF rect)
    {
        Gfx.FillRectangle(GetBrush(color), rect);
    }


    public virtual void HandleKey(KeyEventArgs e)
    {
    }


    public virtual void HandleMouse(MouseEventArgs e)
    {

    }


    public virtual void HandleMessage(StreamMessage message)
    {

    }


    public virtual void Render()
    {
    }


    public virtual void Tick()
    {
    }
}
