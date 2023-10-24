using Emma.Lib;
using Emma.WiktionaryParser.Properties;

namespace Emma.WiktionaryParser;

class Program
{
    static int Main()
    {
        try
        {
            var definitionSet = WiktionaryParser.ParseXmlData(Settings.Default.InputFile);
            definitionSet.WriteToFile(Settings.Default.OutputFile);
            definitionSet.WriteLexicon(Settings.Default.LexiconFile);
            var safeDefs = new DefinitionSet();

            foreach (string word in definitionSet.GetDefinedWords())
            {
                var defs = definitionSet.GetDefinitions(word).ToList();

                if (!defs.Any(d => d.Offensive || d.See != null && definitionSet.GetDefinitions(d.See).Any(d => d.Offensive)))
                {
                    foreach (var def in defs)
                    {
                        safeDefs.Add(def);
                    }
                }
            }

            safeDefs.WriteLexicon(Settings.Default.SafeLexiconFile);

            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Unhandled error: " + ex);
            return 1;
        }
    }
}