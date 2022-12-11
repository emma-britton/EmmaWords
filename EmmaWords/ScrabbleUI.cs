using TwitchLib.Client.Models;

namespace EmmaWords;

class ScrabbleUI : IGameUI
{
    private readonly ScrabbleGame Game;
    private readonly WordService Service;
    
    public RuleSet RuleSet { get; set; }

    private RectangleF[,] SpaceAreas;
    private (int X, int Y) Cursor = (-1, -1);
    private bool Vertical;
    private readonly Dictionary<string, RectangleF> ButtonActions = new();
    private readonly Image EmmaImage = new Bitmap(@"C:\Users\huggl\streaming\points\Wave_112.png");


    public ScrabbleUI(ScrabbleGame game, WordService service, RuleSet ruleSet)
    {
        Game = game;
        Service = service;
        RuleSet = ruleSet;
        SpaceAreas = new RectangleF[0, 0];
    }


    public void HandleMessage(ChatMessage message)
    {

    }


    public void Render(GdiAnimation animation, Graphics gfx, Rectangle area)
    {
        const string font = "Segoe UI";

        var purpleBrush = animation.GetBrush(Color.FromArgb(145, 71, 255));

        var topPad = new RectangleF(0, 0, area.Width, 8);
        gfx.FillRectangle(Brushes.Black, topPad);
        var header = new RectangleF(0, topPad.Bottom, area.Width, area.Height / 18);
        gfx.FillRectangle(Brushes.Black, header);

        float padding = header.Height / 3;

        var scoreArea = new RectangleF(header.Width / 2 - header.Width / 14, 0, header.Width / 7, header.Height + header.Top);
        var leftScoreArea = new RectangleF(scoreArea.Left, scoreArea.Top - 2, scoreArea.Width / 2.5f, scoreArea.Height);
        var rightScoreArea = new RectangleF(scoreArea.Right - scoreArea.Width / 2.5f, scoreArea.Top - 2, scoreArea.Width / 2.5f, scoreArea.Height);

        gfx.FillRectangle(purpleBrush, scoreArea);

        animation.DrawFitTextOneLine(Game.Player1Score.ToString(), font, Color.White, leftScoreArea, animation.CenterRight);
        animation.DrawFitTextOneLine(Game.Player2Score.ToString(), font, Color.White, rightScoreArea, animation.CenterLeft);
        animation.DrawFitTextOneLine("-", font, Color.White, new RectangleF(scoreArea.Left, scoreArea.Top + scoreArea.Height / 6, scoreArea.Width, scoreArea.Height * 3/4), animation.CenterCenter);

        var leftTurnIndicator = new RectangleF(scoreArea.Left - header.Height * 1.2f, header.Top, header.Height - 8, header.Height - 8);
        var rightTurnIndicator = new RectangleF(scoreArea.Right + header.Height / 4 + 8, header.Top, header.Height - 8, header.Height - 8);

        var leftHeader = new RectangleF(2, header.Top, leftTurnIndicator.Left - header.Height / 4, header.Height - 4);
        var rightHeader = new RectangleF(rightTurnIndicator.Right + header.Height / 4, header.Top,
            header.Width - rightTurnIndicator.Right - header.Height / 4, header.Height - 4);

        animation.DrawFitTextOneLine(RuleSet.Player1Name.ToUpper(), font, Color.White, leftHeader, animation.TopRight, !Game.Turn);
        animation.DrawFitTextOneLine(RuleSet.Player2Name.ToUpper(), font, Color.White, rightHeader, animation.CenterLeft, Game.Turn);

        gfx.FillEllipse(Brushes.Tomato, leftTurnIndicator);
        gfx.FillEllipse(Brushes.DodgerBlue, rightTurnIndicator);

        if (Game.Turn)
        {
            rightTurnIndicator.Offset(-1, 3);
            animation.DrawFitTextOneLine("◄", "Arial", Color.White, rightTurnIndicator, animation.CenterCenter);
        }
        else
        {
            leftTurnIndicator.Offset(3, 3);
            animation.DrawFitTextOneLine("►", "Arial", Color.White, leftTurnIndicator, animation.CenterCenter);
        }

        float boardSize = area.Height - header.Bottom - padding * 2;
        var boardArea = new RectangleF((area.Width - boardSize) / 2, header.Bottom + padding, boardSize, boardSize);

        gfx.FillRectangle(Brushes.Black, boardArea);
        var boardMargin = boardArea.Width / 24;

        var playArea = new RectangleF(boardArea.Left + boardMargin, boardArea.Top + boardMargin, boardArea.Width - boardMargin * 2, boardArea.Height - boardMargin * 2);
        gfx.FillRectangle(Brushes.DarkGray, playArea);

        float spaceWidth = playArea.Width / BoardSize;
        
        for (int i = 0; i < BoardSize; i++)
        {
            string column = ((char)('A' + i)).ToString();
            string row = (i + 1).ToString();
            if (row.Length == 1) row = " " + row;

            var columnHeader = new RectangleF(playArea.Left + spaceWidth * i + 1, boardArea.Top - 2 + boardMargin / 8, spaceWidth, boardMargin * 7/8);
            animation.DrawFitTextOneLine(column, font, Color.White, columnHeader, animation.CenterCenter, true);

            var rowHeader = new RectangleF(boardArea.Left, playArea.Top + spaceWidth * i + spaceWidth / 4, playArea.Left - boardArea.Left, spaceWidth / 2);
            animation.DrawFitTextOneLine(row, font, Color.White, rowHeader, animation.CenterCenter, true);
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

                var spaceBrush = GetSpaceBrush(animation, j, i);

                if (Cursor.X == j && Cursor.Y == i)
                {
                    activeArea = spaceArea;
                }
                else if (GetTile(j, i) == null)
                {
                    gfx.FillRectangle(spaceBrush, spaceArea);
                }
            }
        }

