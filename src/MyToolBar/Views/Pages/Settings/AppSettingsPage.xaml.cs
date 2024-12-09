using MyToolBar.Common;
using MyToolBar.Services;
using MyToolBar.ViewModels;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MyToolBar.Views.Pages.Settings
{
    /// <summary>
    /// AppSettingsPage.xaml 的交互逻辑
    /// </summary>
    public partial class AppSettingsPage : Page
    {
        public AppSettingsPage(AppSettingsPageViewModel appSettingsPageViewModel)
        {
            InitializeComponent();
            DataContext = vm = appSettingsPageViewModel;
            Loaded += AppSettingsPage_Loaded;
            Unloaded += AppSettingsPage_Unloaded;
        }

        private void AppSettingsPage_Loaded(object sender, RoutedEventArgs e)
        {
            vm.LoadData();
        }

        private readonly AppSettingsPageViewModel vm;
        private async void AppSettingsPage_Unloaded(object sender, RoutedEventArgs e)
        {
            var conf = new AppSettings.ProxyConf(
                ProxyAddressTextBox.Text, ProxyPortTextBox.Text,
                ProxyUsernameTextBox.Text, ProxyPasswordBox.Password);
            await vm.SaveProxyConfAsync(conf);
        }
    }
}
