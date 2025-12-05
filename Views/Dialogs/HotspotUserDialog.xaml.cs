using System.Windows;

namespace MikrNSN.Views.Dialogs
{
    public partial class HotspotUserDialog : Window
    {
        public string UserName => NameText.Text;
        public string UserPassword => PasswordText.Text;
        public string UserProfile => ProfileText.Text;
        public string UserComment => CommentText.Text;

        public HotspotUserDialog(string name = "", string password = "", string profile = "", string comment = "")
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
