using System.Collections.Concurrent;
using System.IO;
using System.Runtime.InteropServices;
using TwitchLib.Client.Models;

namespace EmmaWords;

static class Program
{
    public static ConcurrentQueue<ChatMessage> ChatMessages { get; } = new();

    static readonly bool Reparse = false;
    static readonly WordService WordService;
    static TwitchBot? TwitchBot;


    static Program()
    {
        if (Reparse && File.Exists(Properties.Settings.Default.WiktionaryXml))
        {
            Console.WriteLine("Parsing Wiktionary file");
            var definitions = WiktionaryReader.ParseXmlData(Properties.Settings.Default.WiktionaryXml);
            definitions.WriteToFile(Properties.Settings.Default.DictionaryFile);
        }

        Console.WriteLine("Starting Emma Words");

        WordService = new WordService
        (
            Properties.Settings.Default.WordListFolder, 
            Properties.Settings.Default.DictionaryFile,
            Properties.Settings.Default.BonusFile,
            Properties.Settings.Default.TwitchUsername,
            Properties.Settings.Default.CommandPrefix + " "
        );
    }


    [STAThread]
    static void Main()
    {
        TwitchBot = new TwitchBot
        (
            Properties.Settings.Default.CommandPrefix + " ",
            WordService,
            Properties.Settings.Default.TwitchClientID,
            Properties.Settings.Default.TwitchAccessToken,
            Properties.Settings.Default.TwitchUsername,
            Properties.Settings.Default.TwitchOAuth,
            Properties.Settings.Default.TwitchChannel
        );

        TwitchBot.Run();

        var form = new MainForm(TwitchBot, WordService);
        Application.Run(form);
    }
}