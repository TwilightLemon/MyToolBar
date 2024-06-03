using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
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
    public partial class CapsuleSettingItem : ItemBase, INotifyPropertyChanged
    {
        public IPlugin Plugin { get; set; }
        private  bool _pluginIsEnabled = false;
        public bool PluginIsEnabled
        {
            get => _pluginIsEnabled;
            set
            {
                _pluginIsEnabled = value;
                OnPropertyChanged("PluginIsEnabled");
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public event Action<IPlugin,bool> OnIsEnableChanged;
        public CapsuleSettingItem(IPlugin plugin,bool isEnable)
        {
            InitializeComponent();
            Click += CapsuleSettingItem_Click;
            PluginIsEnabled = isEnable;
            Plugin = plugin;
            DataContext = this;
        }

        private void CapsuleSettingItem_Click(object sender, RoutedEventArgs e) 
        {
            PluginIsEnabled = !PluginIsEnabled;
            OnIsEnableChanged?.Invoke(Plugin, PluginIsEnabled);
        }
    }
}
