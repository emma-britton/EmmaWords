
namespace Emma.Stream;

public partial class QueueForm : Form
{
    private readonly QueueUI UI;


    public QueueForm(EmmaStream stream)
    {
        InitializeComponent();
        UI = new QueueUI(stream, this);
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
