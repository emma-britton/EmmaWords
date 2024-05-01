using Emma.Lib;
using Emma.Stream.Properties;
using System.Drawing.Imaging;
using System.IO;
using System.Media;
using System.Net;

namespace Emma.Stream;

class StartScreen : Gdi
{
    record ChatBubble()
    {
        public string? Text { get; set; }
        public RectangleF Area { get; set; }
        public string? Name { get; set; }
        public RectangleF TargetArea { get; set; }
        public List<string>? EmoteIds { get; set; }
        public int TTL { get; set; }
        public bool Streamer { get; set; }
    }

    readonly EmmaStream Stream;

    readonly List<ChatBubble> Chats = new();
    readonly Dictionary<string, Image> ProfileCache = new();

    readonly EmoteCache EmoteCache = new();
    readonly Image? BackgroundImage;
    readonly Image? SpinnerImage;
    readonly Image? StreamerImage;
    readonly SoundPlayer SoundPlayer = new();

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
    readonly float EMOTE_SIZE = 48f;

    DateTime StartTime = DateTime.MinValue;
    private bool Countdown = false;


    public StartScreen(EmmaStream stream, Form owner) : base(owner)
    {
        Stream = stream;
        string baseFolder = Properties.Settings.Default.BaseFolder;

        if (Directory.Exists(baseFolder))
        {
            BackgroundImage = Image.FromFile(Path.Combine(baseFolder, "Background.png"));
        }

        string spinnerImageFolder = Path.Combine(baseFolder, "flowers");

        if (Directory.Exists(spinnerImageFolder))
        {
            var spinnerImages = Directory.GetFiles(@"C:\Users\huggl\streaming\program\flowers").ToList();
            string randomImage = spinnerImages[new Random().Next(spinnerImages.Count)];
            SpinnerImage = Image.FromFile(randomImage);
        }

        string emoteCacheFolder = Properties.Settings.Default.EmoteCache;

        if (Directory.Exists(emoteCacheFolder))
        {
            StreamerImage = Image.FromFile(Path.Combine(emoteCacheFolder, "broadcast.png"));
        }
    }


    public void StartStream()
    {
        Stream.Message = "stream starting soon";
        StartTime = DateTime.Now.AddSeconds(5);
        Countdown = true;

        SoundPlayer.Stop();
        SoundPlayer.SoundLocation = Properties.Settings.Default.StartMusic;
        SoundPlayer.Play();
    }


    public void StopStream()
    {
        Countdown = false;

        SoundPlayer.Stop();
    }


    public void ClearChat()
    {
        foreach (var chat in Chats)
        {
            chat.TTL = 0;
        }
    }


