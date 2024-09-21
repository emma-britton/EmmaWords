using Emma.Lib;

namespace Emma.Stream;

public class QueueUI(EmmaStream stream, Form owner) : Gdi(owner)
{
    private readonly EmmaStream m_stream = stream;


    public override void Render()
    {
        Gfx.Clear(Color.FromArgb(0x3A, 0x3A, 0x3A));

        var queue = m_stream.PlayerQueue.ToList();

        if (queue.Count == 0)
        {
            DrawFitTextOneLine("Queue is empty", "Mulish", Color.White, new RectangleF(0, 0, Area.Width / 2, Area.Height / 2), CenterLeft);
        }

        if (queue.Count >= 1)
        {
            var area1 = new Rectangle(0, 0, Area.Width, Area.Height / 4);
            var iconArea1 = new Rectangle(3, 3, area1.Width / 6, area1.Height - 6);
            FillRectangle(Color.FromArgb(0, 202, 33), iconArea1);
            DrawFitTextOneLine("NOW", "Mulish", Color.White, iconArea1, CenterCenter, true);
            var textArea1 = new Rectangle(iconArea1.Right + 8, area1.Top, area1.Width - iconArea1.Right - 8, area1.Height);
            DrawFitTextOneLine(queue[0], "Mulish", Color.White, textArea1, CenterLeft);
        }
        
        if (queue.Count >= 2)
        {
            var area2 = new Rectangle(0, Area.Height / 4, Area.Width, Area.Height / 4);
            var iconArea2 = new Rectangle(3, area2.Top + 3, area2.Width / 6, area2.Height - 6);
            FillRectangle(Color.FromArgb(205, 0, 224), iconArea2);
            DrawFitTextOneLine("NEXT", "Mulish", Color.White, iconArea2, CenterCenter, true);
            var textArea2 = new Rectangle(iconArea2.Right + 8, area2.Top, area2.Width - iconArea2.Right - 8, area2.Height);
            DrawFitTextOneLine(queue[1], "Mulish", Color.White, textArea2, CenterLeft);
        }
        
        if (queue.Count >= 3)
        {
            var area3 = new Rectangle(0, Area.Height * 2 / 4, Area.Width, Area.Height / 4);
            var iconArea3 = new Rectangle(3, area3.Top + 3, area3.Width / 6, area3.Height - 6);
            FillRectangle(Color.FromArgb(0, 145, 224), iconArea3);
            DrawFitTextOneLine("THEN", "Mulish", Color.White, iconArea3, CenterCenter, true);
            var textArea3 = new Rectangle(iconArea3.Right + 8, area3.Top, area3.Width - iconArea3.Right - 8, area3.Height);
            DrawFitTextOneLine(queue[2], "Mulish", Color.White, textArea3, CenterLeft);
        }

        if (queue.Count >= 4)
        {
            var area4 = new Rectangle(0, Area.Height * 3 / 4, Area.Width, Area.Height / 4);
            var iconArea4 = new Rectangle(3, area4.Top + 3, area4.Width / 6, area4.Height - 6);
            FillRectangle(Color.FromArgb(224, 145, 0), iconArea4);
            DrawFitTextOneLine("LATER", "Mulish", Color.White, iconArea4, CenterCenter, true);
            var textArea4 = new Rectangle(iconArea4.Right + 8, area4.Top, area4.Width - iconArea4 .Right - 8, area4.Height);
            DrawFitTextOneLine(queue[3], "Mulish", Color.White, textArea4, CenterLeft);
        }
    }
}

