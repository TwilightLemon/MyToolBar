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
            if(plugins.FirstOrDefault(plugins => _pluginReactiveService.OuterControl == plugins) is IPlugin plugin)
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
    }
}