        for (int i = 0; i < BoardSize; i++)
        {
            for (int j = 0; j < BoardSize; j++)
            {
                if (RuleSet.Board[j, i] != RuleSet.nn)
                {
                    var spaceBrush = GetSpaceBrush(animation, j, i);

                    float d = spaceWidth / 10;
                    float g = spaceWidth / (gridSize * 2);
                    var origin = new PointF(playArea.Left + spaceWidth * j, playArea.Top + spaceWidth * i);

                    if (j > 0)
                    {
                        var point1 = origin + new SizeF(-d - g, spaceWidth / 2);
                        var point2 = origin + new SizeF(-g, spaceWidth / 2 - d);
                        var point3 = origin + new SizeF(-g, spaceWidth / 2 + d);

                        gfx.FillPolygon(spaceBrush, new[] { point1, point2, point3 });
                    }

                    if (j < BoardSize - 1)
                    {
                        var point1 = origin + new SizeF(spaceWidth + d + g, spaceWidth / 2);
                        var point2 = origin + new SizeF(spaceWidth + g, spaceWidth / 2 - d);
                        var point3 = origin + new SizeF(spaceWidth + g, spaceWidth / 2 + d);

                        gfx.FillPolygon(spaceBrush, new[] { point1, point2, point3 });
                    }

                    if (i > 0)
                    {
                        var point1 = origin + new SizeF(spaceWidth / 2, -d - g);
                        var point2 = origin + new SizeF(spaceWidth / 2 - d, -g);
                        var point3 = origin + new SizeF(spaceWidth / 2 + d, -g);

                        gfx.FillPolygon(spaceBrush, new[] { point1, point2, point3 });
                    }

                    if (i < BoardSize - 1)
                    {
                        var point1 = origin + new SizeF(spaceWidth / 2, spaceWidth + d + g);
                        var point2 = origin + new SizeF(spaceWidth / 2 - d, spaceWidth + g);
                        var point3 = origin + new SizeF(spaceWidth / 2 + d, spaceWidth + g);

                        gfx.FillPolygon(spaceBrush, new[] { point1, point2, point3 });
                    }
                }
            }
        }

