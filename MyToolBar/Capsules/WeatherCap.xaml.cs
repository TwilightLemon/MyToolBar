using MyToolBar.Func;
using MyToolBar.PopupWindows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using static MyToolBar.GlobalService;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Text.Json.Serialization;

namespace MyToolBar.Capsules
{
    /// <summary>
    /// 为天气数据提供缓存、请求和保存
    /// </summary>
    public class WeatherCache
    {
        public Dictionary<string, WeatherData> DataCache { get; set; } = new();
        public WeatherApi.City DefaultCity { get; set; } = new();
        public List<WeatherApi.City> FavorCities { get; set; } = new();
        public bool UsingIpAsDefault { get; set; } = true;
        private static string SettingSign="WeatherCache";
        [JsonIgnore]
        public bool isEmpty => DataCache.Count == 0;
        public async Task<WeatherData> RequstCache(WeatherApi.City city)
        {
            if (DataCache.ContainsKey(city.Id))
            {
                if (DateTime.Now - DataCache[city.Id].UpdateTime <= TimeSpan.FromMinutes(10))
                {
                    //cache is valid
                    return DataCache[city.Id];
                }
            }
            var data = new WeatherData();
            data.City = city;
            data.CurrentWeather = await city.GetCurrentWeather();
            data.DailyAirForecast = await city.GetAirForecastAsync();
            data.CurrentAir = await city.GetCurrentAQIAsync();
            data.DailyForecast = await city.GetForecastAsync();
            data.UpdateTime = DateTime.Now;
            DataCache[city.Id] = data;
            SaveCache();
            return data;
        }
        public async void SaveCache()
        {
            await Settings.Save(this, SettingSign);
        }
        public static async Task<WeatherCache> LoadCache()
        {
            var wc=await Settings.Load<WeatherCache>(SettingSign);
            return wc??new WeatherCache();
        }
    }
    /// <summary>
    /// WeatherCap.xaml 的交互逻辑
    /// </summary>
    public partial class WeatherCap : CapsuleBase
    {
        public WeatherCap()
        {
            InitializeComponent();
        }

        private WeatherCache cache = null;
        private async Task LoadWeatherData()
        {
                cache ??=await WeatherCache.LoadCache();

            //初次使用或默认定位城市
            if (cache.isEmpty|| cache.UsingIpAsDefault)
                cache.DefaultCity = await WeatherApi.GetPositionByIpAsync();
            
            if (cache.DefaultCity.Id!=null||await cache.DefaultCity.VerifyCityIdAsync())
            {
                var data=await cache.RequstCache(cache.DefaultCity);
                var wdata = data.CurrentWeather;
                Weather_info.Text = wdata.temp + "℃  " + wdata.status;
                Weather_Icon.Background = new ImageBrush(new BitmapImage(new Uri(WeatherApi.GetIcon(wdata.code))));
                var aqi = data.CurrentAir;
                AirLevel.Background = new SolidColorBrush(WeatherApi.GetAirLevelColor(aqi.level));
                data.UpdateTime = DateTime.Now;
            }
        }
        private SettingsMgr<WeatherApi.Property> KeyMgr;
        public async void LoadData()
        {
            if (KeyMgr == null)
            {
                KeyMgr  =new("WeatherAPIKey", "WeatherCap");
                await KeyMgr.Load();
                KeyMgr.OnDataChanged += async delegate { 
                    await KeyMgr.Load();
                    Dispatcher.Invoke(() => LoadData());
                };
            }
            if (string.IsNullOrEmpty(KeyMgr.Data.key))
            {
                Weather_info.Text = "No Key";
            }
            else
            {
                WeatherApi.SetProperty(KeyMgr.Data);
                await LoadWeatherData();
                GlobalTimer.Elapsed += GlobalTimer_Elapsed;
            }
        }

        private void GlobalTimer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            //update weather data every t minutes
            if (DateTime.Now - cache.DataCache[cache.DefaultCity.Id].UpdateTime >= TimeSpan.FromMinutes(10))
            {
                Dispatcher.Invoke(async () => await LoadWeatherData());
            }
        }
        private bool BoxShowed = false;
        private async void ShowWeatherBox()
        {
            if(BoxShowed) return;
            try
            {
                if (cache.DataCache[cache.DefaultCity.Id].CurrentWeather == null)
                    await LoadWeatherData();
            }
            catch { }
            var wb = new WeatherBox();
            wb.Closed += delegate { BoxShowed = false; };
            await wb.LoadData(cache.DefaultCity,cache);
            wb.DefaultCityChanged += Wb_DefaultCityChanged;
            wb.Left = SystemParameters.WorkArea.Width - wb.Width;
            wb.Show();
            BoxShowed = true;
        }

        private async void Wb_DefaultCityChanged(object? sender, WeatherApi.City e)
        {
            cache.DefaultCity = e;
            cache.UsingIpAsDefault = cache.DefaultCity.Id == e.Id;
            cache.SaveCache();
            await LoadWeatherData();
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
