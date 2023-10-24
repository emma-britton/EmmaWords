
using Emma.Lib;

namespace Emma.IsBot;

public partial class CommandParser
{
    private string m_Username;
    private readonly WordService WordService;
    private readonly Dictionary<string, Func<string[], string?>> Commands = new();
    private readonly Dictionary<string, string> Aliases = new();
    private readonly Dictionary<string, string> HelpMessages = new();

    public string Username => m_Username;


    public CommandParser(WordService wordService)
    {
        WordService = wordService;
        m_Username = "(console)";

        AddCommand("affix", Affix, "affix WORD -- Show prefixes and suffixes for a word");
        AddCommand("anagram", Anagram, "anagram WORD -- Show anagrams of a word, if allowed");
        AddCommand("check", Check, "check WORD -- Check if a word is in the current list");
        AddCommand("cheet", Cheet, "cheet -- Toggle whether the anagram command is allowed");
        AddCommand("commands", ListCommands, "commands -- Lists available commands");
        AddCommand("contains", Contains, "contains WORD -- Show words containing a word");
        AddCommand("count", Count, "count -- Report the number of words in the current lexicon");
        AddCommand("define", Define, "define WORD [POS] [NUMBER] -- Show a dictionary definition");
        AddCommand("egg", Egg, "egg");
        AddCommand("help", Help, "help COMMAND -- Show help for another command");
        AddCommand("hi", Hi, "hi -- say hi");
        AddCommand("hook", Hook, "hook WORD -- Show hooks for a word");
        AddCommand("leave", Leave, "leave RACK -- Evaluate a leave");
        AddCommand("lexicon", SetLexicon, "lexicon [{NAME | all}] -- Set the active lexicon, or show available lexicons");
        AddCommand("lurk", Lurk, "lurk -- This is just here so !lurk doesn't make an error message");
        AddCommand("nicefind", NiceFind, "nicefind WORD -- Say that a word was a nice find");
        AddCommand("pattern", Pattern, "pattern REGEX -- Search for words matching a regular expression");
        AddCommand("prefix", Prefix, "prefix WORD -- Show prefixes for a word");
        AddCommand("prob", Prob, "prob WORD -- Show relative probability of word, compared to other words of the same length");
        AddCommand("related", Related, "related WORD -- Show alternative forms and inflections of a word");
        AddCommand("rules", Rules, "rules [NAME] -- Set the active rule set, or show available rule sets");
        AddCommand("suffix", Suffix, "suffix WORD -- Show suffixes for a word");

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


    public void AddCommand(string keyword, Func<string[], string?> command, string help)
    {
        Commands[keyword.ToLower()] = command;
        HelpMessages[keyword.ToLower()] = help;
    }


    public void AddAlias(string alias, string keyword)
    {
        Aliases[alias.ToLower()] = keyword.ToLower();
    }


    public string? InterpretCommand(StreamMessage message, string command)
    {
        var args = command.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToArray();
        if (args.Length == 0) return null;

        m_Username = message.Username;

        string keyword = args[0].ToLower();

        if (Commands.TryGetValue(keyword, out var function))
        {
            return function(args);
        }
        else if (Aliases.TryGetValue(keyword, out var alias) && Commands.TryGetValue(alias, out var aliasFunction))
        {
            return aliasFunction(args);
        }

        return null;
        //return $"Unknown command - try {Program.Config["CommandPrefix"]}help";
    }


    private string ListCommands(params string[] _)
    {
        return "Supported commands: " + string.Join(", ", Commands.Keys);
    }


    public string? Help(params string[] args)
    {
        string? command = args.LastOrDefault()?.ToLower();

        if (command == "commands")
        {
            return ListCommands(args);
        }
        else if (command == null)
        {
            return $"Syntax: <command> [OPTIONS ...]\\r\\n" + ListCommands(args);
        }
        else if (HelpMessages.TryGetValue(command, out string? helpMessage))
        {
            return helpMessage;
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
