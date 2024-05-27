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
using MyToolBar.ViewModels;
using System.Windows.Navigation;

namespace MyToolBar
{
    /// <summary>
    /// SettingsWindow.xaml 的交互逻辑
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public SettingsWindow(
            SettingsViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;

            InitializeComponent();

            Loaded += SettingsWindow_Loaded;
            Closing += SettingsWindow_Closing;
        }

        private async void SettingsWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            await SaveSettings();
        }

        private async void SettingsWindow_Loaded(object sender, RoutedEventArgs e)
        {
            ViewModel.SelectedPage = ViewModel.SettingsPages.FirstOrDefault();

            await LoadSettings();
        }
        private List<JsonNode> settRef=new();
        private async Task LoadSettings()
        {
            //CapsuleList.Children.Clear();
            foreach (string sign in GlobalService.ManagedSettingsKey)
            {
                string filePath = Settings.GetPathBySign(sign);
                string data = await System.IO.File.ReadAllTextAsync(filePath);
                JsonNode node = JsonNode.Parse(data);
                settRef.Add(node);
                string PackageName= node["PackageName"].ToString();
                var items = node["Data"].AsObject();
                //生成边框
                Border b = new()
                {
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

                // TODO: 将 Capsules 页面逻辑分离
                //CapsuleList.Children.Add(b);
            }
        }

        private async Task SaveSettings()
        {

        }

        public SettingsViewModel ViewModel { get; }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            WindowFrame.Navigate(ViewModel.CurrentPageContent);


            // Clear history
            if (WindowFrame.CanGoBack || WindowFrame.CanGoForward)
            {
                JournalEntry? history;

                do
                {
                    history = WindowFrame.RemoveBackEntry();
                }
                while (history is not null);
            }
        }
    }
}
