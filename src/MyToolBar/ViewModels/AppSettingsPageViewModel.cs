using CommunityToolkit.Mvvm.ComponentModel;
using MyToolBar.Common;
using MyToolBar.Services;
using System.Threading.Tasks;

namespace MyToolBar.ViewModels
{
    public partial class AppSettingsPageViewModel
        (AppSettingsService appSettingsService,
        ApplicationService applicationService
        ): ObservableObject
    {
        [ObservableProperty]
        private AppSettings.WindowMode _windowMode;
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
        private LocalCulture.Language _appLanguage = (LocalCulture.Language)appSettingsService.Settings.Language;
        [ObservableProperty]
        private AppSettings.ProxyMode _proxyMode = appSettingsService.Settings.UserProxyMode;
        [ObservableProperty]
        private AppSettings.ProxyConf? _proxyConf = appSettingsService.Settings.Proxy;
        [ObservableProperty]
        private AppSettings.MenuIcon _mainMenuIcon= appSettingsService.Settings.MainMenuIcon;
        [ObservableProperty]
        private string? _appFont = App.Current.FindResource("AppDefaultFontFamilly").ToString();

        public void LoadData()
        {
            WindowMode = appSettingsService.Settings.CurrentWindowMode;
            BackgroundMode = appSettingsService.Settings.CurrentBackgroundMode;
            ImmerseMode = appSettingsService.Settings.CurrentImmerseMode;
            EnableIsland = appSettingsService.Settings.EnableIsland;
            AutoRunAtStartup = appSettingsService.Settings.AutoRunAtStartup;
            HideWhenFullScreen = GlobalService.EnableHideWhenFullScreen;
        }
        partial void OnHideWhenFullScreenChanged(bool value)
        {
            GlobalService.EnableHideWhenFullScreen = value;
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

        public async Task SaveProxyConfAsync(AppSettings.ProxyConf conf)
        {
            appSettingsService.Settings.Proxy = conf;
            await appSettingsService.Save();
        }
    }
}
