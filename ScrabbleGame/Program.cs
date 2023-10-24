using Emma.Lib;
using Emma.Scrabble.Properties;

namespace Emma.Scrabble;

static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();

        string baseFolder = Settings.Default.BaseFolder;

        if (string.IsNullOrWhiteSpace(baseFolder))
        {
            baseFolder = Application.StartupPath;
        }

        var wordService = new WordService(baseFolder);
        Application.Run(new RulesetEditor(wordService));
    }
}