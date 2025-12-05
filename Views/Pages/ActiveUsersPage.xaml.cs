using System;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Threading;
using tik4net.Objects;
using tik4net.Objects.Ip.Hotspot;
using tik4net.Objects.Ppp;

namespace MikrNSN.Views.Pages
{
    public partial class ActiveUsersPage : UserControl
    {
        private readonly DispatcherTimer _timer = new DispatcherTimer();
        public ActiveUsersPage()
        {
            InitializeComponent();
            _timer.Interval = TimeSpan.FromSeconds(3);
            _timer.Tick += (_, __) => Refresh();
            _timer.Start();
            Refresh();
        }

        private void Refresh()
        {
            try
            {
                var ppp = MikrNSN.MikroTikClient.Connection.LoadAll<PppActive>().Select(a => new ActiveVm
                {
                    Name = a.Name,
                    Address = a.Address,
                    MacAddress = a.CallerId,
                    Service = "pppoe",
                    Uptime = a.Uptime,
                }).ToList();
                ActiveGrid.ItemsSource = ppp;
            }
            catch { }
        }

        private void RefreshBtn_Click(object sender, System.Windows.RoutedEventArgs e) => Refresh();
    }

    public class ActiveVm
    {
        public string Name { get; set; } = "";
        public string Address { get; set; } = "";
        public string MacAddress { get; set; } = "";
        public string Service { get; set; } = "";
        public string Uptime { get; set; } = "";
        public string Bytes { get; set; } = "";
        public string Rate { get; set; } = "";
        public string Status { get; set; } = "";
    }
}
