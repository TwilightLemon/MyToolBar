using MyToolBar.Common;
using MyToolBar.Common.UIBases;
using System.Windows;

namespace MyToolBar.Plugin.BasicPackage.Capsules
{
    /// <summary>
    /// ClockCap.xaml 的交互逻辑
    /// </summary>
    public partial class ClockCap : CapsuleBase
    {
        public class ClockCapSettings
        {
            public string FormatStr { get; set; } = "MM-dd tt h:mm dddd";
        }
        public static readonly string SettingSign= "MyToolBar.Plugin.BasicPackage.ClockCap";
        private readonly SettingsMgr<ClockCapSettings> settingsMgr;
        public ClockCap()
        {
            InitializeComponent();
            settingsMgr = new(SettingSign, ClockCapPlugin._name);
            settingsMgr.OnDataChanged += SettingsMgr_OnDataChanged;
            Loaded += ClockCap_Loaded;
        }
        private void Refresh()
        {
            TimeTb.Text = DateTime.Now.ToString(settingsMgr.Data!.FormatStr);
        }

        private async void SettingsMgr_OnDataChanged()
        {
            await settingsMgr.LoadAsync();
        }

        private async void ClockCap_Loaded(object sender, RoutedEventArgs e)
        {
            await settingsMgr.LoadAsync();
            Refresh();
            GlobalService.GlobalTimer.Elapsed += GlobalTimer_Elapsed;
        }

        private void GlobalTimer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            Dispatcher.Invoke(Refresh);
        }

        public override void Install()
        {
            base.Install();
        }

        public override void Uninstall()
        {
            base.Uninstall();
            GlobalService.GlobalTimer.Elapsed -= GlobalTimer_Elapsed;
        }
    }
}
