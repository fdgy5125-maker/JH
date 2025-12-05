using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Threading.Tasks;

namespace MikrNSN.Views
{
    public partial class HomeView : Window
    {
        public HomeView()
        {
            InitializeComponent();
        }

        private async void LoginBtn_Click(object sender, RoutedEventArgs e)
        {
            StatusText.Text = "";
            var host = HostText.Text?.Trim();
            var user = UserText.Text?.Trim();
            var pass = PassBox.Password ?? "";
            var portText = PortText.Text?.Trim();
            if (string.IsNullOrEmpty(host) || string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass))
            {
                StatusText.Text = "أدخل البيانات كاملة";
                return;
            }

            if (!int.TryParse(portText, out var port)) port = TlsCheck.IsChecked == true ? 8729 : 8728;
            if (TlsCheck.IsChecked == true && port == 8728)
            {
                port = 8729;
                PortText.Text = "8729";
            }

            try
            {
                StatusText.Text = "جاري الاتصال...";
                var connected = await MikroTikClient.ConnectAsync(host, user, pass, port, TlsCheck.IsChecked == true);
                if (!connected)
                {
                    StatusText.Text = "فشل الاتصال";
                    return;
                }

                var identity = await MikroTikClient.GetIdentityAsync();
                StatusText.Text = $"اتصال ناجح: {identity}";
                try
                {
                    var ok = await Task.Run(() => MikroTikClient.IsUsermanRouterConfiguredForCurrent());
                    if (!ok)
                    {
                        StatusText.Text += " — داخل User Manager الراوتر إضافة يجب : IP/Shared Secret/CoA\nPort/Timezone";
                        MessageBox.Show("داخل User Manager الراوتر إضافة يجب : IP/Shared Secret/CoA\nPort/Timezone");
                    }
                }
                catch { }
                var shell = new AdminShell();
                shell.Show();
                Close();
            }
            catch (Exception ex)
            {
                StatusText.Text = ex.Message;
            }
        }
    }
}
