using CommunityToolkit.Mvvm.ComponentModel;
using MyToolBar.Common;
using MyToolBar.Common.WinAPI;
using MyToolBar.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyToolBar.ViewModels
{
    public partial class AppSettingsPageViewModel
        (AppSettingsService appSettingsService,
        ApplicationService applicationService
        ): ObservableObject
    {
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsFloatingMode))]
        private AppSettings.WindowMode _windowMode;

        public bool IsFloatingMode => WindowMode == AppSettings.WindowMode.Floating;
        [ObservableProperty]
        private AppSettings.BackgroundMode _backgroundMode;
        [ObservableProperty]
        private AppSettings.ImmerseMode _immerseMode;
        [ObservableProperty]
        private bool _enableIsland;
        [ObservableProperty]
        private bool _autoRunAtStartup;
        [ObservableProperty]
        private bool _hideWhenFullScreen;
        [ObservableProperty]
        private bool _enableWindowControl;
        [ObservableProperty]
        private bool _enableHighlight;
        [ObservableProperty]
        private LocalCulture.Language _appLanguage = (LocalCulture.Language)appSettingsService.Settings.Language;
        [ObservableProperty]
        private AppSettings.ProxyMode _proxyMode = appSettingsService.Settings.UserProxyMode;
        [ObservableProperty]
        private AppSettings.ProxyConf? _proxyConf = appSettingsService.Settings.Proxy;
        [ObservableProperty]
        private AppSettings.MenuIcon _mainMenuIcon= appSettingsService.Settings.MainMenuIcon;

        [ObservableProperty]
        private double _floatingMarginHorizontal = appSettingsService.Settings.FloatingMarginHorizontal;
        [ObservableProperty]
        private double _floatingMarginVertical = appSettingsService.Settings.FloatingMarginVertical;
        [ObservableProperty]
        private double _appBarHeight = appSettingsService.Settings.AppBarHeight;
        [ObservableProperty]
        private string? _appFont = App.Current.FindResource("AppDefaultFontFamilly").ToString();

        [ObservableProperty]
        private string? _targetMonitorDeviceName = appSettingsService.Settings.TargetMonitorDeviceName;

        [ObservableProperty]
        private List<MonitorDisplayItem> _availableMonitors = new();

        [ObservableProperty]
        private MonitorDisplayItem? _selectedMonitor;

        public void LoadData()
        {
            WindowMode = appSettingsService.Settings.CurrentWindowMode;
            BackgroundMode = appSettingsService.Settings.CurrentBackgroundMode;
            ImmerseMode = appSettingsService.Settings.CurrentImmerseMode;
            EnableIsland = appSettingsService.Settings.EnableIsland;
            AutoRunAtStartup = appSettingsService.Settings.AutoRunAtStartup;
            HideWhenFullScreen = GlobalService.EnableHideWhenFullScreen;
            EnableWindowControl = appSettingsService.Settings.EnableWindowControl;
            EnableHighlight = appSettingsService.Settings.EnableHighlight;
            FloatingMarginHorizontal = appSettingsService.Settings.FloatingMarginHorizontal;
            FloatingMarginVertical = appSettingsService.Settings.FloatingMarginVertical;
            AppBarHeight = appSettingsService.Settings.AppBarHeight;

            // 加载显示器列表
            var monitors = MonitorAPI.EnumerateMonitors();
            var defaultText = (string)App.Current.FindResource("AS_Monitor_Default");
            AvailableMonitors = monitors.Select(m =>
            {
                var width = m.MonitorBounds.right - m.MonitorBounds.left;
                var height = m.MonitorBounds.bottom - m.MonitorBounds.top;
                return new MonitorDisplayItem
                {
                    DeviceName = m.DeviceName,
                    MonitorWidth = width,
                    MonitorHeight = height,
                    IsPrimary = m.IsPrimary
                };
            }).ToList();

            // 匹配已保存的显示器
            TargetMonitorDeviceName = appSettingsService.Settings.TargetMonitorDeviceName;
            SelectedMonitor = AvailableMonitors.FirstOrDefault(
                m => m.DeviceName == TargetMonitorDeviceName)
                ?? AvailableMonitors.FirstOrDefault();
        }
        partial void OnHideWhenFullScreenChanged(bool value)
        {
            GlobalService.EnableHideWhenFullScreen = value;
        }
        partial void OnEnableWindowControlChanged(bool value)
        {
            appSettingsService.Settings.SetEnableWindowControl(value);
        }
        partial void OnEnableHighlightChanged(bool value)
        {
            appSettingsService.Settings.SetEnableHighlight(value);
        }
        partial void OnWindowModeChanged(AppSettings.WindowMode value)
        {
            appSettingsService.Settings.SetWindowMode(value);
        }
        partial void OnBackgroundModeChanged(AppSettings.BackgroundMode value)
        {
            appSettingsService.Settings.SetBackgroundMode(value);
        }
        partial void OnImmerseModeChanged(AppSettings.ImmerseMode value)
        {
            appSettingsService.Settings.SetImmerseMode(value);
        }
        partial void OnEnableIslandChanged(bool value)
        {
            appSettingsService.Settings.SetEnableIsland(value);
        }
        partial void OnAutoRunAtStartupChanged(bool value)
        {
            applicationService.SetAutoRunAtStartup(value);
        }

        partial void OnAppLanguageChanged(LocalCulture.Language value)
        {
            if (value != LocalCulture.Current)
            {
                LocalCulture.SetGlobalLanguage(value);
                appSettingsService.Settings.Language = (int)value;
            }
        }

        partial void OnProxyModeChanged(AppSettings.ProxyMode value)
        {
            appSettingsService.Settings.UserProxyMode = value;
            applicationService.UpdateDefaultProxy();
        }

        partial void OnMainMenuIconChanged(AppSettings.MenuIcon value)
        {
            appSettingsService.Settings.SetMainMenuIcon(value);
        }

        partial void OnFloatingMarginHorizontalChanged(double value)
        {
            appSettingsService.Settings.SetFloatingMargin(value, FloatingMarginVertical);
        }
        partial void OnFloatingMarginVerticalChanged(double value)
        {
            appSettingsService.Settings.SetFloatingMargin(FloatingMarginHorizontal, value);
        }
        partial void OnAppBarHeightChanged(double value)
        {
            appSettingsService.Settings.SetAppBarHeight(value);
        }

        public async Task SaveProxyConfAsync(AppSettings.ProxyConf conf)
        {
            appSettingsService.Settings.Proxy = conf;
            await appSettingsService.Save();
        }

        partial void OnSelectedMonitorChanged(MonitorDisplayItem? value)
        {
            if (value != null)
            {
                TargetMonitorDeviceName = value.DeviceName;
            }
        }

        partial void OnTargetMonitorDeviceNameChanged(string? value)
        {
            appSettingsService.Settings.SetTargetMonitor(value);
        }
    }
}
