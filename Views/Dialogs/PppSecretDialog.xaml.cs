using System.Windows;

namespace MikrNSN.Views.Dialogs
{
    public partial class PppSecretDialog : Window
    {
        public string SecretName => NameText.Text;
        public string SecretPassword => PasswordText.Text;
        public string SecretProfile => ProfileText.Text;
        public string SecretComment => CommentText.Text;

        public PppSecretDialog(string name = "", string password = "", string profile = "", string comment = "")
        {
            InitializeComponent();
            NameText.Text = name;
            PasswordText.Text = password;
            ProfileText.Text = profile;
            CommentText.Text = comment;
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
