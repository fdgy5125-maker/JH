using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using tik4net;

namespace MikrNSN.Views.Pages
{
    public partial class UserManagerPage : UserControl
    {
        private readonly DispatcherTimer _timer = new DispatcherTimer();
        public UserManagerPage()
        {
            InitializeComponent();
            _timer.Interval = TimeSpan.FromSeconds(3);
            _timer.Tick += async (_, __) => await RefreshSessionsAsync();
            _timer.Start();
            if (MikrNSN.MikroTikClient.Connection == null || !MikrNSN.MikroTikClient.Connection.IsOpened)
            {
                MessageBox.Show("غير متصل بالراوتر: افتح شاشة الدخول ثم اتصل");
                return;
            }
            try
            {
                var ok = MikrNSN.MikroTikClient.IsUsermanRouterConfiguredForCurrent();
                if (!ok)
                {
                    MessageBox.Show("داخل User Manager الراوتر إضافة يجب : IP/Shared Secret/CoA\nPort/Timezone");
                }
            }
            catch { }
            _ = RefreshAll();
        }

        private async Task RefreshAll()
        {
            try
            {
                var unified = await Task.Run(() => MikroTikClient.BuildUnifiedProfiles());
                ProfilesGrid.ItemsSource = unified.Select(u => u.Profile).ToList();
                var combo = unified.Select(u => new ProfileOptionVm
                {
                    Name = u.Profile.Name,
                    Validity = u.Profile.Validity,
                    RateLimit = u.Profile.RateLimit,
                    UptimeLimit = u.Limits.FirstOrDefault()?.TimeLimit ?? "",
                    TransferLimit = u.Limits.FirstOrDefault()?.TransferLimit ?? "",
                }).ToList();
                ProfileCombo.ItemsSource = combo;
                var users = await Task.Run(() => MikroTikClient.GetUserManagerUsers());
                UsersGrid.ItemsSource = users;
                await RefreshSessionsAsync();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private async Task RefreshSessionsAsync()
        {
            try
            {
                var list = await Task.Run(() => MikroTikClient.GetUserManagerSessions());
                SessionsGrid.ItemsSource = list;
            }
            catch { }
        }

        private async void RefreshAll_Click(object sender, RoutedEventArgs e)
        {
            await RefreshAll();
        }

        private async void Add_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var selected = ProfileCombo.SelectedItem as ProfileOptionVm;
                var profileName = selected?.Name ?? ProfileCombo.Text;
                await Task.Run(() => MikroTikClient.AddUserManagerUserAdvanced(UserNameText.Text, PasswordText.Text, profileName, TimeText.Text, MaxRateText.Text, CommentText.Text));
                await RefreshAll();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private async void Edit_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await Task.Run(() => MikroTikClient.UpdateUserManagerUser(UserNameText.Text, PasswordText.Text, ProfileCombo.Text, CommentText.Text));
                await RefreshAll();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private async void Delete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await Task.Run(() => MikroTikClient.DeleteUserManagerUser(UserNameText.Text));
                await RefreshAll();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private void CurrentProfile_Click(object sender, RoutedEventArgs e)
        {
            if (UsersGrid.SelectedItem is MikroTikClient.UmUser u)
            {
                MessageBox.Show(u.Profile);
            }
        }

        private async void RefreshSessions_Click(object sender, RoutedEventArgs e)
        {
            await RefreshSessionsAsync();
        }

        private async void AddRouter_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var conn = MikrNSN.MikroTikClient.Connection;
                if (conn == null || !conn.IsOpened)
                {
                    MessageBox.Show("غير متصل بالراوتر: افتح شاشة الدخول ثم اتصل");
                    return;
                }

                var name = RouterNameText.Text;
                var ip = RouterIpText.Text;
                var secret = RouterSecretText.Text;
                var coa = RouterCoaPortText.Text;
                var tz = RouterTimezoneText.Text;

                if (string.IsNullOrWhiteSpace(ip) || string.IsNullOrWhiteSpace(secret) || string.IsNullOrWhiteSpace(coa) || string.IsNullOrWhiteSpace(tz))
                {
                    MessageBox.Show("داخل User Manager الراوتر إضافة يجب : IP/Shared Secret/CoA\nPort/Timezone");
                    return;
                }

                await Task.Run(() =>
                {
                    try
                    {
                        var add = conn.CreateCommandAndParameters("/userman/router/add",
                            "name", name,
                            "address", ip,
                            "shared-secret", secret,
                            "coa-port", coa,
                            "time-zone", tz);
                        add.ExecuteNonQuery();
                    }
                    catch
                    {
                        var add2 = conn.CreateCommandAndParameters("/tool/user-manager/router/add",
                            "name", name,
                            "ip-address", ip,
                            "shared-secret", secret,
                            "coa-port", coa,
                            "time-zone", tz);
                        add2.ExecuteNonQuery();
                    }
                });

                MessageBox.Show("تمت إضافة الراوتر إلى User Manager");
                await RefreshAll();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }
    }

    public class ProfileOptionVm
    {
        public string Name { get; set; } = "";
        public string Validity { get; set; } = "";
        public string RateLimit { get; set; } = "";
        public string UptimeLimit { get; set; } = "";
        public string TransferLimit { get; set; } = "";
    }
}
