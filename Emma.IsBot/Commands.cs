using Emma.Lib;
using System.Text.RegularExpressions;

namespace Emma.IsBot;

partial class CommandParser
{
    private const int MaxResponseLength = 480;
    private bool CanCheet = true;


    private string? Pattern(params string[] args)
    {
        if (args.Length != 2) return Help("pattern");

        try
        {
            string pattern = args[1].Replace("?", ".").Replace("*", ".*");

            var regex = new Regex(args[1], RegexOptions.IgnoreCase);
            var results = new List<string>();

            foreach (string word in WordService.ActiveLexicon)
            {
                if (regex.IsMatch(word))
                {
                    results.Add(word);
                }
            }

            string result = $"{results.Count} matches: " + string.Join(", ", results.Select(r => r + WordService.GetSymbol(r)));

            if (result.Length > MaxResponseLength)
            {
                result = $"{regex} matches {results.Count} words (too many to list)";
            }

            return result;
        }
        catch
        {
            return "error: could not parse pattern";
        }
    }


    private string? Lurk(params string[] args)
    {
        if (Username == null)
        {
            return null;
        }

        return $"Hi {Username.ToLower()}, thank you for the lurk";
    }


    private string? Cheet(params string[] args)
    {
        if (args.Length != 1) return Help("cheet");

        if (CanCheet)
        {
            CanCheet = false;
            return "Anagramming is no longer allowed";
        }
        else
        {
            CanCheet = true;
            return "Anagramming is now allowed";
        }
    }


    private string? NiceFind(params string[] args)
    {
        if (args.Length != 2) return Help("nicefind");

        string word = SanitizeWord(args[1]);

        if (WordService.ActiveLexicon.Contains(word))
        {
            return $"Nice find: {word}{WordService.GetSymbol(word)}";
        }
        else
        {
            return "how can that be a nice find it's not even a word";
        }
    }


    private string? Anagram(params string[] args)
    {
        if (!CanCheet && Username != null)
        {
            return Username.ToLower() + " cheet";
        }

        if (args.Length != 2) return Help("anagram");

        string what = args[1].Trim().ToUpper();
        var results = new HashSet<string>();


        void AddAnagrams(HashSet<string> results, string word)
        {
            if (word.IndexOf('?') is int pos && pos >= 0)
            {
                for (char c = 'A'; c <= 'Z'; c++)
                {
                    string newWord = word[..pos] + c.ToString() + word[(pos + 1)..];
                    AddAnagrams(results, newWord);
                }
            }
            else
            {
                string alphagram = new(what.Order().ToArray());

                foreach (var anagram in WordService.ActiveLexicon.Alphagrams[alphagram])
                {
                    results.Add(anagram + WordService.GetSymbol(anagram));
                }
            }
        }

        AddAnagrams(results, what);

        if (results.Count > 0)
        {
            return string.Join(", ", results);
        }
        else
        {
            return $"{what}: no anagrams";
        }
    }


    private string? Affix(params string[] args)
    {
        if (args.Length != 2) return Help("affix");

        string word = SanitizeWord(args[1]);
        var prefixes = new List<string>();
        var suffixes = new List<string>();

        foreach (string otherWord in WordService.ActiveLexicon)
        {
            if (otherWord != word)
            {
                if (otherWord.EndsWith(word))
                {
                    prefixes.Add($"{otherWord.AsSpan(0, otherWord.Length - word.Length)}{WordService.GetSymbol(otherWord)}-");
                }

                if (otherWord.StartsWith(word))
                {
                    suffixes.Add($"-{otherWord.AsSpan(word.Length)}{WordService.GetSymbol(otherWord)}");
                }
            }
        }

        string symbol = WordService.GetSymbol(word);
        int total = prefixes.Count + suffixes.Count;

        if (total == 0)
        {
            return $"-{word}{symbol}-: no valid affixes";
        }

        string result = $"[{string.Join(", ", prefixes)}] {word}{symbol} [{string.Join(", ", suffixes)}]";

        if (result.Length > MaxResponseLength)
        {
            result = $"-{word}{symbol}-: too many affixes to list ({total})";
        }

        return result;
    }


