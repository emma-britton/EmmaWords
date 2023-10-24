using Emma.Lib;

namespace Emma.MacondoParser;

public static class MacondoAnalysis
{
    public static void AnalyseGames(IEnumerable<ScrabbleGame> games, Stream output)
    {
        long gameCount = 0;
        long turnCount = 0;
        long scoreCount = 0;
        long totalScore = 0;
        long firstPlayerWins = 0;
        long secondPlayerWins = 0;
        long ties = 0;
        long firstScoreTotal = 0;
        long secondScoreTotal = 0;
        long bingoCount = 0;
        long zeroBingoGames = 0;

        (string?, int) highestGameScore = (null, int.MinValue);
        (string?, int) lowestGameScore = (null, int.MaxValue);
        (string?, int) highestPlayerScore = (null, int.MinValue);
        (string?, int) lowestPlayerScore = (null, int.MaxValue);
        (string?, int) mostGameBingos = (null, int.MinValue);
        (string?, int) mostPlayerBingos = (null, int.MinValue);
        (string?, int) mostGameTurns = (null, int.MinValue);
        (string?, int) fewestGameTurns = (null, int.MaxValue);
        (string?, int) largestWinningMargin = (null, int.MinValue);

        foreach (var game in games)
        {
            gameCount++;
            scoreCount += 2;
            int gameScore = game.Player1Score + game.Player2Score;
            totalScore += gameScore;

            if (game.FirstPlayer == 1)
            {
                firstScoreTotal += game.Player1Score;
                secondScoreTotal += game.Player2Score;
            }
            else
            {
                firstScoreTotal += game.Player2Score;
                secondScoreTotal += game.Player1Score;
            }

            if (game.Player1Score > highestPlayerScore.Item2)
            {
                highestPlayerScore = (game.ID, game.Player1Score);
            }
            
            if (game.Player1Score < lowestPlayerScore.Item2)
            {
                lowestPlayerScore = (game.ID, game.Player1Score);
            }

            if (game.Player2Score > highestPlayerScore.Item2)
            {
                highestPlayerScore = (game.ID, game.Player2Score);
            }

            if (game.Player2Score < lowestPlayerScore.Item2)
            {
                lowestPlayerScore = (game.ID, game.Player2Score);
            }

            if (gameScore > highestGameScore.Item2)
            {
                highestGameScore = (game.ID, gameScore);
            }
            
            if (gameScore < lowestGameScore.Item2)
            {
                lowestGameScore = (game.ID, gameScore);
            }

            int winningMargin = Math.Max(game.Player1Score, game.Player2Score) - Math.Min(game.Player1Score, game.Player2Score);

            if (winningMargin > largestWinningMargin.Item2)
            {
                largestWinningMargin = (game.ID, winningMargin);
            }

            int gameBingos = game.Player1Bingos + game.Player2Bingos;
            bingoCount += gameBingos;

            if (gameBingos == 0)
            {
                zeroBingoGames++;
            }

            if (gameBingos > mostGameBingos.Item2)
            {
                mostGameBingos = (game.ID, gameBingos);
            }

            if (game.Player1Bingos > mostPlayerBingos.Item2)
            {
                mostPlayerBingos = (game.ID, game.Player1Bingos);
            }

            if (game.Player2Bingos > mostPlayerBingos.Item2)
            {
                mostPlayerBingos = (game.ID, game.Player2Bingos);
            }

            int gameTurns = game.Player1Turns + game.Player2Turns;
            turnCount += gameTurns;

            if (gameTurns > mostGameTurns.Item2)
            {
                mostGameTurns = (game.ID, gameTurns);
            }

            if (gameTurns < fewestGameTurns.Item2)
            {
                fewestGameTurns = (game.ID, gameTurns);
            }

            if (game.Winner == 0)
            {
                ties++;
            }
            else if (game.Winner == game.FirstPlayer)
            {
                firstPlayerWins++;
            }
            else
            {
                secondPlayerWins++;
            }
        }

        if (gameCount == 0)
        {
            return;
        }

        decimal averageGameScore = (decimal)totalScore / gameCount;
        decimal averagePlayerScore = (decimal)totalScore / scoreCount;
        decimal averageFirstScore = (decimal)firstScoreTotal / gameCount;
        decimal averageSecondScore = (decimal)secondScoreTotal / gameCount;
        decimal averageGameBingos = (decimal)bingoCount / gameCount;
        decimal averageGameTurns = (decimal)turnCount / gameCount;

        using var writer = new StreamWriter(output);
        writer.AutoFlush = true;

        writer.WriteLine($"Games: {gameCount}");
        writer.WriteLine($"Turns: {turnCount}");
        writer.WriteLine($"Points scored: {totalScore}");
        writer.WriteLine();
        writer.WriteLine($"Win for player going first: {firstPlayerWins} {Percent(firstPlayerWins, gameCount)}");
        writer.WriteLine($"Win for player going second: {secondPlayerWins} {Percent(secondPlayerWins, gameCount)}");
        writer.WriteLine($"Tie: {ties}");
        writer.WriteLine();
        writer.WriteLine($"Games with no bingos played: {zeroBingoGames} {Percent(zeroBingoGames, gameCount)}");
        writer.WriteLine();
        writer.WriteLine($"Average game score: {averageGameScore:F2}");
        writer.WriteLine($"Average player score: {averagePlayerScore:F2}");
        writer.WriteLine($"Average score when going first: {averageFirstScore:F2}");
        writer.WriteLine($"Average score when going second: {averageSecondScore:F2}");
        writer.WriteLine($"Average game bingos: {averageGameBingos:F2}");
        writer.WriteLine($"Average game turns: {averageGameTurns:F2}");
        writer.WriteLine();
        writer.WriteLine($"Highest game score: {highestGameScore.Item2} in game {highestGameScore.Item1}");
        writer.WriteLine($"Lowest game score: {lowestGameScore.Item2} in game {lowestGameScore.Item1}");
        writer.WriteLine($"Highest player score: {highestPlayerScore.Item2} in game {highestPlayerScore.Item1}");
        writer.WriteLine($"Lowest player score: {lowestPlayerScore.Item2} in game {lowestPlayerScore.Item1}");
        writer.WriteLine($"Largest winning margin: {largestWinningMargin.Item2} in game {largestWinningMargin.Item1}");
        writer.WriteLine($"Most game bingos: {mostGameBingos.Item2} in game {mostGameBingos.Item1}");
        writer.WriteLine($"Most player bingos: {mostPlayerBingos.Item2} in game {mostPlayerBingos.Item1}");
        writer.WriteLine($"Most game turns: {mostGameTurns.Item2} in game {mostGameTurns.Item1}");
        writer.WriteLine($"Fewest game turns: {fewestGameTurns.Item2} in game {fewestGameTurns.Item1}");
    }