        var starArea = new RectangleF(playArea.Left + spaceWidth * RuleSet.Star.Item1 - 2f, 
            playArea.Top + spaceWidth * RuleSet.Star.Item2 - 3, spaceWidth * 1.2f, spaceWidth * 1.2f);
        animation.DrawFitTextOneLine("«", "Wingdings", Color.FromArgb(193, 139, 148), starArea, animation.CenterCenter);

        if (activeArea != null)
        {
            gfx.FillRectangle(Brushes.LimeGreen, activeArea.Value);

            char arrow = Vertical ? 'ê' : 'è';
            var arrowArea = new RectangleF(activeArea.Value.Left + 3, activeArea.Value.Top + 3, activeArea.Value.Width, activeArea.Value.Height);
            animation.DrawFitTextOneLine(arrow.ToString(), "Wingdings", Color.Green, arrowArea, animation.CenterCenter);
        }

        for (int i = 0; i < BoardSize; i++)
        {
            for (int j = 0; j < BoardSize; j++)
            {
                var tile = GetTile(j, i);

                if (tile != null)
                {
                    var position = new PointF(playArea.Left + spaceWidth * j, playArea.Top + spaceWidth * i);

                    DrawTile(gfx, animation, tile, position.X, position.Y, spaceWidth, false);
                }
            }
        }

        (int startX, int startY) = Game.PlayEndPosition;
        (int endX, int endY) = Game.PlayEndPosition;

        if (Game.PlayScore > 0 && Game.IsViewCurrent)
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

