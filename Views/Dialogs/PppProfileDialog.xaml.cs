using System.Windows;

namespace MikrNSN.Views.Dialogs
{
    public partial class PppProfileDialog : Window
    {
        public string ProfileName => NameText.Text;
        public string LocalAddress => LocalText.Text;
        public string RemoteAddress => RemoteText.Text;
        public string RateLimit => RateText.Text;

        public PppProfileDialog(string name = "", string local = "", string remote = "", string rate = "")
        {
            InitializeComponent();
            NameText.Text = name;
            LocalText.Text = local;
            RemoteText.Text = remote;
            RateText.Text = rate;
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