    public static void AnalysePlays(IEnumerable<ScrabbleGame> games, Stream output)
    {
        long gameCount = 0;
        long turnCount = 0;
        long totalScore = 0;
        long moveCount = 0;
        long passCount = 0;
        long exchangeCount = 0;
        var wordPlays = new Dictionary<string, int>();
        var wordScores = new Dictionary<string, int>();
        var tilesPlayed = new Dictionary<int, int>();
        var racks = new Dictionary<string, int>();

        (ScrabblePlay?, int) highestPlayScore = (null, int.MinValue);
        (ScrabblePlay?, int) mostConsecutiveBingos = (null, int.MinValue);

        foreach (var game in games)
        {
            gameCount++;
            int consecutiveBingos = 0;

            foreach (var play in game.Plays)
            {
                turnCount++;
                totalScore += play.Score;

                if (play.Score > highestPlayScore.Item2)
                {
                    highestPlayScore = (play, play.Score);
                }

                if (play.TilesPlayed == 7)
                {
                    consecutiveBingos++;

                    if (consecutiveBingos > mostConsecutiveBingos.Item2)
                    {
                        mostConsecutiveBingos = (play, consecutiveBingos);
                    }
                }
                else
                {
                    consecutiveBingos = 0;
                }

                if (play.Type == ScrabblePlayType.Move)
                {
                    moveCount++;
                }
                else if (play.Type == ScrabblePlayType.Pass)
                {
                    passCount++;
                }
                else if (play.Type == ScrabblePlayType.Exchange)
                {
                    exchangeCount++;
                }

                if (play.Word != null)
                {
                    wordPlays[play.Word] = wordPlays.GetValueOrDefault(play.Word) + 1;
                    wordScores[play.Word] = wordScores.GetValueOrDefault(play.Word) + play.Score;
                }

                if (play.Rack != null)
                {
                    racks[play.Rack] = racks.GetValueOrDefault(play.Rack) + 1;
                }

                tilesPlayed[play.TilesPlayed] = tilesPlayed.GetValueOrDefault(play.TilesPlayed) + 1;
            }

            if (gameCount >= 100000)
            {
                break;
            }
        }

        if (turnCount == 0)
        {
            return;
        }

        decimal averagePlayScore = (decimal)totalScore / turnCount;
        var mostPlayedWord = wordPlays.MaxBy(x => x.Value);
        var averageWordScores = wordPlays.Where(x => x.Value >= 50).ToDictionary(w => w.Key, w => (decimal)wordScores[w.Key] / w.Value);
        var maxAvgWordScore = averageWordScores.MaxBy(x => x.Value);
        var mostCommonRack = racks.MaxBy(x => x.Value);
        var mostCommonFullRack = racks.Where(x => x.Key.Length == 7).MaxBy(x => x.Value);
        const int uniqueRacks = 3199724;

        using var writer = new StreamWriter(output);
        writer.AutoFlush = true;

        writer.WriteLine($"Games: {gameCount}");
        writer.WriteLine($"Turns: {turnCount}");
        writer.WriteLine($"Points scored: {totalScore}");
        writer.WriteLine();
        writer.WriteLine($"Turns with tiles played: {moveCount} {Percent(moveCount, turnCount)}");
        writer.WriteLine($"Exchanges: {exchangeCount} {Percent(exchangeCount, turnCount)}");
        writer.WriteLine($"Passes: {passCount} {Percent(passCount, turnCount)}");
        writer.WriteLine();
        writer.WriteLine($"Turns with no tiles played: {tilesPlayed.GetValueOrDefault(0)} {Percent(tilesPlayed.GetValueOrDefault(0), turnCount)}");
        writer.WriteLine($"Turns with 1 tile played: {tilesPlayed.GetValueOrDefault(1)} {Percent(tilesPlayed.GetValueOrDefault(1), turnCount)}");
        writer.WriteLine($"Turns with 2 tiles played: {tilesPlayed.GetValueOrDefault(2)} {Percent(tilesPlayed.GetValueOrDefault(2), turnCount)}");
        writer.WriteLine($"Turns with 3 tiles played: {tilesPlayed.GetValueOrDefault(3)} {Percent(tilesPlayed.GetValueOrDefault(3), turnCount)}");
        writer.WriteLine($"Turns with 4 tiles played: {tilesPlayed.GetValueOrDefault(4)} {Percent(tilesPlayed.GetValueOrDefault(4), turnCount)}");
        writer.WriteLine($"Turns with 5 tiles played: {tilesPlayed.GetValueOrDefault(5)} {Percent(tilesPlayed.GetValueOrDefault(5), turnCount)}");
        writer.WriteLine($"Turns with 6 tiles played: {tilesPlayed.GetValueOrDefault(6)} {Percent(tilesPlayed.GetValueOrDefault(6), turnCount)}");
        writer.WriteLine($"Turns with 7 tiles played: {tilesPlayed.GetValueOrDefault(7)} {Percent(tilesPlayed.GetValueOrDefault(7), turnCount)}");
        writer.WriteLine();
        writer.WriteLine($"Average play score: {averagePlayScore:F2}");
        writer.WriteLine($"Highest scoring play: {highestPlayScore.Item1} for {highestPlayScore.Item2} in game {highestPlayScore.Item1?.ID}");
        writer.WriteLine();
        writer.WriteLine($"Most consecutive bingos: {mostConsecutiveBingos.Item2} in game {mostConsecutiveBingos.Item1?.ID}");
        writer.WriteLine();
        writer.WriteLine($"Unique words played: {wordPlays.Count}");
        writer.WriteLine($"Most played word: {mostPlayedWord.Key} played {mostPlayedWord.Value} times {Percent(mostPlayedWord.Value, turnCount)}");
        writer.WriteLine($"Words played only once: {wordPlays.Count(w => w.Value == 1)}");
        writer.WriteLine($"Word with highest average score (minimum 50 plays): {maxAvgWordScore.Key} played {wordPlays[maxAvgWordScore.Key]} times with an average score of {maxAvgWordScore.Value:F2}");
        writer.WriteLine();
        writer.WriteLine($"Unique racks seen: {racks.Count} / {uniqueRacks} {Percent(racks.Count, uniqueRacks)}");
        writer.WriteLine($"Most common rack: {mostCommonRack.Key} seen {mostCommonRack.Value} times");
        writer.WriteLine($"Most common full rack: {mostCommonFullRack.Key} seen {mostCommonFullRack.Value} times");
    }


