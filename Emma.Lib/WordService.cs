
namespace Emma.Lib;

/// <summary>
/// Provides information about words.
/// </summary>
public class WordService
{
    private readonly List<Lexicon> m_Lexicons = new();

    /// <summary>
    /// The currently active lexicon.
    /// </summary>
    public Lexicon ActiveLexicon { get; set; }

    /// <summary>
    /// The currently active rule set.
    /// </summary>
    public RuleSet ActiveRuleSet { get; set; }

    /// <summary>
    /// A lexicon that consists of all words from other available lexicons.
    /// </summary>
    public Lexicon AllLexicons { get; set; }

    /// <summary>
    /// The folder path that contains lexicons, definitions and so on.
    /// </summary>
    public string BaseFolder { get; }

    /// <summary>
    /// Dictionary definitions for words.
    /// </summary>
    public DefinitionSet Definitions { get; }


    /// <summary>
    /// Creates a new WordService instance.
    /// </summary>
    /// <param name="baseFolder">The folder path that contains lexicons, definitions and so on.</param>
    public WordService(string baseFolder)
    {
        if (baseFolder == null)
        {
            baseFolder = ".";
        }

        BaseFolder = baseFolder;

        RuleSets = new List<RuleSet>
        {
            RuleSet.Standard,
            RuleSet.Super
        };

        ActiveRuleSet = RuleSet.Standard;

        var allLexicons = Lexicon.Combine(Lexicons);
        m_Lexicons.Add(allLexicons);
        AllLexicons = allLexicons;
        ActiveLexicon = allLexicons;

        string definitionFile = Path.Combine(baseFolder, "dict", "dictionary.tsv");
        string bonusFile = Path.Combine(baseFolder, "dict", "bonus.tsv");

        Definitions = new DefinitionSet();

        if (File.Exists(definitionFile))
        {
            DefinitionSet.ReadFromFile(Definitions, definitionFile);
        }

        if (File.Exists(bonusFile))
        {
            DefinitionSet.ReadFromFile(Definitions, bonusFile);
        }
    }


    /// <summary>
    /// Returns all available rulesets.
    /// </summary>
    public IList<RuleSet> RuleSets { get; set; }
    

    /// <summary>
    /// Returns the rule set with the specified name, or null if none exists.
    /// </summary>
    /// <param name="name">The rule set name.</param>
    public RuleSet? GetRuleSet(string name)
    {
        foreach (var ruleset in RuleSets)
        {
            if (ruleset.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
            {
                return ruleset;
            }
        }

        return null;
    }


    /// <summary>
    /// Returns all available lexicons.
    /// </summary>
    public IList<Lexicon> Lexicons
    {
        get
        {
            if (m_Lexicons.Count == 0)
            {
                var lexiconFolder = Path.Combine(BaseFolder, "lexicon");

                if (Directory.Exists(lexiconFolder))
                {
                    foreach (string lexiconFile in Directory.GetFiles(lexiconFolder))
                    {
                        Console.WriteLine("Loading lexicon file " + lexiconFile);
                        string name = Path.GetFileNameWithoutExtension(lexiconFile);
                        using var stream = File.OpenRead(lexiconFile);

                        var lexicon = new Lexicon(name, stream);
                        m_Lexicons.Add(lexicon);
                    }
                }
            }

            return m_Lexicons;
        }
    }


    /// <summary>
    /// Returns the lexicon with the specified name, or null if none exists.
    /// </summary>
    /// <param name="name">The lexicon name.</param>
    public Lexicon? GetLexicon(string name)
    {
        foreach (var lexicon in m_Lexicons)
        {
            if (lexicon.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
            {
                return lexicon;
            }
        }

        return null;
    }


    /// <summary>
    /// Returns the lexicon indicating symbol for a word (* if not in lexicon, # if in CSW only, $ if in NWL only).
    /// </summary>
    /// <param name="word">The word.</param>
    public string GetSymbol(string word)
    {
        if (ActiveLexicon == AllLexicons)
        {
            var nwl = GetLexicon("nwl");
            var csw = GetLexicon("csw");

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

        if (!ActiveLexicon.Contains(word))
        {
            return "*";
        }

        return "";
    }
}
