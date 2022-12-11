
namespace EmmaWords
{
    partial class WordService
    {
        private static readonly bool IsHome = Properties.Settings.Default.HomeChannel == Properties.Settings.Default.TwitchChannel;

        public WordGame WordGame { get; set; }
        public ScrabbleGame ScrabbleGame { get; set; }

        public readonly List<RuleSet> RuleSets = new();
        public WordListSet WordLists { get; }
        public WordList CurrentList { get; set; }
        public string CurrentUser { get; set; }
        public string StreamMessage { get; set; } = "stream\r\nstarting\r\nsoon";
        public bool Starting = false;
        public IGameUI? GameUI { get; set; } = null;
        public RuleSet RuleSet { get; set; }

        private readonly DefinitionSet Definitions;
        private WordSet WordSet;
        private readonly int SearchLimit = 50;
        private bool CanCheet = true;
        private readonly string CommandPrefix;


        public WordService(string wordListFolder, string dictionaryFile, string bonusFile, string botUsername, string commandPrefix)
        {
            Console.WriteLine("Initialising word service");

            WordLists = WordListSet.ReadFromFolder(wordListFolder);
            Definitions = new DefinitionSet();
            DefinitionSet.ReadFromFile(Definitions, dictionaryFile);
            DefinitionSet.ReadFromFile(Definitions, bonusFile);

            CommandPrefix = commandPrefix;
            CurrentUser = botUsername;
            CurrentList = WordLists.AllWords;

            RuleSets.Add(RuleSet.Scrabble);
            RuleSets.Add(RuleSet.SuperScrabble);

            RuleSet = RuleSet.Scrabble;
            WordSet = new WordSet(WordLists.AllWords, RuleSet);

            WordGame = new WordGame(this);
            ScrabbleGame = new ScrabbleGame(this);

            //GameUI = new WordLearnUI(this);
        }


        public string GetSymbol(string word)
        {
            if (!CurrentList.Contains(word))
            {
                return "*";
            }

            if (CurrentList == WordLists.AllWords)
            {
                var nwl = WordLists.GetByName("nwl");
                var csw = WordLists.GetByName("csw");

                if (nwl != null && csw != null)
                {
                    bool inNwl = nwl.Contains(word);
                    bool inCsw = csw.Contains(word);

                    if (inCsw)
                    {
                        if (!inNwl)
                        {
                            return "#";
                        }
                    }
                    else if (inNwl)
                    {
                        return "$";
                    }
                }
            }

            return "";
        }


        public IEnumerable<string> Anagram(string word)
        {
            string alphagram = new(word.OrderBy(x => x).ToArray());
            return WordSet.Anagram(alphagram);
        }
    }
}
