﻿using Emma.Anagramming;
using Emma.IsBot;
using Emma.Lib;
using Emma.Scrabble;
using Emma.WordLearner;
using System.IO;
using System.Text.RegularExpressions;

namespace Emma.Stream;

public class EmmaStream
{
    private readonly MainForm MainForm;
    private readonly WordService WordService;
    private readonly CommandParser CommandParser;
    private readonly StartScreen StartScreen;
    
    private ScrabbleUI? ScrabbleUI;
    private AnagramUI? AnagramUI;
    private WordLearnUI? LearnUI;

    private string Player1Name = "Player 1";
    private string Player2Name = "Player 2";
    
    public TwitchBot TwitchBot { get; }
    public string Message { get; set; } = "stream starting soon";
    public Queue<string> PlayerQueue { get; set; } = new();


    public EmmaStream(MainForm mainForm, WordService wordService, CommandParser commandParser)
    {
        MainForm = mainForm;
        WordService = wordService;
        CommandParser = commandParser;
        
        CommandParser.AddCommand("discord", Discord, "discord -- Show the discord link");
        CommandParser.AddCommand("message", ChangeMessage, "message -- Change the message on the title screen");
        CommandParser.AddCommand("subscribe", Subscribe, "subscribe -- Show a message about subscriptions");
        CommandParser.AddCommand("shoutout", Shoutout, "shoutout CHANNEL -- Shout out another streamer");
        CommandParser.AddCommand("stream", Stream, "stream MODE -- Set the stream mode");
        CommandParser.AddCommand("start", Start, "start -- Shows the 'stream start' screen");
        CommandParser.AddCommand("end", End, "end -- Shows the 'stream end' screen");
        CommandParser.AddCommand("brb", Brb, "brb -- Shows the 'be right back' screen");
        CommandParser.AddCommand("suggest", Suggest, "suggest PLAY -- Suggest a play in a game of Scrabble. Play should be formatted like: H6 WORD");
        CommandParser.AddCommand("play", Play, "play PLAY -- Make a play in a game of Scrabble. Play should be formatted like: H6 WORD");
        CommandParser.AddCommand("guess", Guess, "guess WORD -- Guess an answer to the anagramming game");
        commandParser.AddCommand("edit", Edit, "edit -- Edit the current rule set");
        commandParser.AddCommand("join", Join, "join -- Join the queue to play Scrabble with Emma");
        commandParser.AddCommand("next", Next, "next -- Starts the next game of Scrabble");
        commandParser.AddCommand("add", Add, "add PLAYER -- Adds a game of Scrabble to the queue");
        commandParser.AddCommand("remove", Remove, "remove PLAYER -- Removes a game of Scrabble from the queue");
        commandParser.AddCommand("skip", Skip, "skip -- Skips the current game of Scrabble");
        commandParser.AddCommand("clear", Clear, "clear -- Clears the queue to play Scrabble");
        commandParser.AddCommand("raid", Raid, "raid -- Displays raid message");
        CommandParser.AddCommand("hug", Hug, "hug -- Send a hug");

        CommandParser.AddAlias("sub", "subscribe");
        CommandParser.AddAlias("so", "shoutout");
        CommandParser.AddAlias("msg", "message");
        CommandParser.AddAlias("stop", "end");
        CommandParser.AddAlias("idea", "suggest");
        CommandParser.AddAlias("commit", "play");

        if (!string.IsNullOrWhiteSpace(Properties.Settings.Default.TwitchUsername))
        {
            TwitchBot = new TwitchBot(commandParser,
            Properties.Settings.Default.CommandPrefix,
            Properties.Settings.Default.TwitchClientID,
            Properties.Settings.Default.TwitchAccessToken,
            Properties.Settings.Default.TwitchUsername,
            Properties.Settings.Default.TwitchOAuth,
            Properties.Settings.Default.TwitchChannel);

            TwitchBot.ChatCleared += Bot_ChatCleared;
            TwitchBot.Message += Bot_Message;
            TwitchBot.Run();
        }

        StartScreen = new StartScreen(this, MainForm);
        MainForm.UI = StartScreen;
    }


    private void Bot_Message(object? sender, StreamMessage e)
    {
        if (MainForm.UI != null)
        {
            MainForm.UI.HandleMessage(e);
        }
    }


    private void Bot_ChatCleared(object? sender, EventArgs e)
    {
        StartScreen.ClearChat();
    }


    private string? Discord(params string[] args)
    {
        return "Join emma discord to chat with other viewers and be notified of streams: https://discord.gg/daeEQCjs88";
    }


    private string? ChangeMessage(params string[] args)
    {
        if (args.Length != 2) return CommandParser.Help("message");

        Message = args[1].Trim();
        return null;
    }


