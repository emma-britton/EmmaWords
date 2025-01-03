
using Emma.Lib;

namespace Emma.IsBot;

public partial class CommandParser
{
    private string m_Username;
    private readonly WordService WordService;
    private readonly Dictionary<string, Command> Commands = [];
    private readonly Dictionary<string, string> Aliases = [];
    private readonly Dictionary<string, Func<string, string, string?>> Rewards = [];

    public string Username => m_Username;


    public CommandParser(WordService wordService)
    {
        WordService = wordService;
        m_Username = "(console)";

        AddCommand("affix", Affix, "affix WORD -- Show prefixes and suffixes for a word", Permission.Anyone);
        AddCommand("anagram", Anagram, "anagram WORD -- Show anagrams of a word, if allowed", Permission.Anyone);
        AddCommand("check", Check, "check WORD -- Check if a word is in the current list", Permission.Anyone);
        AddCommand("cheet", Cheet, "cheet -- Toggle whether the anagram command is allowed", Permission.VIP);
        AddCommand("commands", ListCommands, "commands -- List available commands", Permission.Anyone);
        AddCommand("contains", Contains, "contains WORD -- Show words containing a word", Permission.Anyone);
        AddCommand("count", Count, "count -- Report the number of words in the current lexicon", Permission.Anyone);
        AddCommand("define", Define, "define WORD [POS] [NUMBER] -- Show a dictionary definition", Permission.Anyone);
        AddCommand("help", Help, "help COMMAND -- Show help for another command", Permission.Anyone);
        AddCommand("hi", Hi, "hi -- say hi", Permission.Anyone);
        AddCommand("hook", Hook, "hook WORD -- Show hooks for a word", Permission.Anyone);
        AddCommand("leave", Leave, "leave RACK -- Evaluate a leave", Permission.Anyone);
        AddCommand("lexicon", SetLexicon, "lexicon [{NAME | all}] -- Set the active lexicon, or show available lexicons", Permission.VIP);
        AddCommand("lurk", Lurk, "lurk -- Say that you are lurking", Permission.Anyone);
        AddCommand("nicefind", NiceFind, "nicefind WORD -- Say that a word was a nice find", Permission.Anyone);
        AddCommand("pattern", Pattern, "pattern REGEX -- Search for words matching a regular expression", Permission.Anyone);
        AddCommand("prefix", Prefix, "prefix WORD -- Show prefixes for a word", Permission.Anyone);
        AddCommand("prob", Prob, "prob WORD -- Show relative probability of word, compared to other words of the same length", Permission.Anyone);
        AddCommand("related", Related, "related WORD -- Show alternative forms and inflections of a word", Permission.Anyone);
        AddCommand("rules", Rules, "rules [NAME] -- Set the active rule set, or show available rule sets", Permission.VIP);
        AddCommand("suffix", Suffix, "suffix WORD -- Show suffixes for a word", Permission.Anyone);

        AddAlias("anag", "anagram");
        AddAlias("dict", "lexicon");
        AddAlias("dictionary", "lexicon");
        AddAlias("dictionaries", "lexicon");
        AddAlias("endswith", "prefix");
        AddAlias("equity", "leave");
        AddAlias("find", "pattern");
        AddAlias("holp", "help");
        AddAlias("hooks", "hook");
        AddAlias("infix", "affix");
        AddAlias("lexicons", "lexicon");
        AddAlias("list", "lexicon");
        AddAlias("lists", "lexicon");
        AddAlias("match", "pattern");
        AddAlias("prefixes", "prefix");
        AddAlias("rule", "rules");
        AddAlias("search", "pattern");
        AddAlias("startswith", "suffix");
        AddAlias("suffixes", "suffix");
        AddAlias("words", "count");
    }


    public void AddCommand(string keyword, Func<string[], string?> action, string help, Permission permission)
    {
        Commands[keyword.ToLower()] = new Command(keyword, action, help, permission);
    }


    public void AddAlias(string alias, string keyword)
    {
        Aliases[alias.ToLower()] = keyword.ToLower();
    }


    public void AddReward(string name, Func<string, string, string?> command)
    {
        Rewards[name.ToLower()] = command;
    }


    public string? InterpretCommand(StreamMessage message, string argstring)
    {
        var args = argstring.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToArray();
        if (args.Length == 0) return null;

        m_Username = message.Username;

        string keyword = args[0].ToLower();

        if (Commands.TryGetValue(keyword, out var command))
        {
            if (!command.HasPermission(message))
            {
                return PermissionDenied(command.Name);
            }

            return command.Action(args);
        }
        else if (Aliases.TryGetValue(keyword, out var alias) && Commands.TryGetValue(alias, out var aliasCommand))
        {
            if (!aliasCommand.HasPermission(message))
            {
                return PermissionDenied(aliasCommand.Name);
            }

            return aliasCommand.Action(args);
        }

        return null;
    }


    private string? PermissionDenied(string command)
    {
        if (Username.Equals("machacatcha", StringComparison.OrdinalIgnoreCase))
        {
            string[] firstPart =
            [
                "macha",
                "matcha",
                "matcher",
                "masher",
                "masha",
                "macher",
                "match",
                "matchy",
                "mashed",
                "mashier",
                "macho"
            ];

            string[] secondPart =
            [
                "catcha",
                "catch",
                "catcher",
                "casha",
                "cacher",
                "cacha",
                "cachet",
                "catchy",
                "casher",
                "chacha",
                "cashtato",
                "cashier",
                "catcho"
            ];

            var random = new Random();
            return $"{firstPart[random.Next(firstPart.Length)]} {secondPart[random.Next(secondPart.Length)]}";
        }
        
        return null;
    }


    public string? InterpretReward(StreamMessage message)
    {
        m_Username = message.Username;

        if (message.RewardName != null)
        {
            string rewardName = message.RewardName.Trim().ToLower();

            if (Rewards.TryGetValue(rewardName, out var function))
            {
                return function(Username, rewardName);
            }
        }

        return null;
    }


    private string ListCommands(params string[] _)
    {
        return "Supported commands: " + string.Join(", ", Commands.Keys.Order());
    }


    public string? Help(params string[] args)
    {
        string? commandName = args.LastOrDefault()?.ToLower();

        if (commandName == "commands")
        {
            return ListCommands(args);
        }
        else if (commandName == null)
        {
            return $"Syntax: <command> [OPTIONS ...]\\r\\n" + ListCommands(args);
        }
        else if (Commands.TryGetValue(commandName, out var command))
        {
            return command.Help;
        }

        return "Unknown command. Available commands: " + ListCommands(args);
    }


    private static string SanitizeWord(string word)
    {
        return new string(word.Where(c => char.IsAsciiLetter(c) || c == '\'' || c == '-').ToArray()).ToUpper();
    }


    private static string SanitizeRack(string rack)
    {
        return new string(rack.Where(c => char.IsAsciiLetter(c) || c == '?').ToArray()).ToUpper();
    }
}
