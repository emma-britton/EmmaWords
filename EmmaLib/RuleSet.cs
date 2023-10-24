
namespace Emma.Lib;

/// <summary>
/// The rules of a game of Scrabble or its variants.
/// </summary>
public class RuleSet
{
    public const int __ = 0;
    public const int DL = 1;
    public const int DW = 2;
    public const int TL = 3;
    public const int TW = 4;
    public const int QL = 5;
    public const int QW = 6;

    static readonly Dictionary<string, string> StandardTileSet = new()
    {
        ["A"] = "A",
        ["B"] = "B",
        ["C"] = "C",
        ["D"] = "D",
        ["E"] = "E",
        ["F"] = "F",
        ["G"] = "G",
        ["H"] = "H",
        ["I"] = "I",
        ["J"] = "J",
        ["K"] = "K",
        ["L"] = "L",
        ["M"] = "M",
        ["N"] = "N",
        ["O"] = "O",
        ["P"] = "P",
        ["Q"] = "Q",
        ["R"] = "R",
        ["S"] = "S",
        ["T"] = "T",
        ["U"] = "U",
        ["V"] = "V",
        ["W"] = "W",
        ["X"] = "X",
        ["Y"] = "Y",
        ["Z"] = "Z",
        ["?"] = " ",
    };

    static readonly Dictionary<string, int> StandardTileDistribution = new()
    {
        ["A"] = 9,
        ["B"] = 2,
        ["C"] = 2,
        ["D"] = 4,
        ["E"] = 12,
        ["F"] = 2,
        ["G"] = 3,
        ["H"] = 2,
        ["I"] = 9,
        ["J"] = 1,
        ["K"] = 1,
        ["L"] = 4,
        ["M"] = 2,
        ["N"] = 6,
        ["O"] = 8,
        ["P"] = 2,
        ["Q"] = 1,
        ["R"] = 6,
        ["S"] = 4,
        ["T"] = 6,
        ["U"] = 4,
        ["V"] = 2,
        ["W"] = 2,
        ["X"] = 1,
        ["Y"] = 2,
        ["Z"] = 1,
        ["?"] = 2,
    };

    static readonly Dictionary<string, int> StandardPointDistribution = new()
    {
        ["A"] = 1,
        ["B"] = 3,
        ["C"] = 3,
        ["D"] = 2,
        ["E"] = 1,
        ["F"] = 4,
        ["G"] = 2,
        ["H"] = 4,
        ["I"] = 1,
        ["J"] = 8,
        ["K"] = 5,
        ["L"] = 1,
        ["M"] = 3,
        ["N"] = 1,
        ["O"] = 1,
        ["P"] = 3,
        ["Q"] = 10,
        ["R"] = 1,
        ["S"] = 1,
        ["T"] = 1,
        ["U"] = 1,
        ["V"] = 4,
        ["W"] = 4,
        ["X"] = 8,
        ["Y"] = 4,
        ["Z"] = 10,
        ["?"] = 0
    };

    static readonly Dictionary<string, int> SuperTileDistribution = new()
    {
        ["A"] = 16,
        ["B"] = 4,
        ["C"] = 6,
        ["D"] = 8,
        ["E"] = 24,
        ["F"] = 4,
        ["G"] = 5,
        ["H"] = 5,
        ["I"] = 13,
        ["J"] = 2,
        ["K"] = 2,
        ["L"] = 7,
        ["M"] = 6,
        ["N"] = 13,
        ["O"] = 15,
        ["P"] = 4,
        ["Q"] = 2,
        ["R"] = 13,
        ["S"] = 10,
        ["T"] = 15,
        ["U"] = 7,
        ["V"] = 3,
        ["W"] = 4,
        ["X"] = 2,
        ["Y"] = 4,
        ["Z"] = 2,
        ["?"] = 4,
    };

