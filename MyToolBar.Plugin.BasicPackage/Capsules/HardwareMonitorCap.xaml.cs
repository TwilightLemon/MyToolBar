using MyToolBar.Common;
using MyToolBar.Common.UIBases;
using MyToolBar.Plugin.BasicPackage.API;
using MyToolBar.Plugin.BasicPackage.PopupWindows;
using System.Windows;
using Windows.System.Power;
using System.Windows.Input;
using static MyToolBar.Common.GlobalService;

namespace MyToolBar.Plugin.BasicPackage.Capsules
{
    /// <summary>
    /// HardwareMonitorCap.xaml 的交互逻辑
    /// </summary>
    public partial class HardwareMonitorCap : CapsuleBase
    {
        private NetworkInfo? ni=null;
        private CPUInfo? ci=null;
        public HardwareMonitorCap()
        {
            InitializeComponent();
            PopupWindowType = typeof(ResourceMonitor);
            InitLangRes();
        }
        private void InitLangRes()
        {
            string uri = $"/MyToolBar.Plugin.BasicPackage;component/LanguageRes/Caps/Lang{LocalCulture.Current switch
            {
                LocalCulture.Language.en_us => "En_US",
                LocalCulture.Language.zh_cn => "Zh_CN",
                _ => throw new Exception("Unsupported Language")
            }}.xaml";
            Resources.MergedDictionaries.Add(new ResourceDictionary() { Source = new Uri(uri, UriKind.Relative) });
        }
        public override void Uninstall()
        {
            base.Uninstall();
            GlobalTimer.Elapsed -= GlobalTimer_Elapsed;
            ni?.Dispose();
            ci?.Dispose();
        }
        public override async void Install()
        {
            if (PowerManager.BatteryStatus == BatteryStatus.NotPresent)
                BatteryViewer.Visibility= Visibility.Collapsed;
            MainPanel.Visibility = Visibility.Collapsed;
            LoadingTextBlk.Visibility = Visibility.Visible;
            try
            {
                ni = await NetworkInfo.Create();
                ci = CPUInfo.Create();
                Tick();
            }
            catch
            {
                LoadingTextBlk.Text = "Not Supported";
                return;
            }
            GlobalTimer.Elapsed += GlobalTimer_Elapsed;
            MainPanel.Visibility = Visibility.Visible;
            LoadingTextBlk.Visibility = Visibility.Collapsed;
        }

        private void GlobalTimer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
            => Dispatcher.Invoke(Tick);

        private void Tick()
        {
            if(ni == null || ci == null)
                return;
            Meo_text.Text = (int)MemoryInfo.GetUsedPercent() + "%";
            Cpu_text.Text = ci.GetCPUUsedPercent();
            Cpu_temp.Text = ci.GetCPUTemperature() + "℃";
            var data = ni.GetNetworkSpeed();
            Network_text.Text = $"↑ {data[1]}/s\r\n↓ {data[0]}/s";

            int battery = PowerManager.RemainingChargePercent;
            Battery_value.Value = battery;
            Battery_text.Text = battery + "%";
            Battery_value.SetResourceReference(ForegroundProperty, battery <= 20?"Battery_Emergency":"Battery_Normal");

            //Notify
            if(battery == 19)
            {
                NotificationManager.Send(new((string)FindResource("Notify_BatteryLow"), NotificationType.Warning, null, NotificationTimeSpan.Short));
            }
        }
    }
}
