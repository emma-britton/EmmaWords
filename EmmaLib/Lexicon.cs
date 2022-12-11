using System.Collections;

namespace EmmaLib;

/// <summary>
/// A collection of words that are playable under a particular rule set.
/// </summary>
public class Lexicon : IEnumerable<string>
{
    private readonly SortedSet<string> Words = new();
    private readonly Dictionary<string, int> Adjustments = new();
    private ILookup<string, string>? Anagrams;


    /// <summary>
    /// The lexicon name.
    /// </summary>
    public string Name { get; }


    /// <summary>
    /// Creates an empty lexicon.
    /// </summary>
    /// <param name="name">The lexicon name.</param>
    public Lexicon(string name)
    {
        Name = name;
    }


    /// <summary>
    /// Creates a lexicon from a file.
    /// </summary>
    /// <param name="name">The lexicon name.</param>
    /// <param name="stream">A text file containing one word per line. Score adjustments for specific words are indicated by a second comma-separated field.
    /// Comments are prefixed with "//".</param>
    public Lexicon(string name, Stream stream) : this(name)
    {
        using var reader = new StreamReader(stream);

        while (!reader.EndOfStream && reader.ReadLine() is string line)
        {
            string word = line.ToUpper().Trim();

            if (word != "" && !word.StartsWith("//"))
            {
                if (word.IndexOf(',') is int pos && pos > 0 &&
                    int.TryParse(word[(pos + 1)..].Trim(), out int adjustment))
                {
                    word = word[..pos];
                    Adjustments[word] = adjustment;
                }

                Words.Add(word);
            }
        }
    }


    /// <summary>
    /// Creates a new lexicon by combining existing lexicons.
    /// </summary>
    /// <param name="lexicons">The lexicons to combine.</param>
    public static Lexicon Combine(IEnumerable<Lexicon> lexicons)
    {
        var result = new Lexicon("All");

        foreach (var lexicon in lexicons)
        {
            result.Words.UnionWith(lexicon.Words);
            
            foreach (var adjustment in lexicon.Adjustments)
            {
                result.Adjustments[adjustment.Key] = adjustment.Value + result.Adjustments.GetValueOrDefault(adjustment.Key);
            }
        }

        return result;
    }


    /// <summary>
    /// The number of words in the lexicon.
    /// </summary>
    public int Count()
    {
        return Words.Count; 
    }


    /// <summary>
    /// Determines whether the lexicon contains a word.
    /// </summary>
    /// <param name="word">The word.</param>
    public bool Contains(string word)
    {
        return Words.Contains(word.ToUpper());
    }


    /// <summary>
    /// Returns the adjustment that applies to a word.
    /// </summary>
    /// <param name="word">The word.</param>
    public int GetAdjustment(string word)
    {
        return Adjustments.GetValueOrDefault(word.ToUpper());
    }


    /// <summary>
    /// Returns words that are anagrams of the specified alphagram.
    /// </summary>
    /// <param name="alphagram">The alphagram.</param>
    public IEnumerable<string> GetAnagrams(string alphagram)
    {
        Anagrams ??= Words.ToLookup(w => new string(w.Order().ToArray()));

        string sorted = new(alphagram.Order().ToArray());
        return Anagrams[sorted];
    }


    /// <summary>
    /// Returns a string representing the object.
    /// </summary>
    public override string ToString() => Name;


    /// <summary>
    /// Enumerates words in the list.
    /// </summary>
    public IEnumerator<string> GetEnumerator() => Words.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => Words.GetEnumerator();
}