    public override void Tick()
    {
        if (Settings.Default.ChatOverlay)
        {
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


    public override void HandleMessage(StreamMessage message)
    {
        if (!Settings.Default.ChatOverlay) return;

        string? text = message.Text;

        if (text == null && message.RewardName != null)
        {
            text = message.Username + " redeemed " + message.RewardName;
        }

        var emoteIds = new List<string>();

        if (message.Emotes != null)
        {
            foreach (var emote in message.Emotes)
            {
                EmoteCache.LoadTwitch(emote.Id, (int)EMOTE_SIZE);
                emoteIds.Add(emote.Id);
                text = text[..emote.StartIndex] + new string(' ', emote.EndIndex - emote.StartIndex + 1) + text[(emote.EndIndex + 1)..];
            }
        }

        text = $" {text} ";

        string[] extraEmotes =
        {
            "catJAM"
        };

        foreach (string emote in extraEmotes)
        {
            int pos = text.IndexOf(emote);

            if (pos > 0)
            {
                if (EmoteCache.LoadCustom(emote) != null)
                {
                    emoteIds.Add(emote);
                }

                emoteIds.Add(emote);
                text = text[..pos] + new string(' ', emote.Length) + text[(pos + emote.Length)..];
            }
        }

        while (text.Contains("  "))
        {
            text = text.Replace("  ", " ");
        }

        text = text.Trim();

        if (!ProfileCache.ContainsKey(message.Username))
        {
            try
            {
                using var client = new WebClient();
                string tempfile = @"C:\Users\huggl\emotes\profile_" + message.Username + ".jpg";

                if (!File.Exists(tempfile))
                {
                    string profileurl = Stream.TwitchBot.GetProfilePicUrl(message.Username);
                    client.DownloadFile(new Uri(profileurl), tempfile);
                }

                ProfileCache[message.Username] = Image.FromFile(tempfile);
            }
            catch
            {
                ProfileCache[message.Username] = new Bitmap(1, 1);
            }
        }

        float bubbleFiller = BUBBLE_HMARGIN * 2 + BUBBLE_VMARGIN + PROFILE_SIZE;

        int maxEmotes = (int)((BUBBLE_WIDTH - bubbleFiller) / (EMOTE_SIZE + BUBBLE_VMARGIN));

        var chat = new ChatBubble
        {
            Area = new RectangleF(SCREEN_HMARGIN, Area.Height + SCREEN_VMARGIN, 10, 10),
            Name = message.Username,
            Text = text,
            EmoteIds = emoteIds.Take(maxEmotes).ToList(),
            TTL = 3600,
            Streamer = message.IsBroadcaster
        };

        float bubbleHeight = BUBBLE_VMARGIN + BUBBLE_YMARGIN + BUBBLE_TITLE;
        float textWidth = 0, emotesWidth = 0;
        var titleMeasure = MeasureString(chat.Name, "ADLaM Display", 16,
            new Size((int)BUBBLE_WIDTH, 999), TextFormatFlags.Default, true);

        if (chat.Streamer)
        {
            titleMeasure.Width += (int)(24 + BUBBLE_VMARGIN);
        }

        if (text.Trim() != "")
        {
            var measure = MeasureString(chat.Text, "Arial", 12, new Size((int)(BUBBLE_WIDTH - bubbleFiller), 999), TextFormatFlags.WordBreak, true);

            bubbleHeight += measure.Height;
            textWidth = Math.Min(BUBBLE_WIDTH, Math.Max(measure.Width + bubbleFiller, titleMeasure.Width + bubbleFiller));
        }

        if (chat.EmoteIds.Any())
        {
            bubbleHeight += EMOTE_SIZE;
            emotesWidth = Math.Min(BUBBLE_WIDTH, Math.Max(bubbleFiller + (EMOTE_SIZE + BUBBLE_VMARGIN) * chat.EmoteIds.Count, titleMeasure.Width + bubbleFiller));
        }

        float bubbleWidth = Math.Max(textWidth, emotesWidth);

        if (text.Trim() != "" && chat.EmoteIds.Any())
        {
            bubbleHeight += BUBBLE_YMARGIN;
        }

        chat.TargetArea = new RectangleF(SCREEN_HMARGIN, Area.Height - bubbleHeight - SCREEN_VMARGIN, bubbleWidth, bubbleHeight);

        for (int i = Chats.Count - 1; i >= 0; i--)
        {
            Chats[i].TargetArea = new RectangleF(Chats[i].TargetArea.X,
                Chats[i].TargetArea.Y - bubbleHeight - BUBBLE_SPACE,
                Chats[i].TargetArea.Width, Chats[i].TargetArea.Height);
        }

        Chats.Add(chat);
    }


    public override void Render()
    {
        Gfx.Clear(Color.FromArgb(255, 40, 40, 40));

        if (BackgroundImage != null)
        {
            Gfx.DrawImageUnscaled(BackgroundImage, 0, 0);
        }

        if (Settings.Default.ChatOverlay)
        {
            foreach (var chat in Chats.ToList())
            {
                FillRoundedRectangle(Color.White, chat.Area, BUBBLE_CORNER);
                float titleLeft = chat.Area.X + PROFILE_SIZE + BUBBLE_VMARGIN + BUBBLE_HMARGIN;

                if (chat.Streamer)
                {
                    if (StreamerImage != null)
                    {
                        Gfx.DrawImage(StreamerImage, titleLeft, chat.Area.Y + BUBBLE_VMARGIN + 3, 24, 24);
                    }

                    titleLeft += 24 + BUBBLE_VMARGIN;
                }

                Gfx.DrawString(chat.Name, GetFont("ADLaM Display", 16, true), Brushes.Black,
                    titleLeft, chat.Area.Y + BUBBLE_VMARGIN);

                if (chat.Text != "")
                {
                    Gfx.DrawString(chat.Text, GetFont("Arial", 12, true), Brushes.Black, new RectangleF(
                        chat.Area.X + PROFILE_SIZE + BUBBLE_VMARGIN + BUBBLE_HMARGIN,
                        chat.Area.Y + BUBBLE_VMARGIN + BUBBLE_TITLE,
                        chat.Area.Width - BUBBLE_HMARGIN * 2,
                        chat.Area.Height - BUBBLE_VMARGIN * 2 - BUBBLE_TITLE));
                }

                float emoteX = chat.Area.X + PROFILE_SIZE + BUBBLE_VMARGIN + BUBBLE_HMARGIN;

                if (chat.Name != null)
                {
                    var profile = ProfileCache[chat.Name];
                    Gfx.DrawImage(profile, chat.Area.X + BUBBLE_HMARGIN, chat.Area.Y + BUBBLE_VMARGIN, PROFILE_SIZE, PROFILE_SIZE);
                }

                if (chat.EmoteIds != null)
                {
                    foreach (var emoteId in chat.EmoteIds)
                    {
                        var image = EmoteCache.Get(emoteId);

                        if (image != null)
                        {
                            int frames = 1;

                            try
                            {
                                frames = image.GetFrameCount(FrameDimension.Time);
                            }
                            catch
                            { }

                            if (frames > 1)
                            {
                                image.SelectActiveFrame(FrameDimension.Time, Frame % frames);
                            }

                            Gfx.DrawImage(image, emoteX, chat.Area.Bottom - BUBBLE_YMARGIN - EMOTE_SIZE, EMOTE_SIZE, EMOTE_SIZE * ((float)image.Height / image.Width));
                            emoteX += EMOTE_SIZE + BUBBLE_VMARGIN;
                        }
                    }
                }
            }
        }

        if (Countdown && StartTime < DateTime.Now)
        {
            Stream.Message = "stream starting now";
            Countdown = false;
        }

        var messageArea = new RectangleF(Area.Width * 0.32f, Area.Height * 0.7f, Area.Width * 0.36f, Area.Height / 9);
        FillRoundedRectangle(Color.Black, messageArea, 10);
        messageArea.Inflate(-5, -5);
        FillRoundedRectangle(Color.White, messageArea, 10);
        DrawFitTextOneLine(Stream.Message.Replace("\r\n", " "), "MV Boli", Color.Black, messageArea, CenterCenter, true);

        if (Countdown)
        {
            var timerArea = new RectangleF(Area.Width * 0.45f, Area.Height * 0.85f, Area.Width * 0.1f, Area.Height * 0.1f);
            FillRoundedRectangle(Color.Black, timerArea, 10);
            timerArea.Inflate(-5, -5);
            FillRoundedRectangle(Color.White, timerArea, 10);

            string remainingTime = (StartTime.AddSeconds(1) - DateTime.Now).ToString(@"m\:ss");
            DrawFitTextOneLine(remainingTime, "MV Boli", Color.Black, timerArea, CenterCenter, true);
        }

        if (SpinnerImage != null)
        {
            var spinnerArea = new RectangleF(Area.Width - Area.Width / 12 - Area.Width / 32,
                Area.Height - Area.Width / 12 - Area.Width / 32, (int)(SpinnerImage.Width * 1.44), (int)(SpinnerImage.Height * 1.44));
            Gfx.FillEllipse(Brushes.Black, spinnerArea);
            spinnerArea.Inflate(-5, -5);
            Gfx.FillEllipse(Brushes.White, spinnerArea);

            using var returnBitmap = new Bitmap(SpinnerImage.Width * 2, SpinnerImage.Height * 2);
            using var g = Graphics.FromImage(returnBitmap);
            g.TranslateTransform((float)returnBitmap.Width / 2, (float)returnBitmap.Height / 2);
            g.RotateTransform(Frame * 1.8f);
            g.TranslateTransform(-(float)returnBitmap.Width / 2, -(float)returnBitmap.Height / 2);
            int offset = SpinnerImage.Width / 2;
            g.DrawImageUnscaled(SpinnerImage, new Point(offset, offset));

            Gfx.DrawImageUnscaled(returnBitmap, (int)(spinnerArea.Left + spinnerArea.Width / 9) - offset + 1, (int)(spinnerArea.Top + spinnerArea.Height / 9) - offset + 1);
        }
    }


    public override void HandleKey(KeyEventArgs e)
    {
        switch (e.KeyCode)
        {
            case Keys.F1:
                Stream.CloseApp();
                break;

            case Keys.F7:
                Stream.RunCommand("next");
                break;

            case Keys.F8:
                Stream.RunCommand("skip");
                break;

            case Keys.F9:
                Stream.RunCommand("raid");
                break;

            case Keys.F10:
                Stream.RunCommand("start");
                break;

            case Keys.F11:
                Stream.RunCommand("brb");
                break;

            case Keys.F12:
                Stream.RunCommand("stop");
                break;

            case Keys.F13:
                Stream.RunCommand("game");
                break;
        }
    }
}