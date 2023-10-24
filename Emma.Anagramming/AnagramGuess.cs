
namespace Emma.Anagramming;

/// <summary>
/// A guess in the anagram game.
/// </summary>
public class AnagramGuess
{
    /// <summary>
    /// The username of the guesser.
    /// </summary>
    public string Username { get; }

    /// <summary>
    /// The word that was guessed.
    /// </summary>
    public string Word { get; }

    /// <summary>
    /// The question that was answered, if the guess was successful.
    /// </summary>
    public AnagramQuestion? Question { get; internal set; }

    /// <summary>
    /// The answer that was found, if the guess was successful.
    /// </summary>
    public AnagramAnswer? Answer { get; internal set; }

    /// <summary>
    /// Whether the guess was correct.
    /// </summary>
    public bool Correct { get; internal set; }

    /// <summary>
    /// Whether the guess duplicated a previous guess.
    /// </summary>
    public bool Duplicate { get; internal set; }

    /// <summary>
    /// Whether the guess was incorrect.
    /// </summary>
    public bool Incorrect { get; internal set; }


    internal AnagramGuess(string username, string word)
    {
        Username = username;
        Word = word;
    }


    /// <summary>
    /// Returns a string representing the object.
    /// </summary>
    public override string ToString() => Word;
}
