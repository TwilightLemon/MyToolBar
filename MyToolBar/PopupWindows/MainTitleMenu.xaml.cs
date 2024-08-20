using Microsoft.Extensions.DependencyInjection;
using MyToolBar.PopupWindows.Items;
using MyToolBar.Views.Windows;
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using MyToolBar.Common.UIBases;
using MyToolBar.Common;
using System.Windows.Documents;

namespace MyToolBar.PopupWindows
{
    /// <summary>
    /// 主菜单
    /// </summary>
    public partial class MainTitleMenu : PopupWindowBase
    {
        public MainTitleMenu()
        {
            InitializeComponent();
            LoadMenuItems();
        }
        private void LoadMenuItems()
        {
            void SetItem(string contResName, string? iconResName, Action<object, RoutedEventArgs> Event)
            {
                var item = new MenuItem();
                item.MenuContent = (string)FindResource($"MenuItem_{contResName}");

                if (iconResName == null)
                    item.Icon = null;
                else
                    item.Icon = (Geometry)FindResource($"Icon_{iconResName}");

                item.Click += (s, e) => Event(s, e);
                ItemPanel.Children.Add(item);
            }

            SetItem("Settings", "Settings", MenuItem_Settings);
            SetItem("Exit", null, MenuItem_Exit);

            this.Height = ItemPanel.Children.Count * 40;
        }
        bool _isSettingsWindowOpen = false;
        private void MenuItem_Settings(object sender, RoutedEventArgs e)
        {
            if (_isSettingsWindowOpen) return;
            _isSettingsWindowOpen = true;

            var window = App.Host.Services.GetRequiredService<SettingsWindow>();
            window.Closing += delegate {
                _isSettingsWindowOpen = false;
            };
            window.Show();
        }
        private void MenuItem_Exit(object sender, RoutedEventArgs e)
        {
            GlobalService.GlobalTimer.Stop();
            _ = App.Host.StopAsync();
        }
    }
}
