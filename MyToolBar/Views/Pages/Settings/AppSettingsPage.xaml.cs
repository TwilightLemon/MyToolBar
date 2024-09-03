using MyToolBar.Common;
using MyToolBar.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
            Unloaded += AppSettingsPage_Unloaded;
            _appSettingsService = appSettingsService;
            _applicationService = applicationService;
        }
        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public bool AlwaysImMode 
        { 
            get => _appSettingsService.Settings.AlwaysUseImmerseMode; 
            set 
            { 
                _appSettingsService.Settings.AlwaysUseImmerseMode = value;
                OnPropertyChanged(nameof(AlwaysImMode));
            }
        }
        public bool AutoRunAtStartup
        {
            get => _appSettingsService.Settings.AutoRunAtStartup;
            set
            {
                _appSettingsService.Settings.AutoRunAtStartup = value;
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

        private async void AppSettingsPage_Unloaded(object sender, RoutedEventArgs e)
        {
            _appSettingsService.Settings.Proxy = new AppSettings.ProxyConf(
                ProxyAddressTextBox.Text, ProxyPortTextBox.Text,
                ProxyUsernameTextBox.Text, ProxyPasswordBox.Password);
            await _appSettingsService.Save();
        }
    }
}
