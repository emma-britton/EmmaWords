
namespace EmmaWords;

class WordGame
{
    private static readonly Random Random = new();

    private int MaxQuestions;
    private int GameColumns;
    private int GameRows;
    private int MinLength;
    private int MaxLength;
    private List<string> Words;
    private ILookup<string, string> WordLookup;

    public Queue<Question> AllQuestions { get; } = new();
    public List<Question?> ActiveQuestions { get; } = new();
    public List<Question> CompletedQuestions { get; } = new();
    public Dictionary<string, int> PlayerScores { get; } = new();
    public List<Guess> Guesses { get; } = new();
    public bool IsRunning { get; set; }
    public WordService WordService { get; }


    public WordGame(WordService wordService)
    {
        WordService = wordService;
        Words = WordService.CurrentList.Select(word => word.ToUpper().Trim()).ToList();
        WordLookup = Words.ToLookup(w => new string(w.ToCharArray().Order().ToArray()));

        MaxQuestions = Properties.Settings.Default.GameColumns * Properties.Settings.Default.GameRows;
        MinLength = Properties.Settings.Default.MinWordLength;
        MaxLength = Properties.Settings.Default.MaxWordLength;
    }


    public void Restart()
    {
        if (!IsRunning)
        {
            IsRunning = true;
        }
        
        Words = WordService.CurrentList.Select(word => word.ToUpper().Trim()).ToList();
        WordLookup = Words.ToLookup(w => new string(w.ToCharArray().Order().ToArray()));

        AllQuestions.Clear();
        ActiveQuestions.Clear();
        GenerateQuestions();

        for (int i = 0; i < MaxQuestions; i++)
        {
            if (AllQuestions.Count == 0)
            {
                ActiveQuestions.Add(null);
            }
            else
            {
                ActiveQuestions.Add(AllQuestions.Dequeue());
            }
        }
    }


    public void Stop()
    {
        if (IsRunning)
        {
            IsRunning = false;
        }
    }


    public void SetMinLength(int minLength)
    {
        MinLength = minLength;
        Restart();
    }


    public void SetMaxLength(int maxLength)
    {
        MaxLength = maxLength;
        Restart();
    }


    public void SetColumns(int columns)
    {
        GameColumns = columns;
        MaxQuestions = GameColumns * GameRows;
        Restart();
    }


    public void SetRows(int rows)
    {
        GameRows = rows;
        MaxQuestions = GameColumns * GameRows;
        Restart();
    }

    private void GenerateQuestions()
    {
        var anagrams = new HashSet<string>();

        Shuffle(Words);

        foreach (string word in Words)
        {
            if (word.Length >= MinLength && word.Length <= MaxLength)
            {
                string sorted = new(word.ToCharArray().Order().ToArray());

                if (!anagrams.Contains(sorted))
                {
                    var answers = new List<Answer>();
                    bool ok = true;

                    foreach (string answerWord in WordLookup[sorted])
                    {
                        answers.Add(new Answer(answerWord));

                        if (answerWord == sorted)
                        {
                            ok = false;
                        }
                    }

                    if (ok)
                    {
                        var question = new Question(sorted, answers);
                        AllQuestions.Enqueue(question);
                    }
                }
            }
        }
    }


    public void SubmitGuess(Guess guess)
    {
        if (!PlayerScores.ContainsKey(guess.Username))
        {
            PlayerScores[guess.Username] = 0;
        }

        bool foundQuestion = false;

        for (int i = 0; i < ActiveQuestions.Count; i++)
        {
            string sorted = new(guess.Word.ToCharArray().Order().ToArray());
            var question = ActiveQuestions[i];

            if (question != null && sorted == question.Anagram)
            {
                foundQuestion = true;
                bool match = false;

                foreach (var answer in question.Answers)
                {
                    if (answer.Word == guess.Word)
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

                                if (AllQuestions.Count == 0)
                                {
                                    ActiveQuestions[i] = null;
                                }
                                else
                                {
                                    ActiveQuestions[i] = AllQuestions.Dequeue();
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
                        return;
                    }
                }
            }
        }
    }


    public static void Shuffle<T>(IList<T> list)
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


class Question
{
    public string Anagram { get; }
    public List<Answer> Answers { get; }
    public int RemainingAnswers { get; set; }

    public Question(string anagram, List<Answer> answers)
    {
        Anagram = anagram;
        Answers = answers;
        RemainingAnswers = answers.Count;
    }
}


class Answer
{
    public string Word { get; }
    public bool Guessed { get; set; }

    public Answer(string word)
    {
        Word = word;
    }
}


class Guess
{
    public string Username { get; }
    public string Word { get; }
    public Question? Question { get; set; }
    public Answer? Answer { get; set; }
    public bool Correct { get; set; }
    public bool Duplicate { get; set; }
    public bool Incorrect { get; set; }

    public Guess(string username, string word)
    {
        Username = username;
        Word = word;
    }
}