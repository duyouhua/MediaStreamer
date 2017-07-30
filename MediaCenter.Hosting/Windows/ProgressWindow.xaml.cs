using System.Windows;

namespace MediaCenter.Hosting.Windows
{
    public delegate void CloseProgressWindowDelegate();
    public partial class ProgressWindow : Window
    {
        public event CloseProgressWindowDelegate CloseProgressWindow;

        public ProgressWindow()
        {
            InitializeComponent();
        }

        public string Status
        {
            get
            {
                return txtStatus.Text;
            }
            set
            {
                txtStatus.Text = value;
            }
        }

        public void AddToOutput(string message)
        {
            lbOutput.Items.Insert(0, message);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (CloseProgressWindow != null)
            {
                CloseProgressWindow();
            }
        }
    }
}
