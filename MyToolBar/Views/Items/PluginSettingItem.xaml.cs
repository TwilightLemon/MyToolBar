using System.Text.Json.Nodes;
using System.Windows;
using System.Windows.Controls;
using MyToolBar.Common;
using MyToolBar.Plugin;

namespace MyToolBar.Views.Items
{
    /// <summary>
    /// PluginSettingItem.xaml 的交互逻辑
    /// </summary>
    public partial class PluginSettingItem : UserControl
    {
        private  IPlugin Plugin;
        public PluginSettingItem(IPlugin plugin)
        {
            InitializeComponent();
            Plugin= plugin;
            DataContext = this;
            Loaded += PluginSettingItem_Loaded;
        }

        private async void PluginSettingItem_Loaded(object sender, RoutedEventArgs e)
        {
            foreach(var sign in Plugin.SettingsSignKeys)
            {
                string filePath = Settings.GetPathBySign(sign,Settings.sType.Settings);
                string data = await System.IO.File.ReadAllTextAsync(filePath);
                JsonNode node = JsonNode.Parse(data);
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
                SettingItemList.Children.Add(b);
            }
        }
    }
}
