using Emma.Anagramming;
using Emma.IsBot;
using Emma.Lib;
using Emma.Scrabble;
using Emma.WordLearner;
using System.IO;
using System.Text.RegularExpressions;
using TwitchLib.PubSub.Events;

namespace Emma.Stream;

public class EmmaStream
{
    private readonly MainForm MainForm;
    private readonly WordService WordService;
    private readonly CommandParser CommandParser;
    private readonly StartScreen StartScreen;
    private readonly List<string> Quotes = [];
    private readonly Dictionary<string, HashSet<string>> Flowers = [];
    private readonly Dictionary<string, int> Firsts = [];
    private readonly Dictionary<string, string> Gifts = [];

    private readonly Random Random = new();

    private ScrabbleUI? ScrabbleUI;
    private AnagramUI? AnagramUI;
    private WordLearnUI? LearnUI;

    private readonly string Player1Name = "Player 1";
    private readonly string Player2Name = "Player 2";
    
    public TwitchBot? TwitchBot { get; set; }
    public AlertUI? AlertUI { get; set; }

    public string Message { get; set; } = "stream starting soon";
    private bool m_QueueActive = false;
    public Queue<string> PlayerQueue { get; set; } = new();

    private bool m_Paused = false;
    private DateTime m_PauseExpires = DateTime.MinValue;


    public EmmaStream(MainForm mainForm, WordService wordService, CommandParser commandParser)
    {
        MainForm = mainForm;
        WordService = wordService;
        CommandParser = commandParser;

        commandParser.AddReward("flower of the day", Flower);
        commandParser.AddReward("first", First);

        CommandParser.AddCommand("discord", Discord, "discord -- Show the discord link", Permission.Anyone);
        CommandParser.AddCommand("message", ChangeMessage, "message -- Change the message on the title screen", Permission.Moderator);
        CommandParser.AddCommand("subscribe", Subscribe, "subscribe -- Show a message about subscriptions", Permission.Anyone);
        CommandParser.AddCommand("shoutout", Shoutout, "shoutout CHANNEL -- Shout out another streamer", Permission.VIP);
        CommandParser.AddCommand("stream", Stream, "stream MODE -- Set the stream mode", Permission.Moderator);
        CommandParser.AddCommand("start", Start, "start -- Show the start screen", Permission.Moderator);
        CommandParser.AddCommand("end", End, "end -- Show the end screen", Permission.Moderator);
        CommandParser.AddCommand("brb", Brb, "brb -- Show the 'be right back' screen", Permission.Moderator);
        CommandParser.AddCommand("english", English, "english -- Request that people speak English in chat", Permission.VIP);
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
        CommandParser.AddCommand("cute", Cute, "cute -- Say that Emma is cute", Permission.Anyone);
        CommandParser.AddCommand("pause", Pause, "pause -- Emma has to stop and chat to you for 3 minutes", Permission.Anyone);
        CommandParser.AddCommand("resume", Resume, "resume -- Emma can get back to gameplay", Permission.Anyone);
        CommandParser.AddCommand("testalert", TestAlert, "testalert -- Test the alert system", Permission.Broadcaster);
        CommandParser.AddCommand("hazelazazelzel", Hazelazazelzel, "hazelazazelzel -- Turn the stream into a Hazel stream", Permission.Anyone);
        CommandParser.AddCommand("say", Say, "say MESSAGE -- Say a message in chat", Permission.Moderator);
        CommandParser.AddCommand("quote", Quote, "quote NUMBER -- Show a quote from the quote list", Permission.Anyone);
        CommandParser.AddCommand("addquote", AddQuote, "addquote QUOTE -- Add a quote to the quote list", Permission.VIP);
        CommandParser.AddCommand("removequote", RemoveQuote, "removequote NUMBER -- Remove a quote from the quote list", Permission.VIP);
        CommandParser.AddCommand("editquote", EditQuote, "editquote NUMBER QUOTE -- Edit a quote in the quote list", Permission.VIP);

        CommandParser.AddAlias("sub", "subscribe");
        CommandParser.AddAlias("so", "shoutout");
        CommandParser.AddAlias("msg", "message");
        CommandParser.AddAlias("stop", "end");
        CommandParser.AddAlias("idea", "suggest");
        CommandParser.AddAlias("play", "join");
        CommandParser.AddAlias("unpause", "resume");

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
            TwitchBot.Alert += TwitchBot_Alert;
            TwitchBot.Run();
        }

