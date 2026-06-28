using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Win32;
using MyToolBar.Plugin;
using MyToolBar.Services;
using MyToolBar.Views.Items;

namespace MyToolBar.Views.Pages.Settings
{
    /// <summary>
    /// 包列表页面（一级页面）：显示所有已安装的插件包，管理启用/禁用，提供入口进入包配置。
    /// </summary>
    public partial class ComponentsSettingsPage : Page
    {
        private readonly ManagedPackageService _managedPackage;

        public ComponentsSettingsPage(ManagedPackageService managedPackage)
        {
            InitializeComponent();
            _managedPackage = managedPackage;
            Loaded += ComponentsSettingsPage_Loaded;
        }

        private void ComponentsSettingsPage_Loaded(object sender, RoutedEventArgs e)
        {
            LoadPage();
        }

        private void LoadPage()
        {
            PackagesPanel.Children.Clear();
            foreach (var pkg in _managedPackage.ManagedPkg.Values)
            {
                var item = new ComponentSettingItem(_managedPackage, pkg.Package);
                item.ConfigureClicked += OnConfigureClicked;
                PackagesPanel.Children.Add(item);
            }
        }

        private void OnConfigureClicked(IPackage package, ManagedPackageService service)
        {
            var configPage = new PackagePluginSettingsPage(package, service);
            NavigationService?.Navigate(configPage);
        }

        private async void AddPluginBtn_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFolderDialog();
            dialog.DefaultDirectory = ManagedPackageService.PackageDirectory;
            if (dialog.ShowDialog() is true && dialog.FolderName is { Length: > 0 } path)
            {
                _managedPackage.SyncFromPackagePath(path);
                await _managedPackage.SaveManagedPkgConf();
                LoadPage();
            }
        }
    }
}
