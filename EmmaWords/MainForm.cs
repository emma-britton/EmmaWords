

namespace EmmaWords;

partial class MainForm : Form
{
    public GdiAnimation Animation;
    private readonly StartScreen StartScreen;
    public WordService WordService;


    private void MainForm_Load(object sender, EventArgs e)
    {
        ThreadPool.QueueUserWorkItem(state => MonitorConsole());
    }


    public MainForm(TwitchBot twitchBot, WordService service)
    {
        InitializeComponent();

        WordService = service;

        Animation = new GdiAnimation(CreateGraphics(), DisplayRectangle);
        Location = Screen.AllScreens.Last().WorkingArea.Location;
        WindowState = FormWindowState.Maximized;

        Resize += (sender, e) => Animation.Resize(DisplayRectangle);
        Shown += (sender, e) => Animation.Start();
        FormClosing += (sender, e) => Animation.Stop();

        StartScreen = new StartScreen(Animation, twitchBot, service);

        Animation.Tick += AnimationTick;
        Animation.Render += AnimationRender;
    }


    private void MonitorConsole()
    {
        while (true)
        {
            Console.Write("> ");

            string? line = Console.ReadLine();

            if (line != null)
            {
                if (line == "exit")
                {
                    Environment.Exit(0);
                }

                line = line.Trim();
                string? result = WordService.InterpretCommand(line);

                if (result != null)
                {
                    Console.WriteLine(result);
                    //TwitchBot.SendMessage(result);
                }
            }
        }
    }


    private void AnimationTick()
    {
        if (Program.ChatMessages.TryDequeue(out var message))
        {
            if (WordService.GameUI != null)
            {
                if (message.Username != Properties.Settings.Default.TwitchUsername)
                {
                    WordService.GameUI.HandleMessage(message);
                }
            }
            else
            {
                StartScreen.ProcessChatMessage(message);
            }
        }

        if (WordService.GameUI == null)
        {
            StartScreen.Tick();
        }
    }


    private void AnimationRender(Graphics gfx)
    {
        if (WordService.GameUI != null)
        {
            WordService.GameUI.Render(Animation, gfx, DisplayRectangle);
        }
        else
        {
            StartScreen.Render(gfx, DisplayRectangle);
        }
    }

    private void MainForm_KeyDown(object sender, KeyEventArgs e)
    {
        if (WordService.GameUI != null)
        {
            WordService.GameUI.HandleKey(e);
        }
    }

    private void MainForm_MouseDown(object sender, MouseEventArgs e)
    {
        if (WordService.GameUI != null)
        {
            WordService.GameUI.HandleMouse(e);
        }
    }
}