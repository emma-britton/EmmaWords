using Emma.Lib;

namespace Emma.Scrabble;

public class ScrabbleUI(ScrabbleGame game, Form owner) : Gdi(owner)
{
    private readonly ScrabbleGame Game = game;
    private readonly RuleSet RuleSet = game.RuleSet;
    private readonly Lexicon Lexicon = game.Lexicon;

    private RectangleF[,] SpaceAreas = new RectangleF[0, 0];
    private (int X, int Y) Cursor = (-1, -1);
    private bool Vertical;
    private readonly Dictionary<string, RectangleF> ButtonActions = [];
    private readonly Image EmmaImage = new Bitmap(@"C:\Users\huggl\streaming\points\Wave_112.png");
    private readonly Random Random = new();


    public override void Render()
    {
        Game.RuleSet.IfOnlyVariant = true;
        var emmaPurple = Color.FromArgb(145, 71, 255);
        const string font = "Segoe UI";

        var topPad = new RectangleF(0, 0, Area.Width, 8);
        var head = new RectangleF(0, topPad.Bottom, Area.Width, Area.Height / 18);
        FillRectangle(Color.Black, head);
        FillRectangle(Color.Black, topPad);

        float padding = head.Height / 3;
        var scoreArea = new RectangleF(head.Width / 2 - head.Width / 14, 0, head.Width / 7, head.Height + head.Top);
        var leftScoreArea = new RectangleF(scoreArea.Left, scoreArea.Top - 2, scoreArea.Width / 2.5f, scoreArea.Height);
        var rightScoreArea = new RectangleF(scoreArea.Right - scoreArea.Width / 2.5f, scoreArea.Top - 2, scoreArea.Width / 2.5f, scoreArea.Height);

        FillRectangle(emmaPurple, scoreArea);

        DrawFitTextOneLine(Game.Player1Score.ToString(), font, Color.White, leftScoreArea, CenterRight);
        DrawFitTextOneLine(Game.Player2Score.ToString(), font, Color.White, rightScoreArea, CenterLeft);
        DrawFitTextOneLine("-", font, Color.White, new RectangleF(scoreArea.Left, scoreArea.Top + scoreArea.Height / 6, scoreArea.Width, scoreArea.Height * 3/4), CenterCenter);

        var leftTurnIndicator = new RectangleF(scoreArea.Left - head.Height * 1.2f, head.Top, head.Height - 8, head.Height - 8);
        var rightTurnIndicator = new RectangleF(scoreArea.Right + head.Height / 4 + 8, head.Top, head.Height - 8, head.Height - 8);

        var leftHeader = new RectangleF(2, head.Top, leftTurnIndicator.Left - head.Height / 4, head.Height - 4);
        var rightHeader = new RectangleF(rightTurnIndicator.Right + head.Height / 4, head.Top,
            head.Width - rightTurnIndicator.Right - head.Height / 4, head.Height - 4);

        DrawFitTextOneLine(Game.Player1Name.ToUpper(), font, Color.White, leftHeader, TopRight, Game.PlayerNumber == 1);
        DrawFitTextOneLine(Game.Player2Name.ToUpper(), font, Color.White, rightHeader, CenterLeft, Game.PlayerNumber == 2);

        Gfx.FillEllipse(Brushes.Tomato, leftTurnIndicator);
        Gfx.FillEllipse(Brushes.DodgerBlue, rightTurnIndicator);

        if (Game.PlayerNumber == 2)
        {
            rightTurnIndicator.Offset(-1, 3);
            DrawFitTextOneLine("◄", "Arial", Color.White, rightTurnIndicator, CenterCenter);
        }
        else
        {
            leftTurnIndicator.Offset(3, 3);
            DrawFitTextOneLine("►", "Arial", Color.White, leftTurnIndicator, CenterCenter);
        }

        float boardSize = Area.Height - head.Bottom - padding * 2;
        var boardArea = new RectangleF((Area.Width - boardSize) / 2, head.Bottom + padding, boardSize, boardSize);

        FillRectangle(Color.Black, boardArea);
        var boardMargin = boardArea.Width / 24;

        var playArea = new RectangleF(boardArea.Left + boardMargin, boardArea.Top + boardMargin, boardArea.Width - boardMargin * 2, boardArea.Height - boardMargin * 2);
        FillRectangle(Color.DarkGray, playArea);

        float spaceWidth = playArea.Width / BoardSize;
        
        for (int i = 0; i < BoardSize; i++)
        {
            string column = ((char)('A' + i)).ToString();
            string row = (i + 1).ToString();
            if (row.Length == 1) row = " " + row;

            var columnHeader = new RectangleF(playArea.Left + spaceWidth * i + 1, boardArea.Top - 2 + boardMargin / 8, spaceWidth, boardMargin * 7/8);
            DrawFitTextOneLine(column, font, Color.White, columnHeader, CenterCenter, true);

            var rowHeader = new RectangleF(boardArea.Left, playArea.Top + spaceWidth * i + spaceWidth / 4, playArea.Left - boardArea.Left, spaceWidth / 2);
            DrawFitTextOneLine(row, font, Color.White, rowHeader, CenterCenter, true);
        }

        RectangleF? activeArea = null;
        const int gridSize = 16;
        SpaceAreas = new RectangleF[BoardSize, BoardSize];

        for (int i = 0; i < BoardSize; i++)
        {
            for (int j = 0; j < BoardSize; j++)
            {
                var spaceArea = new RectangleF(playArea.Left + spaceWidth * j + spaceWidth / (gridSize * 2), playArea.Top + spaceWidth * i + spaceWidth / (gridSize * 2),
                    spaceWidth - spaceWidth / gridSize, spaceWidth - spaceWidth / gridSize);

                SpaceAreas[j, i] = spaceArea;

                if (Cursor.X == j && Cursor.Y == i)
                {
                    activeArea = spaceArea;
                }
                else if (GetTile(j, i) == null)
                {
                    var spaceColor = GetSpaceColor(j, i);
                    FillRectangle(spaceColor, spaceArea);
                }
            }
        }

        for (int i = 0; i < BoardSize; i++)
        {
            for (int j = 0; j < BoardSize; j++)
            {
                if (RuleSet.Board[j, i] != RuleSet.__)
                {
                    var spaceColor = GetSpaceColor(j, i);

                    float d = spaceWidth / 10;
                    float g = spaceWidth / (gridSize * 2);
                    var origin = new PointF(playArea.Left + spaceWidth * j, playArea.Top + spaceWidth * i);

                    if (j > 0)
                    {
                        var point1 = origin + new SizeF(-d - g, spaceWidth / 2);
                        var point2 = origin + new SizeF(-g, spaceWidth / 2 - d);
                        var point3 = origin + new SizeF(-g, spaceWidth / 2 + d);

                        Gfx.FillPolygon(GetBrush(spaceColor), new[] { point1, point2, point3 });
                    }

                    if (j < BoardSize - 1)
                    {
                        var point1 = origin + new SizeF(spaceWidth + d + g, spaceWidth / 2);
                        var point2 = origin + new SizeF(spaceWidth + g, spaceWidth / 2 - d);
                        var point3 = origin + new SizeF(spaceWidth + g, spaceWidth / 2 + d);

                        Gfx.FillPolygon(GetBrush(spaceColor), new[] { point1, point2, point3 });
                    }

                    if (i > 0)
                    {
                        var point1 = origin + new SizeF(spaceWidth / 2, -d - g);
                        var point2 = origin + new SizeF(spaceWidth / 2 - d, -g);
                        var point3 = origin + new SizeF(spaceWidth / 2 + d, -g);

                        Gfx.FillPolygon(GetBrush(spaceColor), new[] { point1, point2, point3 });
                    }

                    if (i < BoardSize - 1)
                    {
                        var point1 = origin + new SizeF(spaceWidth / 2, spaceWidth + d + g);
                        var point2 = origin + new SizeF(spaceWidth / 2 - d, spaceWidth + g);
                        var point3 = origin + new SizeF(spaceWidth / 2 + d, spaceWidth + g);

                        Gfx.FillPolygon(GetBrush(spaceColor), new[] { point1, point2, point3 });
                    }
                }
            }
        }

        var starArea = new RectangleF(playArea.Left + spaceWidth * RuleSet.Star.Item1 - 2f, 
            playArea.Top + spaceWidth * RuleSet.Star.Item2 - 3, spaceWidth * 1.2f, spaceWidth * 1.2f);
        DrawFitTextOneLine("«", "Wingdings", Color.FromArgb(193, 139, 148), starArea, CenterCenter);

        if (activeArea != null)
        {
            FillRectangle(Color.LimeGreen, activeArea.Value);

            char arrow = Vertical ? 'ê' : 'è';
            var arrowArea = new RectangleF(activeArea.Value.Left + 3, activeArea.Value.Top + 3, activeArea.Value.Width, activeArea.Value.Height);
            DrawFitTextOneLine(arrow.ToString(), "Wingdings", Color.Green, arrowArea, CenterCenter);
        }

        for (int i = 0; i < BoardSize; i++)
        {
            for (int j = 0; j < BoardSize; j++)
            {
                var tile = GetTile(j, i);

                if (tile != null)
                {
                    var position = new PointF(playArea.Left + spaceWidth * j, playArea.Top + spaceWidth * i);

                    DrawTile(tile, position.X, position.Y, spaceWidth, false);
                }
            }
        }

        (int startX, int startY) = Game.PlayEndPosition;
        (int endX, int endY) = Game.PlayEndPosition;

        if (Game.PlayScore > 0)
        {
            int scoreX = endX;
            int scoreY = endY;

            if (Game.VerticalPlay)
            {
                if (endY > 0 && Free(endX, endY - 1))
                {
                    scoreY = startY - 1;
                }
                else if (endX > 0 && Free(endX - 1, startY))
                {
                    scoreX--;
                    scoreY = startY;
                }
                else if (endX < BoardSize - 1 && Free(endX + 1, startY))
                {
                    scoreX++;
                    scoreY = startY;
                }
                else if (endX > 0 && Free(endX - 1, endY))
                {
                    scoreX--;
                }
                else if (endX < BoardSize - 1 && Free(endX + 1, endY))
                {
                    scoreX++;
                }
                else if (endY < BoardSize - 1 && Free(endX, endY + 1))
                {
                    scoreY++;
                }
            }
            else
            {
                if (endX > 0 && Free(endX - 1, endY))
                {
                    scoreX--;
                }
                else if (endY < BoardSize - 1 && Free(endX, endY + 1))
                {
                    scoreY++;
                }
                else if (endY < BoardSize - 1 && Free(startX, endY + 1))
                {
                    scoreX = startX;
                    scoreY++;
                }
                else if (endY > 0 && Free(endX, endY - 1))
                {
                    scoreY--;
                }
                else if (endY > 0 && Free(startX, endY - 1))
                {
                    scoreX = startX;
                    scoreY--;
                }
                else if (endX < BoardSize - 1 && Free(endX + 1, endY))
                {
                    scoreX++;
                }
            }

            var playScoreArea = new RectangleF(playArea.Left + spaceWidth * scoreX + spaceWidth / 6, playArea.Top + spaceWidth * scoreY + spaceWidth / 6,
                spaceWidth * 2/3, spaceWidth * 2/3);

            var backColor = Color.DarkRed;
            var foreColor = Color.Red;
            var textColor = Color.LightPink;

            if (Game.ValidPlay)
            {
                backColor = Color.DarkGreen;
                foreColor = Color.Lime;
                textColor = Color.LightGreen;
            }

            FillRectangle(backColor, playScoreArea);
            Gfx.DrawRectangle(GetPen(foreColor), playScoreArea);
            DrawFitTextOneLine(Game.PlayScore.ToString(), font, textColor, playScoreArea, CenterCenter);
        }

        var leftPanel = new RectangleF(padding, head.Bottom + padding, boardArea.Left - padding * 2, Area.Height - head.Bottom - padding * 2);
        
        var rightPanel = new RectangleF(boardArea.Right + padding, head.Bottom + padding, Area.Width - boardArea.Right - padding * 2, Area.Height - head.Bottom - padding * 2);

        var bagPanel = new RectangleF(leftPanel.Left, leftPanel.Top, leftPanel.Width, leftPanel.Height / 3 - padding);
        FillRectangle(Color.Black, bagPanel);

        string bagTitle = Game.Bag.Count > 0 ? $"UNSEEN TILES: {Game.Bag.Count}" : "OPPONENT'S TILES";
        var bagTitleArea = new RectangleF(bagPanel.Left, bagPanel.Top, bagPanel.Width, bagPanel.Height / 8);
        FillRectangle(emmaPurple, bagTitleArea);
        DrawFitTextOneLine(bagTitle, font, Color.White, bagTitleArea, CenterLeft);
        
        var bagContentArea = new RectangleF(bagPanel.Left, bagTitleArea.Bottom, bagPanel.Width, bagPanel.Height - bagTitleArea.Height);
        string remainingTiles = "";

        var unseen = new List<ScrabbleTile>(Game.Bag);
        unseen.AddRange(Game.PlayerNumber == 2 ? Game.Player1Rack : Game.Player2Rack);

        foreach (var group in unseen.GroupBy(tile => tile.Letter))
        {
            for (int i = 0; i < group.Count(); i += 12)
            {
                if (remainingTiles.Length > 0)
                {
                    remainingTiles += " ";
                }

                if (group.Key.Length == 1)
                {
                    remainingTiles += new string(group.Key[0], Math.Min(12, group.Count() - i));
                }
                else
                {
                    for (int j = 0; j < group.Count(); j++)
                    {
                        remainingTiles += "[" + group.Key + "] ";
                    }
                }
            }
        }

        DrawFitText(remainingTiles, "Consolas", Color.White, bagContentArea, TopLeft, false, bagPanel.Height / 10);

        var rackPanel = new RectangleF(leftPanel.Left, bagPanel.Bottom + padding, leftPanel.Width, leftPanel.Height / 3 - padding);
        FillRectangle(Color.Black, rackPanel);

        var rackTurnIndicator = new RectangleF(rackPanel.Left + padding / 2, rackPanel.Top + padding / 2, padding * 2.5f, padding * 2.5f);

        if (!Game.IsComplete)
        {
            Gfx.FillEllipse(Game.PlayerNumber == 2 ? Brushes.DodgerBlue : Brushes.Tomato, rackTurnIndicator);
        }

        string playerName = (Game.PlayerNumber == 2 ? Game.Player2Name : Game.Player1Name).ToUpper();

        if (Game.IsComplete)
        {
            playerName = "Game ended - ";

            if (Game.Player1Score == Game.Player2Score)
            {
                playerName += "tie";
            }
            else if (Game.Player1Score > Game.Player2Score)
            {
                playerName += Game.Player1Name + " wins";
            }
            else
            {
                playerName += Game.Player2Name + " wins";
            }
        }

        var rackNameArea = new RectangleF(rackPanel.Left + padding * 3.5f, rackPanel.Top + padding / 4, rackPanel.Width - padding * 4, padding * 3);
        DrawFitTextOneLine(playerName, font, Color.White, rackNameArea, CenterLeft);

        var tilePanel = new RectangleF(rackPanel.Left, rackTurnIndicator.Bottom + padding / 2, rackPanel.Width, rackPanel.Height - rackTurnIndicator.Height);

        float rackPadding = padding / 2;
        float tileSize = (tilePanel.Width - rackPadding * (RuleSet.RackSize + 1)) / RuleSet.RackSize;
        if (tileSize > spaceWidth) tileSize = spaceWidth;
        float actualTileSpace = tileSize * RuleSet.RackSize + rackPadding * (RuleSet.RackSize - 1);
        float rackMargin = (tilePanel.Width - actualTileSpace) / 2;
        tilePanel.Height /= 2;

        float rackX = tilePanel.Left + rackMargin;
        float rackY = tilePanel.Top + rackPadding;

        for (int i = 0; i < Game.CurrentRack.Count; i++)
        {
            DrawTile(Game.CurrentRack[i], rackX, rackY, tileSize, true);
            rackX += tileSize + rackPadding;
        }

        var buttonPanel = new RectangleF(tilePanel.Left, tilePanel.Bottom, tilePanel.Width, rackPanel.Bottom - tilePanel.Bottom);
        var buttonPadding = padding / 2;

        ButtonActions.Clear();

        var button1 = new RectangleF(buttonPanel.Left + buttonPadding, buttonPanel.Top, (buttonPanel.Width - buttonPadding * 3) / 2, buttonPanel.Height / 2 - buttonPadding);
        FillRectangle(emmaPurple, button1);
        DrawFitTextOneLine("SHUFFLE", font, Color.White, button1, CenterCenter);
        ButtonActions["shuffle"] = button1;

        var button2 = new RectangleF(button1.Right + buttonPadding, button1.Top, button1.Width, button1.Height);
        FillRectangle(emmaPurple, button2);
        DrawFitTextOneLine("RECALL", font, Color.White, button2, CenterCenter);
        ButtonActions["recall"] = button2;

        var button3 = new RectangleF(button1.Left, button1.Bottom + buttonPadding, button1.Width, button1.Height);
        FillRectangle(emmaPurple, button3);
        DrawFitTextOneLine("PASS", font, Color.White, button3, CenterCenter);
        ButtonActions["pass"] = button3;

        var button4 = new RectangleF(button3.Right + buttonPadding, button1.Bottom + buttonPadding, button1.Width, button1.Height);
        FillRectangle(emmaPurple, button4);
        DrawFitTextOneLine("EXCHANGE", font, Color.White, button4, CenterCenter);
        ButtonActions["exchange"] = button4;

        var cameraPanel = new RectangleF(leftPanel.Left, rackPanel.Bottom + padding, leftPanel.Width, leftPanel.Height / 3);
        FillRectangle(Color.Black, cameraPanel);

        var titlePanel = new RectangleF(rightPanel.Left, rightPanel.Top, rightPanel.Width, rightPanel.Width / 6);
        FillRectangle(emmaPurple, titlePanel);

        Gfx.DrawImage(EmmaImage, titlePanel.Left + padding / 2, titlePanel.Top + padding / 2, titlePanel.Height * 3/4, titlePanel.Height * 3/4);

        var titleTextArea = new RectangleF(titlePanel.Left + titlePanel.Height, titlePanel.Top, titlePanel.Width - titlePanel.Height, titlePanel.Height);
        DrawFitTextOneLine("scrabble with emma", font, Color.White, titleTextArea, CenterCenter);

        var rulesPanel = new RectangleF(rightPanel.Left, titlePanel.Bottom + padding, rightPanel.Width, rightPanel.Height / 3 - titlePanel.Height - padding * 2);
        FillRectangle(Color.Black, rulesPanel);
        var rulesTitleArea = new RectangleF(rulesPanel.Left, rulesPanel.Top, rulesPanel.Width, bagTitleArea.Height);
        FillRectangle(emmaPurple, rulesTitleArea);
        DrawFitTextOneLine("GAME RULES", font, Color.White, rulesTitleArea, CenterLeft);

        var rulesContentArea = new RectangleF(rulesPanel.Left, rulesTitleArea.Bottom, rulesPanel.Width, rulesPanel.Height - rulesTitleArea.Height);

        var rules = new List<string>
        {
            "Lexicon: " + Lexicon.Name
        };

        if (RuleSet.ValidateWords)
        {
            rules.Add("Word validation");
        }
        else
        {
            rules.Add("No word validation");
        }

        if (RuleSet.Name != "Scrabble")
        {
            rules.Add(RuleSet.Description);
        }

        DrawFitText(string.Join("\r\n", rules), font, Color.White, rulesContentArea, TopLeft, false, rulesContentArea.Height / 6);

        var playsPanel = new RectangleF(rightPanel.Left, rulesPanel.Bottom + padding, rightPanel.Width, rightPanel.Height - rulesPanel.Height - titlePanel.Height - padding * 2);
        FillRectangle(Color.Black, playsPanel);

        const string playsTitle = "PLAY HISTORY";
        var playTitleArea = new RectangleF(playsPanel.Left, playsPanel.Top, playsPanel.Width, bagTitleArea.Height);
        FillRectangle(emmaPurple, playTitleArea);
        DrawFitTextOneLine(playsTitle, font, Color.White, playTitleArea, CenterLeft);

        // PLAY CONTROLS
        var controlsArea = new RectangleF(playsPanel.Left, playsPanel.Bottom - button1.Height - buttonPadding, playsPanel.Width, button1.Height + buttonPadding);

        /*var playButton1 = new RectangleF(controlsArea.Left + buttonPadding, controlsArea.Top, (controlsArea.Width - buttonPadding * 5) / 4, controlsArea.Height - buttonPadding);
        FillRectangle(emmaPurple, playButton1);
        DrawFitTextOneLine("<<", font, Color.White, playButton1, CenterCenter);
        ButtonActions["first"] = playButton1;

        var playButton2 = new RectangleF(playButton1.Right + buttonPadding, controlsArea.Top, playButton1.Width, playButton1.Height);
        FillRectangle(emmaPurple, playButton2);
        DrawFitTextOneLine("<", font, Color.White, playButton2, CenterCenter);
        ButtonActions["prev"] = playButton2;

        var playButton3 = new RectangleF(playButton2.Right + buttonPadding, controlsArea.Top, playButton1.Width, playButton1.Height);
        FillRectangle(emmaPurple, playButton3);
        DrawFitTextOneLine(">", font, Color.White, playButton3, CenterCenter);
        ButtonActions["next"] = playButton3;

        var playButton4 = new RectangleF(playButton3.Right + buttonPadding, controlsArea.Top, playButton1.Width, playButton1.Height);
        FillRectangle(emmaPurple, playButton4);
        DrawFitTextOneLine(">>", font, Color.White, playButton4, CenterCenter);
        ButtonActions["last"] = playButton4;*/

        var playLogArea = new RectangleF(playsPanel.Left, playTitleArea.Bottom, playsPanel.Width, playsPanel.Height - playTitleArea.Height - controlsArea.Height);
        float playHeight = Math.Max(playLogArea.Height / 20, 16);
        int playCapacity = (int)(playLogArea.Height / playHeight);

        var playsToShow = Game.Plays.Skip(Game.Plays.Count - playCapacity).ToList();
        float y = playLogArea.Top;

        foreach (var play in playsToShow)
        {
            var playTurnIndicator = new RectangleF(playLogArea.Left + playHeight / 4, y + playHeight / 8, playHeight * 3/4, playHeight * 3/4);
            Gfx.FillEllipse(play.PlayerNumber == 2 ? Brushes.DodgerBlue : Brushes.Tomato, playTurnIndicator);

            var playScoreArea = new RectangleF(playLogArea.Right - playLogArea.Width / 5, y, playLogArea.Width / 5, playHeight);
            DrawFitTextOneLine(play.Score.ToString(), font, Color.White, playScoreArea, CenterRight, true);

            var playTextArea = new RectangleF(playTurnIndicator.Right + playHeight / 4, y, playScoreArea.Left - playTurnIndicator.Right - playHeight, playHeight);
            DrawFitTextOneLine(play.PlayString, "Consolas", Color.White, playTextArea, CenterLeft);

            y += playHeight;
        }
    }


