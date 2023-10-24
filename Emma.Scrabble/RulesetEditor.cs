using Emma.Lib;
using System.Text.RegularExpressions;

namespace Emma.Scrabble;

public partial class RulesetEditor : Form
{
    private readonly WordService m_WordService;
    private readonly List<(string letter, string display, int count, int points)> Distribution = new();

    public RuleSet RuleSet { get; set; }

    public string Player1Name => FirstPlayerName.Text;
    public string Player2Name => SecondPlayerName.Text;


    public RulesetEditor(WordService wordService)
    {
        InitializeComponent();
        m_WordService = wordService;
        RuleSet = wordService.ActiveRuleSet;

        Description.Text = RuleSet.Description;
        RackSize.Value = RuleSet.RackSize;
        BoardSize.Value = RuleSet.BoardSize;
        BingoScore.Value = RuleSet.BingoScore;
        AllowIfOnly.Checked = RuleSet.IfOnlyVariant;
        ValidateWords.Checked = RuleSet.ValidateWords;

        SetUpBoardEditor();

        Distribution.Clear();

        foreach (var item in RuleSet.TileDistribution.OrderBy(x => x.Key))
        {
            Distribution.Add((item.Key, RuleSet.TileDisplay[item.Key] ?? " ", item.Value, RuleSet.TilePoints.GetValueOrDefault(item.Key, 0)));
        }

        while (Distribution.Count < 40)
        {
            Distribution.Add(("", "", 0, 0));
        }

        SetUpDistributionEditor();
        TileTotal.Text = Distribution.Sum(d => d.count).ToString() + " tiles";
    }


    private void ContinueButton_Click(object sender, EventArgs e)
    {
        RuleSet.Description = Description.Text;
        RuleSet.RackSize = (int)RackSize.Value;
        RuleSet.BingoScore = (int)BingoScore.Value;
        RuleSet.IfOnlyVariant = AllowIfOnly.Checked;
        RuleSet.ValidateWords = ValidateWords.Checked;

        RuleSet.TileDistribution.Clear();
        RuleSet.TilePoints.Clear();
        RuleSet.TileDisplay.Clear();

        foreach (var (letter, display, count, points) in Distribution)
        {
            if (letter.Trim() != "")
            {
                RuleSet.TileDistribution[letter] = count;
                RuleSet.TileDisplay[letter] = display == "" ? " " : display;
                RuleSet.TilePoints[letter] = points;
            }
        }

        Close();
    }


    private void BoardSize_ValueChanged(object sender, EventArgs e)
    {
        RuleSet.Board = ResizeArray(RuleSet.Board, (int)BoardSize.Value, (int)BoardSize.Value);
        RuleSet.Star = (RuleSet.BoardSize / 2, RuleSet.BoardSize / 2);

        SetUpBoardEditor();
    }


    private static T[,] ResizeArray<T>(T[,] original, int x, int y)
    {
        var newArray = new T[x, y];
        int minX = Math.Min(original.GetLength(0), newArray.GetLength(0));
        int minY = Math.Min(original.GetLength(1), newArray.GetLength(1));

        for (int i = 0; i < minY; ++i)
        {
            Array.Copy(original, i * original.GetLength(0), newArray, i * newArray.GetLength(0), minX);
        }

        return newArray;
    }


    private void SetUpBoardEditor()
    {
        BoardEditor.SuspendLayout();
        BoardEditor.Controls.Clear();
        BoardEditor.ColumnCount = RuleSet.BoardSize;
        BoardEditor.RowCount = RuleSet.BoardSize;

        for (int i = 0; i < BoardEditor.ColumnCount; i++)
        {
            if (BoardEditor.RowStyles.Count <= i)
            {
                BoardEditor.RowStyles.Add(new RowStyle { SizeType = SizeType.AutoSize });
            }

            if (BoardEditor.ColumnStyles.Count <= i)
            {
                BoardEditor.ColumnStyles.Add(new ColumnStyle { SizeType = SizeType.AutoSize });
            }

            BoardEditor.RowStyles[i].SizeType = SizeType.AutoSize;
            BoardEditor.ColumnStyles[i].SizeType = SizeType.AutoSize;

            for (int j = 0; j < BoardEditor.RowCount; j++)
            {
                static void SetColour(Button button, int type)
                {
                    button.BackColor = type switch
                    {
                        RuleSet.DW => Color.LightPink,
                        RuleSet.TW => Color.Tomato,
                        RuleSet.DL => Color.PaleTurquoise,
                        RuleSet.TL => Color.DodgerBlue,
                        _ => Color.White,
                    };
                }

                var button = new Button
                {
                    Width = 24,
                    Height = 24,
                    Tag = (j, i)
                };

                button.Click += (sender, e) =>
                {
                    (int j, int i) = ((int, int))button.Tag;

                    RuleSet.Board[j, i] += 1;
                    if (RuleSet.Board[j, i] > 4) RuleSet.Board[j, i] = 0;
                    SetColour(button, RuleSet.Board[j, i]);
                };

                int spaceType = RuleSet.Board[j, i];
                SetColour(button, spaceType);

                BoardEditor.Controls.Add(button);
            }
        }

        BoardEditor.ResumeLayout();
    }


