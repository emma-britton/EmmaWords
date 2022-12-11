using System.Text.RegularExpressions;
using TwitchLib.Client.Models;

namespace EmmaWords;

class WordGameUI : IGameUI
{
    public int Columns { get; set; }
    public int Rows { get; set; }
    private readonly WordGame Game;
    

    public WordGameUI(WordGame game, int columns, int rows)
    {
        Game = game;
        Columns = columns;
        Rows = rows;
    }


    public void HandleMessage(ChatMessage message)
    {
        string[] guesses = message.Message.ToUpper().Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);

        foreach (string word in guesses)
        {
            if (Regex.IsMatch(word, "[A-Z]+"))
            {
                var guess = new Guess(message.Username, word);
                Game.SubmitGuess(guess);
            }
        }
    }


    public void Render(GdiAnimation Animation, Graphics gfx, Rectangle area)
    {
        float headerHeight = area.Height / 8;
        float sidebarWidth = area.Width / 5;
        float leftMargin = 4;
        float rightMargin = 20;
        float topMargin = 4;

        var playArea = new RectangleF(sidebarWidth + leftMargin, headerHeight, area.Width - sidebarWidth - leftMargin * 2, area.Height - topMargin - headerHeight);
        float cellWidth = playArea.Width / Columns;
        float cellHeight = playArea.Height / Rows;

        for (int x = 0; x < Columns; x++)
        {
            for (int y = 0; y < Rows; y++)
            {
                int index = y * Columns + x;
                var question = Game.ActiveQuestions.ElementAtOrDefault(index);

                if (question != null)
                {
                    float cellx = playArea.Left + x * cellWidth + leftMargin;
                    float celly = playArea.Top + y * cellHeight;

                    var cellArea = new RectangleF(cellx, celly, cellWidth - leftMargin * 2, cellHeight - topMargin * 2);
                    gfx.FillRectangle(Animation.GetBrush(Color.Black), cellArea);

                    var textArea = new RectangleF(cellx + leftMargin + 12, celly + topMargin, cellWidth - leftMargin - rightMargin - 10, cellHeight - topMargin * 2);
                    Animation.DrawFitTextOneLine(question.Anagram, "Consolas", Color.White, textArea, Animation.CenterLeft);

                    if (question.Answers.Count > 1)
                    {
                        gfx.DrawString(question.RemainingAnswers.ToString(), Animation.GetFont("Arial", 18, true),
                            Animation.GetBrush(Color.White), new RectangleF(cellArea.Right - 34 - leftMargin, cellArea.Top + topMargin + 8, 34 + leftMargin, cellArea.Height));
                        gfx.DrawString("/", Animation.GetFont("Arial", 24, true),
                            Animation.GetBrush(Color.White), new RectangleF(cellArea.Right - 24 - leftMargin, cellArea.Top + 18 + topMargin, 24 + leftMargin, cellArea.Height));
                        gfx.DrawString(question.Answers.Count.ToString(), Animation.GetFont("Arial", 18, true),
                            Animation.GetBrush(Color.White), new RectangleF(cellArea.Right - 16 - leftMargin, cellArea.Top + 32 + topMargin, 16 + leftMargin, cellArea.Height));
                    }
                }
            }
        }

        var headerArea1 = new RectangleF(0, 0, area.Width, headerHeight * 3 / 4);
        Animation.DrawFitTextOneLine("emma words", "Arial", Color.White, headerArea1, Animation.CenterCenter);

        var headerArea2 = new RectangleF(0, (headerHeight * 3 / 4) - topMargin, area.Width, headerHeight * 1 / 4);
        Animation.DrawFitTextOneLine("Solve the anagrams! Type your guesses in chat", "Arial", Color.White, headerArea2, Animation.BottomCenter);

        Animation.DrawFitTextOneLine("Word list", "Arial", Color.White,
            new RectangleF(area.Width - leftMargin * 3 - (area.Width / 8), topMargin * 2, area.Width / 8, headerHeight / 4), Animation.TopCenter);
            
        Animation.DrawFitTextOneLine(Game.WordService.CurrentList.Name, "Arial", Color.White,
            new RectangleF(area.Width - leftMargin - (area.Width / 8), topMargin + headerHeight / 4, area.Width / 8, headerHeight * 3 / 4), Animation.CenterCenter);

        var scoreboardArea = new RectangleF(leftMargin * 2, headerHeight + (area.Height - headerHeight) / 2,
            sidebarWidth - leftMargin * 2, ((area.Height - headerHeight) / 2) - topMargin * 3);
        gfx.FillRectangle(Animation.GetBrush(Color.Black), scoreboardArea);

        var scores = Game.PlayerScores.OrderByDescending(x => x.Value);
        float scoreHeight = scoreboardArea.Height / 10;
        float scorey = scoreboardArea.Top + topMargin;

        foreach (var score in scores.Take(10))
        {
            var scoreArea1 = new RectangleF(scoreboardArea.Left + leftMargin * 2, scorey, scoreboardArea.Width * 4 / 5, scoreHeight * 3 / 4);
            var scoreArea2 = new RectangleF(scoreboardArea.Left + leftMargin * 2, scorey, scoreboardArea.Width, scoreHeight * 3 / 4);
            Animation.DrawFitTextOneLine(score.Key, "Arial", Color.White, scoreArea1, Animation.CenterLeft);
            Animation.DrawFitTextOneLine(score.Value.ToString(), "Arial", Color.White, scoreArea2, Animation.CenterRight);
            scorey += scoreHeight;
        }

        var guessArea = new RectangleF(leftMargin * 2, headerHeight, sidebarWidth - leftMargin * 2, scoreboardArea.Top - headerHeight - topMargin * 2);
        gfx.FillRectangle(Animation.GetBrush(Color.Black), guessArea);

        var guesses = Game.Guesses.Skip(Game.Guesses.Count - 10).ToList();
        float guessHeight = guessArea.Height / 15;
        float guessy = guessArea.Top + topMargin;

        foreach (var guess in guesses)
        {
            var guessArea1 = new RectangleF(guessArea.Left + leftMargin * 2, guessy, guessArea.Width - leftMargin * 3, guessHeight);

            if (guess.Correct)
            {
                Animation.DrawFitTextOneLine($"{guess.Username} got {guess.Word}", "Arial", Color.Lime, guessArea1, Animation.CenterLeft);
            }
            else if (guess.Incorrect)
            {
                Animation.DrawFitTextOneLine($"{guess.Username} guessed {guess.Word}", "Arial", Color.Red, guessArea1, Animation.CenterLeft);
            }
            else if (guess.Duplicate)
            {
                Animation.DrawFitTextOneLine($"{guess.Username} missed {guess.Word}", "Arial", Color.Gray, guessArea1, Animation.CenterLeft);
            }

            guessy += scoreHeight;
        }
    }


    public void HandleKey(KeyEventArgs e)
    {
    }


    public void HandleMouse(MouseEventArgs e)
    {
    }
}
