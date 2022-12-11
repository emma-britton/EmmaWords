

namespace EmmaWords
{
    public class RuleSet
    {
        public const int nn = 0;
        public const int dl = 1;
        public const int dw = 2;
        public const int tl = 3;
        public const int tw = 4;

        public int BoardSize { get; set; }
        public int RackSize { get; set; }
        public int BagSize { get; set; }
        public bool AllowFlip { get; set; } = false;
        public bool ValidateWords { get; set; } = true;
        public Dictionary<string, int> LetterDistribution { get; set; }
        public Dictionary<string, string> LetterDisplay { get; set; }
        public Dictionary<string, int> LetterPoints { get; set; }

        public string Player1Name { get; set; } = "Player 1";
        public string Player2Name { get; set; } = "Player 2";
        public string Name { get; set; }
        public string Description { get; set; }
        public int[,] Board { get; set; }
        public (int, int) Star { get; set; }
        public int BingoScore { get; set; } = 50;


        public static RuleSet Scrabble { get; } =
            new RuleSet("Scrabble", 15, 7, 100,
            new Dictionary<string, int>
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
            },

            new Dictionary<string, string>
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
            },

            new Dictionary<string, int>
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
            },

            new int[,]
            {
                { tw, nn, nn, dl, nn, nn, nn, tw, nn, nn, nn, dl, nn, nn, tw },
                { nn, dw, nn, nn, nn, tl, nn, nn, nn, tl, nn, nn, nn, dw, nn },
                { nn, nn, dw, nn, nn, nn, dl, nn, dl, nn, nn, nn, dw, nn, nn },
                { dl, nn, nn, dw, nn, nn, nn, dl, nn, nn, nn, dw, nn, nn, dl },
                { nn, nn, nn, nn, dw, nn, nn, nn, nn, nn, dw, nn, nn, nn, nn },
                { nn, tl, nn, nn, nn, tl, nn, nn, nn, tl, nn, nn, nn, tl, nn },
                { nn, nn, dl, nn, nn, nn, dl, nn, dl, nn, nn, nn, dl, nn, nn },
                { tw, nn, nn, dl, nn, nn, nn, dw, nn, nn, nn, dl, nn, nn, tw },
                { nn, nn, dl, nn, nn, nn, dl, nn, dl, nn, nn, nn, dl, nn, nn },
                { nn, tl, nn, nn, nn, tl, nn, nn, nn, tl, nn, nn, nn, tl, nn },
                { nn, nn, nn, nn, dw, nn, nn, nn, nn, nn, dw, nn, nn, nn, nn },
                { dl, nn, nn, dw, nn, nn, nn, dl, nn, nn, nn, dw, nn, nn, dl },
                { nn, nn, dw, nn, nn, nn, dl, nn, dl, nn, nn, nn, dw, nn, nn },
                { nn, dw, nn, nn, nn, tl, nn, nn, nn, tl, nn, nn, nn, dw, nn },
                { tw, nn, nn, dl, nn, nn, nn, tw, nn, nn, nn, dl, nn, nn, tw },
             });

        public static RuleSet SuperScrabble { get; } =
            new RuleSet("SuperScrabble", 21, 7, 200,
            new Dictionary<string, int>
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
            },

            new Dictionary<string, string>
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
            },

            new Dictionary<string, int>
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
            },

            new int[,]
            {
            });


        public RuleSet(string name, int boardSize, int rackSize, int bagSize,
            Dictionary<string, int> letterDistribution, Dictionary<string, string> letterDisplay, Dictionary<string, int> letterPoints, int[,] board)
        {
            Name = name;
            BoardSize = boardSize;
            RackSize = rackSize;
            BagSize = bagSize;
            LetterDistribution = letterDistribution;
            LetterDisplay = letterDisplay;
            LetterPoints = letterPoints;
            Board = board;
            Star = (boardSize / 2, boardSize / 2);
        }


        public bool IsWordPlayable(string word)
        {
            if (word.Length > BoardSize) return false;

            var copy = new Dictionary<string, int>(LetterDistribution);

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


        public override string ToString()
        {
            return Name;
        }
    }
}
