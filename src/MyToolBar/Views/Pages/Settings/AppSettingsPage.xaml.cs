using MyToolBar.Common;
using MyToolBar.Services;
using System.ComponentModel;
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
            InitializeComponent();
            DataContext = this;
            Loaded += AppSettingsPage_Loaded;
            Unloaded += AppSettingsPage_Unloaded;
            _appSettingsService = appSettingsService;
            _applicationService = applicationService;
        }

        private void AppSettingsPage_Loaded(object sender, RoutedEventArgs e)
        {
            // Load settings
            AlwaysImMode = _appSettingsService.Settings.AlwaysUseImmerseMode;
            AutoRunAtStartup = _appSettingsService.Settings.AutoRunAtStartup;
        }
        #region XAML Bindings
        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        private bool _alwaysImMode = false;
        public bool AlwaysImMode 
        {
            get => _alwaysImMode;
            set 
            {
                _alwaysImMode=_appSettingsService.Settings.AlwaysUseImmerseMode = value;
                OnPropertyChanged(nameof(AlwaysImMode));
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
