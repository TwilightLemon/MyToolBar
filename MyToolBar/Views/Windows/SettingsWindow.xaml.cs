using MyToolBar.PopupWindows.Items;
using MyToolBar.Func;
using MyToolBar.OuterControls;
using MyToolBar.WinApi;
using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Printing;
using System.Text;
using System.Text.Json.Nodes;
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

        private async void SettingsWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            await SaveSettings();
        }

        private void SettingsWindow_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                this.DragMove();
        }

        private async void SettingsWindow_Loaded(object sender, RoutedEventArgs e)
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
            await LoadSettings();
        }
        private List<JsonNode> settRef=new();
        private async Task LoadSettings()
        {
            CapsuleList.Children.Clear();
            foreach (string sign in GlobalService.ManagedSettingsKey)
            {
                string filePath = Settings.GetPathBySign(sign);
                string data = await System.IO.File.ReadAllTextAsync(filePath);
                JsonNode node = JsonNode.Parse(data);
                settRef.Add(node);
                string PackageName= node["PackageName"].ToString();
                var items = node["Data"].AsObject();
                //生成边框
                Border b = new(){
                    Height=200,
                    CornerRadius = new CornerRadius(15),
                    Margin=new Thickness(20)
                };
                b.SetResourceReference(BackgroundProperty, "MaskColor");
                //生成容器
                Grid g = new Grid();
                b.Child = g;
                //生成标题
                TextBlock tb = new TextBlock()
                {
                    Text = PackageName,
                    FontSize = 20,
                    Margin = new Thickness(10),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment=VerticalAlignment.Top
                };
                g.Children.Add(tb);
                g.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Auto) });
                g.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
                g.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(4, GridUnitType.Star) });
                Grid.SetColumnSpan(tb, 2);

                //根据items生成控件：KeyName[TextBlock]: Value[TextBox]
                int i = 1;
                foreach (var item in items)
                {
                    var key = item.Key;
                    var value = item.Value;
                    TextBlock keyTb = new TextBlock()
                    {
                        Text = key,
                        FontSize = 15,
                        Margin = new Thickness(20,10,10,10),
                        VerticalAlignment = VerticalAlignment.Center
                    };
                    TextBox valueTb = new TextBox()
                    {
                        Text =value==null?"":value.ToString(),
                        FontSize = 15,
                        Height=30,
                        Margin = new Thickness(10),
                        Style=FindResource("SimpleTextBoxStyle") as Style,
                        VerticalAlignment = VerticalAlignment.Center
                    };
                    valueTb.SetResourceReference(BackgroundProperty, "MaskColor");
                    valueTb.SetResourceReference(ForegroundProperty, "ForeColor");
                    g.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Auto) });
                    g.Children.Add(keyTb);
                    g.Children.Add(valueTb);
                    Grid.SetRow(keyTb, i);
                    Grid.SetColumn(keyTb, 0);
                    Grid.SetRow(valueTb, i);
                    Grid.SetColumn(valueTb, 1);
                    i++;
                }
                CapsuleList.Children.Add(b);
            }
        }
        private async Task SaveSettings()
        {
            
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
                    case "OutterControl":
                        NSPage(OutterControlPage);
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
