using MyToolBar.Func;
using System;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MyToolBar.PopupWindows.Items
{
    /// <summary>
    /// WeatherDayItem.xaml 的交互逻辑
    /// </summary>
    public partial class WeatherDayItem : UserControl
    {
        public WeatherDayItem()
        {
            InitializeComponent();
        }
        public WeatherApi.WeatherDay DailyData;
        public WeatherDayItem(WeatherApi.WeatherDay day,WeatherApi.AirData air)
        {
            InitializeComponent();
            DailyData = day;
            AQI.Text = air.aqi.ToString();
            AQIColor.Background = new SolidColorBrush(WeatherApi.GetAirLevelColor(air.level));
            icon.Background = new ImageBrush(new BitmapImage(new Uri(WeatherApi.GetIcon(day.code_day))));
            MaxTemp.Text = day.temp_max+"℃";
            MinTemp.Text = day.temp_min + "℃";
        }
    }
}