    private void AllowIfOnly_CheckedChanged(object sender, EventArgs e)
    {
        RuleSet.IfOnlyVariant = AllowIfOnly.Checked;
    }


    private void ClearBoardDesign_Click(object sender, EventArgs e)
    {
        foreach (Control control in BoardEditor.Controls)
        {
            control.BackColor = Color.White;
        }

        for (int i = 0; i < RuleSet.BoardSize; i++)
        {
            for (int j = 0; j < RuleSet.BoardSize; j++)
            {
                RuleSet.Board[j, i] = 0;
            }
        }
    }


    private void ClearTilesButton_Click(object sender, EventArgs e)
    {
        for (int i = 0; i < Distribution.Count; i++)
        {
            var item = Distribution[i];
            item.count = 0;
            Distribution[i] = item;
        }

        SetUpDistributionEditor();
        TileTotal.Text = Distribution.Sum(d => d.count).ToString() + " tiles";
    }


    private void SetUpDistributionEditor()
    {
        TileDistributionEditor.SuspendLayout();
        TileDistributionEditor.Controls.Clear();
        TileDistributionEditor.ColumnCount = 4;
        TileDistributionEditor.RowCount = 40;

        TileDistributionEditor.ColumnStyles[0] = new ColumnStyle { SizeType = SizeType.Percent, Width = 20 };
        TileDistributionEditor.ColumnStyles.Add(new ColumnStyle { SizeType = SizeType.Percent, Width = 20 });
        TileDistributionEditor.ColumnStyles.Add(new ColumnStyle { SizeType = SizeType.Percent, Width = 20 });
        TileDistributionEditor.ColumnStyles.Add(new ColumnStyle { SizeType = SizeType.Percent, Width = 20 });


        for (int i = 0; i < Distribution.Count; i++)
        {
            if (TileDistributionEditor.RowStyles.Count <= i)
            {
                TileDistributionEditor.RowStyles.Add(new RowStyle());
            }

            TileDistributionEditor.RowStyles[i] = new RowStyle { SizeType = SizeType.Absolute, Height = 32 };

            var letterCtrl = new TextBox { Text = Distribution[i].letter, Tag = i };
            TileDistributionEditor.Controls.Add(letterCtrl, 0, i);

            letterCtrl.TextChanged += (sender, e) =>
            {
                int idx = (int)letterCtrl.Tag;
                var item = Distribution[idx];
                item.letter = letterCtrl.Text;
                Distribution[idx] = item;
            };

            var displayCtrl = new TextBox { Text = Distribution[i].display, Tag = i };
            TileDistributionEditor.Controls.Add(displayCtrl, 1, i);

            displayCtrl.TextChanged += (sender, e) =>
            {
                int idx = (int)displayCtrl.Tag;
                var item = Distribution[idx];
                item.display = displayCtrl.Text;
                Distribution[idx] = item;
            };

            var countCtrl = new NumericUpDown
            {
                Minimum = 0,
                Maximum = 100,
                Value = Distribution[i].count,
                Tag = i
            };

            TileDistributionEditor.Controls.Add(countCtrl, 2, i);

            countCtrl.ValueChanged += (sender, e) =>
            {
                int idx = (int)countCtrl.Tag;
                var item = Distribution[idx];
                item.count = (int)countCtrl.Value;
                Distribution[idx] = item;
                TileTotal.Text = Distribution.Sum(d => d.count).ToString() + " tiles";
            };

            var pointsCtrl = new NumericUpDown
            {
                Minimum = 0,
                Maximum = 100,
                Value = Distribution[i].points,
                Tag = i
            };

            TileDistributionEditor.Controls.Add(pointsCtrl, 3, i);

            pointsCtrl.ValueChanged += (sender, e) =>
            {
                int idx = (int)pointsCtrl.Tag;
                var item = Distribution[idx];
                item.points = (int)pointsCtrl.Value;
                Distribution[idx] = item;
            };
        }

        TileDistributionEditor.ResumeLayout();
    }


