using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyToolBar.Plugin;
using MyToolBar.Services;
using MyToolBar.Views.Items;

namespace MyToolBar.Views.Pages.Settings
{
    /// <summary>
    /// CapsulesSettingsPage.xaml 的交互逻辑
    /// </summary>
    [ObservableObject]
    public partial class CapsulesSettingsPage : Page
    {
        private readonly ManagedPackageService _managedPackageService;
        private readonly PluginReactiveService _pluginReactiveService;
        public CapsulesSettingsPage(
            ManagedPackageService managedPackageService,
            PluginReactiveService pluginReactiveService)
        {
            _managedPackageService = managedPackageService;
            _pluginReactiveService = pluginReactiveService;
            Plugins = [.. _managedPackageService.GetTypePlugins(PluginType.Capsule).Select(x => new PluginStateData(x, _pluginReactiveService.Capsules.Keys.Contains(x)))];
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
                await _pluginReactiveService.AddCapsule(data.Plugin);
            }
            else
            {
                await _pluginReactiveService.RemoveCapsule(data.Plugin);
            }
        }
    }
}
