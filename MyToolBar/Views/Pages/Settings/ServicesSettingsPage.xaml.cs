using MyToolBar.Plugin;
using MyToolBar.Services;
using MyToolBar.Views.Items;
using System.Windows.Controls;

namespace MyToolBar.Views.Pages.Settings
{
    /// <summary>
    /// ServicesSettingsPage.xaml 的交互逻辑
    /// </summary>
    public partial class ServicesSettingsPage : Page
    {
        private readonly ManagedPackageService _managedPackageService;
        private readonly PluginReactiveService _pluginReactiveService;
        public ServicesSettingsPage(
            ManagedPackageService managedPackageService,
            PluginReactiveService pluginReactiveService)
        {
            InitializeComponent();
            _managedPackageService = managedPackageService;
            _pluginReactiveService = pluginReactiveService;
            Init();
        }
        private void Init()
        {
            var services = _managedPackageService.GetTypePlugins(PluginType.WindowService);
            foreach (var service in services)
            {
                var IsEnable = _pluginReactiveService.WindowServices.ContainsKey(service);
                var item = new SelectiveSettingItem(service, IsEnable) { Margin = new System.Windows.Thickness(5, 2, 5, 2) };
                item.OnIsEnableChanged += Item_OnIsEnableChanged;
                ServiceList.Children.Add(item);
            }
        }

        private async void Item_OnIsEnableChanged(IPlugin plugin, bool enable)
        {
            if (enable)
            {
                await _pluginReactiveService.AddWindowService(plugin);
            }
            else
            {
                await _pluginReactiveService.RemoveWindowService(plugin);
            }
        }
    }
}