    public static void AnalyseWords(IEnumerable<ScrabbleGame> games, Stream output)
    {
        var wordPlays = new Dictionary<string, int>();
        var wordScores = new Dictionary<string, int>();
        var winCounts = new Dictionary<string, decimal>();
        var lossCounts = new Dictionary<string, decimal>();
        
        int gameCount = 0;

        foreach (var game in games)
        {
            gameCount++;

            foreach (var play in game.Plays)
            {
                void AnalyseWord(string? word)
                {
                    if (word == null) return;

                    wordPlays[word] = wordPlays.GetValueOrDefault(word) + 1;
                    wordScores[word] = wordScores.GetValueOrDefault(word) + play.Score;

                    if (game.Winner == play.PlayerNumber)
                    {
                        winCounts[word] = winCounts.GetValueOrDefault(word) + 1;
                    }
                    else if (game.Winner == 0)
                    {
                        winCounts[word] = winCounts.GetValueOrDefault(word) + 0.5m;
                        lossCounts[word] = lossCounts.GetValueOrDefault(word) + 0.5m;
                    }
                    else
                    {
                        lossCounts[word] = lossCounts.GetValueOrDefault(word) + 1;
                    }
                }

                if (game.IsComplete)
                {
                    AnalyseWord(play.Word);

                    if (play.AdditionalWords != null)
                    {
                        foreach (var word in play.AdditionalWords)
                        {
                            AnalyseWord(word);
                        }
                    }
                }
            }

            if (gameCount % 100000 == 0)
            {
                Console.WriteLine(gameCount + "...");
            }
        }

        using var writer = new StreamWriter(output);
        writer.AutoFlush = true;
        writer.WriteLine("Word,Plays,AvgScorePerPlay,AvgScorePerGame,WinRate");

        foreach (var item in wordScores.OrderByDescending(x => x.Value))
        {
            decimal wordScore = wordScores[item.Key];
            decimal avgGameScore = wordScore / gameCount;
            decimal avgPlayScore = 0;
            decimal winRate = 0;

            int plays = wordPlays[item.Key];

            if (plays > 0)
            {
                avgPlayScore = wordScore / plays;
            }

            if (winCounts.ContainsKey(item.Key) || lossCounts.ContainsKey(item.Key))
            {
                winRate = winCounts.GetValueOrDefault(item.Key) / (winCounts.GetValueOrDefault(item.Key) + lossCounts.GetValueOrDefault(item.Key));
            }

            if (plays > 10)
            {
                writer.WriteLine($"{item.Key},{plays},{avgPlayScore:F2},{avgGameScore:F8},{winRate:F4}");
            }
        }
    }


