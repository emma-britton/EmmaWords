

using System.IO;

namespace EmmaWords
{
    class DefinitionSet
    {
        private readonly Dictionary<string, List<Definition>> Definitions = new();


        public DefinitionSet()
        {
        }


        public void Add(Definition definition)
        {
            if (!Definitions.TryGetValue(definition.word, out var list))
            {
                Definitions[definition.word] = list = new List<Definition>();
            }

            list.Add(definition);
        }


        public static void ReadFromFile(DefinitionSet result, string definitionFile)
        {
            Console.WriteLine("loading defs from " + definitionFile);

            foreach (string line in File.ReadLines(definitionFile))
            {
                string[] parts = line.Split('\t');
                string word = parts[0].ToLower();

                if (!Enum.TryParse<PartOfSpeech>(parts[1], true, out var pos))
                {
                    pos = PartOfSpeech.Undefined;
                }

                var definition = new Definition(word, pos, parts[2], parts[3]);
                result.Add(definition);
            }

            Console.WriteLine($"{result.Definitions.Count} defs loaded");
        }


        public void WriteToFile(string definitionFile)
        {
            using var writer = new StreamWriter(definitionFile);

            foreach (var kvp in Definitions)
            {
                foreach (var def in kvp.Value)
                {
                    writer.WriteLine($"{def.word}\t{def.pos}\t{def.see}\t{def.content}");
                }
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
                var posDefs = possibleDefs.Where(d => d.pos == pos).ToList();
                return posDefs.FirstOrDefault();
            }

            return null;
        }

        public Definition? GetDefinition(string word, PartOfSpeech pos, int index)
        {
            word = word.ToLower().Trim();

            if (Definitions.TryGetValue(word, out var possibleDefs))
            {
                var posDefs = possibleDefs.Where(d => d.pos == pos).ToList();

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

            return Enumerable.Empty<Definition>();
        }

        public IEnumerable<Definition> GetDefinitions()
        {
            return Definitions.Values.SelectMany(x => x);
        }
    }
}
