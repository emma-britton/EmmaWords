using EmmaLib;
using System.Text;

namespace MacondoParser;

/// <summary>
/// Reads Macondo game and play logs.
/// </summary>
public static class MacondoParser
{
    /// <summary>
    /// Reads a Macondo game log and returns the games.
    /// </summary>
    /// <param name="input">The stream to read.</param>
    public static IEnumerable<ScrabbleGame> ReadGameFile(Stream input)
    {
        using var reader = new StreamReader(input);

        if (reader.EndOfStream || reader.ReadLine() is not string header)
        {
            yield break;
        }

        var headers = header.Split(',').ToList();

        int gameIDCol = headers.IndexOf("gameID");
        int firstCol = headers.IndexOf("first");
        int score1Col = headers.FindIndex(h => h.EndsWith("score"));
        int score2Col = headers.FindLastIndex(h => h.EndsWith("score"));
        int bingo1Col = headers.FindIndex(h => h.EndsWith("bingos"));
        int bingo2Col = headers.FindLastIndex(h => h.EndsWith("bingos"));
        int turns1Col = headers.FindIndex(h => h.EndsWith("turns"));
        int turns2Col = headers.FindLastIndex(h => h.EndsWith("turns"));

        if (firstCol == -1)
        {
            throw new Exception("This is not a Macondo game log (did you supply a Macondo play log by mistake?)");
        }

        string player1Name = headers[score1Col][..(headers[score1Col].Length - 6)];
        string player2Name = headers[score2Col][..(headers[score2Col].Length - 6)];

        while (!reader.EndOfStream && reader.ReadLine() is string line)
        {
            var fields = line.Split(',').ToList();
            string id = fields[gameIDCol];

            int firstPlayer = 1;

            if (fields[firstCol] == player2Name)
            {
                firstPlayer = 2;
            }

            var game = new ScrabbleGame(id, player1Name, player2Name)
            {
                FirstPlayer = firstPlayer,
                Player1Score = int.Parse(fields[score1Col]),
                Player2Score = int.Parse(fields[score2Col]),
                Player1Turns = int.Parse(fields[turns1Col]),
                Player2Turns = int.Parse(fields[turns2Col]),
                Player1Bingos = int.Parse(fields[bingo1Col]),
                Player2Bingos = int.Parse(fields[bingo2Col]),
                IsComplete = true
            };

            yield return game;
        }
    }


    /// <summary>
    /// Reads a Macondo play log. Returns the plays organized into games. Can handle games interleaved due to threading.
    /// </summary>
    /// <param name="input">The stream to read.</param>
    public static IEnumerable<ScrabbleGame> ReadPlayFile(Stream input)
    {
        using var reader = new StreamReader(input);

        if (reader.EndOfStream || reader.ReadLine() is not string header)
        {
            yield break;
        }

        var headers = header.Split(',').ToList();

        int playerIDCol = headers.IndexOf("playerID");
        int gameIDCol = headers.IndexOf("gameID");
        int turnCol = headers.IndexOf("turn");
        int rackCol = headers.IndexOf("rack");
        int playCol = headers.IndexOf("play");
        int scoreCol = headers.IndexOf("score");
        int totalScoreCol = headers.IndexOf("totalscore");
        int tilesPlayedCol = headers.IndexOf("tilesplayed");
        int leaveCol = headers.IndexOf("leave");
        int equityCol = headers.IndexOf("equity");
        int tilesRemainingCol = headers.IndexOf("tilesremaining");
        int oppScoreCol = headers.IndexOf("oppscore");

        if (playerIDCol == -1)
        {
            throw new Exception("This is not a Macondo play log (did you supply a Macondo game log by mistake?)");
        }

        var incompleteGames = new Dictionary<string, ScrabbleGame>();
        var boardStates = new Dictionary<string, char[,]>();

        while (!reader.EndOfStream && reader.ReadLine() is string line)
        {
            var fields = line.Split(',').ToList();
            int playerNumber = 1;

            if (fields[playerIDCol] == "p2")
            {
                playerNumber = 2;
            }

            string id = fields[gameIDCol];

            if (!incompleteGames.TryGetValue(id, out var game))
            {
                game = new ScrabbleGame(id, "p1", "p2")
                {
                    FirstPlayer = playerNumber
                };

                incompleteGames[id] = game;
                boardStates[id] = new char[15, 15];
            }

            string move = fields[playCol];
            bool vertical = false;
            int row = -1, col = -1;
            var type = ScrabblePlayType.Move;

            if (move[0] == '(')
            {
                if (move[1] == 'e')
                {
                    type = ScrabblePlayType.Exchange;
                }
                else
                {
                    type = ScrabblePlayType.Pass;
                }
            }
            else if (move[0] == ' ')
            {
                if (move[1] >= 'A')
                {
                    vertical = true;
                    row = move[2] - '1';
                    col = move[1] - 'A';
                }
                else
                {
                    row = move[1] - '1';
                    col = move[2] - 'A';
                }
                
            }
            else if (move[0] == '1')
            {
                row = 10 + move[1] - '1';
                col = move[2] - 'A';
            }
            else
            {
                vertical = true;
                row = 10 + move[2] - '1';
                col = move[0] - 'A';
            }

            var play = new ScrabblePlay
            {
                ID = $"{game.ID} turn {fields[turnCol]}",
                PlayerNumber = playerNumber,
                TurnNumber = int.Parse(fields[turnCol]),
                Rack = fields[rackCol],
                Score = int.Parse(fields[scoreCol]),
                Leave = fields[leaveCol],
                Equity = decimal.Parse(fields[equityCol]),
                TilesPlayed = int.Parse(fields[tilesPlayedCol]),
                TilesRemaining = int.Parse(fields[tilesRemainingCol]),
                Position = (row, col),
                Vertical = vertical,
                Type = type,
                MacondoString = fields[playCol]
            };

            if (play.Type == ScrabblePlayType.Move)
            {
                var bs = boardStates[id];
                var wordBuilder = new StringBuilder(move.Length - 4);
                var playBuilder = new StringBuilder(move.Length * 3);
                playBuilder.Append(move, 0, 4);
                bool dot = false;

                for (int i = 4; i < move.Length; i++)
                {
                    if (move[i] != '.')
                    {
                        bs[col, row] = move[i];

                        if (dot)
                        {
                            playBuilder.Append(')');
                            dot = false;
                        }
                    }
                    else if (!dot)
                    {
                        playBuilder.Append('(');
                        dot = true;
                    }

                    wordBuilder.Append(bs[col, row]);
                    playBuilder.Append(bs[col, row]);

                    if (vertical)
                    {
                        row++;
                    }
                    else
                    {
                        col++;
                    }
                }

                if (dot)
                {
                    playBuilder.Append(')');
                }

                play.Word = wordBuilder.ToString().ToUpper();
                play.PlayString = playBuilder.ToString().ToUpper();
            }
            else
            {
                play.PlayString = play.MacondoString;
            }

            if (play.TilesRemaining == 0 && play.Leave == "")
            {
                game.IsComplete = true;
            }
            else if (play.Type != ScrabblePlayType.Move)
            {
                int passNumber = 0;

                if (game.Plays.Count > 0)
                {
                    passNumber = game.Plays[^1].PassNumber;
                }

                play.PassNumber = ++passNumber;

                if (play.PassNumber == 6)
                {
                    game.IsComplete = true;
                    game.IsPassedOut = true;
                }
            }

            game.Plays.Add(play);

            if (game.IsComplete)
            {
                incompleteGames.Remove(id);
                boardStates.Remove(id);
                yield return game;
            }
        }
    }
}
