
namespace Emma.Lib;

/// <summary>
/// Represents a play in a game of scrabble. This includes moves, exchanges and passes.
/// </summary>
public class ScrabblePlay
{
    /// <summary>
    /// Unique game identifier.
    /// </summary>
    public string? GameID { get; set; }

    /// <summary>
    /// Unique play identifier, comprising unique game identifier and turn number.
    /// </summary>
    public string? ID { get; set; }

    /// <summary>
    /// The number of the player who made this play.
    /// </summary>
    public int PlayerNumber { get; set; }

    /// <summary>
    /// The type of play.
    /// </summary>
    public ScrabblePlayType Type { get; set; }

    /// <summary>
    /// The turn number (1-indexed) on which this play was made.
    /// </summary>
    public int TurnNumber { get; set; }

    /// <summary>
    /// The player's rack before making the play.
    /// </summary>
    public string? Rack { get; set; }

    /// <summary>
    /// The score for the play.
    /// </summary>
    public int Score { get; set; }

    /// <summary>
    /// Number of tiles played on this turn.
    /// </summary>
    public int TilesPlayed { get; set; }

    /// <summary>
    /// The tiles left on the player's rack after making the play.
    /// </summary>
    public string? Leave { get; set; }

    /// <summary>
    /// Equity score assigned to the play by an analyser such as Macondo.
    /// </summary>
    public decimal Equity { get; set; }

    /// <summary>
    /// Number of tiles remaining in the bag before this play was made.
    /// </summary>
    public int TilesRemaining { get; set; }

    /// <summary>
    /// Position on the board of the first tile in the longest word formed (0-indexed). Set to (-1, -1) if no tiles were played.
    /// </summary>
    public (int Row, int Column) Position { get; set; }

    /// <summary>
    /// Whether the play was made vertically.
    /// </summary>
    public bool Vertical { get; set; }

    /// <summary>
    /// The number of consecutive plays, including this one, that had a score of zero. Set to 0 if the play earned points.
    /// </summary>
    public int PassNumber { get; set; }

    /// <summary>
    /// The primary word played. If only one tile was played, this may be either of the words formed.
    /// </summary>
    public string? Word { get; set; }

    /// <summary>
    /// Additional words formed by crossing with the primary word.
    /// </summary>
    public IList<string>? AdditionalWords { get; set; }

    /// <summary>
    /// The play as described by Macondo ('.' represents tile already played).
    /// </summary>
    public string? MacondoString { get; set; }

    /// <summary>
    /// Returns the play in typical notation (tiles already played are enclosed in parentheses).
    /// </summary>
    public string? PlayString { get; set; }

    /// <summary>
    /// Returns a string representing the object.
    /// </summary>
    public override string? ToString()
    {
        return PlayString;
    }
}



/// <summary>
/// Specifies the type of play.
/// </summary>
public enum ScrabblePlayType
{
    /// <summary>
    /// A word was played on the board.
    /// </summary>
    Move,

    /// <summary>
    /// The player passed their turn without playing a word or exchanging.
    /// </summary>
    Pass,

    /// <summary>
    /// The player exchanged tiles.
    /// </summary>
    Exchange,

    /// <summary>
    /// The player gained points for other player's remaining tiles.
    /// </summary>
    Out,

    /// <summary>
    /// The player lost points due to their remaining tiles.
    /// </summary>
    RemainingTiles
}