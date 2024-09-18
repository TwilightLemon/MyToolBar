using System.Linq;
using System.Windows;
using System.Windows.Controls;
using MyToolBar.Common;
using MyToolBar.Plugin;
using MyToolBar.Services;

namespace MyToolBar.Views.Pages.Settings
{
    /// <summary>
    /// OuterControlSettingsPage.xaml 的交互逻辑
    /// </summary>
    public partial class OuterControlSettingsPage : Page
    {
        private ManagedPackageService _managedPackageService;
        private PluginReactiveService _pluginReactiveService;
        public OuterControlSettingsPage(
            ManagedPackageService managedPackageService,
            PluginReactiveService pluginReactiveService
            )
        {
            InitializeComponent();
            _pluginReactiveService = pluginReactiveService;
            _managedPackageService = managedPackageService;
            Loaded += OuterControlSettingsPage_Loaded;
        }

        private void OuterControlSettingsPage_Loaded(object sender, RoutedEventArgs e)
        {
            var plugins=_managedPackageService.GetTypePlugins(PluginType.OuterControl);
            this.DataContext = plugins;
            //寻找当前已启用的OuterControl，并选中
            if (plugins.FirstOrDefault(plugins => _pluginReactiveService.OuterControl == plugins) is IPlugin plugin)
            {
                OCPluginList.SelectedItem = plugin;
            }
        }

        private async void OCPluginList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(OCPluginList.SelectedItem is IPlugin plugin)
            {
                await _pluginReactiveService.SetOuterControl(plugin);
            }
        }

        private async void RemoveOuterControlBtn_Click(object sender, RoutedEventArgs e)
        {
            await _pluginReactiveService.RemoveOuterControl();
            OCPluginList.SelectedItem = null;
        }
        uint i = 1;
        private void testBtn_Click(object sender, RoutedEventArgs e)
        {
            NotificationManager.Send(new("TEST HELLO TWLMGATITO! x"+i, NotificationType.Warning, null, NotificationTimeSpan.Short));
            i++;
        }
    }
}
