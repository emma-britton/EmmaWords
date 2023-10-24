using Emma.Lib;

namespace Emma.Anagramming;

public partial class AnagramConfig : Form
{
    private readonly WordService m_WordService;

    public AnagramConfig()
    {
        InitializeComponent();
        m_WordService = new WordService(Environment.CurrentDirectory);
    }


    private void AnagramConfig_Load(object sender, EventArgs e)
    {
        foreach (var lexicon in m_WordService.Lexicons)
        {
            LexiconList.Items.Add(lexicon);
        }

        var settings = Properties.Settings.Default;

        if (m_WordService.GetLexicon(settings.Lexicon) is var lx)
        {
            LexiconList.SelectedItem = lx;
        }
        
        if (LexiconList.Items.Count > 0 && LexiconList.SelectedIndex == -1)
        {
            LexiconList.SelectedIndex = 0;
        }

        MinWordLength.Value = settings.MinWordLength;
        MaxWordLength.Value = settings.MaxWordLength;
        TwitchUsername.Text = settings.TwitchUsername;
        TwitchChannel.Text = settings.TwitchChannel;
        TwitchClientID.Text = settings.TwitchClientID;
        TwitchOAuth.Text = settings.TwitchOAuth;
    }


    private void StartButton_Click(object sender, EventArgs e)
    {
        var settings = Properties.Settings.Default;
        string? lexiconName = LexiconList.SelectedItem.ToString();

        if (lexiconName != null)
        { 
            settings.Lexicon = lexiconName;
            settings.MinWordLength = (int)MinWordLength.Value;
            settings.MaxWordLength = (int)MaxWordLength.Value;
            settings.TwitchUsername = TwitchUsername.Text;
            settings.TwitchChannel = TwitchChannel.Text;
            settings.TwitchClientID = TwitchClientID.Text;
            settings.TwitchOAuth = TwitchOAuth.Text;
            settings.Save();

            var lexicon = m_WordService.GetLexicon(lexiconName);

            if (lexicon != null)
            {
                var game = new AnagramGame(lexicon);
                var form = new AnagramForm(m_WordService, game);
                form.ShowDialog();
            }
        }
    }
}