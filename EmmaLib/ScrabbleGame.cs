
namespace Emma.Lib;

/// <summary>
/// Represents a game of Scrabble.
/// </summary>
public class ScrabbleGame
{
    private static readonly Random m_Random = new();


    /// <summary>
    /// Unique game identifier.
    /// </summary>
    public string? ID { get; set; }

    /// <summary>
    /// First player's name.
    /// </summary>
    public string Player1Name { get; init; }

    /// <summary>
    /// Second player's name.
    /// </summary>
    public string Player2Name { get; set; }

    /// <summary>
    /// First player's score.
    /// </summary>
    public int Player1Score { get; set; }

    /// <summary>
    /// Second player's score.
    /// </summary>
    public int Player2Score { get; set; }

    /// <summary>
    /// Number of turns taken by the first player.
    /// </summary>
    public int Player1Turns { get; set; }

    /// <summary>
    /// Number of turns taken by the second player.
    /// </summary>
    public int Player2Turns { get; set; }

    /// <summary>
    /// Number of bingos played by the first player.
    /// </summary>
    public int Player1Bingos { get; set; }

    /// <summary>
    /// Number of bingos played by the second player.
    /// </summary>
    public int Player2Bingos { get; set; }

    /// <summary>
    /// Number of the player who takes the first turn.
    /// </summary>
    public int FirstPlayer { get; set; }

    /// <summary>
    /// Number of the player who won the game, if the game has concluded. 0 indicates a tie.
    /// </summary>
    public int? Winner
    {
        get
        {
            if (!IsComplete)
            {
                return null;
            }
            else if (Player1Score > Player2Score)
            {
                return 1;
            }
            else if (Player2Score > Player1Score)
            {
                return 2;
            }

            return 0;
        }
    }
    
    /// <summary>
    /// Whether the game has been completed, and if so, how it ended.
    /// </summary>
    public bool IsComplete { get; set; }

    /// <summary>
    /// Indicates that the game ended with a sequence of consecutive non-scoring plays while both players had tiles remaining.
    /// </summary>
    public bool IsPassedOut { get; set; }

    /// <summary>
    /// The lexicon with which this game was played.
    /// </summary>
    public Lexicon Lexicon { get; }

    /// <summary>
    /// The number of the player with the current turn.
    /// </summary>
    public int PlayerNumber { get; set; }

    /// <summary>
    /// The rule set under which this game was played.
    /// </summary>
    public RuleSet RuleSet { get; }

    /// <summary>
    /// The plays made in this game.
    /// </summary>
    public List<ScrabblePlay> Plays { get; } = new();

    /// <summary>
    /// The contents of the tile bag.
    /// </summary>
    public List<ScrabbleTile> Bag { get; set; } = new();

    /// <summary>
    /// The tiles currently on Player 1's rack.
    /// </summary>
    public List<ScrabbleTile> Player1Rack { get; set; } = new();

    /// <summary>
    /// The tiles currently on Player 2's rack.
    /// </summary>
    public List<ScrabbleTile> Player2Rack { get; set; } = new();

    /// <summary>
    /// The tiles on the current player's rack.
    /// </summary>
    public List<ScrabbleTile> CurrentRack => PlayerNumber == 2 ? Player2Rack : Player1Rack;

    /// <summary>
    /// The current board state.
    /// </summary>
    public ScrabbleTile?[,] Board { get; set; }


    /// <summary>
    /// Creates a representation of a Scrabble game between two players.
    /// </summary>
    /// <param name="ruleSet">Rule set to use for this game.</param>
    /// <param name="lexicon">Lexicon to use for this game.</param>
    /// <param name="player1Name">First player's name.</param>
    /// <param name="player2Name">Second player's name.</param>
    public ScrabbleGame(RuleSet ruleSet, Lexicon lexicon, string player1Name, string player2Name)
    {
        RuleSet = ruleSet;
        Lexicon = lexicon;
        Player1Name = player1Name;
        Player2Name = player2Name;
        Board = new ScrabbleTile?[ruleSet.BoardSize, ruleSet.BoardSize];
    }


