using EmmaLib;

namespace LearnWords;

/// <summary>
/// Facilitates learning of words.
/// </summary>
public class WordLearning
{
    // How often questions come around again (smaller value = more often).
    private const int QUESTION_FREQUENCY = 100;

    private readonly string m_WordFile;
    private readonly string m_AlphagramFile;
    private readonly Random m_Random = new();
    private readonly Lexicon m_Lexicon;
    private readonly Dictionary<string, int> m_WordScores = new();
    private readonly List<string> m_Questions = new();
    private readonly List<string> m_PendingAnswers = new();

    public string? Question { get; private set; }
    public List<string> CorrectAnswers { get; } = new();
    public List<string> MissedAnswers { get; } = new();
    public List<string> IncorrectGuesses { get; } = new();
    public List<(string, bool)> AnswerLog { get; } = new();


    /// <summary>
    /// Number of words with at least two consecutive correct answers.
    /// </summary>
    public int Learned { get; private set; }

    /// <summary>
    /// Number of words that were correctly answered last time, but not previously.
    /// </summary>
    public int Progress { get; private set; }

    /// <summary>
    /// Number of words that were answered incorrectly.
    /// </summary>
    public int Missed { get; private set; }


    /// <summary>
    /// Initialises word learning from data files.
    /// </summary>
    /// <param name="lexicon">The lexicon in use.</param>
    /// <param name="wordData">The word learning file.</param>
    /// <param name="alphagramData">The alphagram learning file.</param>
    public WordLearning(Lexicon lexicon, string wordFile, string alphagramFile)
    {
        m_Lexicon = lexicon;
        m_WordFile = wordFile;
        m_AlphagramFile = alphagramFile;

        using var reader = new StreamReader(wordFile);

        while (!reader.EndOfStream && reader.ReadLine() is string line)
        {
            string trim = line.Trim().ToUpper();

            if (!trim.StartsWith("//"))
            {
                var fields = line.Split(',');

                if (fields.Length == 2)
                {
                    m_WordScores[fields[0]] = int.Parse(fields[1]);
                }
            }
        }

        using var reader2 = new StreamReader(alphagramFile);

        while (!reader2.EndOfStream && reader2.ReadLine() is string line)
        {
            string word = line.Trim().ToUpper();

            if (word.Length > 0 && !word.StartsWith("//"))
            {
                m_Questions.Add(word);
            }
        }

        NextQuestion();
    }


    /// <summary>
    /// Moves onto the next alphagram.
    /// </summary>
    public void NextQuestion()
    {
        if (m_Questions.Count == 0) return;

        Question = m_Questions[0];
        m_Questions.RemoveAt(0);

        Learned = m_WordScores.Count(w => w.Value >= 2);
        Progress = m_WordScores.Count(w => w.Value == 1);
        Missed = m_WordScores.Count(w => w.Value <= 0);

        m_PendingAnswers.Clear();
        CorrectAnswers.Clear();
        MissedAnswers.Clear();
        IncorrectGuesses.Clear();

        foreach (string answer in m_Lexicon.GetAnagrams(Question))
        {
            if (!m_WordScores.ContainsKey(answer))
            {
                m_WordScores[answer] = 0;
            }

            m_PendingAnswers.Add(answer);
        }
    }


    /// <summary>
    /// Marks an answer as correct, or adds to the list of incorrect guesses.
    /// Return value indicates whether the guess was a valid anagram of the question (whether correct or not).
    /// </summary>
    /// <param name="guess">The guessed word.</param>
    public bool SubmitGuess(string guess)
    {
        string guessAlphagram = new(guess.Order().ToArray());

        if (guessAlphagram != Question)
        {
            return false;
        }

        if (MissedAnswers.Contains(guess))
        {
            MissedAnswers.Remove(guess);
            AnswerLog.Add((guess, false));

            if (IncorrectGuesses.Count == 0)
            {
                m_WordScores[guess] = Math.Min(1, m_WordScores[guess] - 1);
            }
        }
        else if (MissedAnswers.Count == 0)
        {
            if (m_PendingAnswers.Contains(guess))
            {
                CorrectAnswers.Add(guess);
                m_PendingAnswers.Remove(guess);
                AnswerLog.Add((guess, IncorrectGuesses.Count == 0));
                m_WordScores[guess] = Math.Max(0, m_WordScores[guess] + 1);
            }
            else if (!IncorrectGuesses.Contains(guess))
            {
                IncorrectGuesses.Add(guess);

                foreach (string missedAnswer in m_PendingAnswers)
                {
                    m_WordScores[missedAnswer] = Math.Min(1, m_WordScores[missedAnswer] - 1);
                }
            }

            if (m_PendingAnswers.Count == 0)
            {
                ReplaceQuestion();
                WriteLearnFiles();
                NextQuestion();
            }
        }

        return true;
    }


    private void ReplaceQuestion()
    {
        // Put the question back into the list.
        // The more successful answers a question has, the further back it goes.

        if (Question != null)
        {
            double averageScore = m_Lexicon.GetAnagrams(Question).Average(q => (double)m_WordScores[q]);
            int newPosition = (int)(QUESTION_FREQUENCY * Math.Pow(2, averageScore) * (0.95 + m_Random.NextDouble() * 0.1));

            if (newPosition > m_Questions.Count)
            {
                newPosition = m_Questions.Count;
            }

            m_Questions.Insert(newPosition, Question);
        }
    }


    private void WriteLearnFiles()
    {
        File.WriteAllLines(m_AlphagramFile, m_Questions);

        using var writer = new StreamWriter(m_WordFile);

        foreach (var wordScore in m_WordScores)
        {
            writer.WriteLine($"{wordScore.Key},{wordScore.Value}");
        }
    }


    /// <summary>
    /// Moves the remaining answers to 'missed answers' and counts them as incorrect.
    /// </summary>
    public void GiveUp()
    {
        foreach (string answer in m_PendingAnswers)
        {
            MissedAnswers.Add(answer);
        }
    }
}
