using Microsoft.Extensions.DependencyInjection;
using MyToolBar.PopupWindows.Items;
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace MyToolBar.PopupWindows
{
    /// <summary>
    /// 主菜单
    /// </summary>
    public partial class MainTitleMeum : PopWindowBase
    {
        public MainTitleMeum()
        {
            InitializeComponent();
            LoadMeumItems();
        }
        private void LoadMeumItems()
        {
            void SetItem(string contResName, string iconResName, Action<object, MouseButtonEventArgs> Event)
            {
                var item = new MeumItem();
                item.MeumContent = (string)FindResource($"MeumItem_{contResName}");

                if (iconResName == null)
                    item.Icon = null;
                else
                    item.Icon = (Geometry)FindResource($"Icon_{iconResName}");

                item.MouseLeftButtonUp += (s, e) => Event(s, e);
                ItemPanel.Children.Add(item);
            }

            SetItem("Settings", "Settings", MeumItem_Settings);
            SetItem("Exit", null, MeumItem_Exit);

            this.Height = ItemPanel.Children.Count * 40;
        }

        private void MeumItem_Settings(object sender, MouseButtonEventArgs e)
        {
            App.Host.Services
                .GetRequiredService<SettingsWindow>()
                .Show();
        }
        private void MeumItem_Exit(object sender, MouseButtonEventArgs e)
        {
            _ = App.Host.StopAsync();
        }
    }
}
