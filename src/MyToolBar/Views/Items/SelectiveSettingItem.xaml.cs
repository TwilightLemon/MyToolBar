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
    public partial class SelectiveSettingItem : ItemBase, INotifyPropertyChanged
    {
        public IPlugin Plugin { get; set; }
        private  bool _pluginIsEnabled = false;
        public bool PluginIsEnabled
        {
            get => _pluginIsEnabled;
            set
            {
                _pluginIsEnabled = value;
                OnPropertyChanged(nameof(PluginIsEnabled));
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public event Action<IPlugin,bool> OnIsEnableChanged;
        public SelectiveSettingItem(IPlugin plugin,bool isEnable)
        {
            InitializeComponent();
            EnableClickEvent = false;
            MaskCornerRadius = new CornerRadius(8);
            PluginIsEnabled = isEnable;
            Plugin = plugin;
            DataContext = this;
        }

        private void EnableToggle_Changed(object sender, RoutedEventArgs e)
        {
            OnIsEnableChanged?.Invoke(Plugin, PluginIsEnabled);
        }
    }
}
