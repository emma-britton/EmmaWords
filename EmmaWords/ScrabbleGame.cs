
namespace EmmaWords;

internal class ScrabbleGame
{
    public WordService WordService { get; set; }
    public RuleSet RuleSet { get; set; }
    private readonly Random Random = new();

    public int Player1Score { get; set; }
    public int Player2Score { get; set; }

    public bool Turn { get; set; }
    public List<ScrabbleTile> Bag { get; set; }

    public List<ScrabbleTile> Player1Rack { get; set; } = new();
    public List<ScrabbleTile> Player2Rack { get; set; } = new();
    public List<ScrabbleTile> ExchangeTiles { get; set; } = new();
    public List<ScrabblePlay> Plays { get; set; } = new();

    public bool Ended { get; set; }

    public List<ScrabbleTile?[,]> BoardHistory { get; set; } = new();
    public int ViewIndex { get; set; }
    public ScrabbleTile?[,] CurrentState => BoardHistory[ViewIndex];
    public bool IsViewCurrent => ViewIndex == BoardHistory.Count - 1;
    
    public (int x, int y) PlayStartPosition { get; set; }
    public (int x, int y) PlayEndPosition { get; set; }
    public bool VerticalPlay { get; set; }
    public bool ValidPlay { get; set; }
    public string? InvalidReason { get; set; }
    public int PlayScore { get; set; }
    public bool PlayMade { get; set; }
    public string PlayText { get; set; }


    public ScrabbleGame(WordService wordService)
    {
        WordService = wordService;
        RuleSet = WordService.RuleSet;
        Bag = new List<ScrabbleTile>();
        
        foreach (var item in RuleSet.LetterDistribution)
        {
            for (int i = 0; i < item.Value; i++)
            {
                string display = RuleSet.LetterDisplay.GetValueOrDefault(item.Key, item.Key);
                var tile = new ScrabbleTile(item.Key, display, " ", RuleSet.LetterPoints.GetValueOrDefault(item.Key, 0));
                Bag.Add(tile);
            }
        }
    }


    public void Start()
    {
        Ended = false;
        PlayMade = false;
        Player1Score = 0;
        Player2Score = 0;
        BoardHistory.Clear();
        BoardHistory.Add(new ScrabbleTile?[RuleSet.BoardSize, RuleSet.BoardSize]);
        ViewIndex = 0;

        if (new Random().NextDouble() >= 0.5)
        {
            Turn = true;
        }

        NextTurn();
    }


    public List<ScrabbleTile> CurrentRack => Turn ? Player2Rack : Player1Rack;


    public void NextTurn()
    {
        bool tilesPlayed = false;

        for (int i = CurrentRack.Count - 1; i >= 0; i--)
        {
            if (CurrentRack[i].Uncommitted)
            {
                tilesPlayed = true;
                CurrentRack.RemoveAt(i);
            }
        }

        for (int i = 0; i < RuleSet.BoardSize; i++)
        {
            for (int j = 0; j < RuleSet.BoardSize; j++)
            {
                if (CurrentState[j, i] is ScrabbleTile tile)
                {
                    if (!tile.Uncommitted)
                    {
                        tile.Player = 0;
                    }

                    tile.Uncommitted = false;
                    PlayMade = true;
                }
            }
        }

        if (tilesPlayed)
        {
            Plays.Add(new ScrabblePlay(Turn, PlayText, PlayScore));
        }

        if (Turn)
        {
            Player2Score += PlayScore;
        }
        else
        {
            Player1Score += PlayScore;
        }
        
        PlayStartPosition = (-1, -1);
        PlayEndPosition = (-1, -1);

        if (tilesPlayed)
        {
            var boardCopy = new ScrabbleTile?[RuleSet.BoardSize, RuleSet.BoardSize];

            for (int i = 0; i < RuleSet.BoardSize; i++)
            {
                for (int j = 0; j < RuleSet.BoardSize; j++)
                {
                    boardCopy[j, i] = CurrentState[j, i];
                }
            }

            while (BoardHistory.Count > ViewIndex + 1)
            {
                BoardHistory.RemoveAt(BoardHistory.Count - 1);
            }

            BoardHistory.Add(boardCopy);
        }
        
        ViewIndex = BoardHistory.Count - 1;
        PlayScore = 0;
        ValidPlay = false;

        Turn = !Turn;

        while (Bag.Count > 0 && CurrentRack.Count < RuleSet.RackSize)
        {
            DrawTile();
        }

        var opponentRack = Turn ? Player1Rack : Player2Rack;

        if (opponentRack.Count == 0 && Bag.Count == 0)
        {
            string play = "tiles left: ";
            int score = 0;

            foreach (var tile in CurrentRack)
            {
                play += tile.Letter;
                score -= RuleSet.LetterPoints[tile.Letter];
            }

            Plays.Add(new ScrabblePlay(Turn, play, score));

            if (Turn)
            {
                Player2Score += score;
            }
            else
            {
                Player1Score += score;
            }

            Ended = true;
            return;
        }

        CurrentRack.Sort((x, y) => string.Compare(x.Display, y.Display));
    }

    
    private void DrawTile()
    {
        int index = Random.Next(Bag.Count);
        var tile = Bag[index];
        tile.Player = Turn ? 2 : 1;
        Bag.RemoveAt(index);
        CurrentRack.Add(tile);
    }