    public static void AnalyseAlphas(IEnumerable<ScrabbleGame> games, Stream output)
    {
        var alphaPlays = new Dictionary<string, int>();
        var alphaScores = new Dictionary<string, int>();
        int gameCount = 0;

        foreach (var game in games)
        {
            gameCount++;

            foreach (var play in game.Plays)
            {
                if (play.Word != null)
                {
                    string alpha = new(play.Word.Order().ToArray());
                    alphaPlays[alpha] = alphaPlays.GetValueOrDefault(alpha) + 1;
                    alphaScores[alpha] = alphaScores.GetValueOrDefault(alpha) + play.Score;
                }
            }

            if (gameCount >= 100000)
            {
                break;
            }
        }

        using var writer = new StreamWriter(output);
        writer.AutoFlush = true;
        writer.WriteLine("Alphagram,Plays,AvgPlayScore,AvgGameScore");

        foreach (var item in alphaScores.OrderByDescending(x => x.Value))
        {
            decimal alphaScore = alphaScores[item.Key];
            decimal avgGameScore = alphaScore / gameCount;
            decimal avgPlayScore = 0;

            int plays = alphaPlays[item.Key];

            if (plays > 0)
            {
                avgPlayScore = alphaScore / plays;
            }

            writer.WriteLine($"{item.Key},{plays},{avgPlayScore:F2},{avgGameScore:F8}");
        }
    }


    private static string Percent(long value, long total)
    {
        return $"({(decimal)(value * 100) / total:F2}%)";
    }
}
