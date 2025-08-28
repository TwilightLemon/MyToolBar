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
        public List<Warning> Warnings { get; set; }
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
        public bool UsingLocatingAsDefault { get; set; } = true;
        private static string SettingSign = "WeatherCache";
        [JsonIgnore]
        public bool isEmpty => DataCache.Count == 0;
        public async Task<WeatherData> RequstCache(WeatherApi.City city)
        {
            if (city == null || city.Id == null) return new();
            if (DataCache.ContainsKey(city.Id))
            {
                if (DateTime.Now - DataCache[city.Id].UpdateTime <= TimeSpan.FromMinutes(10))
                {
                    //cache is valid
                    return DataCache[city.Id];
                }
            }
            var data = new WeatherData
            {
                City = city,
                CurrentWeather = await city.GetCurrentWeather(),
                DailyAirForecast = await city.GetAirForecastAsync(),
                CurrentAir = await city.GetCurrentAQIAsync(),
                DailyForecast = await city.GetForecastAsync(),
                Warnings = await city.GetWarningAsync(),
                UpdateTime = DateTime.Now
            };
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
            PopupWindowType = typeof(WeatherBox);
            InitLangRes();
        }
        private void InitLangRes()
        {
            string uri = $"/MyToolBar.Plugin.BasicPackage;component/LanguageRes/Caps/Lang{
               LocalCulture.Current switch
            {
                LocalCulture.Language.en_us => "En_US",
                LocalCulture.Language.zh_cn => "Zh_CN",
                _ => throw new Exception("Unsupported Language")
            }}.xaml";
            Resources.MergedDictionaries.Add(new ResourceDictionary() { Source = new Uri(uri, UriKind.Relative) });
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
            if (cache.isEmpty || cache.UsingLocatingAsDefault)
            {
                if (cache.LocatingDate == null || DateTime.Now - cache.LocatingDate >= TimeSpan.FromHours(6))
                {
                    Weather_info.Text = FindResource("Tip_Locating").ToString();
                    cache.DefaultCity = await WeatherApi.GetCityByPositionAsync();
                    cache.LocatingDate = DateTime.Now;
                }
            }

            if (cache.DefaultCity != null)
            {
                Weather_info.Text= FindResource("Tip_Loading").ToString();
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
                KeyMgr.Data.lang = LocalCulture.Current switch { 
                LocalCulture.Language.en_us=>"en",
                LocalCulture.Language.zh_cn=>"zh",
                _=>"en"
                };
                WeatherApi.SetProperty(KeyMgr.Data);
                await LoadWeatherData();
                GlobalService.GlobalTimer.Elapsed += GlobalTimer_Elapsed;
            }
        }

        private void GlobalTimer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            //update weather data every t minutes
            if (cache.DefaultCity == null) return;
            if (DateTime.Now - cache.DataCache[cache.DefaultCity.Id].UpdateTime >= TimeSpan.FromMinutes(10))
            {
                Dispatcher.Invoke(async () => await LoadWeatherData());
            }
        }

        protected override void RequestPopup()
        {
            if (string.IsNullOrEmpty(KeyMgr.Data.key))
            {
                return;
            }

            base.RequestPopup();
        }
        protected override async void SetPopupProperty()
        {
            base.SetPopupProperty();
            if (PopupWindowInstance != null && PopupWindowInstance.TryGetTarget(out var window) && window is WeatherBox wb)
            {
                await wb.LoadData(cache.DefaultCity, cache);
                wb.DefaultCityChanged += Wb_DefaultCityChanged;
            }
        }

        private async void Wb_DefaultCityChanged(object? sender, WeatherApi.City e)
        {
            cache.UsingLocatingAsDefault = cache.DefaultCity.Id == e.Id;
            cache.DefaultCity = e;
            cache.SaveCache();
            await LoadWeatherData();
        }
    }
}