    public (int x, int y) PlayStartPosition { get; set; }
    public (int x, int y) PlayEndPosition { get; set; }
    public bool VerticalPlay { get; set; }
    public bool ValidPlay { get; set; }
    public int PlayScore { get; set; }
    public bool PlayMade { get; set; }
    public string? PlayText { get; set; }
    public string? InvalidReason { get; set; }


    public void Start()
    {
        Bag.Clear();

        foreach (var item in RuleSet.TileDistribution)
        {
            for (int i = 0; i < item.Value; i++)
            {
                string display = RuleSet.TileDisplay.GetValueOrDefault(item.Key, item.Key);
                var tile = new ScrabbleTile(item.Key, display, " ", RuleSet.TilePoints.GetValueOrDefault(item.Key, 0));
                Bag.Add(tile);
            }
        }

        Board = new ScrabbleTile[RuleSet.BoardSize, RuleSet.BoardSize];

        IsComplete = false;
        PlayMade = false;
        Player1Score = 0;
        Player2Score = 0;
        Plays.Clear();

        if (new Random().NextDouble() >= 0.5)
        {
            PlayerNumber = 2;
        }
        else
        {
            PlayerNumber = 1;
        }

        NextTurn();
    }


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
                if (Board[j, i] is ScrabbleTile tile)
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
            var play = new ScrabblePlay
            {
                PlayerNumber = PlayerNumber,
                PlayString = PlayText,
                Score = PlayScore,
                TurnNumber = Plays.Count + 1,
                Type = ScrabblePlayType.Move
            };

