
namespace Emma.Lib;

public class DefinitionSet
{
    private readonly Dictionary<string, List<Definition>> Definitions = [];


    public DefinitionSet()
    {
    }


    public void Add(Definition definition)
    {
        if (!Definitions.TryGetValue(definition.Word, out var list))
        {
            Definitions[definition.Word] = list = [];
        }

        list.Add(definition);
    }


    public static void ReadFromFile(DefinitionSet result, string definitionFile)
    {
        Console.WriteLine("Loading dictionary definitions from " + definitionFile);

        foreach (string line in File.ReadLines(definitionFile))
        {
            string[] parts = line.Split('\t');
            string word = parts[0].ToLower();

            if (!Enum.TryParse<PartOfSpeech>(parts[1], true, out var pos))
            {
                pos = PartOfSpeech.Undefined;
            }

            bool offensive = false;

            if (parts.Length > 4)
            {
                if (bool.TryParse(parts[4], out bool b))
                {
                    offensive = b;
                }
            }

            var definition = new Definition(word, pos, parts[2], parts[3])
            {
                Offensive = offensive
            };

            result.Add(definition);
        }

        Console.WriteLine($"{result.Definitions.Count} definitions loaded");
    }


    public void WriteToFile(string definitionFile)
    {
        using var writer = new StreamWriter(definitionFile);

        foreach (var kvp in Definitions)
        {
            foreach (var def in kvp.Value)
            {
                writer.WriteLine($"{def.Word}\t{def.Pos}\t{def.See}\t{def.Content}\t{def.Offensive}");
            }
        }
    }


    public void WriteLexicon(string lexiconFile)
    {
        using var writer = new StreamWriter(lexiconFile);
        
        foreach (var definition in Definitions.Keys.Order())
        {
            writer.WriteLine(definition.ToUpper());
        }
    }


    public Definition? GetDefinition(string word)
    {
        word = word.ToLower().Trim();

        if (Definitions.TryGetValue(word, out var possibleDefs))
        {
            return possibleDefs.FirstOrDefault();
        }

        return null;
    }


    public Definition? GetDefinition(string word, int index)
    {
        word = word.ToLower().Trim();

        if (Definitions.TryGetValue(word, out var possibleDefs))
        {
            if (possibleDefs.Count >= index)
            {
                return possibleDefs[index - 1];
            }                
        }

        return null;
    }

    public Definition? GetDefinition(string word, PartOfSpeech pos)
    {
        word = word.ToLower().Trim();

        if (Definitions.TryGetValue(word, out var possibleDefs))
        {
            var posDefs = possibleDefs.Where(d => d.Pos == pos).ToList();
            return posDefs.FirstOrDefault();
        }

        return null;
    }

    public Definition? GetDefinition(string word, PartOfSpeech pos, int index)
    {
        word = word.ToLower().Trim();

        if (Definitions.TryGetValue(word, out var possibleDefs))
        {
            var posDefs = possibleDefs.Where(d => d.Pos == pos).ToList();

            if (posDefs.Count >= index)
            {
                return posDefs[index - 1];
            }
        }

        return null;

    }

    public IEnumerable<Definition> GetDefinitions(string word)
    {
        word = word.ToLower().Trim();

        if (Definitions.TryGetValue(word, out var possibleDefs))
        {
            return possibleDefs;
        }

        return [];
    }


    public IEnumerable<string> GetDefinedWords()
    {
        return Definitions.Keys;
    }


    public IEnumerable<Definition> GetDefinitions()
    {
        return Definitions.Values.SelectMany(x => x);
    }
}
