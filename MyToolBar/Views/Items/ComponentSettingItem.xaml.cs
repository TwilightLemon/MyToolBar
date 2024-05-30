using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
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
using MyToolBar.Common;
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
        }

        private async void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (Package == null)
                return;
            bool isEnable=EnableCheckBox.IsChecked==true;
            //if the package is already enabled, return
            if (_managedPackageService.ManagedPkg.ContainsKey(Package.PackageName) == isEnable)
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