    public void Pass()
    {
        foreach (var tile in CurrentRack)
        {
            tile.Designation = " ";
            tile.Uncommitted = false;
        }

        Plays.Add(new ScrabblePlay(Turn, "pass", 0));
        NextTurn();
    }


    public void Exchange()
    {
        for (int i = 0; i < RuleSet.BoardSize; i++)
        {
            for (int j = 0; j < RuleSet.BoardSize; j++)
            {
                if (CurrentState[j, i] is ScrabbleTile tile && tile.Uncommitted)
                {
                    CurrentState[j, i] = null;
                }
            }
        }

        for (int i = CurrentRack.Count - 1; i >= 0; i--)
        {
            if (CurrentRack[i].Uncommitted)
            {
                ExchangeTiles.Add(CurrentRack[i]);
                CurrentRack[i].Designation = " ";
                CurrentRack[i].Uncommitted = false;
                CurrentRack.RemoveAt(i);
            }
        }

        string play = "exchange ";
        
        foreach (var tile in ExchangeTiles.OrderBy(x => x.Letter))
        {
            string text = tile.Display;

            if (text == " ")
            {
                text = tile.Letter;
            }

            if (text.Length > 1)
            {
                text = "[" + text + "]";
            }

            play += text;
        }

        Plays.Add(new ScrabblePlay(Turn, play, 0));

        while (Bag.Count > 0 && CurrentRack.Count < RuleSet.RackSize)
        {
            DrawTile();
        }

        Bag.AddRange(ExchangeTiles);
        ExchangeTiles.Clear();
    }


    public void CheckPlay()
    {
        PlayScore = ScoreCurrentPlay();
        ValidPlay = ValidateCurrentPlay();
    }


    private int ScoreCurrentPlay()
    {
        int ScoreTile(ScrabbleTile? t, int spaceType, ref int multiplier)
        {
            if (t == null) return 0;

            int points = t.Points;

            if (t.Uncommitted)
            {
                switch (spaceType)
                {
                    case RuleSet.nn:
                        return points;

                    case RuleSet.dl:
                        return points * 2;
                        
                    case RuleSet.tl:
                        return points * 3;

                    case RuleSet.dw:
                        multiplier *= 2;
                        return points;

                    case RuleSet.tw:
                        multiplier *= 3;
                        return points;
                }
            }
            else
            {
                return points;
            }

            return 0;
        }


        int ScoreSinglePlay(List<(int x, int y)> positions)
        {
            int tilesScore = 0;
            int wordMultiplier = 1;
            string word = "";

            foreach (var position in positions)
            {
                if (CurrentState[position.x, position.y] is ScrabbleTile tile)
                {
                    if (tile.Designation != " ")
                    {
                        word += tile.Designation;
                    }
                    else
                    {
                        word += tile.Letter;
                    }
                }

                tilesScore += ScoreTile(CurrentState[position.x, position.y], RuleSet.Board[position.x, position.y], ref wordMultiplier);
            }

            int wordScore = tilesScore * wordMultiplier;
            wordScore += WordService.CurrentList.GetBonusPoints(word.ToUpper());
            return wordScore;
        }


        var scoredHPlays = new HashSet<(int, int)>();
        var scoredVPlays = new HashSet<(int, int)>();
        int playScore = 0;
        int uncommittedTiles = 0;
        PlayEndPosition = (-1, -1);

        for (int i = 0; i < RuleSet.BoardSize; i++)
        {
            for (int j = 0; j < RuleSet.BoardSize; j++)
            {
                if (CurrentState[j, i] is ScrabbleTile tile && tile.Uncommitted)
                {
                    uncommittedTiles++;

                    var horizontalPlay = GetPlayThrough(j, i, false);

                    if (horizontalPlay != null)
                    {
                        if (horizontalPlay.Last().Item1 > PlayEndPosition.x || horizontalPlay.Last().Item2 > PlayEndPosition.y)
                        {
                            PlayEndPosition = horizontalPlay.Last();
                        }

                        if (!scoredHPlays.Contains(horizontalPlay[0]))
                        {
                            scoredHPlays.Add(horizontalPlay[0]);

                            playScore += ScoreSinglePlay(horizontalPlay);
                        }
                    }

                    var verticalPlay = GetPlayThrough(j, i, true);

                    if (verticalPlay != null)
                    {
                        if (verticalPlay.Last().Item1 > PlayEndPosition.x || verticalPlay.Last().Item2 > PlayEndPosition.y)
                        {
                            PlayEndPosition = verticalPlay.Last();
                        }

                        if (!scoredVPlays.Contains(verticalPlay[0]))
                        {
                            scoredVPlays.Add(verticalPlay[0]);

                            playScore += ScoreSinglePlay(verticalPlay);
                        }
                    }
                }
            }
        }

        if (uncommittedTiles == RuleSet.RackSize)
        {
            playScore += RuleSet.BingoScore;
        }

        return playScore;
    }


