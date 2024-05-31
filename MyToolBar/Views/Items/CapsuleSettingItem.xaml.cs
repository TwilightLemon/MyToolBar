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
        private bool IsMouseLeftButtonDown= false;
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
            this.MouseLeftButtonUp += CapsuleSettingItem_MouseLeftButtonUp;
            MouseLeftButtonDown += CapsuleSettingItem_MouseLeftButtonDown;
            PluginIsEnabled = isEnable;
            Plugin = plugin;
            DataContext = this;
        }

        private void CapsuleSettingItem_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            IsMouseLeftButtonDown = true;
        }

        private void CapsuleSettingItem_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (IsMouseLeftButtonDown)
            {
                IsMouseLeftButtonDown = false;
                PluginIsEnabled = !PluginIsEnabled;
                OnIsEnableChanged?.Invoke(Plugin, PluginIsEnabled);
            }
        }
    }
}
