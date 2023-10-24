using Emma.Lib;

namespace Emma.Stream;

public partial class MainForm : Form
{
    public QueueForm? QueueForm { get; set; }

    private bool IsShown = false;
    private Gdi? m_UI;


    public Gdi? UI
    {
        get => m_UI;

        set
        {
            if (m_UI != null)
            {
                m_UI.Visible = false;
            }

            m_UI = value;

            if (m_UI != null)
            {
                m_UI.Visible = true;

                if (IsShown)
                {
                    BeginInvoke(m_UI.Start);
                }
            }
        }
    }


    public MainForm()
    {
        InitializeComponent();
    }


    private void WordLearningUI_KeyDown(object sender, KeyEventArgs e)
    {
        if (UI != null)
        {
            UI.HandleKey(e);
        }
    }


    private void MainForm_MouseDown(object sender, MouseEventArgs e)
    {
        if (UI != null)
        {
            UI.HandleMouse(e);
        }
    }

    private void MainForm_Shown(object sender, EventArgs e)
    {
        IsShown = true;
        QueueForm?.Show();
    }
}
