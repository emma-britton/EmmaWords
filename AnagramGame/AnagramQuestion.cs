
namespace Emma.Anagramming;

/// <summary>
/// A question in the anagramming game.
/// </summary>
public class AnagramQuestion
{
    /// <summary>
    /// The anagram to be solved.
    /// </summary>
    public string Anagram { get; }

    /// <summary>
    /// The solutions to the anagram.
    /// </summary>
    public List<AnagramAnswer> Answers { get; }

    /// <summary>
    /// The number of solutions remaining.
    /// </summary>
    public int RemainingAnswers { get; internal set; }


    internal AnagramQuestion(string anagram, List<AnagramAnswer> answers)
    {
        Anagram = anagram;
        Answers = answers;
        RemainingAnswers = answers.Count;
    }


    /// <summary>
    /// Returns a string representing the object.
    /// </summary>
    public override string ToString() => Anagram;
}
