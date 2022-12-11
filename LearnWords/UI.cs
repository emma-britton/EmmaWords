using EmmaLib;

namespace LearnWords;

public partial class UI : Form
{
    private readonly WordLearning m_WordLearning;
    private readonly Gdi Gdi;

    private string m_Answer = "";


    public UI(WordLearning wordLearning)
    {
        InitializeComponent();
        m_WordLearning = wordLearning;

        Gdi = new Gdi(CreateGraphics(), DisplayRectangle);
        Gdi.Render += Render;

        Resize += (sender, e) => Gdi.Resize(DisplayRectangle);
        Shown += (sender, e) => Gdi.Start();
        FormClosing += (sender, e) => Gdi.Stop();
    }


    private void WordLearningUI_KeyDown(object sender, KeyEventArgs e)
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


    public void Render( Graphics gfx, Rectangle area)
    {
        gfx.Clear(Color.Black);

        var purple = Color.FromArgb(145, 71, 255);
        float padding = area.Width / 60;

        var titleArea = new RectangleF(area.Width / 3, padding, area.Width / 3, area.Height / 16);
        Gdi.FillRectangle(purple, titleArea);
        Gdi.DrawFitTextOneLine("emma word learning", "Segoe UI", Color.White, titleArea, Gdi.CenterCenter);

        var cameraArea = new RectangleF(area.Width * 0.37f, area.Height - area.Height * 0.3f, area.Width * 0.26f, area.Height * 0.3f);
        Gdi.FillRectangle(purple, cameraArea);

        var questionArea = new RectangleF(area.Width / 5, area.Height / 5, area.Width * 3 / 5, area.Height / 6);
        Gdi.DrawFitTextOneLine(m_WordLearning.Question, "Segoe UI", Color.White, questionArea, Gdi.CenterCenter);

        var answerArea = new RectangleF(area.Width / 5, area.Height * 0.46f, area.Width * 3 / 5, area.Height / 6);
        Gdi.DrawFitTextOneLine(m_Answer, "Segoe UI", Color.FromArgb(145, 71, 255), answerArea, Gdi.CenterCenter);

        var correctTitle = new RectangleF(area.Width * 4 / 5, padding, area.Width / 5 - padding, area.Height / 26);
        Gdi.FillRectangle(purple, correctTitle);
        Gdi.DrawFitTextOneLine("WORDS LEARNED", "Segoe UI", Color.White, correctTitle, Gdi.CenterRight);

        var correctCountArea = new RectangleF(correctTitle.Left, correctTitle.Bottom, correctTitle.Width, area.Height / 14);
        Gdi.DrawFitTextOneLine(m_WordLearning.Learned.ToString(), "Segoe UI", Color.Lime, correctCountArea, Gdi.CenterRight);

        var pendingTitle = new RectangleF(correctTitle.Left, correctCountArea.Bottom + padding / 2, correctTitle.Width, correctTitle.Height);
        Gdi.FillRectangle(purple, pendingTitle);
        Gdi.DrawFitTextOneLine("WORDS IN PROGRESS", "Segoe UI", Color.White, pendingTitle, Gdi.CenterRight);

        var pendingCountArea = new RectangleF(correctTitle.Left, pendingTitle.Bottom, correctTitle.Width, area.Height / 14);
        Gdi.DrawFitTextOneLine(m_WordLearning.Progress.ToString(), "Segoe UI", Color.Yellow, pendingCountArea, Gdi.CenterRight);

        var wrongTitle = new RectangleF(correctTitle.Left, pendingCountArea.Bottom + padding / 2, correctTitle.Width, correctTitle.Height);
        Gdi.FillRectangle(purple, wrongTitle);
        Gdi.DrawFitTextOneLine("WORDS MISSED", "Segoe UI", Color.White, wrongTitle, Gdi.CenterRight);

        var wrongCountArea = new RectangleF(correctTitle.Left, wrongTitle.Bottom, correctTitle.Width, area.Height / 14);
        Gdi.DrawFitTextOneLine(m_WordLearning.Missed.ToString(), "Segoe UI", Color.OrangeRed, wrongCountArea, Gdi.CenterRight);

        var logTitle = new RectangleF(padding, padding, correctTitle.Width, correctTitle.Height);
        Gdi.FillRectangle(purple, logTitle);
        Gdi.DrawFitTextOneLine("RECENT WORDS", "Segoe UI", Color.White, logTitle, Gdi.CenterLeft);

        var logArea = new RectangleF(padding, logTitle.Bottom, area.Width / 4, (area.Height - logTitle.Bottom) / 3);
        float y = logArea.Top;

        foreach ((string word, bool correct) in Enumerable.Reverse(m_WordLearning.AnswerLog).Take(6))
        {
            var logItemArea = new RectangleF(logArea.Left, y, logArea.Width, logArea.Height / 5);
            var color = Color.Orange;

            if (correct)
            {
                color = Color.LimeGreen;
            }

            Gdi.DrawFitTextOneLine(word, "Segoe UI", color, logItemArea, Gdi.CenterLeft);
            y += logArea.Height / 6;
        }

        var wrongListTitle = new RectangleF(padding, logArea.Bottom + padding, logTitle.Width, logTitle.Height);
        Gdi.FillRectangle(purple, wrongListTitle);

        if (m_WordLearning.MissedAnswers.Any())
        {
            Gdi.DrawFitTextOneLine("MISSED WORDS", "Segoe UI", Color.White, wrongListTitle, Gdi.CenterLeft);

            var missedAnswerArea = new RectangleF(padding, wrongListTitle.Bottom, wrongListTitle.Width, area.Height - wrongListTitle.Bottom);
            Gdi.DrawFitText(string.Join("\r\n", m_WordLearning.MissedAnswers.Order()), "Segoe UI", Color.Yellow, missedAnswerArea, Gdi.CenterCenter);
        }
        else
        {
            Gdi.DrawFitTextOneLine("INCORRECT ANSWERS", "Segoe UI", Color.White, wrongListTitle, Gdi.CenterLeft);

            if (m_WordLearning.IncorrectGuesses.Any())
            {
                var incorrectGuessArea = new RectangleF(padding, wrongListTitle.Bottom, wrongListTitle.Width, area.Height - wrongListTitle.Bottom);
                Gdi.DrawFitText(string.Join("\r\n", m_WordLearning.IncorrectGuesses.Order()), "Segoe UI", Color.OrangeRed, incorrectGuessArea, Gdi.CenterCenter);
            }
        }

        var correctAnswerTitle = new RectangleF(correctTitle.Left, wrongListTitle.Top, correctTitle.Width, correctTitle.Height);
        Gdi.FillRectangle(purple, correctAnswerTitle);
        Gdi.DrawFitTextOneLine("CORRECT ANSWERS", "Segoe UI", Color.White, correctAnswerTitle, Gdi.CenterRight);

        if (m_WordLearning.CorrectAnswers.Any())
        {
            var correctAnswerArea = new RectangleF(correctTitle.Left, correctAnswerTitle.Bottom, correctTitle.Width, area.Height - correctAnswerTitle.Bottom);
            Gdi.DrawFitText(string.Join("\r\n", m_WordLearning.CorrectAnswers.Order()), "Segoe UI", Color.LimeGreen, correctAnswerArea, Gdi.CenterCenter);
        }
    }
}
