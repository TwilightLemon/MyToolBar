using System.Windows;
using System.Windows.Media;
using MyToolBar.Common;
using MyToolBar.Common.UIBases;

namespace MyToolBar.Plugin.BasicPackage.OuterControls
{
    /// <summary>
    /// DemoClock.xaml 的交互逻辑
    /// </summary>
    public partial class DemoClock : OuterControlBase
    {
        private class DemoClockSetting
        {
            public string FormatStr { get; set; } = "MM-dd tt h:mm dddd";
            public string Sign { get; set; } = "  ❤  Have a nice day.";
        }
        public static readonly string SettingSign = "MyToolBar.Plugin.BasicPackage.DemoClock";
        private SettingsMgr<DemoClockSetting> settingsMgr;
        public DemoClock()
        {
            InitializeComponent();
            MaxStyleAct = maxStyleAct;
            settingsMgr = new(SettingSign, DemoClockOutControlPlugin._name);
            settingsMgr.OnDataChanged += SettingsMgr_OnDataChanged;
            Loaded += DemoClock_Loaded;
        }
        public override void Dispose()
        {
            base.Dispose();
            GlobalService.GlobalTimer.Elapsed -= RefreshTime;
        }

        private async void SettingsMgr_OnDataChanged() {
            await settingsMgr.Load();
        }

        private async void DemoClock_Loaded(object sender, RoutedEventArgs e)
        {
            await settingsMgr.Load();
            RefreshTime(null, null);
            IsShown = true;
            GlobalService.GlobalTimer.Elapsed += RefreshTime;
        }
        private void maxStyleAct(bool max, Brush? foreColor)
        {
            if (foreColor != null)
                TimeTb.Foreground = foreColor;
            else TimeTb.SetResourceReference(ForegroundProperty, "AppBarFontColor");
        }
        private void RefreshTime(object? sender, System.Timers.ElapsedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                TimeTb.Text = DateTime.Now.ToString(settingsMgr.Data.FormatStr) + settingsMgr.Data.Sign;
            });
        }
    }
}
