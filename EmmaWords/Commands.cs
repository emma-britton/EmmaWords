
using System.Text.RegularExpressions;

namespace EmmaWords;

partial class WordService
{
    private string? Pattern(params string[] args)
    {
        if (args.Length != 2) return Help("pattern");

        try
        {
            var regex = new Regex(args[1], RegexOptions.IgnoreCase);
            var results = new List<string>();

            foreach (string word in CurrentList)
            {
                if (regex.IsMatch(word))
                {
                    results.Add(word);
                }
            }

            if (results.Count > SearchLimit)
            {
                return $"{regex} matches {results.Count} words (too many to list)";
            }

            return $"{results.Count} matches: " + string.Join(", ", results.Select(r => r + GetSymbol(r)));
        }
        catch
        {
            return "error: could not parse pattern";
        }
    }


    private string? Discord(params string[] args)
    {
        return "Join emma discord to chat with other viewers and be notified of streams: https://discord.gg/daeEQCjs88";
    }


    private string? Egg(params string[] args)
    {
        return "egg";
    }


    private string? Edit(params string[] args)
    {
        if (args.Length != 1) return Help("edit");

        if (RuleSet.Name == "Scrabble")
        {
            var newRuleSet = new RuleSet("Custom Variant", RuleSet.BoardSize, RuleSet.RackSize, RuleSet.BagSize,
                RuleSet.LetterDistribution, RuleSet.LetterDisplay, RuleSet.LetterPoints, RuleSet.Board)
            {
                Player1Name = RuleSet.Player1Name,
                Player2Name = RuleSet.Player2Name,
                AllowFlip = RuleSet.AllowFlip,
                ValidateWords = RuleSet.ValidateWords,
                BingoScore = RuleSet.BingoScore,
                Description = RuleSet.Description
            };

            RuleSet = newRuleSet;
        }

        ThreadPool.QueueUserWorkItem(state =>
        {
            var editor = new RulesetEditor(ScrabbleGame.WordService, RuleSet);
            editor.ShowDialog();

            ScrabbleGame.RuleSet = RuleSet;

            if (GameUI is ScrabbleUI sui)
            {
                sui.RuleSet = RuleSet;
            }
        });

        return "Editing rule set";
    }


    private string? Lurk(params string[] args)
    {
        return "Hi " + CurrentUser + ", thank you for the lurk";
    }


    private string? Set(params string[] args)
    {
        if (args.Length < 3) return Help("set");
        string value = args[2];

        switch (args[1].ToLower())
        {
            case "columns":
                if (int.TryParse(value, out int cols))
                {
                    WordGame.SetColumns(cols);

                    if (GameUI is WordGameUI wgu)
                    {
                        wgu.Columns = cols;
                    }

                    return "Set columns to " + cols;
                }
                break;

            case "rows":
                if (int.TryParse(value, out int rows))
                {
                    WordGame.SetRows(rows);

                    if (GameUI is WordGameUI wgu)
                    {
                        wgu.Rows = rows;
                    }

                    return "Set rows to " + rows;
                }
                break;

            case "minlength":
                if (int.TryParse(value, out int minlength))
                {
                    WordGame.SetMinLength(minlength);
                    return "Set min length to " + minlength;
                }
                break;

            case "maxlength":
                if (int.TryParse(value, out int maxLength))
                {
                    WordGame.SetMaxLength(maxLength);
                    return "Set max length to " + maxLength;
                }
                break;
        }

        return "error: unknown setting";
    }


