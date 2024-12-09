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
        private bool _enableIsland;
        [ObservableProperty]
        private bool _useImmerseMode;
        [ObservableProperty]
        private bool _alwaysUseImmerseMode;
        [ObservableProperty]
        private bool _autoRunAtStartup;
        [ObservableProperty]
        private LocalCulture.Language _appLanguage = (LocalCulture.Language)appSettingsService.Settings.Language;
        [ObservableProperty]
        private AppSettings.ProxyMode _proxyMode = appSettingsService.Settings.UserProxyMode;
        [ObservableProperty]
        private AppSettings.ProxyConf? _proxyConf = appSettingsService.Settings.Proxy;
        [ObservableProperty]
        private AppSettings.MenuIcon _mainMenuIcon= appSettingsService.Settings.MainMenuIcon;

        public void LoadData()
        {
            EnableIsland = appSettingsService.Settings.EnableIsland;
            UseImmerseMode = appSettingsService.Settings.UseImmerseMode;
            AlwaysUseImmerseMode = appSettingsService.Settings.AlwaysUseImmerseMode;
            AutoRunAtStartup = appSettingsService.Settings.AutoRunAtStartup;
        }
        partial void OnEnableIslandChanged(bool value)
        {
            appSettingsService.Settings.SetEnableIsland(value);
        }
        partial void OnUseImmerseModeChanged(bool value)
        {
            appSettingsService.Settings.UseImmerseMode = value;
            if (!value)
            {
                AlwaysUseImmerseMode = false;
            }
        }
        partial void OnAlwaysUseImmerseModeChanged(bool value)
        {
            appSettingsService.Settings.AlwaysUseImmerseMode = value;
            if (value)
                UseImmerseMode = true;
        }
        partial void OnAutoRunAtStartupChanged(bool value)
        {
            appSettingsService.Settings.AutoRunAtStartup = value;
            applicationService.SetAutoRunAtStartup();//?
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
