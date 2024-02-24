using Emma.Anagramming;
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
    private readonly Dictionary<string, HashSet<string>> Flowers = new();
    private readonly Random Random = new();

    private ScrabbleUI? ScrabbleUI;
    private AnagramUI? AnagramUI;
    private WordLearnUI? LearnUI;

    private string Player1Name = "Player 1";
    private string Player2Name = "Player 2";
    
    public TwitchBot? TwitchBot { get; }
    public string Message { get; set; } = "stream starting soon";
    private bool m_QueueActive = false;
    public Queue<string> PlayerQueue { get; set; } = new();


    public EmmaStream(MainForm mainForm, WordService wordService, CommandParser commandParser)
    {
        MainForm = mainForm;
        WordService = wordService;
        CommandParser = commandParser;

        commandParser.AddReward("flower of the day", Flower);

        CommandParser.AddCommand("discord", Discord, "discord -- Show the discord link", Permission.Anyone);
        CommandParser.AddCommand("message", ChangeMessage, "message -- Change the message on the title screen", Permission.Moderator);
        CommandParser.AddCommand("subscribe", Subscribe, "subscribe -- Show a message about subscriptions", Permission.Anyone);
        CommandParser.AddCommand("shoutout", Shoutout, "shoutout CHANNEL -- Shout out another streamer", Permission.VIP);
        CommandParser.AddCommand("stream", Stream, "stream MODE -- Set the stream mode", Permission.Moderator);
        CommandParser.AddCommand("start", Start, "start -- Show the start screen", Permission.Moderator);
        CommandParser.AddCommand("end", End, "end -- Show the end screen", Permission.Moderator);
        CommandParser.AddCommand("brb", Brb, "brb -- Show the 'be right back' screen", Permission.Moderator);
        CommandParser.AddCommand("suggest", Suggest, "suggest PLAY -- Suggest a play in a game of Scrabble. Play should be formatted like: H6 WORD", Permission.Anyone);
        CommandParser.AddCommand("common", Commit, "comkmit PLAY -- Commit a play in a game of Scrabble. Play should be formatted like: H6 WORD", Permission.Anyone);
        CommandParser.AddCommand("guess", Guess, "guess WORD -- Guess an answer to the anagramming game", Permission.Anyone);
        CommandParser.AddCommand("edit", Edit, "edit -- Edit the current rule set", Permission.VIP);
        CommandParser.AddCommand("join", Join, "join -- Join the queue to play Scrabble with Emma", Permission.Anyone);
        CommandParser.AddCommand("next", Next, "next -- Start the next game of Scrabble", Permission.VIP);
        CommandParser.AddCommand("add", Add, "add PLAYER -- Add a game of Scrabble to the queue", Permission.VIP);
        CommandParser.AddCommand("remove", Remove, "remove PLAYER -- Remove a game of Scrabble from the queue", Permission.VIP);
        CommandParser.AddCommand("skip", Skip, "skip -- Skip the current game of Scrabble", Permission.VIP);
        CommandParser.AddCommand("clear", Clear, "clear -- Clear the queue to play Scrabble", Permission.VIP);
        CommandParser.AddCommand("raid", Raid, "raid -- Display raid message", Permission.Anyone);
        CommandParser.AddCommand("hug", Hug, "hug -- Send a hug", Permission.Anyone);
        CommandParser.AddCommand("flower", GiveFlower, "flower VIEWER -- Give someone a flower", Permission.Moderator);
        CommandParser.AddCommand("garden", Garden, "garden -- Show your flower garden", Permission.Anyone);
        CommandParser.AddCommand("game", Game, "game -- Describe the game Emma is playing", Permission.Anyone);
        CommandParser.AddCommand("shelf", Shelf, "shelf -- Explains about emma's stream shelf", Permission.Anyone);
        CommandParser.AddCommand("queue", Queue, "queue -- Make the game queue active or inactive", Permission.Moderator);

        CommandParser.AddAlias("sub", "subscribe");
        CommandParser.AddAlias("so", "shoutout");
        CommandParser.AddAlias("msg", "message");
        CommandParser.AddAlias("stop", "end");
        CommandParser.AddAlias("idea", "suggest");
        CommandParser.AddAlias("play", "join");

        if (!string.IsNullOrWhiteSpace(Properties.Settings.Default.TwitchUsername))
        {
            TwitchBot = new TwitchBot(commandParser,
            Properties.Settings.Default.CommandPrefix,
            Properties.Settings.Default.TwitchClientID,
            Properties.Settings.Default.TwitchBotAccessToken,
            Properties.Settings.Default.TwitchUsername,
            Properties.Settings.Default.TwitchChannelAccessToken,
            Properties.Settings.Default.TwitchChannel,
            Properties.Settings.Default.TwitchChannelID);

            TwitchBot.ChatCleared += Bot_ChatCleared;
            TwitchBot.Message += Bot_Message;
            TwitchBot.Run();
        }

        string flowerFile = Path.Combine(Properties.Settings.Default.BaseFolder, "flowers.txt");

        if (File.Exists(flowerFile))
        {
            foreach (string line in File.ReadAllLines(flowerFile))
            {
                string[] parts = line.Split('\t');

                if (parts.Length == 2)
                {
                    string flower = parts[0];
                    string user = parts[1];

                    if (!Flowers.ContainsKey(user))
                    {
                        Flowers.Add(user, new HashSet<string>());
                    }

                    Flowers[user].Add(flower);
                }
            }
        }

        StartScreen = new StartScreen(this, MainForm);
        MainForm.UI = StartScreen;
    }


    public void RunCommand(string command)
    {
        var message = new StreamMessage("(hotkey)", command, null, true);
        string? result = CommandParser.InterpretCommand(message, command);

        if (result != null)
        {
            TwitchBot?.SendMessage(result);
        }
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
            case "stop":
                MainForm.UI = StartScreen;
                Message = "one moment";
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


    public void CloseApp()
    {
        MainForm.UI = StartScreen;
    }


    private string? Start(params string[] args)
    {
        MainForm.UI = StartScreen;
        Message = "stream starting soon";
        StartScreen.StartStream();
        return null;
    }
    

    private string? End(params string[] args)
    {
        MainForm.UI = StartScreen;
        Message = "stream ending soon";
        StartScreen.StopStream();
        return null;
    }


    private string? Brb(params string[] args)
    {
        MainForm.UI = StartScreen;
        Message = "be right back";
        StartScreen.StopStream();
        return null;
    }


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


    private string? Commit(params string[] args)
    {
        if (args.Length != 3) return CommandParser.Help("commit");

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
        if (!m_QueueActive)
        {
            return "Sorry, Emma is not currently playing with viewers";
        }

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


    private string? Garden(params string[] args)
    {
        if (args.Length != 1) return CommandParser.Help("garden");

        if (!Flowers.ContainsKey(CommandParser.Username))
        {
            Flowers.Add(CommandParser.Username, new HashSet<string>());
        }

        int flowerCount = Flowers[CommandParser.Username].Count;

        if (flowerCount == 0)
        {
            return $"@{CommandParser.Username} has no flowers in their garden";
        }
        else
        {
            return $"@{CommandParser.Username} has {flowerCount} {(flowerCount == 1 ? "flower" : "flowers")} in their garden: {string.Join(", ", Flowers[CommandParser.Username])}";
        }
    }




    private string? Flower(string username, string rewardName)
    {
        username = username.ToLower();

        if (!Flowers.TryGetValue(username, out var value))
        {
            value = [];
            Flowers.Add(username, value);
        }

        string[] flowers =
        {
            "agrimony",
            "american willowherb",
            "angelica",
            "annual pearlwort",
            "autumn hawkbit",
            "barren strawberry",
            "beaked hawk's-beard",
            "bell heather",
            "betony",
            "bird's-foot trefoil",
            "biting stonecrop",
            "bittersweet",
            "black bryony",
            "bluebell",
            "bristly ox-tongue",
            "broad-leaved willowherb",
            "brooklime",
            "bugle",
            "bulbous buttercup",
            "bush vetch",
            "butterbur",
            "carline thistle",
            "cat's ear",
            "charlock",
            "cleavers",
            "common spotted orchid",
            "corky-fruited water dropwort",
            "cowslip",
            "creeping cinquefoil",
            "cuckoo flower",
            "cut-leaved crane's-bill",
            "daffodil",
            "daisy",
            "devil's-bit scabious",
            "dove's-foot crane's-bill",
            "early purple orchid",
            "enchanter's nightshade",
            "eyebright",
            "fairy flax",
            "field bindweed",
            "field forget-me-not",
            "field rose",
            "garlic mustard",
            "germander speedwell",
            "greater knapweed",
            "ground ivy",
            "guelder rose",
            "harebell",
            "heath milkwort",
            "herb robert",
            "ivy-leaved toadflax",
            "lady's bedstraw",
            "lesser celandine",
            "lesser trefoil",
            "long-headed poppy",
            "long-stalked crane's-bill",
            "marsh marigold",
            "marsh thistle",
            "meadow buttercup",
            "meadow crane's-bill",
            "meadow vetchling",
            "musk mallow",
            "nipplewort",
            "oxeye daisy",
            "perforate st john's-wort",
            "pignut",
            "primrose",
            "ragged robin",
            "ramsons",
            "red campion",
            "red clover",
            "red dead-nettle",
            "red valerian",
            "rock-rose",
            "rose campion",
            "rosebay willowherb",
            "rough hawkbit",
            "sainfoin",
            "salad burnet",
            "scarlet pimpernel",
            "scentless mayweed",
            "selfheal",
            "sheep's sorrel",
            "shining crane's-bill",
            "small scabious",
            "smooth hawksbeard",
            "snowdrop",
            "sorrel",
            "southern marsh orchid",
            "spear thistle",
            "spiny rest harrow",
            "sticky mouse-ear",
            "sweet violet",
            "tansy",
            "thyme-leaved speedwell",
            "tormentil",
            "tufted vetch",
            "water avens",
            "water mint",
            "wild strawberry",
            "wood anemone",
            "wood avens",
            "wood sorrel",
            "woodruff",
            "woolly thistle",
            "yarrow",
            "yellow archangel"
        };

        string[] rareFlowers =
        {
            "spiked star-of-bethlehem",
            "lady orchid",
            "fly orchid",
            "lizard orchid",
            "burnt orchid",
            "bee orchid",
            "spider orchid",
            "cheddar pink"
        };

        string message;
        string flower;
        var unownedRares = rareFlowers.Where(f => !value.Contains(f)).ToArray();

        if (Random.NextDouble() < 0.05 && unownedRares.Any())
        {
            flower = unownedRares[Random.Next(rareFlowers.Length)];
            message = $"@{username} has received a RARE flower, {flower} gurchyYellow ";
        }
        else
        {
            var unownedFlowers = flowers.Where(f => !value.Contains(f)).ToArray();

            if (unownedFlowers.Length == 0)
            {
                unownedFlowers = flowers;
            }

            flower = unownedFlowers[Random.Next(unownedFlowers.Length)];
            message = $"@{username} has received: {flower} gurchyYellow ";
        }

        value.Add(flower);
        int flowerCount = value.Count;
        message += $"They now have {flowerCount} {(flowerCount == 1 ? "flower" : "flowers")} in their collection!";
        File.AppendAllText(Path.Combine(Properties.Settings.Default.BaseFolder, "flowers.txt"), $"{flower}\t{username}\r\n");

        return message;
    }


    private string? GiveFlower(string[] args)
    {
        if (args.Length == 0) return CommandParser.Help("flower");

        string username = args[1];
        return Flower(username, "flower of the day");
    }


    private string? Game(params string[] args)
    {
        if (args.Length != 1) return CommandParser.Help("game");

        if (MainForm.UI is ScrabbleUI)
        {
            return "Emma is currently playing Scrabble with variant rules";
        }
        else if (MainForm.UI is AnagramUI)
        {
            return "We are currently playing Emma Words";
        }
        else if (MainForm.UI is WordLearnUI)
        {
            return "Emma is currently learning words";
        }
        else
        {
            string gameFile = Path.Combine(Properties.Settings.Default.BaseFolder, "game.txt");

            if (File.Exists(gameFile))
            {
                return File.ReadAllText(gameFile);
            }
        }

        return null;
    }


    private string? Shelf(params string[] args)
    {
        if (args.Length != 1) return CommandParser.Help("shelf");

        return "Feel free to ask about any of the objects on the shelf behind Emma! The penguin's name is Pebbles.";
    }


    public string? Queue(params string[] args)
    {
        if (args.Length == 0)
        {
            m_QueueActive = !m_QueueActive;
            return m_QueueActive ? "Queue active" : "Queue inactive";
        }
        else if (args[0].Equals("on", StringComparison.OrdinalIgnoreCase))
        {
            m_QueueActive = true;
            return "Queue active";
        }
        else if (args[0].Equals("off", StringComparison.OrdinalIgnoreCase))
        {
            m_QueueActive = false;
            return "Queue inactive";
        }

        return CommandParser.Help("queue");
    }
}