    private string? Cheet(params string[] args)
    {
        if (args.Length != 1) return Help("cheet");

        if (WordGame.IsRunning)
        {
            return "Cannot enable the anagram command during a game of Emma Words";
        }

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


    private string? Recommend(params string[] args)
    {
        if (args.Length != 2) return Help("recommend");

        switch (args[1].ToLower())
        {
            case "tv":
                switch (new Random().Next(7))
                {
                    case 0: return "The Wire is all time best series";
                    case 1: return "Anything with David Tennant";
                    case 2: return "Sopranos is cool";
                    case 3: return "I recommend Fargo if you havent gotten to it yet";
                    case 4: return "Ted Lasso maybe?";
                    case 5: return "I watched the first episode of Black Mirror ... was great";
                    case 6: return "Oh, I need to rewatch Band of Brothers";
                }
                break;

            case "movie":
            case "film":
                switch (new Random().Next(1))
                {
                    case 0: return "I would recommend Rounders, such an inspiring and great movie";
                }
                break;

            case "music":
                break;
        }

        return "Sorry, I don't know";
    }


    private string? NiceFind(params string[] args)
    {
        if (args.Length != 2) return Help("nicefind");

        string word = SanitizeWord(args[1]);

        if (CurrentList.Contains(word))
        {
            return $"Nice find: {word}{GetSymbol(word)}";
        }
        else
        {
            return "how can that be a nice find it's not even a word";
        }
    }


    private string? Guess(params string[] args)
    {
        if (args.Length < 2) return Help("guess");

        if (!WordGame.IsRunning)
        {
            return "Cannot submit your guess as there is no game of Emma Words currently running";
        }

        foreach (string arg in args.Skip(1))
        {
            var guess = new Guess(CurrentUser, arg.ToUpper().Trim());
            WordGame.SubmitGuess(guess);
        }

        return null;
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
            T tmp = values[pos1];
            values[pos1] = values[pos2];
            values[pos2] = tmp;
        }
    }


    private string? Anagram(params string[] args)
    {
        if (!CanCheet)
        {
            return CurrentUser + " cheet";
        }

        if (args.Length != 2) return Help("anagram");

        string what = args[1].Trim().ToUpper();
        var results = new HashSet<string>();
        
        if (ulong.TryParse(what, out _))
        {
            if (what.Length > 9)
            {
                return "No";
            }

            var chars = what.ToCharArray();

            foreach (var perm in Permutations(chars))
            {
                ulong number = ulong.Parse(new string(perm));

                if (PrimeTest.IsPrime(number))
                {
                    results.Add(new string(perm));

                    if (results.Count >= 10)
                    {
                        break;
                    }
                }
            }
        }
        else if (what.Contains("?"))
        {
            var arr = what.ToArray();
            int pos = what.IndexOf('?');

            for (char c = 'A'; c <= 'Z'; c++)
            {
                arr[pos] = c;
                Array.Sort(arr);
                string sorted = new(arr);

                if (WordSet.Lookup.TryGetValue(sorted, out var hashset))
                {
                    foreach (var anagram in hashset)
                    {
                        results.Add(anagram + GetSymbol(anagram));
                    }
                }
            }
        }
        else
        {
            var arr = what.ToArray();
            Array.Sort(arr);
            string sorted = new(arr);

            if (WordSet.Lookup.TryGetValue(sorted, out var hashset))
            {
                foreach (var anagram in hashset)
                {
                    results.Add(anagram + GetSymbol(anagram));
                }
            }
            else
            {
                return $"{what}: no anagrams";
            }
        }

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

        foreach (string otherWord in CurrentList)
        {
            if (otherWord != word)
            {
                if (otherWord.EndsWith(word))
                {
                    prefixes.Add($"{otherWord.AsSpan(0, otherWord.Length - word.Length)}{GetSymbol(otherWord)}-");
                }

                if (otherWord.StartsWith(word))
                {
                    suffixes.Add($"-{otherWord.AsSpan(word.Length)}{GetSymbol(otherWord)}");
                }
            }
        }

        string symbol = GetSymbol(word);
        int total = prefixes.Count + suffixes.Count;

        if (total == 0)
        {
            return $"-{word}{symbol}-: no valid affixes";
        }

        if (total > SearchLimit)
        {
            return $"-{word}{symbol}-: too many affixes to list ({total})";
        }

        return $"[{string.Join(", ", prefixes)}] {word}{symbol} [{string.Join(", ", suffixes)}]";
    }