        string quoteFile = Path.Combine(Properties.Settings.Default.BaseFolder, "quotes.txt");

        if (File.Exists(quoteFile))
        {
            Quotes.AddRange(File.ReadAllLines(quoteFile).Where(line => !string.IsNullOrWhiteSpace(line)).Select(q => q.Trim()));
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

                    if (!Flowers.TryGetValue(user, out HashSet<string>? value))
                    {
                        value = [];
                        Flowers.Add(user, value);
                    }

                    value.Add(flower);
                }
            }
        }

        string giftFile = Path.Combine(Properties.Settings.Default.BaseFolder, "gifts.txt");

        if (File.Exists(giftFile))
        {
            foreach (string line in File.ReadAllLines(giftFile))
            {
                string[] parts = line.Split('\t');

                if (parts.Length == 2)
                {
                    Gifts.Add(parts[0].ToLower(), parts[1]);
                }
            }
        }

        string firstFile = Path.Combine(Properties.Settings.Default.BaseFolder, "firsts.txt");

        if (File.Exists(firstFile))
        {
            foreach (string line in File.ReadAllLines(firstFile))
            {
                string[] parts = line.Split('\t');

                if (parts.Length == 2)
                {
                    Firsts.Add(parts[0].ToLower(), int.Parse(parts[1]));
                }
            }
        }

        StartScreen = new StartScreen(this, MainForm);
        MainForm.UI = StartScreen;
    }



    private void TwitchBot_Alert(object? sender, EventArgs e)
    {
        if (AlertUI != null)
        {
            if (e is OnFollowArgs)
            {
                AlertUI.AddAlert(new FollowAlert());
            }
        }
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
        MainForm.UI?.HandleMessage(e);
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
        if (args.Length < 2) return CommandParser.Help("message");

        Message = string.Join(" ", args.Skip(1)).Trim();
        StartScreen.StopStream();

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
                ScrabbleUI?.Stop();

                var scrabbleGame = new ScrabbleGame(WordService.ActiveRuleSet, WordService.ActiveLexicon, Player1Name, Player2Name);
                scrabbleGame.Start();
                ScrabbleUI = new ScrabbleUI(scrabbleGame, MainForm);
                MainForm.UI = ScrabbleUI;
                break;

            case "words":
                AnagramUI?.Stop();

                var anagramGame = new AnagramGame(WordService.ActiveLexicon);
                anagramGame.Start();
                AnagramUI = new AnagramUI(anagramGame, MainForm);
                MainForm.UI = AnagramUI;
                break;

            case "learn":
                LearnUI?.Stop();

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

        if (username.Length > 20 || username.Any(c => !(c >= 'a' && c <= 'z' || c >= 'A' && c <= 'Z' || c >= '0' && c <= '9' || c == ' ' || c == '_')))
        {
            return "no they're not real";
        }

        if (username == "wanderer15")
        {
            return "wanderer15 makes great Scrabble content on YouTube: youtube.com/@wanderer15";
        }

        string message = username.ToLower() switch
        {
            "ophelia6277" =>
                "Ophelia is a wonderful person and is best snail. Join her for slow gameplay and chatting, recent games include Snail Simulator, Dorfromantik and Syberia. " +
                "12:00-16:00 (Central Europe time)",

            "sarahwithstars" =>
                "Sarah is calm, kind and caring, she has recently been playing Cult of the Lamb and Elden Ring. Schedule varies, typically a few streams a week " +
                    "starting around 8-10am (UK time)",

            "dragongirl_89" =>
                "Dragon plays many different games, but especially open world crafting games. Streams whenever she feels like it (Central Europe time)",

            "duustinduude" =>
                "Duustin duude streams word games and sometimes other things. Usually mid evening (Central US time)",

            "rubyinpixels" =>
                "Ruby enjoys point and click adventure games. Usually late evening (Pacific time)",

            "runibl" =>
                "Runi plays story based games, especially space ones, and sometimes uses dice for random dialogue choices! Tuesday and Friday 16:00 (UK time)",

            "izzy_the_penguin" =>
                "Izzy is a penguin! Currently streaming game development, Friday 18:30 / Saturday 17:00 (UK time)",

            "lana_the_panda" =>
                "Lana is a panda themed streamer, lots of emotes and sound effects to try out, someimtes discord polls to choose game. " +
                "Currently playing Star Wars: KOTOR on Tuesday and Thursday and Red Dead Redemption on Sunday, all at 18:00 (UK time)",

            "alice_sits" =>
                "Alice is a super cozy streamer who makes smol things and plays casual games. Wednesday and Sunday 10:00-12:00 (Central Europe time)",

            "thenespa" =>
                "NESpa plays retro Nintendo games on original hardware, their stream also features chatting and self care. Monday, Tuesday and Friday 5pm - 8pm (Eastern time)",

            _ => "#"
        };
        
        if (message.EndsWith('#'))
        {
            return null;
        }

        message += " twitch.tv/" + username;
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
        if (what.Contains(' ')) what = what[..what.IndexOf(' ')];

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
        if (skipped.Contains(' ')) skipped = skipped[..skipped.IndexOf(' ')];

        string what = PlayerQueue.Peek();
        if (what.Contains(' ')) what = what[..what.IndexOf(' ')];

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
            if (gameDesc.StartsWith(username, StringComparison.OrdinalIgnoreCase))
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

        return "gurchyPurple gurchyPurple GURCHY RAID gurchyPurple gurchyPurple";
    }


    private string? Hug(params string[] args)
    {
        if (args.Length <= 1)
        {
            return $"@{CommandParser.Username} gurchyHug";
        }
        else
        {
            return $"@{args[1]} gurchyHug";
        }
    }


    private string? Garden(params string[] args)
    {
        if (args.Length != 1) return CommandParser.Help("garden");

        if (!Flowers.TryGetValue(CommandParser.Username, out HashSet<string>? value))
        {
            value = [];
            Flowers.Add(CommandParser.Username, value);
        }

        int flowerCount = value.Count;

        if (flowerCount == 0)
        {
            return $"@{CommandParser.Username} has no flowers in their garden";
        }
        else
        {
            string message = $"@{CommandParser.Username} has {flowerCount} {(flowerCount == 1 ? "flower" : "flowers")} in their garden: {string.Join(", ", value)}";

            if (message.Length > 500)
            {
                message = $"@{CommandParser.Username} has {flowerCount} {(flowerCount == 1 ? "flower" : "flowers")} in their garden";
            }

            return message;
        }
    }


    private string? First(string username, string rewardName)
    {
        username = username.ToLower();

        if (!Firsts.TryGetValue(username, out int count))
        {
            count = 0;
            Firsts.Add(username, count);
        }

        Firsts[username]++;
        
        string message = $"@{username} is first today gurchyPurple ";

        if (Firsts[username] == 1)
        {
            message += "this is their first time being first";
        }
        else
        {
            string ordinal = count switch
            {
                1 => "first",
                2 => "second",
                3 => "third",
                4 => "fourth",
                5 => "fifth",
                6 => "sixth",
                7 => "seventh",
                8 => "eighth",
                9 => "ninth",
                10 => "tenth",
                11 => "eleventh",
                12 => "twelfth",
                13 => "thirteenth",
                14 => "fourteenth",
                15 => "fifteenth",
                16 => "sixteenth",
                17 => "seventeenth",
                18 => "eighteenth",
                19 => "nineteenth",
                20 => "twentieth",

                int i when i % 10 == 1 => $"{count}st",
                int i when i % 10 == 2 => $"{count}nd",
                int i when i % 10 == 3 => $"{count}rd",
                _ => $"{count}th"
            };

            message += $"this is their {ordinal} time being first";
        }
        
        File.WriteAllText(Path.Combine(Properties.Settings.Default.BaseFolder, "firsts.txt"), string.Join("\r\n", Firsts.Select(kvp => $"{kvp.Key}\t{kvp.Value}")));

        return message;
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
        [
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
        ];


        string message;
        string flower;

        var unownedFlowers = flowers.Where(f => !value.Contains(f)).ToArray();

        if (unownedFlowers.Length == 0)
        {
            unownedFlowers = flowers;
        }

        flower = unownedFlowers[Random.Next(unownedFlowers.Length)];
        message = $"@{username} has received: {flower} gurchyPurple ";
        
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


    private string? Cute(params string[] args)
    {
        return "I am contractually obligated to say that Emma is cute";
    }


    private string? Pause(params string[] args)
    {
        if (m_Paused && m_PauseExpires < DateTime.Now)
        {
            return "We're already paused!";
        }
        else
        {
            m_Paused = true;
            m_PauseExpires = DateTime.Now.AddMinutes(3);
            return "Time for a 3 minute chat break!";
        }
    }


    private string? Resume(params string[] args)
    {
        if (!m_Paused || m_PauseExpires < DateTime.Now)
        {
            return "We're not paused right now";
        }
        else
        {
            m_Paused = false;
            return "Back to gameplay!";
        }
    }


    private string? TestAlert(params string[] args)
    {
        AlertUI?.AddAlert(new FollowAlert());
        return null;
    }


    private string English(params string[] args)
    {
        string[] localizedMessages =
        [
            "Please speak English in chat.",
            "S'il vous plaît parlez anglais.",
            "Bitte sprechen Sie Englisch.",
            "Por favor, hable inglés.",
            "Por favor, fale inglês no chat.",
            "Si prega di parlare inglese.",
            "Va rugam vorbiti in engleza.",
            "Praat alsjeblieft Engels.",
            "Vänligen tala engelska.",
            "Vennligst skriv engelsk I chatten.",
            "Σας Παρακαλούμε να μιλάτε μόνο Αγγλικά.",
            "Пожалуйста, говорите по-английски.",
            "يرجى الكتابه باللغة الانجليزية.",
            "英語で話してください。",
            "영어로 말해주세요.",
            "请在聊天中使用英语。",
        ];

        return string.Join(" ", localizedMessages);
    }


    private string? Hazelazazelzel(params string[] args)
    {
        SendHotkey.Send("^+{F12}");
        return null;
    }


    private string? Say(params string[] args)
    {
        if (args.Length < 2) return CommandParser.Help("say");

        string message = string.Join(" ", args.Skip(1));
        return message;
    }


    private string? Quote(params string[] args)
    {
        if (args.Length == 1 && Quotes.Count > 0)
        {
            return Quotes[new Random().Next(Quotes.Count)];
        }

        if (args[1] == "add")
        {
            return AddQuote(["addquote", .. args.Skip(2)]);
        }

        if (args[1] == "remove" && Quotes.Count > 0)
        {
            return RemoveQuote(["addquote", .. args.Skip(2)]);
        }

        if (args[1] == "edit" && Quotes.Count > 0)
        {
            return EditQuote(["editquote", .. args.Skip(2)]);
        }

        if ((!int.TryParse(args[1], out int index) || index < 1 || index > Quotes.Count) && Quotes.Count > 0)
        {
            return $"No such quote, try a number between 1 and {Quotes.Count}";
        }

        if (Quotes.Count == 0)
        {
            return null;
        }

        return Quotes[index - 1];
    }


    private string? AddQuote(params string[] args)
    {
        if (args.Length < 2) return CommandParser.Help("addquote");

        string quote = string.Join(" ", args.Skip(1));
        Quotes.Add(quote);
        File.AppendAllText(Path.Combine(Properties.Settings.Default.BaseFolder, "quotes.txt"), quote + "\r\n");

        return $"Quote #{Quotes.Count} added";
    }


    private string? RemoveQuote(params string[] args)
    {
        if (args.Length != 2) return CommandParser.Help("removequote");

        if (!int.TryParse(args[1], out int index) || index < 1 || index > Quotes.Count)
        {
            return $"No such quote, try a number between 1 and {Quotes.Count}";
        }

        Quotes.RemoveAt(index - 1);
        File.WriteAllText(Path.Combine(Properties.Settings.Default.BaseFolder, "quotes.txt"), string.Join("\r\n", Quotes));

        return $"Quote #{index} removed";
    }


    private string? EditQuote(params string[] args)
    {
        if (args.Length < 3) return CommandParser.Help("editquote");

        if (!int.TryParse(args[1], out int index) || index < 1 || index > Quotes.Count)
        {
            return $"No such quote, try a number between 1 and {Quotes.Count}";
        }

        string quote = string.Join(" ", args.Skip(2));
        Quotes[index - 1] = quote;
        File.WriteAllText(Path.Combine(Properties.Settings.Default.BaseFolder, "quotes.txt"), string.Join("\r\n", Quotes));

        return $"Quote #{index} updated";
    }
}
