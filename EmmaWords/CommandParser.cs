
namespace EmmaWords;

partial class WordService
{
    public string? InterpretCommand(string command)
    {
        var args = command.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToArray();
        if (args.Length == 0) return null;

        string commandName = args[0].ToLower();

        Func<string[], string?> function = commandName switch
        {
            "affix" or "infix" => Affix,
            "anagram" => Anagram,
            "check" => Check,
            "cheet" => Cheet,
            "commands" => Commands,
            "contains" => Contains,
            "define" => Define,
            "discord" => Discord,
            "edit" => Edit,
            "egg" => Egg,
            "guess" => Guess,
            "help" or "holp" => Help,
            "hi" => Hi,
            "hook" or "hooks" => Hook,
            "leave" or "equity" => Leave,
            "lexicon" or "dict" or "dictionary" or "dictionaries" or "list" or "lists" => Lexicon,
            "lurk" => Lurk,
            "nicefind" => NiceFind,
            "pattern" or "match" => Pattern,
            "play" => Play,
            "prefix" or "prefixes" or "endswith" => Prefix,
            "prob" => Prob,
            "recommend" => Recommend,
            "related" => Related,
            "rule" or "rules" => Rules,
            "set" => Set,
            "shoutout" or "so" => Shoutout,
            "stream" => Stream,
            "suggest" => Suggest,
            "suffix" or "suffixes" or "startswith" => Suffix,
            _ => Unknown
        };

        return function(args);
    }


    private string? Unknown(params string[] _)
    {
        return $"Unknown command - try {CommandPrefix}help";
    }


    private static string Commands(params string[] _)
    {
        var commands = new List<string>
        {
            "affix",
            "anagram",
            "check",
            "cheet",
            "commands",
            "contains",
            "define",
            "discord",
            "egg",
            "help",
            "hook",
            "leave",
            "list",
            "pattern",
            "prefix",
            "prob",
            "recommend",
            "related",
            "rules",
            "shoutout",
            "suggest",
            "suffix"
        };

        string[] homeChannelCommands =
        {
            "guess",
            "nicefind",
            "play",
            "set",
            "stream"
        };

        if (IsHome)
        {
            commands.AddRange(homeChannelCommands);
            commands.Sort();
        }

        return "Supported commands: " + string.Join(", ", commands);
    }


    private string? Help(params string[] args)
    {
        string? command = args.LastOrDefault();

        if (command == "commands")
        {
            return Commands(args);
        }

        string syntax = command switch
        {
            "affix" => "affix WORD -- Show prefixes and suffixes for a word",
            "anagram" => "anagram WORD -- Show anagrams of a word, if allowed",
            "check" => "check WORD -- Check if a word is in the current list",
            "cheet" => "cheet -- Toggle whether the anagram command is allowed",
            "contains" => "contains WORD -- Show words containing a word",
            "define" => "define WORD [POS] [NUMBER] -- Show a dictionary definition",
            "discord" => "discord -- Show the discord link",
            "edit" => "edit -- Edit the current rule set",
            "egg" => "egg",
            "guess" => "guess [GUESSES ...] -- Submit guesses to the anagramming game",
            "help" => "help COMMAND -- Show help for another command\r\n" + Commands(args),
            "hi" => "hi -- say hi",
            "hook" => "hook WORD -- Show hooks for a word",
            "leave" => "leave RACK -- Evaluate a leave",
            "lexicon" => "lexicon [{NAME | all}] -- Set the active lexicon, or show available lexicons",
            "lurk" => "lurk -- This is just here so !lurk doesn't make an error message",
            "nicefind" => "nicefind WORD -- Say that a word was a nice find",
            "pattern" => "pattern REGEX -- Search for words matching a regular expression",
            "play" => "play POSITION WORD -- Play a word in a Scrabble game",
            "prefix" => "prefix WORD -- Show prefixes for a word",
            "prob" => "prob WORD -- Show relative probability of word, compared to other words of the same length",
            "related" => "related WORD -- Show alternative forms and inflections of a word",
            "recommend" => "recommend CATEGORY -- Provide recommendations",
            "rules" => "rules [NAME] -- Set the active rule set, or show available rule sets",
            "set" => "set SETTING VALUE -- Change game settings",
            "shoutout" => "shoutout CHANNEL -- Shout out another streamer",
            "stream" => "stream MODE -- Sets the stream mode",
            "suggest" => "suggest POSITION WORD -- Suggest a play without committing it",
            "suffix" => "suffix WORD -- Show suffixes for a word",
            _ => "<command> [OPTIONS ...]\r\n" + Commands(args)
        };

        return $"Usage: {CommandPrefix}{syntax}";
    }


    private static string SanitizeWord(string word)
    {
        return new string(word.Where(char.IsAsciiLetter).ToArray()).ToUpper();
    }


    private static string SanitizeRack(string rack)
    {
        return new string(rack.Where(c => char.IsAsciiLetter(c) || c == '?').ToArray()).ToUpper();
    }
}