    private string? Contains(params string[] args)
    {
        if (args.Length != 2) return Help("contains");

        string word = SanitizeWord(args[1]);
        var infixes = new List<string>();

        foreach (string otherWord in WordService.ActiveLexicon)
        {
            if (otherWord.Contains(word))
            {
                infixes.Add(otherWord + WordService.GetSymbol(otherWord));
            }
        }

        string symbol = WordService.GetSymbol(word);

        if (infixes.Count == 0)
        {
            return $"{word}{symbol} does not appear in any words";
        }

        string result = $"{word}{symbol} appears in: {string.Join(", ", infixes)}";

        if (result.Length > MaxResponseLength)
        {
            result = $"{word}{symbol} appears in {infixes.Count} words (too many to list)";
        }

        return result;
    }


    private string? Define(params string[] args)
    {
        if (args.Length < 2 || args.Length > 4) return Help("define");

        string word = SanitizeWord(args[1]);
        if (word.Length == 0) return Help("define");

        string symbol = WordService.GetSymbol(word);
        var def = WordService.Definitions.GetDefinition(word);
        bool defAvailable = def != null;

        if (args.Length == 3 && int.TryParse(args[2], out int index))
        {
            def = WordService.Definitions.GetDefinition(word, index);

            if (def == null && defAvailable)
            {
                return $"{word}{symbol}: no definition with that number, other definitions available";
            }
        }
        else if (args.Length == 3 || args.Length == 4)
        {
            string posName = args[2].ToLower();

            posName = posName switch
            {
                "n" => "noun",
                "v" => "verb",
                "adj" => "adjective",
                "adv" => "adverb",
                _ => posName
            };

            if (!Enum.TryParse<PartOfSpeech>(posName, true, out var pos))
            {
                return $"{word}{symbol}: part of speech for definition not recognized";
            }

            if (args.Length == 4)
            {
                if (!int.TryParse(args[3], out int index2))
                {
                    return Help("define");
                }

                def = WordService.Definitions.GetDefinition(word, pos, index2);

                if (def == null && defAvailable)
                {
                    return $"{word}{symbol}: no {pos.ToString().ToLower()} definition with that number, other definitions available";
                }
            }
            else
            {
                def = WordService.Definitions.GetDefinition(word, pos);

                if (def == null && defAvailable)
                {
                    return $"{word}{symbol}: no {pos.ToString().ToLower()} form defined, other definitions available";
                }
            }
        }

        if (def == null)
        {
            return $"{word}{symbol}: no definition available";
        }

        string phoneyText = "";

        if (!WordService.ActiveLexicon.Contains(word))
        {
            if (WordService.ActiveLexicon == WordService.AllLexicons)
            {
                phoneyText = "(Not in any lexicon) ";
            }
            else
            {
                phoneyText = $"(Not in {WordService.ActiveLexicon.Name}) ";
            }
        }

        string result = $"{word}{symbol}: {phoneyText}{def.Pos.ToString().ToLower()} - {def.Content.TrimEnd('.')}";

        if (def.See != null && WordService.Definitions.GetDefinition(def.See, def.Pos) is Definition d2)
        {
            result += ": " + d2.Content.TrimEnd('.');
        }

        if (WordService.ActiveLexicon.Contains(word + "S"))
        {
            result += " [-S]";
        }

        return result;
    }


    private string? Hook(params string[] args)
    {
        if (args.Length != 2) return Help("hook");

        string word = SanitizeWord(args[1]);

        var preHooks = new List<string>();
        var postHooks = new List<string>();

        for (char c = 'A'; c <= 'Z'; c++)
        {
            string preHookWord = c + word;
            string postHookWord = word + c;

            if (WordService.ActiveLexicon.Contains(preHookWord))
            {
                preHooks.Add($"{c}{WordService.GetSymbol(preHookWord)}-");
            }

            if (WordService.ActiveLexicon.Contains(postHookWord))
            {
                postHooks.Add($"-{c}{WordService.GetSymbol(postHookWord)}");
            }
        }

        if (preHooks.Count == 0 && postHooks.Count == 0)
        {
            return word + " has no valid hooks";
        }

        string result = word + WordService.GetSymbol(word);

        if (preHooks.Any())
        {
            result = $"[{string.Join(", ", preHooks)}] " + result;
        }

        if (postHooks.Any())
        {
            result += $" [{string.Join(", ", postHooks)}]";
        }

        return result;
    }


