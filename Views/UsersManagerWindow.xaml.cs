using System.Linq;
using System.Windows;
using tik4net.Objects;
using tik4net.Objects.Ppp;
using tik4net.Objects.Ip.Hotspot;
using MikrNSN.Views.Dialogs;

namespace MikrNSN.Views
{
    public partial class UsersManagerWindow : Window
    {
        public UsersManagerWindow()
        {
            InitializeComponent();
            RefreshPpp();
            RefreshHotspot();
            RefreshProfiles();
        }

        private void RefreshPpp()
        {
            var list = MikroTikClient.Connection.LoadAll<PppSecret>().ToList();
            PppGrid.ItemsSource = list;
        }

        private void RefreshHotspot()
        {
            var list = MikroTikClient.Connection.LoadAll<HotspotUser>().ToList();
            HotspotGrid.ItemsSource = list;
        }

        private void RefreshProfiles()
        {
            var list = MikroTikClient.Connection.LoadAll<PppProfile>().ToList();
            ProfilesGrid.ItemsSource = list;
        }

        private void RefreshPpp_Click(object sender, RoutedEventArgs e) => RefreshPpp();
        private void RefreshHotspot_Click(object sender, RoutedEventArgs e) => RefreshHotspot();
        private void RefreshProfiles_Click(object sender, RoutedEventArgs e) => RefreshProfiles();

        private void AddPpp_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new PppSecretDialog();
            if (dlg.ShowDialog() == true)
            {
                var s = new PppSecret()
                {
                    Name = dlg.SecretName,
                    Password = dlg.SecretPassword,
                    Service = "pppoe",
                    Profile = dlg.SecretProfile,
                    Comment = dlg.SecretComment,
                };
                MikroTikClient.Connection.Save(s);
                RefreshPpp();
            }
        }

        private void EditPpp_Click(object sender, RoutedEventArgs e)
        {
            if (PppGrid.SelectedItem is PppSecret s)
            {
                var dlg = new PppSecretDialog(s.Name, s.Password, s.Profile, s.Comment);
                if (dlg.ShowDialog() == true)
                {
                    s.Name = dlg.SecretName;
                    s.Password = dlg.SecretPassword;
                    s.Profile = dlg.SecretProfile;
                    s.Comment = dlg.SecretComment;
                    MikroTikClient.Connection.Save(s);
                    RefreshPpp();
                }
            }
        }

        private void DeletePpp_Click(object sender, RoutedEventArgs e)
        {
            if (PppGrid.SelectedItem is PppSecret s)
            {
                MikroTikClient.Connection.Delete(s);
                RefreshPpp();
            }
        }

        private void AddHotspot_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new HotspotUserDialog();
            if (dlg.ShowDialog() == true)
            {
                var u = new HotspotUser()
                {
                    Name = dlg.UserName,
                    Password = dlg.UserPassword,
                    Profile = dlg.UserProfile,
                    Comment = dlg.UserComment,
                };
                MikroTikClient.Connection.Save(u);
                RefreshHotspot();
            }
        }

        private void EditHotspot_Click(object sender, RoutedEventArgs e)
        {
            if (HotspotGrid.SelectedItem is HotspotUser u)
            {
                var dlg = new HotspotUserDialog(u.Name, u.Password, u.Profile, u.Comment);
                if (dlg.ShowDialog() == true)
                {
                    u.Name = dlg.UserName;
                    u.Password = dlg.UserPassword;
                    u.Profile = dlg.UserProfile;
                    u.Comment = dlg.UserComment;
                    MikroTikClient.Connection.Save(u);
                    RefreshHotspot();
                }
            }
        }

        private void DeleteHotspot_Click(object sender, RoutedEventArgs e)
        {
            if (HotspotGrid.SelectedItem is HotspotUser u)
            {
                MikroTikClient.Connection.Delete(u);
                RefreshHotspot();
            }
        }

        private void AddProfile_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new PppProfileDialog();
            if (dlg.ShowDialog() == true)
            {
                var p = new PppProfile()
                {
                    Name = dlg.ProfileName,
                    LocalAddress = dlg.LocalAddress,
                    RemoteAddress = dlg.RemoteAddress,
                    RateLimit = dlg.RateLimit,
                };
                MikroTikClient.Connection.Save(p);
                RefreshProfiles();
            }
        }

        private void EditProfile_Click(object sender, RoutedEventArgs e)
        {
            if (ProfilesGrid.SelectedItem is PppProfile p)
            {
                var dlg = new PppProfileDialog(p.Name, p.LocalAddress, p.RemoteAddress, p.RateLimit);
                if (dlg.ShowDialog() == true)
                {
                    p.Name = dlg.ProfileName;
                    p.LocalAddress = dlg.LocalAddress;
                    p.RemoteAddress = dlg.RemoteAddress;
                    p.RateLimit = dlg.RateLimit;
                    MikroTikClient.Connection.Save(p);
                    RefreshProfiles();
                }
            }
        }

        private void DeleteProfile_Click(object sender, RoutedEventArgs e)
        {
            if (ProfilesGrid.SelectedItem is PppProfile p)
            {
                MikroTikClient.Connection.Delete(p);
                RefreshProfiles();
            }
        }
    }
}
