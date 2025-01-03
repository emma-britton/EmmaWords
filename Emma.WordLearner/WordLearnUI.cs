using Emma.Lib;

namespace Emma.WordLearner;

public class WordLearnUI(WordLearn wordLearning, Form owner) : Gdi(owner)
{
    private readonly WordLearn m_WordLearning = wordLearning;
    private string m_Answer = "";

    public string Title { get; set; } = "emma word learning";

    public Bitmap? Logo { get; set; }


    public override void HandleKey(KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Escape)
        {
            m_WordLearning.GiveUp();
        }
        else if (e.KeyCode == Keys.Back && m_Answer.Length > 0)
        {
            m_Answer = m_Answer[..^1];
        }
        else if (e.KeyCode == Keys.Space)
        {
            m_Answer += " ";
        }
        else if (e.KeyCode == Keys.Enter)
        {
            string[] guesses = m_Answer.Split(" ", StringSplitOptions.RemoveEmptyEntries);
            bool validGuess = false;

            foreach (string guess in guesses)
            {
                if (m_WordLearning.SubmitGuess(guess))
                {
                    validGuess = true;
                }
            }

            if (validGuess)
            {
                m_Answer = "";
            }
        }
        else if (e.KeyCode >= Keys.A && e.KeyCode <= Keys.Z)
        {
            string key = e.KeyCode.ToString();

            if (m_WordLearning.Question is string question && question.Contains(key))
            {
                m_Answer += key;
            }
        }
    }


    public override void Render()
    {
        Gfx.Clear(Color.Black);

        var purple = Color.FromArgb(145, 71, 255);
        float padding = Area.Width / 60;

        var titleArea = new RectangleF(Area.Width * 0.3f, padding, Area.Width * 0.4f, Area.Height / 14);
        FillRectangle(purple, titleArea);
        DrawFitTextOneLine(Title, "Segoe UI", Color.White, titleArea, CenterCenter);

        if (Logo != null)
        {
            Gfx.DrawImage(Logo, titleArea.Left + 8, titleArea.Top + 4, titleArea.Height - 4, titleArea.Height - 4);
        }
        
        var cameraArea = new RectangleF(Area.Width * 0.37f, Area.Height - Area.Height * 0.3f, Area.Width * 0.26f, Area.Height * 0.3f);
        FillRectangle(purple, cameraArea);

        var questionArea = new RectangleF(Area.Width / 5, Area.Height / 5, Area.Width * 3 / 5, Area.Height / 6);
        DrawFitTextOneLine(m_WordLearning.Question, "Segoe UI", Color.White, questionArea, CenterCenter);

        var answerArea = new RectangleF(Area.Width / 5, Area.Height * 0.46f, Area.Width * 3 / 5, Area.Height / 6);
        DrawFitTextOneLine(m_Answer, "Segoe UI", Color.FromArgb(145, 71, 255), answerArea, CenterCenter);

        var correctTitle = new RectangleF(Area.Width * 4 / 5, padding, Area.Width / 5 - padding, Area.Height / 26);
        FillRectangle(purple, correctTitle);
        DrawFitTextOneLine("WORDS LEARNED", "Segoe UI", Color.White, correctTitle, CenterRight);

        var correctCountArea = new RectangleF(correctTitle.Left, correctTitle.Bottom, correctTitle.Width, Area.Height / 14);
        DrawFitTextOneLine(m_WordLearning.Learned.ToString(), "Segoe UI", Color.Lime, correctCountArea, CenterRight);

        var pendingTitle = new RectangleF(correctTitle.Left, correctCountArea.Bottom + padding / 2, correctTitle.Width, correctTitle.Height);
        FillRectangle(purple, pendingTitle);
        DrawFitTextOneLine("WORDS IN PROGRESS", "Segoe UI", Color.White, pendingTitle, CenterRight);

        var pendingCountArea = new RectangleF(correctTitle.Left, pendingTitle.Bottom, correctTitle.Width, Area.Height / 14);
        DrawFitTextOneLine(m_WordLearning.Progress.ToString(), "Segoe UI", Color.Yellow, pendingCountArea, CenterRight);

        var wrongTitle = new RectangleF(correctTitle.Left, pendingCountArea.Bottom + padding / 2, correctTitle.Width, correctTitle.Height);
        FillRectangle(purple, wrongTitle);
        DrawFitTextOneLine("WORDS MISSED", "Segoe UI", Color.White, wrongTitle, CenterRight);

        var wrongCountArea = new RectangleF(correctTitle.Left, wrongTitle.Bottom, correctTitle.Width, Area.Height / 14);
        DrawFitTextOneLine(m_WordLearning.Missed.ToString(), "Segoe UI", Color.OrangeRed, wrongCountArea, CenterRight);

        var logTitle = new RectangleF(padding, padding, correctTitle.Width, correctTitle.Height);
        FillRectangle(purple, logTitle);
        DrawFitTextOneLine("RECENT WORDS", "Segoe UI", Color.White, logTitle, CenterLeft);

        var logArea = new RectangleF(padding, logTitle.Bottom, Area.Width / 4, (Area.Height - logTitle.Bottom) / 3);
        float y = logArea.Top;

        foreach ((string word, bool correct) in Enumerable.Reverse(m_WordLearning.AnswerLog).Take(6))
        {
            var logItemArea = new RectangleF(logArea.Left, y, logArea.Width, logArea.Height / 5);
            var color = Color.Orange;

            if (correct)
            {
                color = Color.LimeGreen;
            }

            DrawFitTextOneLine(word, "Segoe UI", color, logItemArea, CenterLeft);
            y += logArea.Height / 6;
        }

        var wrongListTitle = new RectangleF(padding, logArea.Bottom + padding, logTitle.Width, logTitle.Height);
        FillRectangle(purple, wrongListTitle);

        if (m_WordLearning.MissedAnswers.Count > 0)
        {
            DrawFitTextOneLine("MISSED WORDS", "Segoe UI", Color.White, wrongListTitle, CenterLeft);

            var missedAnswerArea = new RectangleF(padding, wrongListTitle.Bottom, wrongListTitle.Width, Area.Height - wrongListTitle.Bottom);
            DrawFitText(string.Join("\r\n", m_WordLearning.MissedAnswers.Order()), "Segoe UI", Color.Yellow, missedAnswerArea, CenterCenter);
        }
        else
        {
            DrawFitTextOneLine("INCORRECT ANSWERS", "Segoe UI", Color.White, wrongListTitle, CenterLeft);

            if (m_WordLearning.IncorrectGuesses.Count > 0)
            {
                var incorrectGuessArea = new RectangleF(padding, wrongListTitle.Bottom, wrongListTitle.Width, Area.Height - wrongListTitle.Bottom);
                DrawFitText(string.Join("\r\n", m_WordLearning.IncorrectGuesses.Order()), "Segoe UI", Color.OrangeRed, incorrectGuessArea, CenterCenter);
            }
        }

        var correctAnswerTitle = new RectangleF(correctTitle.Left, wrongListTitle.Top, correctTitle.Width, correctTitle.Height);
        FillRectangle(purple, correctAnswerTitle);
        DrawFitTextOneLine("CORRECT ANSWERS", "Segoe UI", Color.White, correctAnswerTitle, CenterRight);

        if (m_WordLearning.CorrectAnswers.Count > 0)
        {
            var correctAnswerArea = new RectangleF(correctTitle.Left, correctAnswerTitle.Bottom, correctTitle.Width, Area.Height - correctAnswerTitle.Bottom);
            DrawFitText(string.Join("\r\n", m_WordLearning.CorrectAnswers.Order()), "Segoe UI", Color.LimeGreen, correctAnswerArea, CenterCenter);
        }
    }
}

