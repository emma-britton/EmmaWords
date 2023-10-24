
namespace Emma.WordLearner;

public partial class WordLearnForm : Form
{
    private readonly WordLearnUI UI;


    public WordLearnForm(WordLearn wordLearning)
    {
        InitializeComponent();
        UI = new WordLearnUI(wordLearning, this);
    }


    private void WordLearningUI_KeyDown(object sender, KeyEventArgs e)
    {
        UI.HandleKey(e);
    }


    private void WordLearnForm_FormClosed(object sender, FormClosedEventArgs e)
    {
        Application.Exit();
    }
}
