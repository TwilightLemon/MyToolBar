using System;
using System.ComponentModel;
using System.Windows;
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
