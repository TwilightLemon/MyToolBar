using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
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
            Unloaded += PluginSettingItem_Unloaded;
        }

        private void PluginSettingItem_Unloaded(object sender, RoutedEventArgs e)
        {
            var changedItems = _sDic.Values.Where(x => x.changed).ToList();
            foreach (var item in changedItems)
            {
                System.IO.File.WriteAllText(item.filePath, item.jsonObj.ToString());
            }
        }

        private record struct SettingJsonItem(string filePath,JsonNode jsonObj,bool changed);
        private record struct SettingItem(string sign,string dataKey,string dataValue);
        private Dictionary<string,SettingJsonItem> _sDic=[];
        private async void PluginSettingItem_Loaded(object sender, RoutedEventArgs e)
        {
            foreach(var sign in Plugin.SettingsSignKeys)
            {
                string filePath = Settings.GetPathBySign(sign,Settings.sType.Settings);
                if(!System.IO.File.Exists(filePath))
                {
                    continue;
                }
                string data = await System.IO.File.ReadAllTextAsync(filePath);
                JsonNode node = JsonNode.Parse(data);
                _sDic.Add(sign,new SettingJsonItem(filePath,node,false));
                string PackageName= node["PackageName"].ToString();
                var items = node["Data"].AsObject();
                //生成边框
                Border b = new()
                {
                    Height=double.NaN,
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
                    var value = item.Value==null?"":item.Value.ToString();
                    TextBlock keyTb = new TextBlock()
                    {
                        Text = key,
                        FontSize = 15,
                        Margin = new Thickness(20,10,10,10),
                        VerticalAlignment = VerticalAlignment.Center
                    };
                    TextBox valueTb = new TextBox()
                    {
                        Text =value,
                        FontSize = 15,
                        Height=30,
                        Margin = new Thickness(10),
                        Style=FindResource("SimpleTextBoxStyle") as Style,
                        VerticalAlignment = VerticalAlignment.Center,
                        Tag=new SettingItem(sign,key,value)
                    };
                    valueTb.TextChanged += ValueTb_TextChanged;
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

        private void ValueTb_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox tb && tb.Tag is SettingItem item)
            {
                var refObj=_sDic[item.sign];
                refObj.jsonObj["Data"][item.dataKey] = tb.Text;
                //标记为已修改  如果值不同
                refObj.changed = item.dataValue != tb.Text;
                _sDic[item.sign] = refObj;
            }
        }
    }
}
