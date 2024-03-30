using MyToolBar.Func;
using MyToolBar.WinApi;
using System;
using System.Diagnostics;
using System.Windows;
using static MyToolBar.GlobalService;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace MyToolBar.PopWindow
{
    /// <summary>
    /// WeatherBox.xaml 的交互逻辑
    /// </summary>
    public partial class WeatherBox : Window
    {
        public WeatherBox()
        {
            InitializeComponent();
            this.Activate();
            this.Top = -1 * this.Height;
            this.Deactivated += WeatherBox_Deactivated;
            this.Loaded += WeatherBox_Loaded;
        }

        private void WeatherBox_Loaded(object sender, RoutedEventArgs e)
        {
            var da = new DoubleAnimation(-1 * this.Height, 30, TimeSpan.FromMilliseconds(300));
            da.EasingFunction = new CubicEase();
            BeginAnimation(TopProperty, da);
        }

        private void WeatherBox_Deactivated(object? sender, EventArgs e)
        {
            var da = new DoubleAnimation(this.Top,-1*this.Height,TimeSpan.FromMilliseconds(300));
            da.EasingFunction = new CubicEase();
            da.Completed += Da_Completed;
            BeginAnimation(TopProperty, da);
        }

        private void Da_Completed(object? sender, EventArgs e)
        {
            Close();
        }

        public void LoadData(WeatherApi.WeatherNow wdata)
        {
            //Now Weather:
            Now_Location.Tag = wdata.link;
            Now_Location.Text = WeatherDataCache.CurrentCity.area+" "+WeatherDataCache.CurrentCity.city;
            Now_Temp.Text = wdata.temp+"℃";
            Now_desc.Text = wdata.status;
            Now_icon.Background= new ImageBrush(new BitmapImage(new Uri(WeatherApi.GetIcon(wdata.code))));
            WindDir.Text = wdata.windDir;
            WindScale.Text = "Level "+wdata.windScale;
            Humidity.Text = wdata.humidity+"%";
            vis.Text = wdata.vis+" km";
            FeelsLike.Text = wdata.feel+"℃";
            UpdateTime.Text=WeatherDataCache.UpdateTime.ToString("HH:mm");

            Days.Children.Clear();
            int averageTemp = 0;
            var data= WeatherDataCache.DailyForecast;
            var airData = WeatherDataCache.DailyAirForecast;
            //找出temp_max和temp_min的最大值和最小值
            int max = int.MinValue, min =int.MaxValue;
            foreach (var item in data)
            {
                if (item.temp_max > max) max = item.temp_max;
                if (item.temp_min < min) min = item.temp_min;
            }
            for(int i=0;i<5;i++)
            {
                averageTemp += (data[i].temp_max+data[i].temp_min)/2;
                Days.Children.Add(new WeatherDayItem(data[i], airData[i]));
            }
            averageTemp /= 5;
            double offset_max = 50.0 / (max-averageTemp),
                offset_min=50.0/(averageTemp-min);
            DateTime date= DateTime.Now;
            foreach(WeatherDayItem item in Days.Children)
            {
                //0:230 100 ,100:180 50
                var margin = item.TempLine.Margin;
                margin.Left=230-(item.DailyData.temp_max-averageTemp)*offset_max;
                margin.Right = 100 - ( averageTemp- item.DailyData.temp_min) * offset_min;
                item.TempLine.Margin = margin;
                item.Day.Text = date.DayOfWeek.ToString()[..3];
                date=date.AddDays(1);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ToolWindowApi.SetToolWindow(this);
            WindowAccentCompositor wac = new(this,false, (c) =>
            {
                c.A = 255;
                Background = new SolidColorBrush(c);
            });
            wac.Color = DarkMode ?
            Color.FromArgb(180, 0, 0, 0) :
            Color.FromArgb(180, 255, 255, 255);
            wac.DarkMode = DarkMode;
            wac.IsEnabled = true;
        }

        private void Now_Location_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Process.Start("explorer",Now_Location.Tag.ToString());
        }
    }
}
