using System.Windows;

namespace MediaCenter.Hosting.Windows
{
    public partial class NameOfMovie : Window
    {
        public NameOfMovie()
        {
            InitializeComponent();
        }

        public string DiscName
        {
            get
            {
                return txtNameOfDisc.Text;
            }
            set
            {
                txtNameOfDisc.Text = value;
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
