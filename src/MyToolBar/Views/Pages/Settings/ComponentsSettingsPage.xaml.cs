using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using MyToolBar.Services;
using MyToolBar.Views.Items;

namespace MyToolBar.Views.Pages.Settings
{
    /// <summary>
    /// 显示可用的插件包 启用/禁用 以及插件包的托管设置项
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
            Loaded -= ComponentsSettingsPage_Loaded;
        }

        private void LoadPage()
        {
            PackagesPanel.Children.Clear();
            foreach (var pkg in _managedPackage.ManagedPkg.Values)
            {
                PackagesPanel.Children.Add(new ComponentSettingItem(_managedPackage, pkg.Package));
            }
        }

        private async void AddPluginBtn_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFolderDialog();
            dialog.DefaultDirectory = ManagedPackageService.PackageDirectory;
            if(dialog.ShowDialog() is true && dialog.FolderName is {Length:>0} path)
            {
                _managedPackage.SyncFromPackagePath(path);
                await _managedPackage.SaveManagedPkgConf();
                LoadPage();
            }
        }
    }
}
