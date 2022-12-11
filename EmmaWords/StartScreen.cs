using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Net;
using TwitchLib.Client.Models;
using TwitchLib.PubSub.Models.Responses.Messages;

namespace EmmaWords;

class StartScreen
{
    record Circle()
    {
        public PointF Centre { get; set; }
        public Color Color { get; set; }
        public float Alpha { get; set; }
        public float Size { get; set; }
    }

    record Chat()
    {
        public string? Text { get; set; }
        public RectangleF Area { get; set; }
        public string Name { get; set; }
        public RectangleF TargetArea { get; set; }
        public List<string> EmoteIds { get; set; }
        public int TTL { get; set; }
        public bool Streamer { get; set; }
    }

    readonly List<Circle> Circles = new();
    readonly List<Chat> Chats = new();
    readonly Random Random = new();
    readonly Dictionary<string, Image> ProfileCache = new();
    readonly Image StreamerImage;
    readonly EmoteCache EmoteCache = new();
    readonly GdiAnimation Animation;
    readonly TwitchBot TwitchBot;
    readonly WordService WordService;
    readonly Image BackgroundImage = new Bitmap(@"C:\Users\huggl\streaming\Emma_Background.png");
    readonly Image SpinnerImage = new Bitmap(@"C:\Users\huggl\streaming\points\Flower2_112.png");

    private System.Media.SoundPlayer SoundPlayer;


    public StartScreen(GdiAnimation animation, TwitchBot twitchBot, WordService service)
    {
        Animation = animation;
        TwitchBot = twitchBot;
        WordService = service;
        StreamerImage = Image.FromFile(@"C:\Users\huggl\emotes\broadcast.png");
    }

    readonly float FADE_RATE = 0.4f;
    readonly float GROW_RATE = 1f;
    readonly float SPAWN_RATE = 0.06f;
    readonly float BUBBLE_CORNER = 20;
    readonly float ANIMATION_SPEED = 0.08f;
    readonly float BUBBLE_WIDTH = 520f;
    readonly float BUBBLE_VMARGIN = 6f;
    readonly float BUBBLE_YMARGIN = 4f;
    readonly float BUBBLE_HMARGIN = 16f;
    readonly float BUBBLE_TITLE = 32f;
    readonly float SCREEN_VMARGIN = 48f;
    readonly float SCREEN_HMARGIN = 48f;
    readonly float BUBBLE_SPACE = 8f;
    readonly float PROFILE_SIZE = 48f;

    public static readonly float EMOTE_SIZE = 48f;


    readonly (int R, int G, int B)[] Palette =
    {
        (204, 225, 242),
        (198, 248, 229),
        (251, 247, 213),
        (249, 222, 215),
        (245, 205, 222),
        (226, 190, 241)
    };


