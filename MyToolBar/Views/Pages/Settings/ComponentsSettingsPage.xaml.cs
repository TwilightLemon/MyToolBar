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
            foreach(var pkg in _managedPackage.ManagedPkg.Values)
            {
                PackagesPanel.Children.Add(new ComponentSettingItem(_managedPackage, pkg.Package));
            }
            Loaded -= ComponentsSettingsPage_Loaded;
        }
    }
}
