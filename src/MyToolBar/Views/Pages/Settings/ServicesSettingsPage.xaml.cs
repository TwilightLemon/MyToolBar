using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyToolBar.Plugin;
using MyToolBar.Services;
using MyToolBar.Views.Items;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace MyToolBar.Views.Pages.Settings
{
    /// <summary>
    /// ServicesSettingsPage.xaml 的交互逻辑
    /// </summary>
    [ObservableObject]
    public partial class ServicesSettingsPage : Page
    {
        private readonly ManagedPackageService _managedPackageService;
        private readonly PluginReactiveService _pluginReactiveService;
        public ServicesSettingsPage(
            ManagedPackageService managedPackageService,
            PluginReactiveService pluginReactiveService)
        {
            _managedPackageService = managedPackageService;
            _pluginReactiveService = pluginReactiveService;
            Plugins = [.. _managedPackageService.GetTypePlugins(PluginType.UserService).Select(x => new PluginStateData(x, _pluginReactiveService.UserServices.Keys.Contains(x)))];
            DataContext = this;
            InitializeComponent();
        }

        [ObservableProperty]
        private List<PluginStateData> _plugins;

        [RelayCommand]
        public async Task SwitchPlugin(PluginStateData data)
        {
            if (data.IsEnabled)
            {
                await _pluginReactiveService.AddUserService(data.Plugin);
            }
            else
            {
                await _pluginReactiveService.RemoveUserService(data.Plugin);
            }
        }
    }
}