    private string? Contains(params string[] args)
    {
        if (args.Length != 2) return Help("contains");

        string word = SanitizeWord(args[1]);
        var result = new List<string>();

        foreach (string otherWord in CurrentList)
        {
            if (otherWord.Contains(word))
            {
                result.Add(otherWord + GetSymbol(otherWord));
            }
        }

        string symbol = GetSymbol(word);

        if (result.Count == 0)
        {
            return $"{word}{symbol} does not appear in any words";
        }

        if (result.Count > SearchLimit)
        {
            return $"{word}{symbol} appears in {result.Count} words (too many to list)";
        }

        return $"{word}{symbol} appears in: {string.Join(", ", result)}";
    }


    private string? Define(params string[] args)
    {
        if (args.Length < 2 || args.Length > 4) return Help("define");

        if (args[1].ToLower() == "lady_m")
        {
            return "LADY_M: the nicest person on the internet";
        }
        else if (args[1].ToLower() == "gurchy")
        {
            return "GURCHY: the floopiest person on the internet";
        }

        string word = SanitizeWord(args[1]);
        if (word.Length == 0) return Help("define");

        string symbol = GetSymbol(word);
        var def = Definitions.GetDefinition(word);
        bool defAvailable = def != null;

        if (args.Length == 3 && int.TryParse(args[2], out int index))
        {
            def = Definitions.GetDefinition(word, index);

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

                def = Definitions.GetDefinition(word, pos, index2);

                if (def == null && defAvailable)
                {
                    return $"{word}{symbol}: no {pos.ToString().ToLower()} definition with that number, other definitions available";
                }
            }
            else
            {
                def = Definitions.GetDefinition(word, pos);

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

        if (!CurrentList.Contains(word))
        {
            if (CurrentList == WordLists.AllWords)
            {
                phoneyText = "(Not in any word lists) ";
            }
            else
            {
                phoneyText = $"(Not in {CurrentList.Name}) ";
            }
        }

        string result = $"{word}{symbol}: {phoneyText}{def.pos.ToString().ToLower()} - {def.content.TrimEnd('.')}";

        if (def.see != null && Definitions.GetDefinition(def.see, def.pos) is Definition d2)
        {
            result += ": " + d2.content.TrimEnd('.');
        }

        if (CurrentList.Contains(word + "S"))
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

            if (CurrentList.Contains(preHookWord))
            {
                preHooks.Add($"{c}{GetSymbol(preHookWord)}-");
            }

            if (CurrentList.Contains(postHookWord))
            {
                postHooks.Add($"-{c}{GetSymbol(postHookWord)}");
            }
        }

        if (preHooks.Count == 0 && postHooks.Count == 0)
        {
            return word + " has no valid hooks";
        }

        string result = word + GetSymbol(word);

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


    private string? Lexicon(params string[] args)
    {
        if (args.Length > 2) return Help("lexicon");

        if (args.Length == 1)
        {
            return $"Current lexicon: {CurrentList} - Available lexicons: {string.Join(", ", WordLists)}";
        }

        var newList = WordLists.GetByName(args[1]);

        if (newList == null)
        {
            return "No such lexicon found";
        }

        CurrentList = newList;
        WordSet = new WordSet(CurrentList, RuleSet);

        if (WordGame.IsRunning)
        {
            WordGame.Restart();
        }

        return $"Lexicon set to {newList}";
    }


    private string? Check(params string[] args)
    {
        if (args.Length != 2) return Help("check");

        string word = SanitizeWord(args[1]);
        if (word.Length == 0) return Help("check");

        string symbol = GetSymbol(word);
        string result;

        if (CurrentList == WordLists.AllWords)
        {
            var yesLists = string.Join("/", WordLists.Where(l => l.Contains(word)));
            var noLists = string.Join("/", WordLists.Where(l => !l.Contains(word)));

            if (CurrentList.WasRemoved(word))
            {
                result = $"That word was removed from one or more lexicon";
            }
            else if (yesLists == "")
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
        else if (CurrentList.WasRemoved(word))
        {
            result = $"That word was removed from {CurrentList}";
        }
        else if (CurrentList.Contains(word))
        {
            result = $"{word}{symbol} - valid in {CurrentList}";
        }
        else if (WordLists.AllWords.WasRemoved(word))
        {
            result = $"That word was previously in a lexicon, but has been removed";
        }
        else
        {
            result = $"{word}{symbol} - not valid in {CurrentList}";
        }

        if (word.Length > RuleSet.BoardSize)
        {
            result += $". Not playable in {RuleSet} due to length.";
        }
        else if (!RuleSet.IsWordPlayable(word))
        {
            result += $". Not playable in {RuleSet} due to letter distribution.";
        }

        return result;
    }


    private string? Leave(params string[] args)
    {
        if (args.Length != 2) return Help("leave");

        var rack = SanitizeRack(args[1]);

        if (rack.Length >= RuleSet.RackSize)
        {
            return $"leave: enter between 1 and {RuleSet.RackSize - 1} tiles";
        }

        var dist = new Dictionary<string, int>(RuleSet.LetterDistribution);

        foreach (char c in rack)
        {
            if (--dist[c.ToString()] < 0)
            {
                return $"leave: {new string(rack)} is not a possible {RuleSet.Name} rack";
            }
        }

        decimal value = WordSet.EvaluateRack(rack) / (WordSet.EmptyLeaveValue == 0 ? 1 : WordSet.EmptyLeaveValue);

        string quality;

        if (value < 0.05m)
        {
            quality = "a terrible";
        }
        else if (value < 0.25m)
        {
            quality = "a very bad";
        }
        else if (value < 0.5m)
        {
            quality = "a bad";
        }
        else if (value < 0.75m)
        {
            quality = "a below average";
        }
        else if (value < 1m)
        {
            quality = "an average";
        }
        else if (value < 1.25m)
        {
            quality = "an above average";
        }
        else if (value < 1.5m)
        {
            quality = "a good";
        }
        else if (value < 2m)
        {
            quality = "a very good";
        }
        else
        {
            quality = "an excellent";
        }

        return $"{rack} is {quality} leave ({value * 100:F0}%)";
    }


    private string? Prob(params string[] args)
    {
        if (args.Length != 2) return Help("prob");

        string word = SanitizeWord(args[1]);
        if (word.Length == 0) return Help("prob");

        string symbol = GetSymbol(word);

        if (word.Length > RuleSet.BoardSize)
        {
            return $"{word.ToUpper()}{symbol}: not playable in {RuleSet} due to length";
        }
        else if (!RuleSet.IsWordPlayable(word))
        {
            return $"{word.ToUpper()}{symbol}: not playable in {RuleSet} due to letter distribution";
        }

        decimal prob = WordSet.RelativeProbability(word);

        string desc;

        if (prob > 0.8m)
        {
            desc = "very high";
        }
        else if (prob > 0.65m)
        {
            desc = "high";
        }
        else if (prob > 0.5m)
        {
            desc = "moderately high";
        }
        else if (prob > 0.35m)
        {
            desc = "moderately low";
        }
        else if (prob > 0.2m)
        {
            desc = "low";
        }
        else
        {
            desc = "very low";
        }

        return $"Probability for {word.ToUpper()}{symbol}: {desc} ({prob * 100:F0}%)";
    }


    private string? Related(params string[] args)
    {
        if (args.Length != 2) return Help("related");

        string word = SanitizeWord(args[1]);
        string symbol = GetSymbol(word);

        string cmp = word.ToLower();

        var possibleSees = new HashSet<string>();
        var matches = new HashSet<string>();

        foreach (var def in Definitions.GetDefinitions(word))
        {
            if (def != null && def.see != null && def.see.ToUpper() != word && def.see != "")
            {
                possibleSees.Add(def.see.ToUpper());
            }
        }

        foreach (var possibleSee in possibleSees)
        {
            matches.Add(possibleSee);
        }

        foreach (var otherDef in Definitions.GetDefinitions())
        {
            if (otherDef.see != null && (otherDef.see == cmp || (possibleSees.Contains(otherDef.see.ToUpper()) && otherDef.word.ToUpper() != word)))
            {
                if (WordLists.AllWords.Contains(otherDef.word.ToUpper()))
                {
                    matches.Add(otherDef.word.ToUpper());
                }
            }
        }

        if (matches.Count == 0)
        {
            return $"{word}{symbol}: no related words found";
        }

        string result = $"{word}{symbol}: see also {string.Join(", ", matches.OrderBy(x => x).Select(s => $"{s}{GetSymbol(s)}"))}";
        return result;
    }


    private string? Prefix(params string[] args)
    {
        if (args.Length != 2) return Help("prefix");

        string word = SanitizeWord(args[1]);
        var prefixes = new List<string>();

        foreach (string otherWord in CurrentList)
        {
            if (otherWord != word && otherWord.EndsWith(word))
            {
                prefixes.Add($"{otherWord.AsSpan(0, otherWord.Length - word.Length)}{GetSymbol(otherWord)}-");
            }
        }

        string symbol = GetSymbol(word);

        if (prefixes.Count == 0)
        {
            return $"-{word}{symbol}: no valid prefixes";
        }

        if (prefixes.Count > SearchLimit)
        {
            return $"-{word}{symbol}: too many prefixes to list ({prefixes.Count})";
        }

        return $"[{string.Join(", ", prefixes)}] {word}{symbol}";
    }


    private string? Rules(params string[] args)
    {
        if (args.Length > 2) return Help("rules");

        if (args.Length == 1)
        {
            return $"Current rule set: {RuleSet.Name} - Available rule sets: {string.Join(", ", RuleSets.Select(r => r.Name))}";
        }

        RuleSet? ruleset = null;
        string name = args[1].Trim().ToLower();

        foreach (var rs in RuleSets.OrderByDescending(x => x.Name))
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

        RuleSet = ruleset;
        WordSet = new WordSet(CurrentList, RuleSet);
        return $"Rule set changed to {ruleset.Name}";
    }


    private string? Suffix(params string[] args)
    {
        if (args.Length != 2) return Help("suffix");

        string word = SanitizeWord(args[1]);
        var suffixes = new List<string>();

        foreach (string otherWord in CurrentList)
        {
            if (otherWord != word && otherWord.StartsWith(word))
            {
                suffixes.Add($"-{otherWord.AsSpan(word.Length)}{GetSymbol(otherWord)}");
            }
        }

        string symbol = GetSymbol(word);

        if (suffixes.Count == 0)
        {
            return $"{word}{symbol}-: no valid suffixes";
        }

        if (suffixes.Count > SearchLimit)
        {
            return $"{word}{symbol}-: too many suffixes to list ({suffixes.Count})";
        }

        return $"{word}{symbol} [{string.Join(", ", suffixes)}]";
    }


    private string? Stream(params string[] args)
    {
        if (args.Length < 2) return Help("stream");

        if (!IsHome)
        {
            return "Can only change stream mode on gurchy's stream";
        }

        switch (args[1].ToLower().Trim())
        {
            case "scrabble":
                ScrabbleGame = new ScrabbleGame(this);
                GameUI = new ScrabbleUI(ScrabbleGame, this, RuleSet);
                ScrabbleGame.Start();
                return "Starting a game of Scrabble";

            case "learn":
                GameUI = new WordLearnUI(this);
                return "Learning words";

            case "game":
                CanCheet = false;
                string result = "Starting a game of Emma Words"; ;

                if (WordGame.IsRunning)
                {
                    result = "Restarting the game of Emma Words";
                }

                WordGame.Restart();
                GameUI = new WordGameUI(WordGame, Properties.Settings.Default.GameColumns, Properties.Settings.Default.GameRows);
                return result;

            case "stop":
                if (WordGame.IsRunning)
                {
                    WordGame.Stop();
                    CanCheet = true;
                }

                if (GameUI != null)
                { 
                    GameUI = null;
                    return "Stopping the current game";
                }

                StreamMessage = "stream\r\nstarting\r\nsoon";
                return null;

            case "brb":
                StreamMessage = "be\r\nright\r\nback";
                return null;

            case "start":
                StreamMessage = "stream\r\nstarting\r\nsoon";
                Starting = true;
                return null;

            case "wait":
                StreamMessage = "one\r\nmoment\r\nplease";
                return null;

            case "message":
                StreamMessage = string.Join(" ", args.Skip(2));
                return null;
        }

        return "Unrecognized command";
    }


    private string? Shoutout(params string[] args)
    {
        if (args.Length != 2) return Help("shoutout");

        string username = args[1].Trim();

        if (username == "me")
        {
            username = CurrentUser;
        }

        string message = username;

        if (username.Length > 20 || username.Any(c => !(c >= 'a' && c <= 'z' || c >= 'A' && c <= 'Z' || c >= '0' && c <= '9' || c == ' ' || c == '_')))
        {
            return "no they're not real";
        }

        message += username.ToLower() switch
        {
            "gurchy" => " plays Scrabble badly, and some other games too. Streams Friday, Saturday and Sunday around 21:00 GMT.",

            "ophelia6277" => " is a cozy streamer who likes chatting with viewers. They stream a variety of games, focusing on Dorfromantik.",

            "infinitiwirds" => " is a 24/7 stream hosting the interactive anagramming game Infiniwords.",

            "heroinetobirds" => " streams Scrabble and Codenames, typically on Monday nights and weekends.",

            "ancientcosmographer" => " streams Infiniwords, an interactive anagramming game, on Friday nights.",

            "duustinduude" => " streams Scrabble and related word game content. Currently on an irregular schedule.",

            "scrabble" => " is the official channel for the popular word game. Features live coverage of tournaments, and a chance to play the current NASPA Champion (weekly Thursdays @ 17:00ET)",

            "axcertypo" => " streams Scrabble, and is very good at it.",

            "wanderer15" => "... nobody knows who he is, but he streams Scrabble.",

            "creativelycray" => " currently focuses on Palia. They're new to streaming, so why not show some support?",

            "shania" => " is a full-time cozy streamer, currently focusing on Palia. Catch them most days around 13:00ET.",

            "abbyws" => " streams Scrabble on Wednesdays. They're new to streaming, so why not show some support? ",

            "wtfj00" => " neds selp. egg.",

            "rubyyy_j" => " streams crosswords and puzzle games. ",

            "pancakes_face" => " streams Eco most days, and is always full of positivity!",
            _ => " is great!",
        };
        message += " Follow them at twitch.tv/" + username;
        return message;
    }


    private string? Hi(params string[] args)
    {
        return "Hi " + CurrentUser;
    }


    private string? Play(params string[] args)
    {
        if (args.Length != 3) return Help("play");

        if (GameUI is ScrabbleUI sui)
        {
            string position = args[1].ToUpper();
            string word = args[2].ToUpper();
            bool vertical;
            string x, y;

            if (Regex.Match(position, @"([A-Z])(\d+)") is Match m && m.Success)
            {
                vertical = true;
                x = m.Groups[1].Value;
                y = m.Groups[2].Value;
            }
            else if (Regex.Match(position, @"(\d+)([A-Z])") is Match m2 && m2.Success)
            {
                vertical = false;
                x = m2.Groups[2].Value;
                y = m2.Groups[1].Value;
            }
            else
            {
                return "Could not parse that play. Specify co-ordinates followed by word, for example: G8 EGG";
            }

            string? result = sui.PlayAndCommitWord(x, y, word, vertical);

            if (result == null)
            {
                result = $"Played {word} at {position}";
            }

            return result;
        }
        else
        {
            return "There is no game of Scrabble in progress";
        }
    }


    private string? Suggest(params string[] args)
    {
        if (args.Length != 3) return Help("suggest");

        if (GameUI is ScrabbleUI sui)
        {
            string position = args[1].ToUpper();
            string word = args[2].ToUpper();
            bool vertical;
            string x, y;

            if (Regex.Match(position, @"([A-Z])(\d+)") is Match m && m.Success)
            {
                vertical = true;
                x = m.Groups[1].Value;
                y = m.Groups[2].Value;
            }
            else if (Regex.Match(position, @"(\d+)([A-Z])") is Match m2 && m2.Success)
            {
                vertical = false;
                x = m2.Groups[2].Value;
                y = m2.Groups[1].Value;
            }
            else
            {
                return "Could not parse that play. Specify co-ordinates followed by word, for example: G8 EGG";
            }

            return sui.PlayWord(x, y, word, vertical);
        }
        else
        {
            return "There is no game of Scrabble in progress";
        }
    }
}