    private void ResetBoardButton_Click(object sender, EventArgs e)
    {
        RuleSet.Board = ResizeArray(RuleSet.Board, (int)BoardSize.Value, (int)BoardSize.Value);

        for (int i = 0; i < RuleSet.BoardSize; i++)
        {
            for (int j = 0; j < RuleSet.BoardSize; j++)
            {
                RuleSet.Board[j, i] = RuleSet.Standard.Board[j, i];
            }
        }

        SetUpBoardEditor();
    }


    private void ResetTilesButton_Click(object sender, EventArgs e)
    {
        RuleSet.TileDisplay = new(RuleSet.Standard.TileDisplay);
        RuleSet.TileDistribution = new(RuleSet.Standard.TileDistribution);
        RuleSet.TilePoints = new(RuleSet.Standard.TilePoints);

        Distribution.Clear();

        foreach (var item in RuleSet.TileDistribution.OrderBy(x => x.Key))
        {
            Distribution.Add((item.Key, RuleSet.TileDisplay[item.Key] ?? " ", item.Value, RuleSet.TilePoints.GetValueOrDefault(item.Key, 0)));
        }

        while (Distribution.Count < 40)
        {
            Distribution.Add(("", "", 0, 0));
        }

        SetUpDistributionEditor();
    }


    private void LoadButton_Click(object sender, EventArgs e)
    {
        string filename = Path.Combine(m_WordService.BaseFolder, "rules", "variant.txt");
        if (!File.Exists(filename)) return;

        var parms = new List<string>();

        bool MatchRegex(string value, string pattern)
        {
            if (Regex.Match(value, pattern) is Match m && m.Success)
            {
                parms = m.Groups.Cast<Group>().Skip(1).Select(m => m.Value).ToList();
                return true;
            }

            parms = new List<string>();
            return false;
        }

        Distribution.Clear();

        foreach (string line in File.ReadLines(filename))
        {
            if (MatchRegex(line, "description:(.+)"))
            {
                Description.Text = parms[0];
            }
            else if (MatchRegex(line, @"racksize:(\d+)"))
            {
                RackSize.Value = int.Parse(parms[0]);
            }
            else if (MatchRegex(line, @"boardsize:(\d+)"))
            {
                BoardSize.Value = int.Parse(parms[0]);
            }
            else if (MatchRegex(line, @"bingoscore:(\d+)"))
            {
                BingoScore.Value = int.Parse(parms[0]);
            }
            else if (MatchRegex(line, @"validate:(.+)"))
            {
                ValidateWords.Checked = bool.Parse(parms[0]);
            }
            else if (MatchRegex(line, @"flip:(.+)"))
            {
                AllowIfOnly.Checked = bool.Parse(parms[0]);
            }
            else if (MatchRegex(line, @"board_(\d+)_(\d+):(\d+)"))
            {
                RuleSet.Board[int.Parse(parms[0]), int.Parse(parms[1])] = int.Parse(parms[2]);
            }
            else if (MatchRegex(line, @"tile:(.+)_(.+)_(\d+)_(\d+)"))
            {
                Distribution.Add((parms[0], parms[1], int.Parse(parms[2]), int.Parse(parms[3])));
            }
        }

        while (Distribution.Count < 40)
        {
            Distribution.Add(("", "", 0, 0));
        }

        SetUpBoardEditor();
        SetUpDistributionEditor();
    }


    private void SaveButton_Click(object sender, EventArgs e)
    {
        string filename = Path.Combine(m_WordService.BaseFolder, "rules", "variant.txt");

        using var writer = new StreamWriter(filename);

        writer.WriteLine("description:" + Description.Text);
        writer.WriteLine("racksize:" + (int)RackSize.Value);
        writer.WriteLine("boardsize:" + (int)BoardSize.Value);
        writer.WriteLine("bingoscore:" + (int)BingoScore.Value);
        writer.WriteLine("validate:" + ValidateWords.Checked);
        writer.WriteLine("flip:" + AllowIfOnly.Checked);

        for (int i = 0; i < RuleSet.BoardSize; i++)
        {
            for (int j = 0; j < RuleSet.BoardSize; j++)
            {
                writer.WriteLine($"board_{j}_{i}:" + RuleSet.Board[j, i].ToString());
            }
        }

        foreach (var (letter, display, count, points) in Distribution)
        {
            writer.WriteLine($"tile:{letter}_{display}_{count}_{points}");
        }
    }
}
