namespace ExplosiveCatsUi;

public partial class InsertCardForm : Form
{
    public byte SelectedIndex { get; private set; }
        
    public InsertCardForm()
    {
        var trackBar = new TrackBar { Minimum = 0, Maximum = 5, Dock = DockStyle.Top };
        var btnOk = new Button { Text = "OK", Dock = DockStyle.Bottom };
            
        btnOk.Click += (s, e) => 
        {
            SelectedIndex = (byte)trackBar.Value;
            DialogResult = DialogResult.OK;
        };
            
        Controls.Add(trackBar);
        Controls.Add(btnOk);
    }
}