    public void Tick()
    {
        if (WordService.Starting)
        {
            WordService.Starting = false;

            if (SoundPlayer != null)
            {
                SoundPlayer.Stop();
            }
            
            SoundPlayer = new System.Media.SoundPlayer();
            SoundPlayer.SoundLocation = Properties.Settings.Default.StartMusic;
            SoundPlayer.Play();
        }

        lock (Circles)
        {
            /*if (Random.NextDouble() < SPAWN_RATE)
            {
                float x = (float)(Random.NextDouble() * Animation.Area.Width * 1.2 - Animation.Area.Width * 0.1);
                float y = (float)(Random.NextDouble() * Animation.Area.Height * 1.2 - Animation.Area.Height * 0.1);

                var ce = Palette[Random.Next(Palette.Length)];
                var color = Color.FromArgb(240, ce.R, ce.G, ce.B);

                var circle = new Circle { Centre = new PointF(x, y), Color = color, Alpha = 240 };
                Circles.Add(circle);
            }

            for (int i = Circles.Count - 1; i >= 0; i--)
            {
                var circle = Circles[i];
                circle.Alpha -= FADE_RATE;
                circle.Color = Color.FromArgb((int)circle.Alpha, circle.Color.R, circle.Color.G, circle.Color.B);
                circle.Size += GROW_RATE;

                if (circle.Alpha <= 0)
                {
                    Circles.RemoveAt(i);
                }
            }*/

            if (TwitchBot.ClearChat)
            {
                foreach (var chat in Chats)
                {
                    chat.TTL = 0;
                }

                TwitchBot.ClearChat = false;
            }

            for (int i = Chats.Count - 1; i >= 0; i--)
            {
                var chat = Chats[i];

                if (Math.Abs(chat.Area.X - chat.TargetArea.X) < 0.1f && Math.Abs(chat.Area.Y - chat.TargetArea.Y) < 0.1f
                    && Math.Abs(chat.Area.Width - chat.TargetArea.Width) < 0.1f && Math.Abs(chat.Area.Height - chat.TargetArea.Height) < 0.1f)
                {
                    chat.Area = chat.TargetArea;
                }

                if (chat.Area != chat.TargetArea)
                {
                    chat.Area = new RectangleF
                    (
                        chat.Area.X + (chat.TargetArea.X - chat.Area.X) * ANIMATION_SPEED,
                        chat.Area.Y + (chat.TargetArea.Y - chat.Area.Y) * ANIMATION_SPEED,
                        chat.Area.Width + (chat.TargetArea.Width - chat.Area.Width) * ANIMATION_SPEED,
                        chat.Area.Height + (chat.TargetArea.Height - chat.Area.Height) * ANIMATION_SPEED
                    );
                }

                chat.TTL--;

                if (chat.TTL <= 0)
                {
                    chat.TargetArea = new RectangleF(chat.TargetArea.X,
                        -(200 + chat.Area.Height), chat.TargetArea.Width, chat.TargetArea.Height);
                }

                if (chat.Area.Bottom < 0)
                {
                    Chats.RemoveAt(i);
                }
            }
        }
    }


