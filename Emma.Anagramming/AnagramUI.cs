using Emma.Lib;
using System.Text.RegularExpressions;

namespace Emma.Anagramming;

public class AnagramUI : Gdi
{
    public AnagramGame Game { get; }


    public AnagramUI(AnagramGame game, Form owner) : base(owner)
    {
        Game = game;
    }


    public override void HandleMessage(StreamMessage message)
    {
        if (message.Text != null)
        {
            string[] guesses = message.Text.ToUpper().Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);

            foreach (string word in guesses)
            {
                if (Regex.IsMatch(word, "[A-Z]+"))
                {
                    Game.SubmitGuess(message.Username, word);
                }
            }
        }
    }


    public override void Render()
    {
        float headerHeight = Area.Height / 8;
        float sidebarWidth = Area.Width / 5;
        float leftMargin = 4;
        float rightMargin = 20;
        float topMargin = 4;

        var playArea = new RectangleF(sidebarWidth + leftMargin, headerHeight, Area.Width - sidebarWidth - leftMargin * 2, Area.Height - topMargin - headerHeight);
        float cellWidth = playArea.Width / Game.GameColumns;
        float cellHeight = playArea.Height / Game.GameRows;

        for (int x = 0; x < Game.GameColumns; x++)
        {
            for (int y = 0; y < Game.GameRows; y++)
            {
                int index = y * Game.GameColumns + x;
                var question = Game.ActiveQuestions.ElementAtOrDefault(index);

                if (question != null)
                {
                    float cellx = playArea.Left + x * cellWidth + leftMargin;
                    float celly = playArea.Top + y * cellHeight;

                    var cellArea = new RectangleF(cellx, celly, cellWidth - leftMargin * 2, cellHeight - topMargin * 2);
                    FillRectangle(Color.Black, cellArea);

                    float numberWidth = cellWidth / 8;
                    var textArea = new RectangleF(cellx + leftMargin + 12, celly + topMargin, cellWidth - leftMargin - numberWidth - rightMargin, cellHeight - topMargin * 2);
                    DrawFitTextOneLine(question.Anagram, "Consolas", Color.White, textArea, CenterLeft);

                    if (question.Answers.Count > 1)
                    {
                        DrawFitTextOneLine(question.RemainingAnswers.ToString(), "Arial", Color.White,
                            new RectangleF(cellArea.Right - numberWidth - rightMargin, cellArea.Top + topMargin, numberWidth + leftMargin, cellArea.Height / 2), CenterCenter);

                        DrawFitTextOneLine("/", "Arial", Color.White,
                            new RectangleF(cellArea.Right - (numberWidth * 2 / 3) - rightMargin, cellArea.Top + topMargin, numberWidth + leftMargin, cellArea.Height), CenterCenter);

                        DrawFitTextOneLine(question.Answers.Count.ToString(), "Arial", Color.White,
                            new RectangleF(cellArea.Right - (numberWidth * 1 / 3) - rightMargin, cellArea.Top + (cellHeight * 1/3) + topMargin, numberWidth + leftMargin, cellArea.Height / 2), CenterCenter);
                    }
                }
            }
        }

        var headerArea1 = new RectangleF(0, 0, Area.Width, headerHeight * 3 / 4);
        DrawFitTextOneLine("emma words", "Arial", Color.White, headerArea1, CenterCenter);

        var headerArea2 = new RectangleF(0, (headerHeight * 3 / 4) - topMargin, Area.Width, headerHeight * 1 / 4);
        DrawFitTextOneLine("Solve the anagrams! Type your guesses in chat", "Arial", Color.White, headerArea2, BottomCenter);

        DrawFitTextOneLine("Word list", "Arial", Color.White,
            new RectangleF(Area.Width - leftMargin * 3 - (Area.Width / 8), topMargin * 2, Area.Width / 8, headerHeight / 4), TopCenter);

        DrawFitTextOneLine(Game.Lexicon.Name, "Arial", Color.White,
            new RectangleF(Area.Width - leftMargin - (Area.Width / 8), topMargin + headerHeight / 4, Area.Width / 8, headerHeight * 3 / 4), CenterCenter);

        var scoreboardArea = new RectangleF(leftMargin * 2, headerHeight + (Area.Height - headerHeight) / 2,
            sidebarWidth - leftMargin * 2, ((Area.Height - headerHeight) / 2) - topMargin * 3);
        Gfx.FillRectangle(GetBrush(Color.Black), scoreboardArea);

        var scores = Game.PlayerScores.OrderByDescending(x => x.Value);
        float scoreHeight = scoreboardArea.Height / 10;
        float scorey = scoreboardArea.Top + topMargin;

        foreach (var score in scores.Take(10))
        {
            var scoreArea1 = new RectangleF(scoreboardArea.Left + leftMargin * 2, scorey, scoreboardArea.Width * 4 / 5, scoreHeight * 3 / 4);
            var scoreArea2 = new RectangleF(scoreboardArea.Left + leftMargin * 2, scorey, scoreboardArea.Width * 9 / 10, scoreHeight * 3 / 4);
            DrawFitTextOneLine(score.Key, "Arial", Color.White, scoreArea1, CenterLeft);
            DrawFitTextOneLine(score.Value.ToString(), "Arial", Color.White, scoreArea2, CenterRight);
            scorey += scoreHeight;
        }

        var guessArea = new RectangleF(leftMargin * 2, headerHeight, sidebarWidth - leftMargin * 2, scoreboardArea.Top - headerHeight - topMargin * 2);
        Gfx.FillRectangle(GetBrush(Color.Black), guessArea);

        var guesses = Game.Guesses.Skip(Game.Guesses.Count - 10).ToList();
        float guessHeight = guessArea.Height / 15;
        float guessy = guessArea.Top + topMargin;

        foreach (var guess in guesses)
        {
            var guessArea1 = new RectangleF(guessArea.Left + leftMargin * 2, guessy, guessArea.Width - leftMargin * 3, guessHeight);

            if (guess.Correct)
            {
                DrawFitTextOneLine($"{guess.Username} got {guess.Word}", "Arial", Color.Lime, guessArea1, CenterLeft);
            }
            else if (guess.Incorrect)
            {
                DrawFitTextOneLine($"{guess.Username} guessed {guess.Word}", "Arial", Color.Red, guessArea1, CenterLeft);
            }
            else if (guess.Duplicate)
            {
                DrawFitTextOneLine($"{guess.Username} missed {guess.Word}", "Arial", Color.Gray, guessArea1, CenterLeft);
            }

            guessy += scoreHeight;
        }
    }
}
