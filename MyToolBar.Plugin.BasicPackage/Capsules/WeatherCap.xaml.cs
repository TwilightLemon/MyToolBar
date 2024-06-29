using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Text.Json.Serialization;
using static MyToolBar.Plugin.BasicPackage.API.WeatherApi;
using MyToolBar.Plugin.BasicPackage.API;
using MyToolBar.Common;
using MyToolBar.Common.UIBases;
using MyToolBar.Plugin.BasicPackage.PopupWindows;

namespace MyToolBar.Plugin.BasicPackage.Capsules
{
    public class WeatherData
    {
        public DateTime UpdateTime { get; set; }
        public City City { get; set; }
        public AirData CurrentAir { get; set; }
        public WeatherNow CurrentWeather { get; set; }
        public List<WeatherDay> DailyForecast { get; set; }
        public List<AirData> DailyAirForecast { get; set; }
    }
    /// <summary>
    /// 为天气数据提供缓存、请求和保存
    /// </summary>
    public class WeatherCache
    {
        /// <summary>
        /// 对应城市ID的天气缓存数据
        /// </summary>
        public Dictionary<string, WeatherData> DataCache { get; set; } = new();
        /// <summary>
        /// Capsule上显示的默认城市
        /// </summary>
        public WeatherApi.City DefaultCity { get; set; } = new();
        /// <summary>
        /// 收藏的城市列表
        /// </summary>
        public List<WeatherApi.City> FavorCities { get; set; } = new();
        /// <summary>
        /// 上一次更新定位的时间
        /// </summary>
        [JsonIgnore]
        public DateTime? LocatingDate { get; set; } = null;
        public bool UsingIpAsDefault { get; set; } = true;
        private static string SettingSign = "WeatherCache";
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
            await Settings.Save(this, SettingSign, Settings.sType.Cache);
        }
        public static async Task<WeatherCache> LoadCache()
        {
            var wc = await Settings.Load<WeatherCache>(SettingSign, Settings.sType.Cache);
            return wc ?? new WeatherCache();
        }
    }
    /// <summary>
    /// WeatherCap.xaml 的交互逻辑
    /// </summary>
    public partial class WeatherCap : CapsuleBase
    {
        internal static readonly string _settingsAPIKey = "MyToolBar.Plugin.BasicPackage.WeatherAPIKey";

        public WeatherCap()
        {
            InitializeComponent();
        }
        public override void Uninstall()
        {
            base.Uninstall();
            GlobalService.GlobalTimer.Elapsed -= GlobalTimer_Elapsed;
        }

        private WeatherCache cache = null;
        private async Task LoadWeatherData()
        {
            //测试网络环境
            if (!await IsNetworkConnecting())
            {
                Weather_info.Text = "No Network";
                return;
            }
            cache ??= await WeatherCache.LoadCache();

            //初次使用或默认定位城市
            if (cache.isEmpty || cache.UsingIpAsDefault)
            {
                if (cache.LocatingDate == null || DateTime.Now - cache.LocatingDate >= TimeSpan.FromHours(6))
                {
                    Weather_info.Text = "Locating...";
                    cache.DefaultCity = await WeatherApi.GetCityByPositionAsync();
                    cache.LocatingDate = DateTime.Now;
                }
            }

            if (cache.DefaultCity != null)
            {
                Weather_info.Text= "Loading...";
                var data = await cache.RequstCache(cache.DefaultCity);
                var wdata = data.CurrentWeather;
                Weather_info.Text = wdata.temp + "℃  " + wdata.status;
                Weather_Icon.Background = new ImageBrush(new BitmapImage(new Uri(WeatherApi.GetIcon(wdata.code))));
                var aqi = data.CurrentAir;
                AirLevel.Background = new SolidColorBrush(WeatherApi.GetAirLevelColor(aqi.level));
                data.UpdateTime = DateTime.Now;
            }
        }
        private SettingsMgr<WeatherApi.Property> KeyMgr;
        public override async void Install()
        {
            if (KeyMgr == null)
            {
                KeyMgr = new(_settingsAPIKey, WeatherCapPlugin._name);
                await KeyMgr.Load();
                KeyMgr.OnDataChanged += async delegate
                {
                    await KeyMgr.Load();
                    Dispatcher.Invoke(() => Install());
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
                GlobalService.GlobalTimer.Elapsed += GlobalTimer_Elapsed;
            }
        }

        private void GlobalTimer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            //update weather data every t minutes
            if (cache==null||DateTime.Now - cache.DataCache[cache.DefaultCity.Id].UpdateTime >= TimeSpan.FromMinutes(10))
            {
                Dispatcher.Invoke(async () => await LoadWeatherData());
            }
        }
        private bool BoxShowed = false;
        private async void ShowWeatherBox()
        {
            if (string.IsNullOrEmpty(KeyMgr.Data.key))
            {
                return;
            }
            if (BoxShowed) return;
            try
            {
                if (cache.DataCache[cache.DefaultCity.Id].CurrentWeather == null)
                    await LoadWeatherData();
            }
            catch { }
            var wb = new WeatherBox();
            wb.Closed += delegate { BoxShowed = false; };
            await wb.LoadData(cache.DefaultCity, cache);
            wb.DefaultCityChanged += Wb_DefaultCityChanged;
            wb.Left = GlobalService.GetPopupWindowLeft(this, wb);
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