    private string? SetLexicon(params string[] args)
    {
        if (args.Length > 2) return Help("lexicon");

        if (args.Length == 1)
        {
            return $"Current lexicon: {WordService.ActiveLexicon.Name} - " +
                $"Available lexicons: {string.Join(", ", WordService.Lexicons.Select(l => l.Name))}";
        }

        var newLexicon = WordService.GetLexicon(args[1]);

        if (newLexicon == null)
        {
            return "No such lexicon found";
        }

        WordService.ActiveLexicon = newLexicon;
        return $"Lexicon set to {newLexicon}";
    }


    private string? Check(params string[] args)
    {
        if (args.Length != 2) return Help("check");

        string word = SanitizeWord(args[1]);
        if (word.Length == 0) return Help("check");

        string symbol = WordService.GetSymbol(word);
        string result;

        if (WordService.ActiveLexicon == WordService.AllLexicons)
        {
            var yesLists = string.Join("/", WordService.Lexicons.Where(l => l.Contains(word)));
            var noLists = string.Join("/", WordService.Lexicons.Where(l => !l.Contains(word)));

            if (yesLists == "")
            {
                result = $"{word}{symbol} - not in any lexicon";
            }
            else if (noLists == "")
            {
                result = $"{word}{symbol} - valid in {yesLists}";
            }
            else
            {
                result = $"{word}{symbol} - valid in {yesLists}, not valid in {noLists}";
            }
        }
        else if (WordService.ActiveLexicon.Contains(word))
        {
            result = $"{word}{symbol} - valid in {WordService.ActiveLexicon.Name}";
        }
        else
        {
            result = $"{word}{symbol} - not valid in {WordService.ActiveLexicon.Name}";
        }

        if (word.Length > WordService.ActiveRuleSet.BoardSize)
        {
            result += $". Not playable in {WordService.ActiveRuleSet.Name} due to length.";
        }
        else if (!WordService.ActiveRuleSet.IsWordPlayable(word))
        {
            result += $". Not playable in {WordService.ActiveRuleSet.Name} due to letter distribution.";
        }

        return result;
    }


    private string? Leave(params string[] args)
    {
        if (args.Length != 2) return Help("leave");

        var rack = SanitizeRack(args[1]);

        if (rack.Length >= WordService.ActiveRuleSet.RackSize)
        {
            return $"leave: enter between 1 and {WordService.ActiveRuleSet.RackSize - 1} tiles";
        }

        var dist = new Dictionary<string, int>(WordService.ActiveRuleSet.TileDistribution);

        foreach (char c in rack)
        {
            if (--dist[c.ToString()] < 0)
            {
                return $"leave: {new string(rack)} is not a possible rack";
            }
        }

        return "leave evaluation is not currently enabled";
    }


    private string? Prob(params string[] args)
    {
        if (args.Length != 2) return Help("prob");

        string word = SanitizeWord(args[1]);
        if (word.Length == 0) return Help("prob");

        string symbol = WordService.GetSymbol(word);

        if (word.Length > WordService.ActiveRuleSet.BoardSize)
        {
            return $"{word.ToUpper()}{symbol}: not playable in {WordService.ActiveRuleSet.Name} due to length";
        }
        else if (!WordService.ActiveRuleSet.IsWordPlayable(word))
        {
            return $"{word.ToUpper()}{symbol}: not playable in {WordService.ActiveRuleSet.Name} due to letter distribution";
        }

        return "probability calculation is not currently enabled";
    }


    private string? Related(params string[] args)
    {
        if (args.Length != 2) return Help("related");

        string word = SanitizeWord(args[1]);
        string symbol = WordService.GetSymbol(word);

        string cmp = word.ToLower();

        var possibleSees = new HashSet<string>();
        var matches = new HashSet<string>();

        foreach (var def in WordService.Definitions.GetDefinitions(word))
        {
            if (def != null && def.See != null && def.See.ToUpper() != word && def.See != "")
            {
                possibleSees.Add(def.See.ToUpper());
            }
        }

        foreach (var possibleSee in possibleSees)
        {
            matches.Add(possibleSee);
        }

        foreach (var otherDef in WordService.Definitions.GetDefinitions())
        {
            if (otherDef.See != null && (otherDef.See == cmp || (possibleSees.Contains(otherDef.See.ToUpper()) && otherDef.Word.ToUpper() != word)))
            {
                if (WordService.AllLexicons.Contains(otherDef.Word.ToUpper()))
                {
                    matches.Add(otherDef.Word.ToUpper());
                }
            }
        }

        if (matches.Count == 0)
        {
            return $"{word}{symbol}: no related words found";
        }

        string result = $"{word}{symbol}: see also {string.Join(", ", matches.OrderBy(x => x).Select(s => $"{s}{WordService.GetSymbol(s)}"))}";
        return result;
    }


