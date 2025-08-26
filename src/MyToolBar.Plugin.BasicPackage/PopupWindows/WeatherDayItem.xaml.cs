using System;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MyToolBar.Common.UIBases;
using MyToolBar.Plugin.BasicPackage.API;

namespace MyToolBar.Plugin.BasicPackage.PopupWindows
{
    /// <summary>
    /// WeatherDayItem.xaml 的交互逻辑
    /// </summary>
    public partial class WeatherDayItem : ItemBase
    {
        public WeatherDayItem()
        {
            InitializeComponent();
        }
        public WeatherApi.WeatherDay DailyData;
        public WeatherDayItem(WeatherApi.WeatherDay day,WeatherApi.AirData? air)
        {
            InitializeComponent();
            DailyData = day;
            if (air != null)
            {
                AQI.Text = air.aqi.ToString();
                AQIColor.Background = new SolidColorBrush(WeatherApi.GetAirLevelColor(air.level));
            }
            else
            {
                AQIColor.Visibility = System.Windows.Visibility.Collapsed;
            }
            icon.Background = new ImageBrush(new BitmapImage(new Uri(WeatherApi.GetIcon(day.code_day))));
            MaxTemp.Text = day.temp_max+"℃";
            MinTemp.Text = day.temp_min + "℃";
        }
    }
}