    private string? Stream(params string[] args)
    {
        if (args.Length != 2) return CommandParser.Help("stream");

        string mode = args[1].Trim().ToLower();

        switch (mode)
        {
            case "start":
                MainForm.UI = StartScreen;
                Message = "stream starting soon";
                StartScreen.StartStream();
                break;

            case "end":
                MainForm.UI = StartScreen;
                Message = "stream ending soon";
                StartScreen.StopStream();
                break;

            case "brb":
                MainForm.UI = StartScreen;
                Message = "be right back";
                break;

            case "scrabble":
                if (ScrabbleUI != null)
                {
                    ScrabbleUI.Stop();
                }

                var scrabbleGame = new ScrabbleGame(WordService.ActiveRuleSet, WordService.ActiveLexicon, Player1Name, Player2Name);
                scrabbleGame.Start();
                ScrabbleUI = new ScrabbleUI(scrabbleGame, MainForm);
                MainForm.UI = ScrabbleUI;
                break;

            case "words":
                if (AnagramUI != null)
                {
                    AnagramUI.Stop();
                }

                var anagramGame = new AnagramGame(WordService.ActiveLexicon);
                anagramGame.Start();
                AnagramUI = new AnagramUI(anagramGame, MainForm);
                MainForm.UI = AnagramUI;
                break;

            case "learn":
                if (LearnUI != null)
                {
                    LearnUI.Stop();
                }

                string file1 = Path.Combine(Properties.Settings.Default.BaseFolder, "learn", "CSW21-word-learn.txt");
                string file2 = Path.Combine(Properties.Settings.Default.BaseFolder, "learn", "CSW21-alphagram-learn.txt");

                var learning = new WordLearn(WordService.ActiveLexicon, file1, file2, 100);
                LearnUI = new WordLearnUI(learning, MainForm);
                MainForm.UI = LearnUI;
                break;
        }
        
        return null;
    }

    private string? Start(params string[] args) => Stream("start", "start");
    private string? End(params string[] args) => Stream("end", "end");
    private string? Brb(params string[] args) => Stream("brb", "brb");


    private string? Subscribe(params string[] args)
    {
        return "If you're enjoying the content, subscribing is a great way to show Emma your support, and gives you some cute emotes to use!";
    }


    private string? Shoutout(params string[] args)
    {
        if (args.Length != 2) return CommandParser.Help("shoutout");

        string username = args[1].Trim();

        if (username == "me")
        {
            username = CommandParser.Username.ToLower();
        }

        string message = username;

        if (username.Length > 20 || username.Any(c => !(c >= 'a' && c <= 'z' || c >= 'A' && c <= 'Z' || c >= '0' && c <= '9' || c == ' ' || c == '_')))
        {
            return "no they're not real";
        }

        if (username == "wanderer15")
        {
            return "nobody knows who he is, but wanderer15 makes great Scrabble content on YouTube: youtube.com/@wanderer15";
        }

        message += username.ToLower() switch
        {
            "abbyws" => " streams Scrabble on Wednesdays. They're new to streaming, so why not show some support? ",

            "ancientcosmographer" => " streams Infiniwords, an interactive anagramming game, on Friday nights.",

            "axcertypo" => " streams Scrabble, and is very good at it.",

            "duustinduude" => " streams Scrabble and other word game content. Currently on an irregular schedule.",

            "gurchy" => " plays Scrabble badly, and some other games too. Streams on Friday, Saturday and Sunday around 21:00GMT.",

            "heroinetobirds" => " streams Scrabble and Codenames, typically on Monday nights and weekends.",

            "infinitiwirds" => " is a 24/7 stream hosting the interactive anagramming game Infiniwords.",

            "ophelia6277" => " is a cozy streamer who likes chatting with viewers. They stream a variety of games, focusing on Dorfromantik.",

            "pancakes_face" => " streams Eco most days, and is always full of positivity!",

            "rubyyy_j" => " streams crosswords and puzzle games. ",

            "scrabble" => " is the official channel for the popular word game. Features live coverage of tournaments.",

            "shania" => " is a full-time cozy streamer. Catch them most days around 13:00ET.",

            "wtfj00" => " neds selp. egg.",

            "y2j_twitch" => " take on a variety of challenges - blindfolded Hades, split-controller Celeste, and more!",

            _ => "#",
        };

        if (message.EndsWith("#"))
        {
            return "sorry I don't know who that is";
        }

        message += " Follow them at twitch.tv/" + username;
        return message;
    }