    private bool ValidateWord(string word)
    {
        if (ulong.TryParse(word, out ulong number))
        {
            if (!PrimeTest.IsPrime(number))
            {
                InvalidReason = $"{number} is not prime";
                return false;
            }
        }
        else if (!WordService.CurrentList.Contains(word))
        {
            InvalidReason = $"{word} is not a word";
            return false;
        }

        return true;
    }


    private bool ValidateCurrentPlay()
    {
        if (ViewIndex != BoardHistory.Count - 1)
        {
            InvalidReason = "Viewing board history";
            return false;
        }

        var playedTiles = new List<(int x, int y)>();

        for (int i = 0; i < RuleSet.BoardSize; i++)
        {
            for (int j = 0; j < RuleSet.BoardSize; j++)
            {
                if (CurrentState[j, i] is ScrabbleTile t && t.Uncommitted)
                {
                    playedTiles.Add((j, i));
                    var horizontalPlay = GetPlayThrough(j, i, false);

                    if (horizontalPlay != null && RuleSet.ValidateWords)
                    {
                        string word = "";

                        foreach (var (x, y) in horizontalPlay)
                        {
                            var tile = CurrentState[x, y];
                             
                            if (tile != null)
                            {
                                if (tile.Designation == " ")
                                {
                                    word += tile.Letter;
                                }
                                else
                                {
                                    word += tile.Designation;
                                }
                            }
                        }

                        if (!ValidateWord(word)) return false;
                    }

                    var verticalPlay = GetPlayThrough(j, i, true);

                    if (verticalPlay != null && RuleSet.ValidateWords)
                    {
                        string word = "";

                        foreach (var (x, y) in verticalPlay)
                        {
                            var tile = CurrentState[x, y];

                            if (tile != null)
                            {
                                if (tile.Designation == " ")
                                {
                                    word += tile.Letter;
                                }
                                else
                                {
                                    word += tile.Designation;
                                }
                            }
                        }

                        if (!ValidateWord(word)) return false;
                    }
                }
            }
        }

        if (playedTiles.Count == 0)
        {
            // no tiles played
            InvalidReason = "No tiles played";
            return false;
        }

        if (!PlayMade && !playedTiles.Contains(RuleSet.Star))
        {
            // did not cross star
            InvalidReason = "First play must touch star";
            return false;
        }

        bool touchesExisting = false;

        if (PlayMade)
        {
            foreach (var playedTile in playedTiles)
            {
                if (playedTile.x > 0 && CurrentState[playedTile.x - 1, playedTile.y] is ScrabbleTile t1 && !t1.Uncommitted
                    || playedTile.y > 0 && CurrentState[playedTile.x, playedTile.y - 1] is ScrabbleTile t2 && !t2.Uncommitted
                    || playedTile.x < RuleSet.BoardSize - 1 && CurrentState[playedTile.x + 1, playedTile.y] is ScrabbleTile t3 && !t3.Uncommitted
                    || playedTile.y < RuleSet.BoardSize - 1 && CurrentState[playedTile.x, playedTile.y + 1] is ScrabbleTile t4 && !t4.Uncommitted)
                {
                    touchesExisting = true;
                }
            }

            if (!touchesExisting)
            {
                // disconnected play
                InvalidReason = "Play has disconnected tiles";
                return false;
            }
        }
        
        if (playedTiles.Select(t => t.y).Distinct().Count() > 1)
        {
            if (playedTiles.Select(t => t.x).Distinct().Count() > 1)
            {
                // disconnected play
                InvalidReason = "Play has disconnected tiles";
                return false;
            }

            VerticalPlay = true;
            PlayStartPosition = (playedTiles[0].x, playedTiles.Select(t => t.y).Min());
            PlayEndPosition = (playedTiles[0].x, playedTiles.Select(t => t.y).Max());

            for (int y = PlayStartPosition.y; y <= PlayEndPosition.y; y++)
            {
                if (CurrentState[PlayStartPosition.x, y] == null)
                {
                    // disconnected play
                    InvalidReason = "Play has disconnected tiles";
                    return false;
                }
            }

            var mainPlay = GetPlayThrough(PlayStartPosition.x, PlayStartPosition.y, true);

            if (mainPlay != null)
            {
                UpdatePlay(mainPlay, true, playedTiles.Count >= RuleSet.RackSize);
            }
        }
        else
        {
            VerticalPlay = false;
            PlayStartPosition = (playedTiles.Select(t => t.x).Min(), playedTiles[0].y);
            PlayEndPosition = (playedTiles.Select(t => t.x).Max(), playedTiles[0].y);

            for (int x = PlayStartPosition.x; x <= PlayEndPosition.x; x++)
            {
                if (CurrentState[x, PlayStartPosition.y] == null)
                {
                    // disconnected play
                    InvalidReason = "Play has disconnected tiles";
                    return false;
                }
            }

            var mainPlay = GetPlayThrough(PlayStartPosition.x, PlayStartPosition.y, false);

            if (mainPlay != null)
            {
                UpdatePlay(mainPlay, false, playedTiles.Count >= RuleSet.RackSize);
            }
        }

        InvalidReason = null;
        return true;
    }