    private void DrawTile(ScrabbleTile tile, float x, float y, float size, bool rack)
    {
        if (rack && tile.Uncommitted)
        {
            return;
        }

        var baseColor = Color.FromArgb(255, 239, 213);
        var accentColor1 = Color.FromArgb(242, 214, 169);
        var accentColor2 = Color.FromArgb(216, 179, 119);

        if (!rack && tile.Player == 1)
        {
            baseColor = Color.FromArgb(255, 216, 213);
            accentColor1 = Color.FromArgb(242, 174, 169);
            accentColor2 = Color.FromArgb(216, 126, 119);
        }

        if (!rack && tile.Player == 2)
        {
            baseColor = Color.FromArgb(213, 239, 255);
            accentColor1 = Color.FromArgb(169, 214, 242);
            accentColor2 = Color.FromArgb(119, 179, 216);
        }

        FillRectangle(baseColor, new RectangleF(x, y, size, size));
        FillRectangle(accentColor1, new RectangleF(x, y + size - (size / 12), size, size / 12));
        FillRectangle(accentColor2, new RectangleF(x + size - (size / 12), y, size / 12, size));

        string points = tile.Points.ToString();

        if (tile.Designation != " ")
        {
            DrawFitTextOneLine(tile.Designation.ToString(), "Gill Sans MT", Color.Red,
                new RectangleF(x - size / 20, y - size / 20, size * 1.1f, size * 1.1f), CenterCenter);
        }
        else
        {
            DrawFitTextOneLine(tile.Display.ToString(), "Gill Sans MT", Color.Black,
                new RectangleF(x - size / 20, y - size / 20, size * 1.1f, size * 1.1f), CenterCenter);
        }

        if (points != "0")
        {
            DrawFitTextOneLine(points, "Segoe UI", Color.Black,
                new RectangleF(x + size * 0.6f, y + size * 0.55f, size * 0.4f, size * 0.4f), BottomRight);
        }
    }


