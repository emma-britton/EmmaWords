using Emma.IsBot;
using Emma.Lib;

namespace Emma.Stream;

static class Program
{
    [STAThread]
    static void Main()
    {
        try
        {
            Console.WriteLine("Starting Emma Stream");

            var wordService = new WordService(Properties.Settings.Default.BaseFolder);
            var commandParser = new CommandParser(wordService);
            var form = new MainForm();
            var stream = new EmmaStream(form, wordService, commandParser);
            var queueForm = new QueueForm(stream);
            form.QueueForm = queueForm;

            Console.WriteLine("Ready");
            ThreadPool.QueueUserWorkItem(_ => MonitorConsole(commandParser, stream.TwitchBot));

            Application.Run(form);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Unhandled error:");
            Console.WriteLine(ex);
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey(true);
        }
    }


    static void MonitorConsole(CommandParser commandParser, TwitchBot? bot)
    {
        Thread.CurrentThread.Name = "Console";

        while (true)
        {
            Console.Write("> ");
            string? command = Console.ReadLine();

            if (command?.ToLower() == "exit")
            {
                Application.Exit();
            }
            else if (command != null)
            {
                var message = new StreamMessage("(console)", command, null, true);
                string? result = commandParser.InterpretCommand(message, command);

                if (result != null)
                {
                    if (bot != null)
                    {
                        bot.SendMessage(result);
                    }

                    Console.WriteLine(result);
                }
            }
        }
    }
}