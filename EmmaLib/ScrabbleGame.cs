
namespace EmmaLib;

/// <summary>
/// Represents a game of Scrabble.
/// </summary>
public class ScrabbleGame
{ 
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
    /// The plays made in this game.
    /// </summary>
    public List<ScrabblePlay> Plays { get; } = new();


    /// <summary>
    /// Creates a representation of a Scrabble game between two players.
    /// </summary>
    /// <param name="id">Unique game identifier.</param>
    /// <param name="player1Name">First player's name.</param>
    /// <param name="player2Name">Second player's name.</param>
    public ScrabbleGame(string id, string player1Name, string player2Name)
    {
        ID = id;
        Player1Name = player1Name;
        Player2Name = player2Name;
    }
}
