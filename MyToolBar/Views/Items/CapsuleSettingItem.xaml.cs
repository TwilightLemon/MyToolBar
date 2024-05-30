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
using MyToolBar.Common.UIBases;
using MyToolBar.Plugin;

namespace MyToolBar.Views.Items
{
    /// <summary>
    /// CapsuleSettingItem.xaml 的交互逻辑
    /// </summary>
    public partial class CapsuleSettingItem : ItemBase
    {
        private  IPlugin Plugin;
        private bool PluginIsEnabled=false;
        public event Action<IPlugin,bool> OnIsEnableChanged;
        public CapsuleSettingItem(IPlugin plugin,bool isEnable)
        {
            InitializeComponent();
            this.MouseLeftButtonUp += CapsuleSettingItem_MouseLeftButtonUp;
            PluginIsEnabled = isEnable;
            Plugin = plugin;
            DataContext = this;
        }

        private void CapsuleSettingItem_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            PluginIsEnabled = !PluginIsEnabled;
            OnIsEnableChanged?.Invoke(Plugin, PluginIsEnabled);
        }
    }
}
