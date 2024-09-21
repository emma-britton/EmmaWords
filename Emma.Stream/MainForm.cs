using Emma.Lib;

namespace Emma.Stream;

public partial class MainForm : Form
{
    public QueueForm? QueueForm { get; set; }

    private bool IsShown = false;
    private Gdi? m_UI;

    private readonly GlobalHotkey m_Hotkey = new();


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
        m_Hotkey.KeyPressed += GlobalHotkey_KeyPressed;
        m_Hotkey.RegisterHotKey(Stream.ModifierKeys.Control | Stream.ModifierKeys.Shift, Keys.F1);
        m_Hotkey.RegisterHotKey(Stream.ModifierKeys.Control | Stream.ModifierKeys.Shift, Keys.F2);
        m_Hotkey.RegisterHotKey(Stream.ModifierKeys.Control | Stream.ModifierKeys.Shift, Keys.F3);
        m_Hotkey.RegisterHotKey(Stream.ModifierKeys.Control | Stream.ModifierKeys.Shift, Keys.F4);
        m_Hotkey.RegisterHotKey(Stream.ModifierKeys.Control | Stream.ModifierKeys.Shift, Keys.F5);
        m_Hotkey.RegisterHotKey(Stream.ModifierKeys.Control | Stream.ModifierKeys.Shift, Keys.F6);
        m_Hotkey.RegisterHotKey(Stream.ModifierKeys.Control | Stream.ModifierKeys.Shift, Keys.F7);
        m_Hotkey.RegisterHotKey(Stream.ModifierKeys.Control | Stream.ModifierKeys.Shift, Keys.F8);
        m_Hotkey.RegisterHotKey(Stream.ModifierKeys.Control | Stream.ModifierKeys.Shift, Keys.F9);
        m_Hotkey.RegisterHotKey(Stream.ModifierKeys.Control | Stream.ModifierKeys.Shift, Keys.F10);
        m_Hotkey.RegisterHotKey(Stream.ModifierKeys.Control | Stream.ModifierKeys.Shift, Keys.F11);
        m_Hotkey.RegisterHotKey(Stream.ModifierKeys.Control | Stream.ModifierKeys.Shift, Keys.F12);
        m_Hotkey.RegisterHotKey(Stream.ModifierKeys.Control | Stream.ModifierKeys.Shift, Keys.F13);
        m_Hotkey.RegisterHotKey(Stream.ModifierKeys.Control | Stream.ModifierKeys.Shift, Keys.F14);
        m_Hotkey.RegisterHotKey(Stream.ModifierKeys.Control | Stream.ModifierKeys.Shift, Keys.F15);
        m_Hotkey.RegisterHotKey(Stream.ModifierKeys.Control | Stream.ModifierKeys.Shift, Keys.F16);
        m_Hotkey.RegisterHotKey(Stream.ModifierKeys.Control | Stream.ModifierKeys.Shift, Keys.F17);
        m_Hotkey.RegisterHotKey(Stream.ModifierKeys.Control | Stream.ModifierKeys.Shift, Keys.F18);
        m_Hotkey.RegisterHotKey(Stream.ModifierKeys.Control | Stream.ModifierKeys.Shift, Keys.F19);
        m_Hotkey.RegisterHotKey(Stream.ModifierKeys.Control | Stream.ModifierKeys.Shift, Keys.F20);
        m_Hotkey.RegisterHotKey(Stream.ModifierKeys.Control | Stream.ModifierKeys.Shift, Keys.F21);
        m_Hotkey.RegisterHotKey(Stream.ModifierKeys.Control | Stream.ModifierKeys.Shift, Keys.F22);
        m_Hotkey.RegisterHotKey(Stream.ModifierKeys.Control | Stream.ModifierKeys.Shift, Keys.F23);
        m_Hotkey.RegisterHotKey(Stream.ModifierKeys.Control | Stream.ModifierKeys.Shift, Keys.F24);
    }


    private void GlobalHotkey_KeyPressed(object? sender, KeyPressedEventArgs e)
    {
        UI?.HandleKey(new KeyEventArgs(e.Key));
    }

    private void MainForm_KeyDown(object sender, KeyEventArgs e)
    {
        UI?.HandleKey(e);
    }


    private void MainForm_MouseDown(object sender, MouseEventArgs e)
    {
        UI?.HandleMouse(e);
    }

    private void MainForm_Shown(object sender, EventArgs e)
    {
        IsShown = true;
        //QueueForm?.Show();
    }
}