    static readonly int[,] StandardBoardLayout = new int[,]
    {
        { TW, __, __, DL, __, __, __, TW, __, __, __, DL, __, __, TW },
        { __, DW, __, __, __, TL, __, __, __, TL, __, __, __, DW, __ },
        { __, __, DW, __, __, __, DL, __, DL, __, __, __, DW, __, __ },
        { DL, __, __, DW, __, __, __, DL, __, __, __, DW, __, __, DL },
        { __, __, __, __, DW, __, __, __, __, __, DW, __, __, __, __ },
        { __, TL, __, __, __, TL, __, __, __, TL, __, __, __, TL, __ },
        { __, __, DL, __, __, __, DL, __, DL, __, __, __, DL, __, __ },
        { TW, __, __, DL, __, __, __, DW, __, __, __, DL, __, __, TW },
        { __, __, DL, __, __, __, DL, __, DL, __, __, __, DL, __, __ },
        { __, TL, __, __, __, TL, __, __, __, TL, __, __, __, TL, __ },
        { __, __, __, __, DW, __, __, __, __, __, DW, __, __, __, __ },
        { DL, __, __, DW, __, __, __, DL, __, __, __, DW, __, __, DL },
        { __, __, DW, __, __, __, DL, __, DL, __, __, __, DW, __, __ },
        { __, DW, __, __, __, TL, __, __, __, TL, __, __, __, DW, __ },
        { TW, __, __, DL, __, __, __, TW, __, __, __, DL, __, __, TW },
    };

    static readonly int[,] SuperBoardLayout = new int[,]
    {
        { QW, __, __, DL, __, __, __, TW, __, __, DL, __, __, TW, __, __, __, DL, __, __, QW },
        { __, DW, __, __, TL, __, __, __, DW, __, __, __, DW, __, __, __, TL, __, __, DW, __ },
        { __, __, DW, __, __, QL, __, __, __, DW, __, DW, __, __, __, QL, __, __, DW, __, __ },
        { DL, __, __, TW, __, __, DL, __, __, __, TW, __, __, __, DL, __, __, TW, __, __, DL },
        { __, TL, __, __, DW, __, __, __, TL, __, __, __, TL, __, __, __, DW, __, __, TL, __ },
        { __, __, QL, __, __, DW, __, __, __, DL, __, DL, __, __, __, DW, __, __, QL, __, __ },
        { __, __, __, DL, __, __, DW, __, __, __, DL, __, __, __, DW, __, __, DL, __, __, __ },
        { TW, __, __, __, __, __, __, DW, __, __, __, __, __, DW, __, __, __, __, __, __, TW },
        { __, DW, __, __, TL, __, __, __, TL, __, __, __, TL, __, __, __, TL, __, __, DW, __ },
        { __, __, DW, __, __, DL, __, __, __, DL, __, DL, __, __, __, DL, __, __, DW, __, __ },
        { DL, __, __, TW, __, __, DL, __, __, __, DW, __, __, __, DL, __, __, TW, __, __, DL },
        { __, __, DW, __, __, DL, __, __, __, DL, __, DL, __, __, __, DL, __, __, DW, __, __ },
        { __, DW, __, __, TL, __, __, __, TL, __, __, __, TL, __, __, __, TL, __, __, DW, __ },
        { TW, __, __, __, __, __, __, DW, __, __, __, __, __, DW, __, __, __, __, __, __, TW },
        { __, __, __, DL, __, __, DW, __, __, __, DL, __, __, __, DW, __, __, DL, __, __, __ },
        { __, __, QL, __, __, DW, __, __, __, DL, __, DL, __, __, __, DW, __, __, QL, __, __ },
        { __, TL, __, __, DW, __, __, __, TL, __, __, __, TL, __, __, __, DW, __, __, TL, __ },
        { DL, __, __, TW, __, __, DL, __, __, __, TW, __, __, __, DL, __, __, TW, __, __, DL },
        { __, __, DW, __, __, QL, __, __, __, DW, __, DW, __, __, __, QL, __, __, DW, __, __ },
        { __, DW, __, __, TL, __, __, __, DW, __, __, __, DW, __, __, __, TL, __, __, DW, __ },
        { QW, __, __, DL, __, __, __, TW, __, __, DL, __, __, TW, __, __, __, DL, __, __, QW },
    };


    /// <summary>
    /// Number of rows and columns on the board.
    /// </summary>
    public int BoardSize => Board.GetLength(0);
    /// <summary>
    /// Maximum number of tiles each player holds in their rack.
    /// </summary>
    public int RackSize { get; set; }

    /// <summary>
    /// Total number of tiles used for the game.
    /// </summary>
    public int BagSize => TileDistribution.Values.Sum();

    /// <summary>
    /// Whether each player is permitted to designate one tile in their play as a blank per turn, unless they have a blank tile on that turn.
    /// </summary>
    public bool IfOnlyVariant { get; set; } = false;

    /// <summary>
    /// Whether to prevent the play of words not in the selected lexicon.
    /// </summary>
    public bool ValidateWords { get; set; } = true;

    /// <summary>
    /// The number of copies of each tile used for the game.
    /// </summary>
    public Dictionary<string, int> TileDistribution { get; set; }

