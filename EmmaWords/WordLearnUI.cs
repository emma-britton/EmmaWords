using EmmaWords.Properties;
using System.IO;
using TwitchLib.Client.Models;

namespace EmmaWords;

class WordLearnUI : IGameUI
{
    private readonly WordService WordService;
    private readonly List<(string, decimal)> Questions = new();
    private readonly List<string> Answers = new();
    private readonly List<string> CorrectAnswers = new();
    private readonly List<string> MissedAnswers = new();
    private readonly List<string> DoneMissedAnswers = new();
    private readonly List<string> IncorrectGuesses = new();

    private readonly List<(string Answers, decimal Score)> Log = new();
    private (string Alphagram, decimal Score) CurrentQuestion;
    private string CurrentAnswer = "";

    private readonly HashSet<string> LearnedWords = new();
    private readonly HashSet<string> PendingWords = new();
    private readonly HashSet<string> MissedWords = new();
    private readonly Random Random = new();


    public WordLearnUI(WordService wordService)
    {
        WordService = wordService;

        foreach (string line in File.ReadLines(Path.Combine(Settings.Default.LearnFolder, "questions.txt")))
        {
            Questions.Add((line[..line.IndexOf(',')], decimal.Parse(line[(line.IndexOf(',') + 1)..])));
        }

        string learnedFile = Path.Combine(Settings.Default.LearnFolder, "learned.txt");
        string pendingFile = Path.Combine(Settings.Default.LearnFolder, "seen.txt");
        string missedFile = Path.Combine(Settings.Default.LearnFolder, "missed.txt");

        if (File.Exists(learnedFile))
        {
            LearnedWords = File.ReadLines(learnedFile).ToHashSet();
        }

        if (File.Exists(pendingFile))
        {
            PendingWords = File.ReadLines(pendingFile).ToHashSet();
        }

        if (File.Exists(missedFile))
        {
            MissedWords = File.ReadLines(missedFile).ToHashSet();
        }

        NextQuestion();
    }


    private void Save()
    {
        using var writer = new StreamWriter(Path.Combine(Settings.Default.LearnFolder, "questions.txt"));

        foreach (var entry in Questions)
        {
            writer.WriteLine($"{entry.Item1},{entry.Item2}");
        }

        File.WriteAllLines(Path.Combine(Settings.Default.LearnFolder, "learned.txt"), LearnedWords.ToArray());
        File.WriteAllLines(Path.Combine(Settings.Default.LearnFolder, "seen.txt"), PendingWords.ToArray());
        File.WriteAllLines(Path.Combine(Settings.Default.LearnFolder, "missed.txt"), MissedWords.ToArray());
    }


    private void NextQuestion()
    {
        CurrentQuestion = Questions[0];
        CurrentAnswer = "";
        CorrectAnswers.Clear();
        IncorrectGuesses.Clear();
        MissedAnswers.Clear();
        DoneMissedAnswers.Clear();
        Answers.Clear();
        Answers.AddRange(WordService.Anagram(CurrentQuestion.Alphagram));
    }


    private void SubmitGuess(string guess)
    {
        if (Answers.Contains(guess))
        {
            CorrectAnswers.Add(guess);
            Answers.Remove(guess);

            if (MissedWords.Contains(guess) && MissedAnswers.Count == 0 && IncorrectGuesses.Count == 0)
            {
                MissedWords.Remove(guess);
                PendingWords.Add(guess);
            }
            else if (PendingWords.Contains(guess) && MissedAnswers.Count == 0 && IncorrectGuesses.Count == 0)
            {
                PendingWords.Remove(guess);
                LearnedWords.Add(guess);
            }
            else if (!LearnedWords.Contains(guess))
            {
                PendingWords.Add(guess);
            }

            if (Answers.Count == 0)
            {
                RescheduleQuestion();
                NextQuestion();
            }
        }
        else if (MissedAnswers.Contains(guess))
        {
            if (LearnedWords.Contains(guess))
            {
                LearnedWords.Remove(guess);
                PendingWords.Add(guess);
            }
            else if (PendingWords.Contains(guess) && guess.Length < 6)
            {
                PendingWords.Remove(guess);
                MissedWords.Add(guess);
            }
            else if (PendingWords.Contains(guess) && (CorrectAnswers.Count == 0))
            {
                PendingWords.Remove(guess);
                MissedWords.Add(guess);
            }
            else if (!PendingWords.Contains(guess) && !MissedWords.Contains(guess) && guess.Length < 6)
            {
                MissedWords.Add(guess);
            }
            else if (!PendingWords.Contains(guess) && !MissedWords.Contains(guess))
            {
                PendingWords.Add(guess);
            }

            MissedAnswers.Remove(guess);
            DoneMissedAnswers.Add(guess);

            if (MissedAnswers.Count == 0)
            {
                RescheduleQuestion();
                NextQuestion();
            }
        }
        else if (!CorrectAnswers.Contains(guess) && !IncorrectGuesses.Contains(guess))
        {
            IncorrectGuesses.Add(guess);
            
            foreach (var answer in Answers)
            {
                if (LearnedWords.Contains(answer))
                {
                    LearnedWords.Remove(answer);
                    PendingWords.Add(answer);
                }
                else if (PendingWords.Contains(answer) && answer.Length < 6)
                {
                    PendingWords.Remove(answer);
                    MissedWords.Add(answer);
                }
                else if (!LearnedWords.Contains(answer) && !PendingWords.Contains(answer) && answer.Length < 6)
                {
                    MissedWords.Add(answer);
                }
                else if (answer.Length < 6)
                {
                    MissedWords.Add(answer);
                }
            }
        }
    }


