using MyToolBar.PopupWindows.Items;
using MyToolBar.WinApi;
using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MyToolBar
{
    /// <summary>
    /// SettingsWindow.xaml 的交互逻辑
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();
            Loaded += SettingsWindow_Loaded;
            Closing += SettingsWindow_Closing;
            MouseLeftButtonDown += SettingsWindow_MouseLeftButtonDown;
        }

        private void SettingsWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            SaveSettings();
        }

        private void SettingsWindow_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                this.DragMove();
        }

        private void SettingsWindow_Loaded(object sender, RoutedEventArgs e)
        {
            WindowAccentCompositor wac = new(this, false, (c) =>
            {
                c.A = 255;
                Background = new SolidColorBrush(c);
            });
            wac.Color = GlobalService.IsDarkMode ?
            Color.FromArgb(180, 0, 0, 0) :
            Color.FromArgb(180, 255, 255, 255);
            wac.DarkMode = GlobalService.IsDarkMode;
            wac.IsEnabled = true;

            foreach(MeumItem i in SettingsItemList.Children)
            {
                i.MouseDown += MeumItem_MouseLeftButtonUp;
            }
            LoadSettings();
        }

        private void LoadSettings()
        {
            WeatherCap_Key.Text = GlobalService.WeatherApiKey.key;
            WeatherCap_host.Text = GlobalService.WeatherApiKey.host;
            WeatherCap_Lang.Text = GlobalService.WeatherApiKey.lang;
        }
        private void SaveSettings()
        {
            GlobalService.WeatherApiKey.key = WeatherCap_Key.Text;
            GlobalService.WeatherApiKey.host = WeatherCap_host.Text;
            GlobalService.WeatherApiKey.lang = WeatherCap_Lang.Text;
            GlobalService.WeatherApiKey.SaveKey();
        }

        private Grid _NowPage = null;
        private void NSPage(Grid page)
        {
            _NowPage ??= CapsulePage;
            _NowPage.Visibility=Visibility.Collapsed;
            page.Visibility = Visibility.Visible;
            var da = new ThicknessAnimation(new Thickness(0,50,0,-50),new Thickness(0),TimeSpan.FromMilliseconds(300));
            da.EasingFunction = new CubicEase() {EasingMode=EasingMode.EaseOut};
            page.BeginAnimation(MarginProperty, da);
            _NowPage = page;
        }

        private void MeumItem_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var item = sender as MeumItem;
            if (item != null)
            {
                switch (item.Tag.ToString())
                {
                    case "Compo":
                        NSPage(ComponentPage);
                        break;
                    case "Capsule":
                        NSPage(CapsulePage);
                        break;
                    case "About":
                        NSPage(AboutPage);
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
