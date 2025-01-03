using System.ComponentModel;


namespace Emma.Stream;

public partial class AlertForm : Form
{
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public AlertUI? UI { get; set; }


    public AlertForm()
    {
        InitializeComponent();
    }


    private void AlertForm_KeyDown(object sender, KeyEventArgs e)
    {
        UI?.HandleKey(e);
    }


    private void AlertForm_FormClosed(object sender, FormClosedEventArgs e)
    {
        Application.Exit();
    }
}
