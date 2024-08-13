using Microsoft.Extensions.DependencyInjection;
using MyToolBar.PopupWindows.Items;
using MyToolBar.Views.Windows;
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using MyToolBar.Common.UIBases;
using MyToolBar.Common;

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
            LoadMeumItems();
        }
        private void LoadMeumItems()
        {
            void SetItem(string contResName, string iconResName, Action<object, RoutedEventArgs> Event)
            {
                var item = new MeumItem();
                item.MeumContent = (string)FindResource($"MeumItem_{contResName}");

                if (iconResName == null)
                    item.Icon = null;
                else
                    item.Icon = (Geometry)FindResource($"Icon_{iconResName}");

                item.Click += (s, e) => Event(s, e);
                ItemPanel.Children.Add(item);
            }

            SetItem("Settings", "Settings", MeumItem_Settings);
            SetItem("Exit", null, MeumItem_Exit);

            this.Height = ItemPanel.Children.Count * 40;
        }
        private void MeumItem_Settings(object sender, RoutedEventArgs e)
        {
            App.Host.Services
                .GetRequiredService<SettingsWindow>()
                .ShowDialog();
        }
        private void MeumItem_Exit(object sender, RoutedEventArgs e)
        {
            GlobalService.GlobalTimer.Stop();
            _ = App.Host.StopAsync();
        }
    }
}