            gfx.FillRectangle(animation.GetBrush(backColor), playScoreArea);
            gfx.DrawRectangle(animation.GetPen(foreColor), playScoreArea);
            animation.DrawFitTextOneLine(Game.PlayScore.ToString(), font, textColor, playScoreArea, animation.CenterCenter);
        }

        var leftPanel = new RectangleF(padding, header.Bottom + padding, boardArea.Left - padding * 2, area.Height - header.Bottom - padding * 2);
        
        var rightPanel = new RectangleF(boardArea.Right + padding, header.Bottom + padding, area.Width - boardArea.Right - padding * 2, area.Height - header.Bottom - padding * 2);

        var bagPanel = new RectangleF(leftPanel.Left, leftPanel.Top, leftPanel.Width, leftPanel.Height / 3 - padding);
        gfx.FillRectangle(Brushes.Black, bagPanel);

        string bagTitle = Game.Bag.Count > 0 ? $"UNSEEN TILES: {Game.Bag.Count}" : "OPPONENT'S TILES";
        var bagTitleArea = new RectangleF(bagPanel.Left, bagPanel.Top, bagPanel.Width, bagPanel.Height / 8);
        gfx.FillRectangle(purpleBrush, bagTitleArea);
        animation.DrawFitTextOneLine(bagTitle, font, Color.White, bagTitleArea, animation.CenterLeft);
        
        var bagContentArea = new RectangleF(bagPanel.Left, bagTitleArea.Bottom, bagPanel.Width, bagPanel.Height - bagTitleArea.Height);
        string remainingTiles = "";

        var unseen = new List<ScrabbleTile>(Game.Bag);
        unseen.AddRange(Game.Turn ? Game.Player1Rack : Game.Player2Rack);

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

        animation.DrawFitText(remainingTiles, "Consolas", Color.White, bagContentArea, animation.TopLeft, false, bagPanel.Height / 10);

        var rackPanel = new RectangleF(leftPanel.Left, bagPanel.Bottom + padding, leftPanel.Width, leftPanel.Height / 3 - padding);
        gfx.FillRectangle(Brushes.Black, rackPanel);

        var rackTurnIndicator = new RectangleF(rackPanel.Left + padding / 2, rackPanel.Top + padding / 2, padding * 2.5f, padding * 2.5f);

        if (!Game.Ended)
        {
            gfx.FillEllipse(Game.Turn ? Brushes.DodgerBlue : Brushes.Tomato, rackTurnIndicator);
        }

        string playerName = (Game.Turn ? RuleSet.Player2Name : RuleSet.Player1Name).ToUpper();

        if (Game.Ended)
        {
            playerName = "Game ended - ";

            if (Game.Player1Score == Game.Player2Score)
            {
                playerName += "tie";
            }
            else if (Game.Player1Score > Game.Player2Score)
            {
                playerName += RuleSet.Player1Name + " wins";
            }
            else
            {
                playerName += RuleSet.Player2Name + " wins";
            }
        }

        var rackNameArea = new RectangleF(rackPanel.Left + padding * 3.5f, rackPanel.Top + padding / 4, rackPanel.Width - padding * 4, padding * 3);
        animation.DrawFitTextOneLine(playerName, font, Color.White, rackNameArea, animation.CenterLeft);

        var tilePanel = new RectangleF(rackPanel.Left, rackTurnIndicator.Bottom + padding / 2, rackPanel.Width, rackPanel.Height - rackTurnIndicator.Height);

        float rackPadding = padding / 2;
        float tileSize = (tilePanel.Width - rackPadding * (RuleSet.RackSize + 1)) / RuleSet.RackSize;
        if (tileSize > spaceWidth) tileSize = spaceWidth;
        float actualTileSpace = tileSize * RuleSet.RackSize + rackPadding * (RuleSet.RackSize - 1);
        float rackMargin = (tilePanel.Width - actualTileSpace) / 2;
        tilePanel.Height = tilePanel.Height / 2;

        float rackX = tilePanel.Left + rackMargin;
        float rackY = tilePanel.Top + rackPadding;

        for (int i = 0; i < Game.CurrentRack.Count; i++)
        {
            DrawTile(gfx, animation, Game.CurrentRack[i], rackX, rackY, tileSize, true);
            rackX += tileSize + rackPadding;
        }

        var buttonPanel = new RectangleF(tilePanel.Left, tilePanel.Bottom, tilePanel.Width, rackPanel.Bottom - tilePanel.Bottom);
        var buttonPadding = padding / 2;

        ButtonActions.Clear();

        var button1 = new RectangleF(buttonPanel.Left + buttonPadding, buttonPanel.Top, (buttonPanel.Width - buttonPadding * 3) / 2, buttonPanel.Height / 2 - buttonPadding);
        gfx.FillRectangle(purpleBrush, button1);
        animation.DrawFitTextOneLine("SHUFFLE", font, Color.White, button1, animation.CenterCenter);
        ButtonActions["shuffle"] = button1;

        var button2 = new RectangleF(button1.Right + buttonPadding, button1.Top, button1.Width, button1.Height);
        gfx.FillRectangle(purpleBrush, button2);
        animation.DrawFitTextOneLine("RECALL", font, Color.White, button2, animation.CenterCenter);
        ButtonActions["recall"] = button2;

        var button3 = new RectangleF(button1.Left, button1.Bottom + buttonPadding, button1.Width, button1.Height);
        gfx.FillRectangle(purpleBrush, button3);
        animation.DrawFitTextOneLine("PASS", font, Color.White, button3, animation.CenterCenter);
        ButtonActions["pass"] = button3;

        var button4 = new RectangleF(button3.Right + buttonPadding, button1.Bottom + buttonPadding, button1.Width, button1.Height);
        gfx.FillRectangle(purpleBrush, button4);
        animation.DrawFitTextOneLine("EXCHANGE", font, Color.White, button4, animation.CenterCenter);
        ButtonActions["exchange"] = button4;

        var cameraPanel = new RectangleF(leftPanel.Left, rackPanel.Bottom + padding, leftPanel.Width, leftPanel.Height / 3);
        gfx.FillRectangle(Brushes.Black, cameraPanel);

        var titlePanel = new RectangleF(rightPanel.Left, rightPanel.Top, rightPanel.Width, rightPanel.Width / 6);
        gfx.FillRectangle(purpleBrush, titlePanel);

        gfx.DrawImage(EmmaImage, titlePanel.Left + padding / 2, titlePanel.Top + padding / 2, titlePanel.Height * 3/4, titlePanel.Height * 3/4);

        var titleTextArea = new RectangleF(titlePanel.Left + titlePanel.Height, titlePanel.Top, titlePanel.Width - titlePanel.Height, titlePanel.Height);
        animation.DrawFitTextOneLine("scrabble with emma", font, Color.White, titleTextArea, animation.CenterCenter);

        var rulesPanel = new RectangleF(rightPanel.Left, titlePanel.Bottom + padding, rightPanel.Width, rightPanel.Height / 3 - titlePanel.Height - padding * 2);
        gfx.FillRectangle(Brushes.Black, rulesPanel);
        var rulesTitleArea = new RectangleF(rulesPanel.Left, rulesPanel.Top, rulesPanel.Width, bagTitleArea.Height);
        gfx.FillRectangle(purpleBrush, rulesTitleArea);
        animation.DrawFitTextOneLine("GAME RULES", font, Color.White, rulesTitleArea, animation.CenterLeft);

        var rulesContentArea = new RectangleF(rulesPanel.Left, rulesTitleArea.Bottom, rulesPanel.Width, rulesPanel.Height - rulesTitleArea.Height);

        var rules = new List<string>();

        rules.Add("Lexicon: " + Game.WordService.CurrentList.Name);
        
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

        animation.DrawFitText(string.Join("\r\n", rules), font, Color.White, rulesContentArea, animation.TopLeft, false, rulesContentArea.Height / 6);

        var playsPanel = new RectangleF(rightPanel.Left, rulesPanel.Bottom + padding, rightPanel.Width, rightPanel.Height - rulesPanel.Height - titlePanel.Height - padding * 2);
        gfx.FillRectangle(Brushes.Black, playsPanel);

        const string playsTitle = "PLAY HISTORY";
        var playTitleArea = new RectangleF(playsPanel.Left, playsPanel.Top, playsPanel.Width, bagTitleArea.Height);
        gfx.FillRectangle(purpleBrush, playTitleArea);
        animation.DrawFitTextOneLine(playsTitle, font, Color.White, playTitleArea, animation.CenterLeft);

        // PLAY CONTROLS
        var controlsArea = new RectangleF(playsPanel.Left, playsPanel.Bottom - button1.Height - buttonPadding, playsPanel.Width, button1.Height + buttonPadding);

        var playButton1 = new RectangleF(controlsArea.Left + buttonPadding, controlsArea.Top, (controlsArea.Width - buttonPadding * 5) / 4, controlsArea.Height - buttonPadding);
        gfx.FillRectangle(purpleBrush, playButton1);
        animation.DrawFitTextOneLine("<<", font, Color.White, playButton1, animation.CenterCenter);
        ButtonActions["first"] = playButton1;

        var playButton2 = new RectangleF(playButton1.Right + buttonPadding, controlsArea.Top, playButton1.Width, playButton1.Height);
        gfx.FillRectangle(purpleBrush, playButton2);
        animation.DrawFitTextOneLine("<", font, Color.White, playButton2, animation.CenterCenter);
        ButtonActions["prev"] = playButton2;

        var playButton3 = new RectangleF(playButton2.Right + buttonPadding, controlsArea.Top, playButton1.Width, playButton1.Height);
        gfx.FillRectangle(purpleBrush, playButton3);
        animation.DrawFitTextOneLine(">", font, Color.White, playButton3, animation.CenterCenter);
        ButtonActions["next"] = playButton3;

        var playButton4 = new RectangleF(playButton3.Right + buttonPadding, controlsArea.Top, playButton1.Width, playButton1.Height);
        gfx.FillRectangle(purpleBrush, playButton4);
        animation.DrawFitTextOneLine(">>", font, Color.White, playButton4, animation.CenterCenter);
        ButtonActions["last"] = playButton4;


        var playLogArea = new RectangleF(playsPanel.Left, playTitleArea.Bottom, playsPanel.Width, playsPanel.Height - playTitleArea.Height - controlsArea.Height);
        float playHeight = Math.Max(playLogArea.Height / 20, 16);
        int playCapacity = (int)(playLogArea.Height / playHeight);

        var playsToShow = Game.Plays.Skip(Game.Plays.Count - playCapacity).ToList();
        float y = playLogArea.Top;

        foreach (var play in playsToShow)
        {
            var playTurnIndicator = new RectangleF(playLogArea.Left + playHeight / 4, y + playHeight / 8, playHeight * 3/4, playHeight * 3/4);
            gfx.FillEllipse(play.Player ? Brushes.DodgerBlue : Brushes.Tomato, playTurnIndicator);

            var playScoreArea = new RectangleF(playLogArea.Right - playLogArea.Width / 5, y, playLogArea.Width / 5, playHeight);
            animation.DrawFitTextOneLine(play.Score.ToString(), font, Color.White, playScoreArea, animation.CenterRight, true);

            var playTextArea = new RectangleF(playTurnIndicator.Right + playHeight / 4, y, playScoreArea.Left - playTurnIndicator.Right - playHeight, playHeight);
            animation.DrawFitTextOneLine(play.Play, "Consolas", Color.White, playTextArea, animation.CenterLeft);

            y += playHeight;
        }
    }


    private void DrawTile(Graphics gfx, GdiAnimation animation, ScrabbleTile tile, float x, float y, float size, bool rack)
    {
        if (rack && tile.Uncommitted)
        {
            return;
        }

        var baseBrush = animation.GetBrush(Color.FromArgb(255, 239, 213));
        var accentBrush1 = animation.GetBrush(Color.FromArgb(242, 214, 169));
        var accentBrush2 = animation.GetBrush(Color.FromArgb(216, 179, 119));

        if (!rack && tile.Player == 1)
        {
            baseBrush = animation.GetBrush(Color.FromArgb(255, 216, 213));
            accentBrush1 = animation.GetBrush(Color.FromArgb(242, 174, 169));
            accentBrush2 = animation.GetBrush(Color.FromArgb(216, 126, 119));
        }

        if (!rack && tile.Player == 2)
        {
            baseBrush = animation.GetBrush(Color.FromArgb(213, 239, 255));
            accentBrush1 = animation.GetBrush(Color.FromArgb(169, 214, 242));
            accentBrush2 = animation.GetBrush(Color.FromArgb(119, 179, 216));
        }

        gfx.FillRectangle(baseBrush, x, y, size, size);
        gfx.FillRectangle(accentBrush1, x, y + size - (size / 12), size, size / 12);
        gfx.FillRectangle(accentBrush2, x + size - (size / 12), y, size / 12, size);

        string points = tile.Points.ToString();

        if (tile.Designation != " ")
        {
            animation.DrawFitTextOneLine(tile.Designation.ToString(), "Gill Sans MT", Color.Red,
                new RectangleF(x - size / 20, y - size / 20, size * 1.1f, size * 1.1f), animation.CenterCenter);
        }
        else
        {
            animation.DrawFitTextOneLine(tile.Display.ToString(), "Gill Sans MT", Color.Black,
                new RectangleF(x - size / 20, y - size / 20, size * 1.1f, size * 1.1f), animation.CenterCenter);
        }

        if (points != "0")
        {
            animation.DrawFitTextOneLine(points, "Segoe UI", Color.Black,
                new RectangleF(x + size * 0.6f, y + size * 0.55f, size * 0.4f, size * 0.4f), animation.BottomRight);
        }
    }


    private Brush GetSpaceBrush(GdiAnimation animation, int j, int i)
    {
        return RuleSet.Board[j, i] switch
        {
            RuleSet.dw => Brushes.LightPink,
            RuleSet.tw => Brushes.Tomato,
            RuleSet.dl => animation.GetBrush(Color.FromArgb(255, 170, 210, 255)),
            RuleSet.tl => Brushes.DodgerBlue,
            _ => Brushes.Ivory
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
                    tile.Points = RuleSet.LetterPoints.GetValueOrDefault(tile.Letter, 0);
                    SetTile(j, i, null);
                }
            }
        }

        Game.CheckPlay();
    }


    private void Commit()
    {
        if (Game.ValidPlay && Game.IsViewCurrent)
        {
            Cursor = (-1, -1);
            Game.NextTurn();
        }
    }


    public void HandleKey(KeyEventArgs e)
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
                        tile.Points = RuleSet.LetterPoints.GetValueOrDefault(tile.Letter, 0);
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
                        tile.Points = RuleSet.LetterPoints.GetValueOrDefault(tile.Letter, 0);
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

            if (letter >= 'A' && letter <= 'Z' && RuleSet.LetterDistribution.ContainsKey(letter.ToString()) && 
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
            string digit = e.KeyCode.ToString().Substring(1, 1);

            if (Free(Cursor.X, Cursor.Y) /*|| !PlayLetter(digit.ToString())*/)
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


    private void PlayTile(ScrabbleTile tile)
    {
        if (!Game.IsViewCurrent)
        {
            return;
        }

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
                tile.Points = RuleSet.LetterPoints.GetValueOrDefault(tile.Letter, tileToPlay.Points);
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
                    tile.Points = RuleSet.LetterPoints.GetValueOrDefault(tile.Letter, tileToPlay.Points);
                    break;
                }
            }
        }

        bool flipped = Game.CurrentRack.Any(tile => tile.Letter != "?" && tile.Designation != " ");
        bool flipAllowed = RuleSet.AllowFlip && !Game.CurrentRack.Any(tile => tile.Letter == "?");

        if (tileToPlay == null && flipAllowed && !flipped)
        {
            tileToPlay = Game.CurrentRack.FirstOrDefault(t => !t.Uncommitted);

            if (tileToPlay != null)
            {
                tileToPlay.Designation = letter;
                tileToPlay.Points = RuleSet.LetterPoints.GetValueOrDefault("?", 0);
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
                        alternativeTile.Points = RuleSet.LetterPoints.GetValueOrDefault("?", 0);

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
                        tileToPlay.Points = RuleSet.LetterPoints.GetValueOrDefault(tileToPlay.Letter, tileToPlay.Points);
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


    public void HandleMouse(MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
        {
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
                            WordGame.Shuffle(Game.CurrentRack);
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
                                Game.Exchange();
                                Game.NextTurn();
                            }
                            break;

                        case "first":
                            Game.ViewIndex = 0;
                            break;

                        case "prev":
                            if (Game.ViewIndex > 0)
                            {
                                Game.ViewIndex--;
                            }
                            break;

                        case "next":
                            if (Game.ViewIndex < Game.BoardHistory.Count - 1)
                            {
                                Game.ViewIndex++;
                            }
                            break;

                        case "last":
                            ViewCurrent();
                            break;
                    }
                }
            }
        }
    }


    public string? PlayWord(string x, string y, string word, bool vertical)
    {
        Recall();
        ViewCurrent();
        
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
        ViewCurrent();
        
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


    private void ViewCurrent()
    {
        Game.ViewIndex = Game.BoardHistory.Count - 1;
    }


    private int BoardSize => RuleSet.BoardSize;
    private bool Free(int j, int i) => j >= 0 && i >= 0 && j < BoardSize && i < BoardSize && Game.CurrentState[j, i] == null;
    private ScrabbleTile? GetTile(int j, int i) => Game.CurrentState[j, i];
    private void SetTile(int j, int i, ScrabbleTile? tile) => Game.CurrentState[j, i] = tile;
}
