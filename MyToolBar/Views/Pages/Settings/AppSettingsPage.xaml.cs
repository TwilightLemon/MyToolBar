using MyToolBar.Services;
using System;
using System.Collections.Generic;
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
    public partial class AppSettingsPage : Page
    {
        private readonly AppSettingsService _appSettingsService;
        public AppSettingsPage(AppSettingsService appSettingsService)
        {
            InitializeComponent();
            _appSettingsService = appSettingsService;
            LoadSettings();
        }

        private void LoadSettings()
        {
            AlwaysImModeCb.IsChecked = _appSettingsService.Settings.AlwaysUseImmerseMode;
            AutoRunCb.IsChecked = _appSettingsService.Settings.AutoRunAtStartup;
        }

        private void AlwaysImModeCb_Click(object sender, RoutedEventArgs e)
        {
            _appSettingsService.Settings.AlwaysUseImmerseMode = (bool)AlwaysImModeCb.IsChecked;
        }

        private void AutoRunCb_Click(object sender, RoutedEventArgs e)
        {
            _appSettingsService.Settings.AutoRunAtStartup = (bool)AutoRunCb.IsChecked;
        }
    }
}
