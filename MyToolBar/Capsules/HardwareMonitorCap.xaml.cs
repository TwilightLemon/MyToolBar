using MyToolBar.Func;
using System.Windows.Controls;
using static MyToolBar.GlobalService;

namespace MyToolBar.Capsules
{
    /// <summary>
    /// HardwareMonitorCap.xaml 的交互逻辑
    /// </summary>
    public partial class HardwareMonitorCap : UserControl
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
            var data = ni.GetNetworkspeed();
            Network_text.Text = $"↑ {data[1]}/s\r\n↓ {data[0]}/s";
        }
    }
}
