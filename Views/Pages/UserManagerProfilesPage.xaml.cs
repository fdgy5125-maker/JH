using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MikrNSN.Views.Pages
{
    public partial class UserManagerProfilesPage : UserControl
    {
        public UserManagerProfilesPage()
        {
            InitializeComponent();
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
                var unified = await Task.Run(() => MikrNSN.MikroTikClient.BuildUnifiedProfiles());
                var vms = unified.Select(u => new UnifiedVm(u)).ToList();
                ProfilesGrid.ItemsSource = vms;
                AssignmentsGrid.ItemsSource = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private async void Refresh_Click(object sender, RoutedEventArgs e)
        {
            await RefreshAll();
        }

        private async void Add_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await Task.Run(() => MikrNSN.MikroTikClient.AddUserManagerProfile(NameText.Text, RateText.Text, TransferText.Text, ValidityText.Text, PriceText.Text, NoteText.Text));
                await RefreshAll();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private async void Edit_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await Task.Run(() => MikrNSN.MikroTikClient.UpdateUserManagerProfile(NameText.Text, RateText.Text, TransferText.Text, ValidityText.Text, PriceText.Text, NoteText.Text));
                await RefreshAll();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private async void Delete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await Task.Run(() => MikrNSN.MikroTikClient.DeleteUserManagerProfile(NameText.Text));
                await RefreshAll();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private void ProfilesGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ProfilesGrid.SelectedItem is UnifiedVm vm)
            {
                AssignmentsGrid.ItemsSource = vm.Assignments;
                NameText.Text = vm.Profile.Name;
                RateText.Text = vm.Profile.RateLimit;
                ValidityText.Text = vm.Profile.Validity;
                PriceText.Text = vm.Profile.Price;
                NoteText.Text = vm.Profile.Note;
                TransferText.Text = vm.Limits.FirstOrDefault()?.TransferLimit ?? "";
            }
        }

        private async void RowEdit_Click(object sender, RoutedEventArgs e)
        {
            if (ProfilesGrid.SelectedItem is UnifiedVm vm)
            {
                try
                {
                    await Task.Run(() => MikrNSN.MikroTikClient.UpdateUserManagerProfile(vm.Profile.Name, RateText.Text, TransferText.Text, ValidityText.Text, PriceText.Text, NoteText.Text));
                    await RefreshAll();
                }
                catch (Exception ex) { MessageBox.Show(ex.Message); }
            }
        }

        private async void RowDelete_Click(object sender, RoutedEventArgs e)
        {
            if (ProfilesGrid.SelectedItem is UnifiedVm vm)
            {
                try
                {
                    await Task.Run(() => MikrNSN.MikroTikClient.DeleteUserManagerProfile(vm.Profile.Name));
                    await RefreshAll();
                }
                catch (Exception ex) { MessageBox.Show(ex.Message); }
            }
        }

        private void RowUsers_Click(object sender, RoutedEventArgs e)
        {
            if (ProfilesGrid.SelectedItem is UnifiedVm vm)
            {
                AssignmentsGrid.ItemsSource = vm.Assignments;
            }
        }
    }

    public class UnifiedVm
    {
        public UnifiedVm(MikrNSN.MikroTikClient.UnifiedProfileModel m)
        {
            Profile = m.Profile;
            Limits = m.Limits;
            Assignments = m.Assignments;
        }

        public MikrNSN.MikroTikClient.UserManagerProfile Profile { get; }
        public List<MikrNSN.MikroTikClient.ProfileLimit> Limits { get; }
        public List<MikrNSN.MikroTikClient.UserProfileAssignment> Assignments { get; }

        public string TransferDisplay
        {
            get
            {
                var t = Limits.FirstOrDefault()?.TransferLimit ?? "";
                return t;
            }
        }

        public string DlUlDisplay
        {
            get
            {
                var d = Limits.FirstOrDefault()?.DownloadLimit ?? "";
                var u = Limits.FirstOrDefault()?.UploadLimit ?? "";
                if (!string.IsNullOrWhiteSpace(d) && !string.IsNullOrWhiteSpace(u)) return d + "/" + u;
                return string.IsNullOrWhiteSpace(d) ? u : d;
            }
        }
    }
}