    private string? Prefix(params string[] args)
    {
        if (args.Length != 2) return Help("prefix");

        string word = SanitizeWord(args[1]);
        var prefixes = new List<string>();

        foreach (string otherWord in WordService.ActiveLexicon)
        {
            if (otherWord != word && otherWord.EndsWith(word))
            {
                prefixes.Add($"{otherWord.AsSpan(0, otherWord.Length - word.Length)}{WordService.GetSymbol(otherWord)}-");
            }
        }

        string symbol = WordService.GetSymbol(word);

        if (prefixes.Count == 0)
        {
            return $"-{word}{symbol}: no valid prefixes";
        }

        string result = $"[{string.Join(", ", prefixes)}] {word}{symbol}";

        if (result.Length > MaxResponseLength)
        {
            result = $"-{word}{symbol}: too many prefixes to list ({prefixes.Count})";
        }

        return result;
    }


    private string? Rules(params string[] args)
    {
        if (args.Length > 2) return Help("rules");

        if (args.Length == 1)
        {
            return $"Current rule set: {WordService.ActiveRuleSet.Name} - " +
                $"Available rule sets: {string.Join(", ", WordService.RuleSets.Select(r => r.Name))}";
        }

        RuleSet? ruleset = null;
        string name = args[1].Trim().ToLower();

        foreach (var rs in WordService.RuleSets.OrderByDescending(x => x.Name))
        {
            if (rs.Name.ToLower().StartsWith(name))
            {
                ruleset = rs;
            }
        }

        if (ruleset == null)
        {
            return "No such rule set found";
        }

        WordService.ActiveRuleSet = ruleset;
        return $"Rule set changed to {ruleset.Name}";
    }


    private string? Suffix(params string[] args)
    {
        if (args.Length != 2) return Help("suffix");

        string word = SanitizeWord(args[1]);
        var suffixes = new List<string>();

        foreach (string otherWord in WordService.ActiveLexicon)
        {
            if (otherWord != word && otherWord.StartsWith(word))
            {
                suffixes.Add($"-{otherWord.AsSpan(word.Length)}{WordService.GetSymbol(otherWord)}");
            }
        }

        string symbol = WordService.GetSymbol(word);

        if (suffixes.Count == 0)
        {
            return $"{word}{symbol}-: no valid suffixes";
        }

        string result = $"{word}{symbol} [{string.Join(", ", suffixes)}]";

        if (result.Length > MaxResponseLength)
        {
            result = $"{word}{symbol}-: too many suffixes to list ({suffixes.Count})";
        }

        return result;
    }


    private string? Hi(params string[] args)
    {
        if (Username == null)
        {
            return "hi";
        }

        return $"hi {Username.ToLower()}";
    }


    private string? Count(params string[] args)
    {
        return $"The lexicon {WordService.ActiveLexicon.Name} contains {WordService.ActiveLexicon.WordCount} words";
    }


    public static IEnumerable<T[]> Permutations<T>(T[] values, int fromInd = 0)
    {
        if (fromInd + 1 == values.Length)
            yield return values;
        else
        {
            foreach (var v in Permutations(values, fromInd + 1))
                yield return v;

            for (var i = fromInd + 1; i < values.Length; i++)
            {
                SwapValues(values, fromInd, i);
                foreach (var v in Permutations(values, fromInd + 1))
                    yield return v;
                SwapValues(values, fromInd, i);
            }
        }
    }


    private static void SwapValues<T>(T[] values, int pos1, int pos2)
    {
        if (pos1 != pos2)
        {
            (values[pos2], values[pos1]) = (values[pos1], values[pos2]);
        }
    }
}