    private string? Suggest(params string[] args)
    {
        if (args.Length != 3) return CommandParser.Help("suggest");

        if (MainForm.UI is ScrabbleUI sui)
        {
            string position = args[1].ToUpper();
            string word = args[2].ToUpper().Replace("(", "").Replace(")", "");
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


    private string? Play(params string[] args)
    {
        if (args.Length != 3) return CommandParser.Help("play");

        if (MainForm.UI is ScrabbleUI sui)
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
            result ??= $"Played {word} at {position}";

            return result;
        }
        else
        {
            return "There is no game of Scrabble in progress";
        }
    }


    private string? Guess(params string[] args)
    {
        if (args.Length < 2) return CommandParser.Help("guess");

        if (MainForm.UI is AnagramUI aui)
        {
            foreach (string arg in args.Skip(1))
            {
                string word = arg.ToUpper().Trim();
                aui.Game.SubmitGuess(CommandParser.Username, word);
            }
        }
        else
        {
            return "There is no game of Emma Words in progress";
        }

        return null;
    }


    private string? Edit(params string[] args)
    {
        var ruleSet = WordService.ActiveRuleSet;

        if (ruleSet.Name != "Variant rules")
        {
            ruleSet = ruleSet.Clone();
            WordService.ActiveRuleSet = ruleSet;
        }

        var editor = new RulesetEditor(WordService);

        if (editor.ShowDialog() == DialogResult.OK)
        {
            WordService.RuleSets.Add(ruleSet);
        }

        return "Rule set updated";
    }


    private string? Join(params string[] args)
    {
        string what = (CommandParser.Username + " - " + string.Join(" ", args.Skip(1))).TrimEnd(' ', '-');

        int index = PlayerQueue.ToList().FindIndex(q => q.StartsWith(CommandParser.Username));

        if (index >= 0)
        {
            return $"You are currently position {index + 1} in the queue";
        }
        else
        {
            PlayerQueue.Enqueue(what);

            if (PlayerQueue.Count == 1)
            {
                return $"@{CommandParser.Username} is next to play!";
            }
            else
            {
                return $"You are now position {PlayerQueue.Count} in the queue";
            }
        }
    }


    private string? Next(params string[] args)
    {
        if (args.Length != 1) return CommandParser.Help("next");

        if (PlayerQueue.Count > 0)
        {
            PlayerQueue.Dequeue();
        }

        if (PlayerQueue.Count == 0)
        {
            return "The queue is empty";
        }

        string what = PlayerQueue.Peek();
        if (what.Contains(' ')) what = what[..what.IndexOf(" ")];

        return $"@{what} is next to play";
    }


    private string? Skip(params string[] args)
    {
        if (args.Length != 1) return CommandParser.Help("skip");

        if (PlayerQueue.Count == 0)
        {
            return "The queue is empty";
        }
        
        string skipped = PlayerQueue.Dequeue();
        PlayerQueue.Enqueue(skipped);
        if (skipped.Contains(' ')) skipped = skipped[..skipped.IndexOf(" ")];

        string what = PlayerQueue.Peek();
        if (what.Contains(' ')) what = what[..what.IndexOf(" ")];

        return $"Skipped {skipped}, @{what} is next to play";
    }


    private string? Add(params string[] args)
    {
        if (args.Length < 2) return CommandParser.Help("add");

        string username = args[1].Trim();
        string gameDesc = (username + (args.Length == 2 ? "" : " - " + string.Join(" ", args.Skip(2)))).Trim();

        int index = PlayerQueue.ToList().FindIndex(q => q.StartsWith(username));

        if (index >= 0)
        {
            return $"{username} is currently position {index + 1} in the queue";
        }
        else
        {
            PlayerQueue.Enqueue(gameDesc);

            if (PlayerQueue.Count == 1)
            {
                return $"@{username} is next to play!";
            }
            else
            {
                return $"{username} is now position {PlayerQueue.Count} in the queue";
            }
        }
    }


    private string? Remove(params string[] args)
    {
        if (args.Length < 2) return CommandParser.Help("remove");

        string username = args[1].Trim();
        var current = PlayerQueue.ToList();

        PlayerQueue.Clear();
        string result = $"{username} is not in the queue";

        foreach (string gameDesc in current)
        {
            if (gameDesc == username.ToLower() || gameDesc.StartsWith(username.ToLower()))
            {
                result = $"{username} has been removed from the queue";
            }
            else
            {
                PlayerQueue.Enqueue(gameDesc);
            }
        }

        return result;
    }


    private string? Clear(params string[] args)
    {
        if (args.Length != 1) return CommandParser.Help("clear");

        PlayerQueue.Clear();
        return null;
    }

    
    private string? Raid(params string[] args)
    {
        if (args.Length != 1) return CommandParser.Help("raid");

        return "gurchyBlue gurchyBlue GURCHY RAID gurchyBlue gurchyBlue";
    }


    private string? Hug(params string[] args)
    {
        return $"@{CommandParser.Username} gurchyHug";
    }
}