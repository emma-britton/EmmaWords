using Emma.Lib;
using Microsoft.Extensions.Configuration;

namespace Emma.MacondoParser;

internal class Program
{
    internal static IConfigurationRoot Config { get; }

    static Program()
    {
        Config = new ConfigurationBuilder().AddJsonFile("appSettings.json", true, true).Build();
    }


    private static int Main(string[] args)
    {
        try
        {
            if (args.Length < 2)
            {
                ShowHelp();
                return 1;
            }

            Stream input, output;

            if (args.Length > 1)
            {
                input = File.OpenRead(args[1]);
            }
            else
            {
                input = Console.OpenStandardInput();
            }
            
            if (args.Length > 2)
            {
                output = File.OpenWrite(args[2]);
            }
            else
            {
                output = Console.OpenStandardOutput();
            }

            IEnumerable<ScrabbleGame> games;

            switch (args[0].ToLower())
            {
                case "games":
                    games = MacondoParser.ReadGameFile(input);
                    MacondoAnalysis.AnalyseGames(games, output);
                    break;

                case "plays":
                    games = MacondoParser.ReadPlayFile(input);
                    MacondoAnalysis.AnalysePlays(games, output);
                    break;

                case "words":
                    games = MacondoParser.ReadPlayFile(input);
                    MacondoAnalysis.AnalyseWords(games, output);
                    break;

                case "alphas":
                    games = MacondoParser.ReadPlayFile(input);
                    MacondoAnalysis.AnalyseAlphas(games, output);
                    break;

                default:
                    ShowHelp();
                    return 1;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Unhandled error: " + ex);
            return 1;
        }

        return 0;
    }


    private static void ShowHelp()
    {
        const string help =
@"This is a program for reading Macondo game and play logs.

First argument specifies type of output required.

   games - statistics from the game log
   plays - statistics from the play log
   words - data about each word played
   alphas - data about each alphagram played

Second argument is name of input file, or use standard input if missing.
Third argument is name of output file, or use standard output if missing.

Example:
   macondoparser games ""C:\example\plays.txt"" ""C:\example\stats.txt""
";
        Console.WriteLine(help);
    }
}