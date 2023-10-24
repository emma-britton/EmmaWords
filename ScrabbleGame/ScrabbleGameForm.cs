using Emma.Lib;

namespace Emma.Scrabble;

public partial class ScrabbleGameForm : Form
{
    private readonly ScrabbleUI UI;


    public ScrabbleGameForm(ScrabbleGame game)
    {
        InitializeComponent();
        UI = new ScrabbleUI(game, this);
    }


    private void WordLearningUI_KeyDown(object sender, KeyEventArgs e)
    {
        UI.HandleKey(e);
    }
}
