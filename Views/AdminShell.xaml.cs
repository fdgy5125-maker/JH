using System.Windows;

namespace MikrNSN.Views
{
    public partial class AdminShell : Window
    {
        public AdminShell()
        {
            InitializeComponent();
            if (MikrNSN.MikroTikClient.Connection == null || !MikrNSN.MikroTikClient.Connection.IsOpened)
            {
                var home = new HomeView();
                home.Show();
                Close();
                return;
            }
            ContentHost.Content = new Pages.UserManagerPage();
        }

        private void Home_Click(object sender, RoutedEventArgs e)
        {
            ContentHost.Content = new Pages.UserManagerPage();
        }

        private void Packages_Click(object sender, RoutedEventArgs e)
        {
            ContentHost.Content = new Pages.UserManagerProfilesPage();
        }

        private void Templates_Click(object sender, RoutedEventArgs e)
        {
            ContentHost.Content = new Pages.CardsTemplatesPage();
        }

        private void PrintCards_Click(object sender, RoutedEventArgs e)
        {
            ContentHost.Content = new Pages.PrintCardsPage();
        }

        private void Users_Click(object sender, RoutedEventArgs e)
        {
            ContentHost.Content = new Pages.UserManagerPage();
        }

        private void Active_Click(object sender, RoutedEventArgs e)
        {
            ContentHost.Content = new Pages.UserManagerPage();
        }
    }
}