            Plays.Add(play);
        }

        if (tilesPlayed)
        {
            if (PlayerNumber == 2)
            {
                Player2Score += PlayScore;
            }
            else
            {
                Player1Score += PlayScore;
            }
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
                    boardCopy[j, i] = Board[j, i];
                }
            }
        }

        PlayScore = 0;
        ValidPlay = false;

        PlayerNumber = PlayerNumber == 2 ? 1 : 2;

        while (Bag.Count > 0 && CurrentRack.Count < RuleSet.RackSize)
        {
            DrawTile();
        }

        var opponentRack = PlayerNumber == 2 ? Player1Rack : Player2Rack;

        if (opponentRack.Count == 0 && Bag.Count == 0)
        {
            string remainingPlay = "tiles left: ";
            int score = 0;

            foreach (var tile in CurrentRack)
            {
                remainingPlay += tile.Letter;
                score -= RuleSet.TilePoints[tile.Letter];
            }

            var play = new ScrabblePlay
            {
                TurnNumber = Plays.Count + 1,
                PlayerNumber = PlayerNumber == 2 ? 1 : 2,
                Score = score,
                PlayString = remainingPlay,
                Type = ScrabblePlayType.RemainingTiles
            };

            Plays.Add(play);

            if (PlayerNumber == 2)
            {
                Player2Score += score;
            }
            else
            {
                Player1Score += score;
            }

            IsComplete = true;
            return;
        }

        CurrentRack.Sort((x, y) => string.Compare(x.Display, y.Display));
    }


    private void DrawTile()
    {
        int index = m_Random.Next(Bag.Count);
        var tile = Bag[index];
        tile.Player = PlayerNumber;
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

        Plays.Add(new ScrabblePlay
        {
            PlayerNumber = PlayerNumber,
            Type = ScrabblePlayType.Pass,
            Score = 0
        });

        NextTurn();
    }


    public void Exchange(IList<ScrabbleTile> exchangeTiles)
    {
        for (int i = 0; i < RuleSet.BoardSize; i++)
        {
            for (int j = 0; j < RuleSet.BoardSize; j++)
            {
                if (Board[j, i] is ScrabbleTile tile && tile.Uncommitted)
                {
                    Board[j, i] = null;
                }
            }
        }

        foreach (var exchangeTile in exchangeTiles)
        {
            exchangeTile.Designation = " ";
            exchangeTile.Uncommitted = false;
            CurrentRack.Remove(exchangeTile);
        }

        string play = "exchange ";

        foreach (var tile in exchangeTiles.OrderBy(x => x.Letter))
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

        Plays.Add(new ScrabblePlay
        {
            PlayerNumber = PlayerNumber,
            PlayString = play,
            Type = ScrabblePlayType.Exchange,
            Score = 0 
        });

        while (Bag.Count > 0 && CurrentRack.Count < RuleSet.RackSize)
        {
            DrawTile();
        }

        Bag.AddRange(exchangeTiles);
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
                    case RuleSet.__:
                        return points;

                    case RuleSet.DL:
                        return points * 2;

                    case RuleSet.TL:
                        return points * 3;

                    case RuleSet.QL:
                        return points * 4;
                        
                    case RuleSet.DW:
                        multiplier *= 2;
                        return points;

                    case RuleSet.TW:
                        multiplier *= 3;
                        return points;

                    case RuleSet.QW:
                        multiplier *= 4;
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

            foreach (var (x, y) in positions)
            {
                if (Board[x, y] is ScrabbleTile tile)
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

                tilesScore += ScoreTile(Board[x, y], RuleSet.Board[x, y], ref wordMultiplier);
            }

            int wordScore = tilesScore * wordMultiplier;
            wordScore += Lexicon.GetAdjustment(word);
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
                if (Board[j, i] is ScrabbleTile tile && tile.Uncommitted)
                {
                    uncommittedTiles++;

                    var horizontalPlay = GetPlayThrough(j, i, false);

                    if (horizontalPlay != null)
                    {
                        if (horizontalPlay.Last().x > PlayEndPosition.x || horizontalPlay.Last().y > PlayEndPosition.y)
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
                        if (verticalPlay.Last().x > PlayEndPosition.x || verticalPlay.Last().y > PlayEndPosition.y)
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
        if (!Lexicon.Contains(word))
        {
            InvalidReason = $"{word} is not a word";
            return false;
        }

        return true;
    }


    private bool ValidateCurrentPlay()
    {
        var playedTiles = new List<(int x, int y)>();

        for (int i = 0; i < RuleSet.BoardSize; i++)
        {
            for (int j = 0; j < RuleSet.BoardSize; j++)
            {
                if (Board[j, i] is ScrabbleTile t && t.Uncommitted)
                {
                    playedTiles.Add((j, i));
                    var horizontalPlay = GetPlayThrough(j, i, false);

                    if (horizontalPlay != null && RuleSet.ValidateWords)
                    {
                        string word = "";

                        foreach (var (x, y) in horizontalPlay)
                        {
                            var tile = Board[x, y];

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
                            var tile = Board[x, y];

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
            foreach (var (x, y) in playedTiles)
            {
                if (x > 0 && Board[x - 1, y] is ScrabbleTile t1 && !t1.Uncommitted
                    || y > 0 && Board[x, y - 1] is ScrabbleTile t2 && !t2.Uncommitted
                    || x < RuleSet.BoardSize - 1 && Board[x + 1, y] is ScrabbleTile t3 && !t3.Uncommitted
                    || y < RuleSet.BoardSize - 1 && Board[x, y + 1] is ScrabbleTile t4 && !t4.Uncommitted)
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
                if (Board[PlayStartPosition.x, y] == null)
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
                if (Board[x, PlayStartPosition.y] == null)
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
            var tile = Board[x, y];
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
        if (Board[j, i] == null) return null;

        var result = new List<(int, int)>();
        if (vertical)
        {
            while (i > 0 && Board[j, i - 1] is not null)
            {
                i--;
            }

            while (i < RuleSet.BoardSize && Board[j, i] is not null)
            {
                result.Add((j, i));
                i++;
            }
        }
        else
        {
            while (j > 0 && Board[j - 1, i] is not null)
            {
                j--;
            }

            while (j < RuleSet.BoardSize && Board[j, i] is not null)
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
