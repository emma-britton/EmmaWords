using Emma.Lib;
using Microsoft.Extensions.Configuration;
using TwitchLib.Client.Models;

namespace Emma.IsBot;

internal class Program
{
    internal static IConfigurationRoot Config { get; }


    static Program()
    {
        Config = new ConfigurationBuilder().AddJsonFile("appsettings.json", true, true).Build();
    }


    static void Main()
    {
        try
        {
            var wordService = new WordService(Config["BaseFolder"]);
            var commandParser = new CommandParser(wordService);

            var bot = new TwitchBot
            (
                commandParser,
                Config["CommandPrefix"],
                Config["TwitchClientID"],
                Config["TwitchBotAccessToken"],
                Config["TwitchUsername"],
                Config["TwitchChannelAccessToken"],
                Config["TwitchChannel"],
                Config["TwitchChannelID"]
            );

            Console.WriteLine("Starting Emma Is Bot...");
            bot.Run();

            while (true)
            {
                Console.Write("> ");
                string? command = Console.ReadLine();

                if (command?.ToLower() == "exit")
                {
                    return;
                }
                else if (command != null)
                {
                    var message = new StreamMessage("(console)", command, null, true);
                    string? result = commandParser.InterpretCommand(message, command);

                    if (result != null)
                    {
                        bot.SendMessage(result);
                        Console.WriteLine(result);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Unhandled error:");
            Console.WriteLine(ex);
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey(true);
        }
    }
}