    private Color GetSpaceColor(int j, int i)
    {
        return RuleSet.Board[j, i] switch
        {
            RuleSet.DW => Color.LightPink,
            RuleSet.TW => Color.Tomato,
            RuleSet.DL => Color.FromArgb(255, 170, 210, 255),
            RuleSet.TL => Color.DodgerBlue,
            _ => Color.Ivory
        };
    }


    private void Recall()
    {
        Cursor = (-1, -1);

        for (int i = 0; i < BoardSize; i++)
        {
            for (int j = 0; j < BoardSize; j++)
            {
                if (GetTile(j, i) is ScrabbleTile tile && tile.Uncommitted)
                {
                    tile.Uncommitted = false;
                    tile.Designation = " ";
                    tile.Points = RuleSet.TilePoints.GetValueOrDefault(tile.Letter, 0);
                    SetTile(j, i, null);
                }
            }
        }

        Game.CheckPlay();
    }


    private void Commit()
    {
        if (Game.ValidPlay)
        {
            Cursor = (-1, -1);
            Game.NextTurn();
        }
    }


    public override void HandleKey(KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Enter)
        {
            Commit();
        }
        else if (e.KeyCode == Keys.Escape)
        {
            Recall();
        }
        else if (e.KeyCode == Keys.Tab)
        {
            Vertical = !Vertical;
        }
        else if (e.KeyCode == Keys.Space)
        {
            CursorForward();
        }
        else if (e.KeyCode == Keys.Back)
        {
            Backspace();
        }
        else if (e.KeyCode == Keys.Up)
        {
            CursorUp();
        }
        else if (e.KeyCode == Keys.Down)
        {
            if (Cursor.Y < BoardSize - 1)
            {
                CursorDown();
            }
        }
        else if (e.KeyCode == Keys.Left)
        {
            CursorLeft();
        }
        else if (e.KeyCode == Keys.Right)
        {
            if (Cursor.X < BoardSize - 1)
            {
                CursorRight();
            }
        }
        else if (e.KeyCode.ToString().Length == 1)
        {
            char letter = e.KeyCode.ToString().First();

            if (letter >= 'A' && letter <= 'Z' && RuleSet.TileDistribution.ContainsKey(letter.ToString()) && 
                Cursor.X > -1 && Cursor.X < BoardSize && Cursor.Y > -1 && Cursor.Y < BoardSize)
            {
                if (Free(Cursor.X, Cursor.Y))
                {
                    PlayLetter(letter.ToString());
                }
            }
        }
        else if (e.KeyCode.ToString().StartsWith("D") && e.KeyCode.ToString().Length == 2)
        {
            if (Free(Cursor.X, Cursor.Y))
            {
                int idx = int.Parse(e.KeyCode.ToString().Substring(1, 1)) - 1;

                if (Game.CurrentRack.Count > idx && idx >= 0 && Game.CurrentRack[idx] != null && !Game.CurrentRack[idx].Uncommitted)
                {
                    var tile = Game.CurrentRack[idx];

                    if (tile.Letter != "?")
                    {
                        PlayTile(tile);
                    }
                }
            }
        }
    }


    private void Backspace()
    {
        if (Vertical)
        {
            if (Cursor.Y > 0)
            {
                Cursor.Y--;
            }

            while (Cursor.Y >= 0 && Cursor.Y < BoardSize && !Free(Cursor.X, Cursor.Y))
            {
                if (GetTile(Cursor.X, Cursor.Y) is ScrabbleTile tile && tile.Uncommitted)
                {
                    SetTile(Cursor.X, Cursor.Y, null);
                    tile.Designation = " ";
                    tile.Points = RuleSet.TilePoints.GetValueOrDefault(tile.Letter, 0);
                    tile.Uncommitted = false;
                    Game.CheckPlay();
                    break;
                }

                if (Cursor.Y == 0)
                {
                    break;
                }

                Cursor.Y--;
            }
        }
        else
        {
            if (Cursor.X > 0)
            {
                Cursor.X--;
            }

            while (Cursor.X >= 0 && Cursor.X < BoardSize && !Free(Cursor.X, Cursor.Y))
            {
                if (GetTile(Cursor.X, Cursor.Y) is ScrabbleTile tile && tile.Uncommitted)
                {
                    SetTile(Cursor.X, Cursor.Y, null);
                    tile.Designation = " ";
                    tile.Points = RuleSet.TilePoints.GetValueOrDefault(tile.Letter, 0);
                    tile.Uncommitted = false;
                    Game.CheckPlay();
                    break;
                }

                if (Cursor.X == 0)
                {
                    break;
                }

                Cursor.X--;
            }
        }
    }


    private void PlayTile(ScrabbleTile tile)
    {
        if (Free(Cursor.X, Cursor.Y))
        {
            tile.Uncommitted = true;
            SetTile(Cursor.X, Cursor.Y, tile);
            Game.CheckPlay();
            CursorForward();
        }
    }


    private bool PlayLetter(string letter)
    {
        ScrabbleTile? tileToPlay = null;

        for (int t = 0; t < Game.CurrentRack.Count; t++)
        {
            var tile = Game.CurrentRack[t];

            if (tile.Letter == letter && !tile.Uncommitted)
            {
                tileToPlay = tile;
                tile.Designation = " ";
                tile.Points = RuleSet.TilePoints.GetValueOrDefault(tile.Letter, tileToPlay.Points);
                break;
            }
        }

        if (tileToPlay == null)
        {
            for (int t = 0; t < Game.CurrentRack.Count; t++)
            {
                var tile = Game.CurrentRack[t];

                if (tile.Letter == "?" && !tile.Uncommitted)
                {
                    tileToPlay = tile;
                    tile.Designation = letter;
                    tile.Points = RuleSet.TilePoints.GetValueOrDefault(tile.Letter, tileToPlay.Points);
                    break;
                }
            }
        }

        bool flipped = Game.CurrentRack.Any(tile => tile.Letter != "?" && tile.Designation != " ");
        bool flipAllowed = RuleSet.IfOnlyVariant && !Game.CurrentRack.Any(tile => tile.Letter == "?");

        if (tileToPlay == null && flipAllowed && !flipped)
        {
            tileToPlay = Game.CurrentRack.FirstOrDefault(t => !t.Uncommitted);

            if (tileToPlay != null)
            {
                tileToPlay.Designation = letter;
                tileToPlay.Points = RuleSet.TilePoints.GetValueOrDefault("?", 0);
            }
        }

        if (tileToPlay == null && flipAllowed && flipped)
        {
            foreach (var tile in Game.CurrentRack)
            {
                if (tile.Uncommitted && tile.Designation != " " && tile.Letter == letter)
                {
                    var alternativeTile = Game.CurrentRack.First(t => !t.Uncommitted);

                    if (alternativeTile != null)
                    {
                        alternativeTile.Uncommitted = true;
                        alternativeTile.Designation = tile.Designation;
                        alternativeTile.Points = RuleSet.TilePoints.GetValueOrDefault("?", 0);

                        for (int i = 0; i < BoardSize; i++)
                        {
                            for (int j = 0; j < BoardSize; j++)
                            {
                                if (GetTile(j, i) is ScrabbleTile t && t.Equals(tile))
                                {
                                    SetTile(j, i, alternativeTile);
                                    break;
                                }
                            }
                        }

                        tileToPlay = tile;
                        tileToPlay.Designation = " ";
                        tileToPlay.Points = RuleSet.TilePoints.GetValueOrDefault(tileToPlay.Letter, tileToPlay.Points);
                        break;
                    }
                }
            }
        }

        if (tileToPlay != null)
        {
            PlayTile(tileToPlay);
            return true;
        }

        return false;
    }


    private void CursorLeft()
    {
        if (Cursor.X > 0)
        {
            Cursor.X--;

            if (Cursor.Y < 0)
            {
                Cursor.Y = 0;
            }
        }

        while (Cursor.X > 0 && !Free(Cursor.X, Cursor.Y))
        {
            Cursor.X--;
        }
    }


    private void CursorRight()
    {
        if (Cursor.X < BoardSize)
        {
            Cursor.X++;

            if (Cursor.Y < 0)
            {
                Cursor.Y = 0;
            }
        }

        while (Cursor.X < BoardSize && !Free(Cursor.X, Cursor.Y))
        {
            Cursor.X++;
        }
    }


    private void CursorUp()
    {
        if (Cursor.Y > 0)
        {
            Cursor.Y--;

            if (Cursor.X < 0)
            {
                Cursor.X = 0;
            }
        }

        while (Cursor.Y > 0 && !Free(Cursor.X, Cursor.Y))
        {
            Cursor.Y--;
        }
    }


    private void CursorDown()
    {
        if (Cursor.Y < BoardSize)
        {
            Cursor.Y++;

            if (Cursor.X < 0)
            {
                Cursor.X = 0;
            }
        }

        while (Cursor.Y < BoardSize && !Free(Cursor.X, Cursor.Y))
        {
            Cursor.Y++;
        }
    }


    private void CursorForward()
    {
        if (Vertical)
        {
            CursorDown();
        }
        else
        {
            CursorRight();
        }
    }


    public override void HandleMouse(MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
        {
            if (SpaceAreas.Length == 0)
            {
                return;
            }

            for (int i = 0; i < BoardSize; i++)
            {
                for (int j = 0; j < BoardSize; j++)
                {
                    var rect = SpaceAreas[j, i];

                    if (e.X > rect.Left && e.X < rect.Right && e.Y > rect.Top && e.Y < rect.Bottom)
                    {
                        TileClick(j, i);
                    }
                }
            }

            foreach (var item in ButtonActions)
            {
                if (e.X > item.Value.Left && e.X < item.Value.Right && e.Y > item.Value.Top && e.Y < item.Value.Bottom)
                {
                    switch (item.Key)
                    {
                        case "shuffle":
                            Shuffle(Game.CurrentRack);
                            break;

                        case "recall":
                            Recall();
                            break;

                        case "pass":
                            Game.Pass();
                            break;

                        case "exchange":
                            if (Game.Bag.Count >= RuleSet.RackSize && Game.CurrentRack.Any(t => t.Uncommitted))
                            {
                                var tilesToExchange = Game.CurrentRack.Where(t => t.Uncommitted).ToList();
                                Game.Exchange(tilesToExchange);
                                Game.NextTurn();
                            }
                            break;
                    }
                }
            }
        }
    }


    public string? PlayWord(string x, string y, string word, bool vertical)
    {
        Recall();
        
        string? result = TryPlayWord(x, y, word, vertical);
    
        if (result != null)
        {
            Recall();
        }

        return result;
    }


    public string? PlayAndCommitWord(string x, string y, string word, bool vertical)
    {
        Recall();

        string? result = TryPlayWord(x, y, word, vertical);

        if (result != null)
        {
            Recall();
        }

        Commit();
        return result;
    }

    
    public string? TryPlayWord(string x, string y, string word, bool vertical)
    {
        int j = x[0] - 'A';
        int i = int.Parse(y) - 1;

        for (int c = 0; c < word.Length; c++)
        {
            string letter = word[c].ToString();

            if (j < 0 || j > BoardSize - 1 || i < 0 || i > BoardSize - 1)
            {
                return "Cannot make that play - it is off the board";
            }

            Cursor = (j, i);
            Vertical = vertical;

            if (Free(j, i))
            {
                if (!PlayLetter(letter))
                {
                    return $"Cannot make that play - no '{letter}' available in position " + c;
                }
            }
            else if (GetTile(j, i) is ScrabbleTile tile && tile.Letter != letter)
            {
                return $"Cannot make that play - cannot play '{letter}' in position {c} occupied by '{tile.Letter}'";
            }

            if (vertical)
            {
                i++;
            }
            else
            {
                j++;
            }
        }

        if (!Game.ValidPlay)
        {
            return "Cannot make that play - " + Game.InvalidReason;
        }

        return null;
    }


    private void TileClick(int j, int i)
    {
        if (Cursor == (j, i))
        {
            Vertical = !Vertical;
        }
        else if (Free(j, i))
        {
            Cursor = (j, i);
        }
        else if (Vertical && (i == BoardSize - 1 || Free(j, i + 1)))
        {
            Cursor = (j, i + 1);
        }
        else if (!Vertical && (j == BoardSize - 1 || Free(j + 1, i)))
        {
            Cursor = (j + 1, i);
        }
    }



    private int BoardSize => RuleSet.BoardSize;
    private bool Free(int j, int i) => j >= 0 && i >= 0 && j < BoardSize && i < BoardSize && Game.Board[j, i] == null;
    private ScrabbleTile? GetTile(int j, int i) => Game.Board[j, i];
    private void SetTile(int j, int i, ScrabbleTile? tile) => Game.Board[j, i] = tile;


    private void Shuffle<T>(IList<T> list)
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