    public void ProcessChatMessage(ChatMessage message)
    {
        string noEmotes = message.Message;
        var emoteIds = new List<string>();

        foreach (var emote in message.EmoteSet.Emotes)
        {
            EmoteCache.LoadTwitch(emote.Id);
            emoteIds.Add(emote.Id);
            noEmotes = noEmotes.Substring(0, emote.StartIndex) + new string(' ', emote.EndIndex - emote.StartIndex + 1) + noEmotes.Substring(emote.EndIndex + 1);
        }

        noEmotes = " " + noEmotes + " ";

        string[] extraEmotes =
        {
            "catJAM"
        };

        foreach (string emote in extraEmotes)
        {
            int pos = noEmotes.IndexOf(emote);

            if (pos > 0)
            {
                if (EmoteCache.LoadCustom(emote) != null)
                {
                    emoteIds.Add(emote);
                }

                emoteIds.Add(emote);
                noEmotes = noEmotes.Substring(0, pos) + new string(' ', emote.Length) + noEmotes.Substring(pos + emote.Length);
            }
        }

        while (noEmotes.Contains("  "))
        {
            noEmotes = noEmotes.Replace("  ", " ");
        }

        noEmotes = noEmotes.Trim();

        if (!ProfileCache.ContainsKey(message.Username))
        {
            try
            {
                using (var client = new WebClient())
                {
                    string tempfile = @"C:\Users\huggl\emotes\profile_" + message.Username + ".jpg";

                    if (!File.Exists(tempfile))
                    {
                        string profileurl = TwitchBot.GetProfilePicUrl(message.Username);
                        client.DownloadFile(new Uri(profileurl), tempfile);
                    }

                    ProfileCache[message.Username] = Image.FromFile(tempfile);
                }
            }
            catch
            {
                ProfileCache[message.Username] = new Bitmap(1, 1);
            }
        }

        float bubbleFiller = BUBBLE_HMARGIN * 2 + BUBBLE_VMARGIN + PROFILE_SIZE;

        int maxEmotes = (int)((BUBBLE_WIDTH - bubbleFiller) / (EMOTE_SIZE + BUBBLE_VMARGIN));

        var chat = new Chat
        {
            Area = new RectangleF(SCREEN_HMARGIN, Animation.Area.Height + SCREEN_VMARGIN, 10, 10),
            Name = message.Username,
            Text = noEmotes,
            EmoteIds = emoteIds.Take(maxEmotes).ToList(),
            TTL = 3600,
            Streamer = message.IsBroadcaster
        };

        float bubbleHeight = BUBBLE_VMARGIN + BUBBLE_YMARGIN + BUBBLE_TITLE;
        float textWidth = 0, emotesWidth = 0;
        var titleMeasure = Animation.MeasureString(chat.Name, "ADLaM Display", 16,
            new Size((int)BUBBLE_WIDTH, 999), TextFormatFlags.Default, true);

        if (chat.Streamer)
        {
            titleMeasure.Width += (int)(24 + BUBBLE_VMARGIN);
        }

        if (noEmotes.Trim() != "")
        {
            var measure = Animation.MeasureString(chat.Text, "Arial", 12, new Size((int)(BUBBLE_WIDTH - bubbleFiller), 999), TextFormatFlags.Default, true);

            bubbleHeight += measure.Height;
            textWidth = Math.Min(BUBBLE_WIDTH, Math.Max(measure.Width + bubbleFiller, titleMeasure.Width + bubbleFiller));
        }

        if (chat.EmoteIds.Any())
        {
            bubbleHeight += EMOTE_SIZE;
            emotesWidth = Math.Min(BUBBLE_WIDTH, Math.Max(bubbleFiller + (EMOTE_SIZE + BUBBLE_VMARGIN) * chat.EmoteIds.Count, titleMeasure.Width + bubbleFiller));
        }

        float bubbleWidth = Math.Max(textWidth, emotesWidth);

        if (noEmotes.Trim() != "" && chat.EmoteIds.Any())
        {
            bubbleHeight += BUBBLE_YMARGIN;
        }

        chat.TargetArea = new RectangleF(SCREEN_HMARGIN, Animation.Area.Height - bubbleHeight - SCREEN_VMARGIN, bubbleWidth, bubbleHeight);

        for (int i = Chats.Count - 1; i >= 0; i--)
        {
            Chats[i].TargetArea = new RectangleF(Chats[i].TargetArea.X,
                Chats[i].TargetArea.Y - bubbleHeight - BUBBLE_SPACE,
                Chats[i].TargetArea.Width, Chats[i].TargetArea.Height);
        }

        Chats.Add(chat);
    }


    readonly StringFormat Format = new()
    {
        LineAlignment = StringAlignment.Center,
        Alignment = StringAlignment.Center
    };


