using System.Linq;
using System.Windows;
using System.Windows.Controls;
using tik4net.Objects;
using tik4net.Objects.Ppp;

namespace MikrNSN.Views.Pages
{
    public partial class UsersPage : UserControl
    {
        public UsersPage()
        {
            InitializeComponent();
            ProfileCombo.ItemsSource = MikrNSN.MikroTikClient.Connection.LoadAll<PppProfile>().Select(p => p.Name).ToList();
            Refresh();
        }

        private void Refresh()
        {
            UsersGrid.ItemsSource = MikrNSN.MikroTikClient.Connection.LoadAll<PppSecret>().ToList();
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            var s = new PppSecret()
            {
                Name = UserNameText.Text,
                Password = PasswordText.Text,
                Service = "pppoe",
                Profile = ProfileCombo.Text,
                Comment = CommentText.Text,
            };
            MikrNSN.MikroTikClient.Connection.Save(s);
            Refresh();
        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            if (UsersGrid.SelectedItem is PppSecret s)
            {
                s.Password = PasswordText.Text;
                s.Profile = ProfileCombo.Text;
                s.Comment = CommentText.Text;
                MikrNSN.MikroTikClient.Connection.Save(s);
                Refresh();
            }
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (UsersGrid.SelectedItem is PppSecret s)
            {
                MikrNSN.MikroTikClient.Connection.Delete(s);
                Refresh();
            }
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            Refresh();
        }

        private void CurrentProfile_Click(object sender, RoutedEventArgs e)
        {
            if (UsersGrid.SelectedItem is PppSecret s)
            {
                MessageBox.Show(s.Profile);
            }
        }
    }
}