    private void GiveUp()
    {
        MissedAnswers.AddRange(Answers);
        Answers.Clear();
    }


    private void RescheduleQuestion()
    {
        decimal scoreChange;

        if (CurrentQuestion.Alphagram.Length >= 6)
        {
            int correct = CorrectAnswers.Count - IncorrectGuesses.Count;
            int total = CorrectAnswers.Count + DoneMissedAnswers.Count;

            scoreChange = (correct / (decimal)total * 1.5m) - 0.5m;
        }
        else if (!DoneMissedAnswers.Any() && !IncorrectGuesses.Any())
        {
            scoreChange = 1;
        }
        else
        {
            scoreChange = -1;
        }

        foreach (string answer in CorrectAnswers)
        {
            Log.Add((answer, scoreChange));
        }

        foreach (string answer in DoneMissedAnswers)
        {
            Log.Add((answer, scoreChange));
        }

        CurrentQuestion.Score += scoreChange;
        int newIndex = Math.Min(Questions.Count - 1, (int)(Random.Next(90, 110) * Math.Pow(2, (double)CurrentQuestion.Score)));
        Questions.Insert(newIndex, CurrentQuestion);
        Questions.RemoveAt(0);
    }


    public void HandleKey(KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Escape)
        {
            GiveUp();
        }
        else if (e.KeyCode == Keys.Back && CurrentAnswer.Length > 0)
        {
            CurrentAnswer = CurrentAnswer[..^1];
        }
        else if (e.KeyCode == Keys.Space)
        {
            CurrentAnswer += " ";
        }
        else if (e.KeyCode == Keys.Enter)
        {
            string[] guesses = CurrentAnswer.Split(" ", StringSplitOptions.RemoveEmptyEntries);
            bool tookGuess = false;

            foreach (var guess in guesses)
            {
                if (guess.Length == CurrentQuestion.Alphagram.Length)
                {
                    SubmitGuess(guess);
                    tookGuess = true;

                    if (Answers.Count == 0)
                    {
                        break;
                    }
                }
            }

            if (tookGuess)
            {
                CurrentAnswer = "";
            }
        }
        else if (e.KeyCode == Keys.F4)
        {
            Save();
        }
        else if (e.KeyCode >= Keys.A && e.KeyCode <= Keys.Z)
        {
            string key = e.KeyCode.ToString();

            if (CurrentQuestion.Alphagram.Contains(key))
            {
                CurrentAnswer += key;
            }
        }
    }

    public void HandleMessage(ChatMessage message)
    {

    }

    public void HandleMouse(MouseEventArgs e)
    {

    }

    public void Render(GdiAnimation animation, Graphics gfx, Rectangle area)
    {
        gfx.Clear(Color.Black);

        var purple = Color.FromArgb(145, 71, 255);
        float padding = area.Width / 60;

        var titleArea = new RectangleF(area.Width / 3, padding, area.Width / 3, area.Height / 16);
        animation.FillRectangle(purple, titleArea);
        animation.DrawFitTextOneLine("emma learns some words", "Segoe UI", Color.White, titleArea, animation.CenterCenter);

        var cameraArea = new RectangleF(area.Width * 0.37f, area.Height - area.Height * 0.3f, area.Width * 0.26f, area.Height * 0.3f);
        animation.FillRectangle(purple, cameraArea);

        var questionArea = new RectangleF(area.Width / 5, area.Height / 5, area.Width * 3 / 5, area.Height / 6);
        animation.DrawFitTextOneLine(CurrentQuestion.Alphagram, "Segoe UI", Color.White, questionArea, animation.CenterCenter);

        var answerArea = new RectangleF(area.Width / 5, area.Height * 0.46f, area.Width * 3 / 5, area.Height / 6);
        animation.DrawFitTextOneLine(CurrentAnswer, "Segoe UI", Color.FromArgb(145, 71, 255), answerArea, animation.CenterCenter);

        var correctTitle = new RectangleF(area.Width * 4 / 5, padding, area.Width / 5 - padding, area.Height / 26);
        animation.FillRectangle(purple, correctTitle);
        animation.DrawFitTextOneLine("WORDS LEARNED", "Segoe UI", Color.White, correctTitle, animation.CenterRight);

        var correctCountArea = new RectangleF(correctTitle.Left, correctTitle.Bottom, correctTitle.Width, area.Height / 14);
        animation.DrawFitTextOneLine(LearnedWords.Count.ToString(), "Segoe UI", Color.Lime, correctCountArea, animation.CenterRight);

        var pendingTitle = new RectangleF(correctTitle.Left, correctCountArea.Bottom + padding / 2, correctTitle.Width, correctTitle.Height);
        animation.FillRectangle(purple, pendingTitle);
        animation.DrawFitTextOneLine("WORDS IN PROGRESS", "Segoe UI", Color.White, pendingTitle, animation.CenterRight);

        var pendingCountArea = new RectangleF(correctTitle.Left, pendingTitle.Bottom, correctTitle.Width, area.Height / 14);
        animation.DrawFitTextOneLine(PendingWords.Count.ToString(), "Segoe UI", Color.Yellow, pendingCountArea, animation.CenterRight);

        var wrongTitle = new RectangleF(correctTitle.Left, pendingCountArea.Bottom + padding / 2, correctTitle.Width, correctTitle.Height);
        animation.FillRectangle(purple, wrongTitle);
        animation.DrawFitTextOneLine("WORDS MISSED", "Segoe UI", Color.White, wrongTitle, animation.CenterRight);

        var wrongCountArea = new RectangleF(correctTitle.Left, wrongTitle.Bottom, correctTitle.Width, area.Height / 14);
        animation.DrawFitTextOneLine(MissedWords.Count.ToString(), "Segoe UI", Color.OrangeRed, wrongCountArea, animation.CenterRight);

        var logTitle = new RectangleF(padding, padding, correctTitle.Width, correctTitle.Height);
        animation.FillRectangle(purple, logTitle);
        animation.DrawFitTextOneLine("RECENT WORDS", "Segoe UI", Color.White, logTitle, animation.CenterLeft);

        var logArea = new RectangleF(padding, logTitle.Bottom, area.Width / 4, (area.Height - logTitle.Bottom) / 3);
        float y = logArea.Top;

        foreach (var item in Enumerable.Reverse(Log).Take(6))
        {
            var logItemArea = new RectangleF(logArea.Left, y, logArea.Width, logArea.Height / 5);
            Color color;

            if (item.Score >= 1)
            {
                color = Color.LimeGreen;
            }
            else if (item.Score > 0)
            {
                color = Color.Yellow;
            }
            else if (item.Score > -1)
            {
                color = Color.Orange;
            }
            else
            {
                color = Color.OrangeRed;
            }

            animation.DrawFitTextOneLine(item.Answers, "Segoe UI", color, logItemArea, animation.CenterLeft);
            y += logArea.Height / 6;
        }

        var wrongListTitle = new RectangleF(padding, logArea.Bottom + padding, logTitle.Width, logTitle.Height);
        animation.FillRectangle(purple, wrongListTitle);

        if (MissedAnswers.Any())
        {
            animation.DrawFitTextOneLine("MISSED WORDS", "Segoe UI", Color.White, wrongListTitle, animation.CenterLeft);

            var missedAnswerArea = new RectangleF(padding, wrongListTitle.Bottom, wrongListTitle.Width, area.Height - wrongListTitle.Bottom);
            animation.DrawFitText(string.Join("\r\n", MissedAnswers), "Segoe UI", Color.Yellow, missedAnswerArea, animation.CenterCenter);
        }
        else
        {
            animation.DrawFitTextOneLine("INCORRECT ANSWERS", "Segoe UI", Color.White, wrongListTitle, animation.CenterLeft);

            if (IncorrectGuesses.Any())
            {
                var incorrectGuessArea = new RectangleF(padding, wrongListTitle.Bottom, wrongListTitle.Width, area.Height - wrongListTitle.Bottom);
                animation.DrawFitText(string.Join("\r\n", IncorrectGuesses), "Segoe UI", Color.OrangeRed, incorrectGuessArea, animation.CenterCenter);
            }
        }

        var correctAnswerTitle = new RectangleF(correctTitle.Left, wrongListTitle.Top, correctTitle.Width, correctTitle.Height);
        animation.FillRectangle(purple, correctAnswerTitle);
        animation.DrawFitTextOneLine("CORRECT ANSWERS", "Segoe UI", Color.White, correctAnswerTitle, animation.CenterRight);

        if (CorrectAnswers.Any())
        {
            var correctAnswerArea = new RectangleF(correctTitle.Left, correctAnswerTitle.Bottom, correctTitle.Width, area.Height - correctAnswerTitle.Bottom);
            animation.DrawFitText(string.Join("\r\n", CorrectAnswers), "Segoe UI", Color.LimeGreen, correctAnswerArea, animation.CenterCenter);
        }
    }
}
