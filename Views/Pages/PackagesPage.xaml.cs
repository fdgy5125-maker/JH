using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using tik4net.Objects;
using tik4net.Objects.Ppp;
using System.Collections.Generic;

namespace MikrNSN.Views.Pages
{
    public partial class PackagesPage : UserControl
    {
        public ObservableCollection<PackageVm> Items { get; } = new();
        public PackagesPage()
        {
            InitializeComponent();
            PackagesGrid.ItemsSource = Items;
            GroupCombo.ItemsSource = new[] { "Default", "VIP", "Reseller" };
            Refresh();
            RefreshUserManagerProfiles();
        }

        private void RefreshUserManagerProfiles()
        {
            try
            {
                var profiles = MikrNSN.MikroTikClient.GetUserManagerProfiles();
                if (profiles != null && profiles.Count > 0)
                {
                    GroupCombo.ItemsSource = profiles.ConvertAll(p => p.Name);
                }
            }
            catch { }
        }

        private void Refresh()
        {
            try
            {
                Items.Clear();
                var profiles = MikrNSN.MikroTikClient.Connection.LoadAll<PppProfile>();
                foreach (var p in profiles)
                {
                    var meta = Utils.ProfileMeta.Parse(p.Comment);
                    Items.Add(new PackageVm
                    {
                        Name = p.Name ?? "",
                        Time = meta.GetValueOrDefault("time") ?? "",
                        Validity = meta.GetValueOrDefault("validity") ?? "",
                        Quota = meta.GetValueOrDefault("quota") ?? "",
                        Price = meta.GetValueOrDefault("price") ?? "",
                        Commission = meta.GetValueOrDefault("commission") ?? "",
                        Rate = p.RateLimit ?? "",
                        UsersCount = int.TryParse(meta.GetValueOrDefault("users"), out var u) ? u : 0,
                        Group = meta.GetValueOrDefault("group") ?? "",
                        ProfileStatus = meta.GetValueOrDefault("status", "enabled") ?? "enabled",
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void AddBtn_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                var p = new PppProfile()
                {
                    Name = NameText.Text,
                    RateLimit = ComposeRate(),
                };
                p.Comment = Utils.ProfileMeta.Serialize(new System.Collections.Generic.Dictionary<string, string>
                {
                    ["time"] = ComposeTime(),
                    ["validity"] = ValidityText.Text,
                    ["quota"] = QuotaText.Text,
                    ["price"] = PriceText.Text,
                    ["commission"] = CommissionText.Text,
                    ["users"] = UsersCountText.Text,
                    ["group"] = GroupCombo.Text,
                    ["status"] = "enabled",
                });
                MikrNSN.MikroTikClient.Connection.Save(p);
                Refresh();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private void EditBtn_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (PackagesGrid.SelectedItem is PackageVm vm)
            {
                try
                {
                    var p = MikrNSN.MikroTikClient.Connection.LoadAll<PppProfile>().FirstOrDefault(x => x.Name == vm.Name);
                    if (p == null) return;
                    p.RateLimit = ComposeRate();
                    p.Comment = Utils.ProfileMeta.Serialize(new System.Collections.Generic.Dictionary<string, string>
                    {
                        ["time"] = ComposeTime(),
                        ["validity"] = ValidityText.Text,
                        ["quota"] = QuotaText.Text,
                        ["price"] = PriceText.Text,
                        ["commission"] = CommissionText.Text,
                        ["users"] = UsersCountText.Text,
                        ["group"] = GroupCombo.Text,
                        ["status"] = vm.ProfileStatus,
                    });
                    MikrNSN.MikroTikClient.Connection.Save(p);
                    Refresh();
                }
                catch (Exception ex) { MessageBox.Show(ex.Message); }
            }
        }

        private void DeleteBtn_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (PackagesGrid.SelectedItem is PackageVm vm)
            {
                try
                {
                    var p = MikrNSN.MikroTikClient.Connection.LoadAll<PppProfile>().FirstOrDefault(x => x.Name == vm.Name);
                    if (p == null) return;
                    MikrNSN.MikroTikClient.Connection.Delete(p);
                    Refresh();
                }
                catch (Exception ex) { MessageBox.Show(ex.Message); }
            }
        }

        private void RefreshBtn_Click(object sender, System.Windows.RoutedEventArgs e) => Refresh();

        private string ComposeRate()
        {
            var up = UpRateText.Text;
            var down = DownRateText.Text;
            if (!string.IsNullOrWhiteSpace(up) && !string.IsNullOrWhiteSpace(down)) return up + "/" + down;
            return string.IsNullOrWhiteSpace(up) ? down : up;
        }

        private string ComposeTime()
        {
            var d = DaysText.Text;
            var h = HoursText.Text;
            var m = MinutesText.Text;
            return $"{d}d {h}h {m}m";
        }
    }

    public class PackageVm
    {
        public string Name { get; set; } = "";
        public string Time { get; set; } = "";
        public string Validity { get; set; } = "";
        public string Quota { get; set; } = "";
        public string Price { get; set; } = "";
        public string Commission { get; set; } = "";
        public string Rate { get; set; } = "";
        public int UsersCount { get; set; } = 0;
        public string Group { get; set; } = "";
        public string ProfileStatus { get; set; } = "";
    }
}