    private void UpdatePlay(List<(int x, int y)> play, bool vertical, bool bonus)
    {
        PlayText = "";

        if (vertical)
        {
            PlayText += ((char)('A' + play[0].x)).ToString();
            PlayText += (play[0].y + 1).ToString();
        }
        else
        {
            PlayText += (play[0].y + 1).ToString();
            PlayText += ((char)('A' + play[0].x)).ToString();
        }

        PlayText += " ";

        if (PlayText.Length == 3)
        {
            PlayText += " ";
        }

        bool bracket = false;

        foreach (var (x, y) in play)
        {
            var tile = CurrentState[x, y];
            if (tile == null) continue;

            if (!tile.Uncommitted && !bracket)
            {
                PlayText += "(";
                bracket = true;
            }
            else if (tile.Uncommitted && bracket)
            {
                PlayText += ")";
                bracket = false;
            }

            if (tile.Designation != " ")
            {
                PlayText += tile.Designation.ToString().ToLower();
            }
            else if (tile.Display == "" || tile.Display == " ")
            {
                PlayText += tile.Letter;
            }
            else
            {
                PlayText += tile.Display;
            }
        }

        if (bracket)
        {
            PlayText += ")";
        }

        if (bonus)
        {
            PlayText += " 🤍";
        }
    }


    private List<(int x, int y)>? GetPlayThrough(int j, int i, bool vertical)
    {
        if (CurrentState[j, i] == null) return null;

        var result = new List<(int, int)>();
        if (vertical)
        {
            while (i > 0 && CurrentState[j, i - 1] is not null)
            {
                i--;
            }

            while (i < RuleSet.BoardSize && CurrentState[j, i] is not null)
            {
                result.Add((j, i));
                i++;
            }
        }
        else
        {
            while (j > 0 && CurrentState[j - 1, i] is not null)
            {
                j--;
            }

            while (j < RuleSet.BoardSize && CurrentState[j, i] is not null)
            {
                result.Add((j, i));
                j++;
            }
        }

        if (result.Count <= 1)
        {
            return null;
        }

        return result;
    }
}
