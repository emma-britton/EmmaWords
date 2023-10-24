

using Emma.IsBot;
using Emma.Lib;

namespace Emma.Anagramming
{
    public partial class AnagramForm : Form
    {
        private readonly AnagramUI UI;


        public AnagramForm(WordService wordService, AnagramGame game)
        {
            InitializeComponent();
            game.MinWordLength = Properties.Settings.Default.MinWordLength;
            game.MaxWordLength = Properties.Settings.Default.MaxWordLength;
            
            UI = new AnagramUI(game, this);
            game.Start();

            if (!string.IsNullOrWhiteSpace(Properties.Settings.Default.TwitchUsername))
            {
                var commandParser = new CommandParser(wordService);

                var twitchBot = new TwitchBot(
                    commandParser,
                    "",
                    Properties.Settings.Default.TwitchClientID,
                    "",
                    Properties.Settings.Default.TwitchUsername,
                    Properties.Settings.Default.TwitchOAuth,
                    Properties.Settings.Default.TwitchChannel
                );

                twitchBot.Message += Bot_Message;
                twitchBot.Run();
            }
        }


        private void Bot_Message(object? sender, StreamMessage e)
        {
            if (UI != null)
            {
                UI.HandleMessage(e);
            }
        }
    }
}