    public void Render(Graphics gfx, Rectangle area)
    {
        lock (Circles)
        {
            gfx.SmoothingMode = SmoothingMode.HighQuality;
            gfx.InterpolationMode = InterpolationMode.HighQualityBicubic;
            gfx.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
            gfx.Clear(Color.FromArgb(255, 40, 40, 40));

            gfx.DrawImageUnscaled(BackgroundImage, 0, 0);

            foreach (var circle in Circles)
            {
                gfx.FillEllipse(Animation.GetBrush(circle.Color), circle.Centre.X - circle.Size / 2, circle.Centre.Y - circle.Size / 2, circle.Size, circle.Size);
            }

            foreach (var chat in Chats.ToList())
            {
                Animation.FillRoundedRectangle(Color.White, chat.Area, BUBBLE_CORNER);
                float titleLeft = chat.Area.X + PROFILE_SIZE + BUBBLE_VMARGIN + BUBBLE_HMARGIN;

                if (chat.Streamer)
                {
                    gfx.DrawImage(StreamerImage, titleLeft, chat.Area.Y + BUBBLE_VMARGIN + 3, 24, 24);
                    titleLeft += 24 + BUBBLE_VMARGIN;
                }

                gfx.DrawString(chat.Name, Animation.GetFont("ADLaM Display", 16, true), Brushes.Black,
                    titleLeft, chat.Area.Y + BUBBLE_VMARGIN);

                if (chat.Text != "")
                {
                    gfx.DrawString(chat.Text, Animation.GetFont("Arial", 12, true), Brushes.Black, new RectangleF(
                        chat.Area.X + PROFILE_SIZE + BUBBLE_VMARGIN + BUBBLE_HMARGIN,
                        chat.Area.Y + BUBBLE_VMARGIN + BUBBLE_TITLE,
                        chat.Area.Width - BUBBLE_HMARGIN * 2,
                        chat.Area.Height - BUBBLE_VMARGIN * 2 - BUBBLE_TITLE));
                }

                float emoteX = chat.Area.X + PROFILE_SIZE + BUBBLE_VMARGIN + BUBBLE_HMARGIN;

                var profile = ProfileCache[chat.Name];

                gfx.DrawImage(profile, chat.Area.X + BUBBLE_HMARGIN, chat.Area.Y + BUBBLE_VMARGIN, PROFILE_SIZE, PROFILE_SIZE);

                foreach (var emoteId in chat.EmoteIds)
                {
                    var image = EmoteCache.Get(emoteId);
                    int frames = 1;

                    try
                    {
                        frames = image.GetFrameCount(FrameDimension.Time);
                    }
                    catch
                    { }

                    if (frames > 1)
                    {
                        image.SelectActiveFrame(FrameDimension.Time, Animation.Frame % frames);
                    }

                    gfx.DrawImage(image, emoteX, chat.Area.Bottom - BUBBLE_YMARGIN - EMOTE_SIZE, EMOTE_SIZE, EMOTE_SIZE * ((float)image.Height / image.Width));
                    emoteX += EMOTE_SIZE + BUBBLE_VMARGIN;
                }
            }

            var messageArea = new RectangleF(area.Width * 0.32f, area.Height * 3 / 4, area.Width * 0.36f, area.Height / 9);
            Animation.FillRoundedRectangle(Color.Black, messageArea, 10);
            messageArea.Inflate(-5, -5);
            Animation.FillRoundedRectangle(Color.White, messageArea, 10);
            Animation.DrawFitTextOneLine(WordService.StreamMessage.Replace("\r\n", " "), "MV Boli", Color.Black, messageArea, Animation.CenterCenter, true);

            var spinnerArea = new RectangleF(area.Width - area.Width / 12 - area.Width / 32, area.Height - area.Width / 12 - area.Width / 32, (int)(SpinnerImage.Width * 1.41), (int)(SpinnerImage.Height * 1.41));
            gfx.FillEllipse(Brushes.Black, spinnerArea);
            spinnerArea.Inflate(-5, -5);
            gfx.FillEllipse(Brushes.White, spinnerArea);

            //create a new empty bitmap to hold rotated image
            using var returnBitmap = new Bitmap(SpinnerImage.Width, SpinnerImage.Height);
            using var g = Graphics.FromImage(returnBitmap);
            //move rotation point to center of image
            g.TranslateTransform((float)SpinnerImage.Width / 2, (float)SpinnerImage.Height / 2);
            //rotate
            g.RotateTransform((int)(Animation.Frame * 2));
            //move image back
            g.TranslateTransform(-(float)SpinnerImage.Width / 2, -(float)SpinnerImage.Height / 2);
            //draw passed in image onto graphics object
            g.DrawImageUnscaled(SpinnerImage, new Point(0, 0));
            
            gfx.DrawImageUnscaled(returnBitmap, (int)(spinnerArea.Left + spinnerArea.Width / 9) + 1, (int)(spinnerArea.Top + spinnerArea.Height / 9) + 1);
        }
    }
}
