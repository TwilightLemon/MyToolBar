using System.Linq;
using System.Windows;
using System.Windows.Controls;
using MyToolBar.Plugin;
using MyToolBar.Services;

namespace MyToolBar.Views.Items
{
    /// <summary>
    /// ComponentSettingItem.xaml 的交互逻辑
    /// </summary>
    public partial class ComponentSettingItem : UserControl
    {
        private readonly ManagedPackageService _managedPackageService;
        private IPackage Package;
        public ComponentSettingItem(ManagedPackageService managedPackageService, IPackage package)
        {
            InitializeComponent();
            DataContext = package;
            _managedPackageService = managedPackageService;
            Package = package;
            EnableCheckBox.IsChecked=managedPackageService.ManagedPkg.Any(p=>p.Key==package.PackageName&&p.Value.IsEnabled);
            Init();
        }
        private void Init() {
            foreach(IPlugin plugin in Package.Plugins)
            {
                if(plugin.SettingsSignKeys!=null)
                {
                    SettingsPanel.Children.Add(new PluginSettingItem(plugin));
                }
            }
            if (SettingsPanel.Children.Count == 0)
            {
                PluginTb.Visibility = Visibility.Collapsed;
            }
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            if (Package == null)
                return;
            bool isEnable=EnableCheckBox.IsChecked==true;
            //if the package is already enabled, return
            if (_managedPackageService.ManagedPkg.Any(p=>p.Key==Package.PackageName&&p.Value.IsEnabled==isEnable))
                return;
            if (isEnable)
            {
                _managedPackageService.EnableInRegistered(Package.PackageName);
            }
            else
            {
                _managedPackageService.UnloadFromRegistered(Package.PackageName);
            }
        }
    }
}
