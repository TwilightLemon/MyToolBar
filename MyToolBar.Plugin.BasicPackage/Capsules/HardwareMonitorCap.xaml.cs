using MyToolBar.Common;
using MyToolBar.Common.UIBases;
using MyToolBar.Plugin.BasicPackage.API;
using MyToolBar.Plugin.BasicPackage.PopupWindows;
using System.Windows;
using System.Windows.Input;
using static MyToolBar.Common.GlobalService;

namespace MyToolBar.Plugin.BasicPackage.Capsules
{
    /// <summary>
    /// HardwareMonitorCap.xaml 的交互逻辑
    /// </summary>
    public partial class HardwareMonitorCap : CapsuleBase
    {
        private NetworkInfo ni;
        private CPUInfo ci;
        public HardwareMonitorCap()
        {
            InitializeComponent();
        }
        public override void Uninstall()
        {
            base.Uninstall();
            GlobalTimer.Elapsed -= GlobalTimer_Elapsed;
            ni.Dispose();
            ci.Dispose();
        }
        public override async void Install()
        {
            MainPanel.Visibility = Visibility.Collapsed;
            LoadingTextBlk.Visibility = Visibility.Visible;
            ni = await NetworkInfo.Create();
            ci = CPUInfo.Create();
            GlobalTimer.Elapsed += GlobalTimer_Elapsed;
            Tick();
            MainPanel.Visibility = Visibility.Visible;
            LoadingTextBlk.Visibility = Visibility.Collapsed;
        }

        private void GlobalTimer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
            => Dispatcher.Invoke(Tick);

        private void Tick()
        {
            Meo_text.Text = (int)MemoryInfo.GetUsedPercent() + "%";
            Cpu_text.Text = ci.GetCPUUsedPercent();
            Cpu_temp.Text = ci.GetCPUTemperature() + "℃";
            var data = ni.GetNetworkSpeed();
            Network_text.Text = $"↑ {data[1]}/s\r\n↓ {data[0]}/s";

        }
        private bool BoxShowed = false;
        private void OpenBox()
        {
            if (BoxShowed) return;
            var w = new ResourceMonitor();
            w.Left = GlobalService.GetPopupWindowLeft(this, w);
            w.Closing += delegate { BoxShowed = false; };
            w.Show();
            BoxShowed = true;
        }
        private void uc_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            OpenBox();
        }

        private void uc_TouchLeave(object sender, TouchEventArgs e)
        {
            OpenBox();
        }
    }
}
