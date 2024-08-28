using MyToolBar.Common.UIBases;
using MyToolBar.Plugin.BasicPackage.API;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;

namespace MyToolBar.Plugin.BasicPackage.PopupWindows
{
    /// <summary>
    /// WeatherWarningItem.xaml 的交互逻辑
    /// </summary>
    public partial class WeatherWarningItem : ItemBase
    {
        public WeatherApi.Warning? Warning { get; private set; }
        public WeatherWarningItem(WeatherApi.Warning warning)
        {
            InitializeComponent();
            Click += WeatherWarningItem_Click;
            Warning = warning;
            Load();
        }

        private void WeatherWarningItem_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            DetalPart.Visibility=DetalPart.Visibility==Visibility.Visible?Visibility.Collapsed:Visibility.Visible;
        }

        private void Load()
        {
            if (Warning == null) return;
            LevelColor.Fill = new SolidColorBrush(WeatherApi.GetWarningLevelColor(Warning.level));
            SeverityTb.Text = Warning.severity;
            TypeTb.Text = Warning.typeName;
            DetalTb.Text = Warning.text;
            PublisherTb.Text = Warning.sender;
        }
    }
}
