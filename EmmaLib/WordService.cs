namespace EmmaLib;

/// <summary>
/// Provides information about words.
/// </summary>
public class WordService
{
    private readonly List<Lexicon> m_Lexicons = new();


    /// <summary>
    /// The folder path that contains lexicons, definitions and so on.
    /// </summary>
    public string BaseFolder { get; }


    /// <summary>
    /// Creates a new WordService instance.
    /// </summary>
    /// <param name="baseFolder">The folder path that contains lexicons, definitions and so on.</param>
    public WordService(string baseFolder)
    {
        BaseFolder = baseFolder;
    }


    public IList<Lexicon> Lexicons
    {
        get
        {
            if (m_Lexicons.Count == 0)
            {
                var lexiconFolder = Path.Combine(BaseFolder, "lexicon");

                foreach (string lexiconFile in Directory.GetFiles(lexiconFolder))
                {
                    string name = Path.GetFileNameWithoutExtension(lexiconFile);
                    using var stream = File.OpenRead(lexiconFile);

                    var lexicon = new Lexicon(name, stream);
                    m_Lexicons.Add(lexicon);
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
}
