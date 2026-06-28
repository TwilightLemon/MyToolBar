using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using MyToolBar.Plugin;
using MyToolBar.Services;
using MyToolBar.Views.Items;

namespace MyToolBar.Views.Pages.Settings
{
    /// <summary>
    /// 包级别插件配置页面（二级页面），显示一个包内所有插件的设置项，提供显式 Apply 按钮。
    /// </summary>
    public partial class PackagePluginSettingsPage : Page
    {
        private readonly IPackage _package;
        private readonly ManagedPackageService _managedService;
        private readonly List<PluginSettingItem> _settingItems = [];

        public PackagePluginSettingsPage(IPackage package, ManagedPackageService managedService)
        {
            InitializeComponent();
            _package = package;
            _managedService = managedService;
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Loaded -= OnLoaded;

            // 设置页面标题为包名
            PageTitle.Text = _package.DisplayName;

            // 为包内每个有配置的插件生成设置 UI
            foreach (var plugin in _package.Plugins)
            {
                if (plugin.SettingsTypes != null || plugin.SettingsSignKeys != null)
                {
                    var item = new PluginSettingItem(plugin, _package.PackageName);
                    SettingsPanel.Children.Add(item);
                    _settingItems.Add(item);
                }
            }
        }

        private async void ApplyBtn_Click(object sender, RoutedEventArgs e)
        {
            ApplyBtn.IsEnabled = false;
            try
            {
                var tasks = _settingItems.Select(item => item.SaveAsync()).ToArray();
                await Task.WhenAll(tasks);
            }
            finally
            {
                ApplyBtn.IsEnabled = true;
            }
        }

        private void BackBtn_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.GoBack();
        }
    }
}
