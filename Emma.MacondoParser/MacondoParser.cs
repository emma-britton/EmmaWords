using Emma.Lib;
using System.Text;

namespace Emma.MacondoParser;

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
        var ws = new WordService(Program.Config["BaseFolder"] ?? "");

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

            var game = new ScrabbleGame(ws.ActiveRuleSet, ws.ActiveLexicon, player1Name, player2Name)
            {
                ID = id,
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
    /// Writes a Macondo game log entry for a game.
    /// </summary>
    /// <param name="game"></param>
    /// <param name="writer">The text writer to write to.</param>
    public static void WriteGame(ScrabbleGame game, TextWriter writer)
    {
        writer.Write(game.ID);
        writer.Write(',');
        writer.Write(game.FirstPlayer);
        writer.Write(',');
        writer.Write(game.Player1Score);
        writer.Write(',');
        writer.Write(game.Player2Score);
        writer.Write(',');
        writer.Write(game.Player1Bingos);
        writer.Write(',');
        writer.Write(game.Player2Bingos);
        writer.Write(',');
        writer.Write(game.Player1Turns);
        writer.Write(',');
        writer.Write(game.Player2Turns);
        writer.WriteLine();
    }


    /// <summary>
    /// Reads a Macondo play log. Returns the plays organized into games. Can handle games interleaved due to threading.
    /// </summary>
    /// <param name="input">The stream to read.</param>
    public static IEnumerable<ScrabbleGame> ReadPlayFile(Stream input)
    {
        var ws = new WordService(Program.Config["BaseFolder"] ?? "");

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
                game = new ScrabbleGame(ws.ActiveRuleSet, ws.ActiveLexicon, "p1", "p2")
                {
                    ID = id,
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
                GameID = game.ID,
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
                var crossWordBuilder = new StringBuilder(15);
                play.AdditionalWords = [];

                for (int i = 4; i < move.Length; i++)
                {
                    if (move[i] != '.')
                    {
                        bs[col, row] = move[i];
                        crossWordBuilder.Clear();

                        int row2 = row;
                        int col2 = col;
                        
                        if (vertical)
                        {
                            while (col2 > 0 && bs[col2 - 1, row2] != '\0')
                            {
                                col2--;
                            }

                            while (col2 < 15 && bs[col2, row2] != '\0')
                            {
                                crossWordBuilder.Append(bs[col2, row2]);
                                col2++;
                            }
                        }
                        else
                        {
                            while (row2 > 0 && bs[col2, row2 - 1] != '\0')
                            {
                                row2--;
                            }
                            
                            while (row2 < 15 && bs[col2, row2] != '\0')
                            {
                                crossWordBuilder.Append(bs[col2, row2]);
                                row2++;
                            }
                        }

                        if (crossWordBuilder.Length > 1)
                        {
                            play.AdditionalWords.Add(crossWordBuilder.ToString().ToUpper());
                        }

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

            if (play.PlayerNumber == 1)
            {
                game.Player1Score = int.Parse(fields[totalScoreCol]);
            }
            else
            {
                game.Player2Score = int.Parse(fields[totalScoreCol]);
            }

            if (game.IsComplete)
            {
                incompleteGames.Remove(id);
                boardStates.Remove(id);
                yield return game;
            }
        }
    }


    /// <summary>
    /// Writes a Macondo play log entry for a play.
    /// </summary>
    /// <param name="play">The play.</param>
    /// <param name="writer">The text writer to write to.</param>
    public static void WritePlay(ScrabblePlay play, TextWriter writer)
    {
        writer.Write(play.PlayerNumber);
        writer.Write(',');
        writer.Write(play.GameID);
        writer.Write(',');
        writer.Write(play.TurnNumber);
        writer.Write(',');
        writer.Write(play.Rack);
        writer.Write(',');
        writer.Write(play.PlayString);
        writer.Write(',');
        writer.Write(play.Score);
        writer.Write(',');
        //writer.Write(play.TotalScore);
        writer.Write(',');
        writer.Write(play.TilesPlayed);
        writer.Write(',');
        writer.Write(play.Leave);
        writer.Write(',');
        writer.Write(play.Equity);
        writer.Write(',');
        writer.Write(play.TilesRemaining);
        writer.Write(',');
        //writer.Write(play.OppScore);
        writer.WriteLine();
    }

}
