using Emma.Lib;

namespace Emma.Anagramming;

/// <summary>
/// An interactive anagramming game.
/// </summary>
public class AnagramGame
{
    private readonly Random Random = new();
    private bool Started;

    private Lexicon m_Lexicon;
    private int m_MinWordLength = 3;
    private int m_MaxWordLength = 8;
    private int m_GameColumns = 4;
    private int m_GameRows = 10;
    private int m_GameQuestions = 40;
    

    /// <summary>
    /// The lexicon used by the game.
    /// </summary>
    public Lexicon Lexicon
    {
        get => m_Lexicon;

        set
        {
            m_Lexicon = value;
            if (Started) Start();
        }
    }
    
    /// <summary>
    /// The minimum length of word to show.
    /// </summary>
    public int MinWordLength
    {
        get => m_MinWordLength;

        set
        {
            m_MinWordLength = value;
            if (Started) Start();
        }
    }
    
    /// <summary>
    /// The maximum length of word to show.
    /// </summary>
    public int MaxWordLength
    {
        get => m_MaxWordLength;

        set
        {
            m_MaxWordLength = value;
            if (Started) Start();
        }
    }

    /// <summary>
    /// The number of columns of words shown.
    /// </summary>
    public int GameColumns
    {
        get => m_GameColumns;

        set
        {
            m_GameColumns = value;
            m_GameQuestions = m_GameRows * m_GameColumns;
            if (Started) Start();
        }
    }

    /// <summary>
    /// The number of rows of words shown.
    /// </summary>
    public int GameRows
    {
        get => m_GameRows;

        set
        {
            m_GameRows = value;
            m_GameQuestions = m_GameRows * m_GameColumns;
            if (Started) Start();
        }
    }


    /// <summary>
    /// Creates a new anagramming game.
    /// </summary>
    /// <param name="lexicon">The lexicon to use.</param>
    public AnagramGame(Lexicon lexicon)
    {
        m_Lexicon = lexicon;
    }


    /// <summary>
    /// The questions remaining to be asked.
    /// </summary>
    public Queue<AnagramQuestion> Questions { get; } = new();

    /// <summary>
    /// The currently active questions.
    /// </summary>
    public List<AnagramQuestion?> ActiveQuestions { get; } = new();

    /// <summary>
    /// The questions that have been completed.
    /// </summary>
    public List<AnagramQuestion> CompletedQuestions { get; } = new();

    /// <summary>
    /// Names of all participating players and their scores.
    /// </summary>
    public Dictionary<string, int> PlayerScores { get; } = new();

    /// <summary>
    /// The guesses that have been submitted.
    /// </summary>
    public List<AnagramGuess> Guesses { get; } = new();


    /// <summary>
    /// Starts the anagramming game. 
    /// </summary>
    public void Start()
    {
        Started = true;

        Questions.Clear();
        ActiveQuestions.Clear();
        GenerateQuestions();

        for (int i = 0; i < m_GameQuestions; i++)
        {
            if (Questions.Count == 0)
            {
                ActiveQuestions.Add(null);
            }
            else
            {
                ActiveQuestions.Add(Questions.Dequeue());
            }
        }
    }


    private void GenerateQuestions()
    {
        var questionsUsed = new HashSet<string>();
        var randomList = new List<string>(Lexicon);

        Shuffle(randomList);

        foreach (string word in randomList)
        {
            if (word.Length >= MinWordLength && word.Length <= MaxWordLength)
            {
                string question = new(word.Order().ToArray());

                if (!questionsUsed.Contains(question))
                {
                    var answers = new List<AnagramAnswer>();

                    foreach (string answer in Lexicon.GetAnagrams(word))
                    {
                        answers.Add(new AnagramAnswer(answer));
                    }

                    if (answers.Count == 1 && answers[0].Word == question)
                    {
                        // Exclude "anagrams" with only one solution that is already in alphabetical order.
                        continue;
                    }
                    
                    Questions.Enqueue(new AnagramQuestion(question, answers));
                }
            }
        }
    }


    /// <summary>
    /// Submit a guess to the anagramming game.
    /// </summary>
    /// <param name="username">The name of the player submitting the guess.</param>
    /// <param name="word">The word being guessed.</param>
    public AnagramGuess SubmitGuess(string username, string word)
    {
        if (!PlayerScores.ContainsKey(username))
        {
            PlayerScores[username] = 0;
        }

        var guess = new AnagramGuess(username, word);
        string alphagram = new(word.Order().ToArray());
        bool foundQuestion = false;

        for (int i = 0; i < ActiveQuestions.Count; i++)
        {
            var question = ActiveQuestions[i];

            if (question != null && alphagram == question.Anagram)
            {
                foundQuestion = true;
                bool match = false;

                foreach (var answer in question.Answers)
                {
                    if (answer.Word == word)
                    {
                        match = true;
                        Guesses.Add(guess);
                        guess.Question = question;
                        guess.Answer = answer;

                        if (answer.Guessed)
                        {
                            guess.Duplicate = true;
                        }
                        else
                        {
                            answer.Guessed = true;
                            guess.Correct = true;
                            PlayerScores[guess.Username] += answer.Word.Length;
                            question.RemainingAnswers--;

                            if (question.RemainingAnswers == 0)
                            {
                                CompletedQuestions.Add(question);

                                if (Questions.Count == 0)
                                {
                                    ActiveQuestions[i] = null;
                                }
                                else
                                {
                                    ActiveQuestions[i] = Questions.Dequeue();
                                }
                            }
                        }

                        break;
                    }
                }

                if (!match)
                {
                    Guesses.Add(guess);
                    guess.Incorrect = true;
                    PlayerScores[guess.Username] -= 1;
                }
            }
        }

        if (!foundQuestion)
        {
            foreach (var completed in CompletedQuestions)
            {
                foreach (var answer in completed.Answers)
                {
                    if (answer.Word == guess.Word)
                    {
                        Guesses.Add(guess);
                        guess.Duplicate = true;
                        break;
                    }
                }

                if (guess.Duplicate) break;
            }
        }

        return guess;
    }


    public void Shuffle<T>(IList<T> list)
    {
        int n = list.Count;

        while (n > 1)
        {
            n--;
            int k = Random.Next(n + 1);
            (list[n], list[k]) = (list[k], list[n]);
        }
    }
}
