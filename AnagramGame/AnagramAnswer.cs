
namespace Emma.Anagramming;

/// <summary>
/// Answer to a question in the anagram game.
/// </summary>
public class AnagramAnswer
{
    /// <summary>
    /// The answer word.
    /// </summary>
    public string Word { get; }

    /// <summary>
    /// Whether this answer has been guessed yet.
    /// </summary>
    public bool Guessed { get; set; }


    internal AnagramAnswer(string word)
    {
        Word = word;
    }


    /// <summary>
    /// Returns a string representing the object.
    /// </summary>
    public override string ToString() => Word;
}
