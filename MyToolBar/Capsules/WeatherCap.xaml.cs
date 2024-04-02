using MyToolBar.Func;
using MyToolBar.PopWindow;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using static MyToolBar.GlobalService;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace MyToolBar.Capsules
{
    /// <summary>
    /// WeatherCap.xaml 的交互逻辑
    /// </summary>
    public partial class WeatherCap : UserControl
    {
        public WeatherCap()
        {
            InitializeComponent();
        }
        private async Task LoadWeatherData()
        {
            var city = WeatherDataCache.CurrentCity = await WeatherApi.GetPositionByIpAsync();
            if (await city.SearchCityAsync())
            {
                var wdata = WeatherDataCache.CurrentWeather = await city.GetCurrentWeather();
                Weather_info.Text = wdata.temp + "℃  " + wdata.status;
                Weather_Icon.Background = new ImageBrush(new BitmapImage(new Uri(WeatherApi.GetIcon(wdata.code))));
                var adata = WeatherDataCache.DailyAirForecast = await city.GetAirAsync();
                WeatherDataCache.DailyForecast = await city.GetForecastAsync();
                AirLevel.Background = new SolidColorBrush(WeatherApi.GetAirLevelColor(adata[0].level));
                WeatherDataCache.UpdateTime = DateTime.Now;
            }
        }
        public async void LoadData()
        {
            await LoadWeatherData();
            GlobalTimer.Elapsed += GlobalTimer_Elapsed;
        }

        private void GlobalTimer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            //update weather data every t minutes
            if (DateTime.Now - WeatherDataCache.UpdateTime >= TimeSpan.FromMinutes(10))
            {
                Dispatcher.Invoke(async () => await LoadWeatherData());
            }
        }

        private void UserControl_MouseEnter(object sender, MouseEventArgs e)
        {
            ViewerMask.BeginAnimation(OpacityProperty, new DoubleAnimation(0.2, 1, TimeSpan.FromMilliseconds(300)));
        }

        private void UserControl_MouseLeave(object sender, MouseEventArgs e)
        {
            ViewerMask.BeginAnimation(OpacityProperty, new DoubleAnimation(1,0, TimeSpan.FromMilliseconds(300)));
        }
        private bool BoxShowed = false;
        private async void ShowWeatherBox()
        {
            if(BoxShowed) return;
            if (WeatherDataCache.CurrentWeather == null)
                await LoadWeatherData();
            var wb = new WeatherBox();
            wb.Closed += delegate { BoxShowed = false; };
            wb.LoadData(WeatherDataCache.CurrentWeather);
            wb.Left = SystemParameters.WorkArea.Width - wb.Width;
            wb.Show();
            BoxShowed = true;
        }
        private void UserControl_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ShowWeatherBox();
        }

        private void UserControl_TouchEnter(object sender, TouchEventArgs e)
        {
            ShowWeatherBox();
        }
    }
}
