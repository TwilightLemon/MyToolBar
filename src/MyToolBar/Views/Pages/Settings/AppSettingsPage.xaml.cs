using MyToolBar.Common;
using MyToolBar.Services;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MyToolBar.Views.Pages.Settings
{
    /// <summary>
    /// AppSettingsPage.xaml 的交互逻辑
    /// </summary>
    public partial class AppSettingsPage : Page,INotifyPropertyChanged
    {
        private readonly AppSettingsService _appSettingsService;
        private readonly ApplicationService _applicationService;
        public AppSettingsPage(AppSettingsService appSettingsService, ApplicationService applicationService)
        {
            _appSettingsService = appSettingsService;
            _applicationService = applicationService;

            _useImMode = _appSettingsService.Settings.UseImmerseMode;
            _alwaysImMode = _appSettingsService.Settings.AlwaysUseImmerseMode;
            _autoRunAtStartup = _appSettingsService.Settings.AutoRunAtStartup;
            InitializeComponent();
            DataContext = this;
            Unloaded += AppSettingsPage_Unloaded;
        }

        #region XAML Bindings
        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        private bool _useImMode = true;
        public bool UseImMode
        {
            get => _useImMode;
            set 
            {
                _useImMode = _appSettingsService.Settings.UseImmerseMode = value;
                OnPropertyChanged(nameof(UseImMode));
                if (!value)
                {
                    AlwaysImMode = false;
                }
            }
        }
        private bool _alwaysImMode = false;
        public bool AlwaysImMode 
        {
            get => _alwaysImMode;
            set 
            {
                _alwaysImMode=_appSettingsService.Settings.AlwaysUseImmerseMode = value;
                OnPropertyChanged(nameof(AlwaysImMode));
                if (value)
                {
                    UseImMode = true;
                }
            }
        }
        private bool _autoRunAtStartup = false;
        public bool AutoRunAtStartup
        {
            get => _autoRunAtStartup;
            set
            {
                _autoRunAtStartup = _appSettingsService.Settings.AutoRunAtStartup = value;
                OnPropertyChanged(nameof(AutoRunAtStartup));
                _applicationService.SetAutoRunAtStartup();
            }
        }
        public LocalCulture.Language AppLanguage
        {
            get => LocalCulture.Current;
            set
            {
                if (value != LocalCulture.Current)
                {
                    LocalCulture.SetGlobalLanguage(value);
                    _appSettingsService.Settings.Language = (int)value;
                    OnPropertyChanged(nameof(AppLanguage));
                }
            }
        }

        public AppSettings.ProxyMode ProxyMode
        {
            get => _appSettingsService.Settings.UserProxyMode;
            set
            {
                _appSettingsService.Settings.UserProxyMode = value;
                OnPropertyChanged(nameof(ProxyMode));
                _applicationService.UpdateDefaultProxy();
            }
        }

        public AppSettings.MenuIcon MainMenuIcon
        {
            get => _appSettingsService.Settings.MainMenuIcon;
            set 
            {
                _appSettingsService.Settings.SetMainMenuIcon(value);
                OnPropertyChanged(nameof(MainMenuIcon));
            }
        }

        public AppSettings.ProxyConf? ProxyConf {
            get => _appSettingsService.Settings.Proxy;
        }
        #endregion

        private async void AppSettingsPage_Unloaded(object sender, RoutedEventArgs e)
        {
            _appSettingsService.Settings.Proxy = new AppSettings.ProxyConf(
                ProxyAddressTextBox.Text, ProxyPortTextBox.Text,
                ProxyUsernameTextBox.Text, ProxyPasswordBox.Password);
            await _appSettingsService.Save();
        }
    }
}
