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
using MyToolBar.Views.Items;

namespace MyToolBar.Views.Pages.Settings
{
    /// <summary>
    /// CapsulesSettingsPage.xaml 的交互逻辑
    /// </summary>
    public partial class CapsulesSettingsPage : Page
    {
        private readonly ManagedPackageService _managedPackageService;
        private readonly PluginReactiveService _pluginReactiveService;
        public CapsulesSettingsPage(
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
            var caps=_managedPackageService.GetTypePlugins(PluginType.Capsule);
            foreach(var cap in caps)
            {
                var IsEnable=_pluginReactiveService.Capsules.ContainsKey(cap);
                var item=new CapsuleSettingItem(cap,IsEnable);
                item.OnIsEnableChanged += Item_OnIsEnableChanged;
                CapsuleList.Children.Add(item);
            }
        }

        private async void Item_OnIsEnableChanged(IPlugin plugin, bool isEnable) 
        {
            if (isEnable)
            {
                await _pluginReactiveService.AddCapsule(plugin);
            }
            else
            {
               await _pluginReactiveService.RemoveCapsule(plugin);
            }
        }
    }
}
