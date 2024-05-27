using MyToolBar.Func;
using MyToolBar.PopupWindows;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using static MyToolBar.GlobalService;

namespace MyToolBar.Capsules
{
    /// <summary>
    /// HardwareMonitorCap.xaml 的交互逻辑
    /// </summary>
    public partial class HardwareMonitorCap : CapsuleBase
    {
        public HardwareMonitorCap()
        {
            InitializeComponent();
        }

        public void Start()
        {
            CPUInfo.Load();
            GlobalTimer.Elapsed += (s, e) => Dispatcher.Invoke(Tick);
        }
        NetworkInfo ni = new NetworkInfo();

        private void Tick()
        {
            Meo_text.Text = (int)MemoryInfo.GetUsedPercent() + "%";
            Cpu_text.Text = CPUInfo.GetCPUUsedPercent();
            Cpu_temp.Text = CPUInfo.GetCPUTemperature() + "℃";
            var data = ni.GetNetworkSpeed();
            Network_text.Text = $"↑ {data[1]}/s\r\n↓ {data[0]}/s";

        }
        private bool BoxShowed = false;
        private void OpenBox()
        {
            if (BoxShowed) return;
            var w = new ResourceMonitor();
            w.Left = TransformToAncestor(Application.Current.MainWindow).Transform(new Point(0, 0)).X-ActualWidth/4;
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