    /// <summary>
    /// What to display on the face of each tile.
    /// </summary>
    public Dictionary<string, string> TileDisplay { get; set; }

    /// <summary>
    /// The point value assigned to each tile.
    /// </summary>
    public Dictionary<string, int> TilePoints { get; set; }

    /// <summary>
    /// Name of the rule set.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Description of the rule set.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// The board design.
    /// </summary>
    public int[,] Board { get; set; }

    /// <summary>
    /// Location of the star through which the first word must be played.
    /// </summary>
    public (int, int) Star { get; set; }

    /// <summary>
    /// Number of additional points given for playing all tiles from a full rack.
    /// </summary>
    public int BingoScore { get; set; } = 50;


    /// <summary>
    /// The standard rule set.
    /// </summary>
    public static RuleSet Standard { get; } = new RuleSet("standard Scrabble", "Standard Scrabble rules", 7, StandardBoardLayout,
        StandardTileSet, StandardTileDistribution, StandardPointDistribution);


    /// <summary>
    /// The super rule set (larger board and twice as many tiles).
    /// </summary>
    public static RuleSet Super { get; } = new RuleSet("Super Scrabble", "Super Scrabble rules", 7, SuperBoardLayout,
        StandardTileSet, SuperTileDistribution, StandardPointDistribution);


    /// <summary>
    /// Creates a custom rule set.
    /// </summary>
    /// <param name="name">Rule set name.</param>
    /// <param name="description">Description of the rule set.</param>
    /// <param name="rackSize">Maximum number of tiles each player holds in their rack.</param>
    /// <param name="board">The board design.</param>
    /// <param name="tileDisplay">What to display on the face of each tile.</param>
    /// <param name="tileDistribution">The number of copies of each tile used for the game.</param>
    /// <param name="tilePoints">The point value assigned to each tile.</param>
    public RuleSet(string name, string description, int rackSize, int[,] board, Dictionary<string, string> tileDisplay,
        Dictionary<string, int> tileDistribution, Dictionary<string, int> tilePoints)
    {
        Name = name;
        Description = description;
        RackSize = rackSize;
        Board = board;
        TileDisplay = tileDisplay;
        TileDistribution = tileDistribution;
        TilePoints = tilePoints;
        Star = (BoardSize / 2, BoardSize / 2);
    }


    /// <summary>
    /// Determines whether a word is playable in this ruleset.
    /// </summary>
    /// <param name="word">The word to check.</param>
    public bool IsWordPlayable(string word)
    {
        if (word.Length > BoardSize) return false;

        var copy = new Dictionary<string, int>(TileDistribution);

        foreach (char c in word)
        {
            if (!copy.TryGetValue(c.ToString(), out int count))
            {
                return false;
            }

            if (count == 0)
            {
                if (copy["?"] is int blanks && blanks == 0)
                {
                    return false;
                }

                copy["?"] = blanks - 1;
            }
            else
            {
                copy[c.ToString()] = count - 1;
            }
        }

        return true;
    }


    /// <summary>
    /// Calculates the relative probability of a word being played in this ruleset.
    /// This is not the same as the probability of drawing the word onto the rack (which is not defined for words longer than the rack size).
    /// </summary>
    /// <param name="word">The word.</param>
    public decimal WordProbability(string word)
    {
        decimal prob = 1;
        int bagSize = BagSize;
        var dist = new Dictionary<string, int>(TileDistribution);

        foreach (char c in word)
        {
            if (dist.TryGetValue(c.ToString(), out int count) && count > 0)
            {
                prob *= (decimal)count / bagSize;
                bagSize--;
                dist[c.ToString()]--;
            }
            else if (dist.TryGetValue("?", out int blanks) && blanks > 0)
            {
                prob *= (decimal)blanks / bagSize;
                bagSize--;
                dist["?"]--;
            }
            else
            {
                return 0;
            }
        }

        return prob;
    }



    /// <summary>
    /// Makes a copy of this rule set.
    /// </summary>
    public RuleSet Clone()
    {
        string newName = "Variant rules";
        var copy = new RuleSet(newName, Description, RackSize, Board, new Dictionary<string, string>(TileDisplay),
            new Dictionary<string, int>(TileDistribution), new Dictionary<string, int>(TilePoints));
        return copy;
    }


    /// <summary>
    /// Returns a string representing the object.
    /// </summary>
    public override string ToString()
    {
        return Name;
    }
}
