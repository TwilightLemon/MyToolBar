using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using MyToolBar.Common;
using MyToolBar.Common.UIBases;
using MyToolBar.Plugin.BasicPackage.Weather.Models;
using MyToolBar.Plugin.BasicPackage.Weather.Services;

namespace MyToolBar.Plugin.BasicPackage.Weather.Views;

/// <summary>
/// WeatherCap.xaml 的交互逻辑
/// </summary>
public partial class WeatherCap : CapsuleBase
{
    internal static readonly string _settingsAPIKey = "MyToolBar.Plugin.BasicPackage.WeatherAPIKey";

    private readonly IWeatherService _weatherService = new WeatherService();
    private WeatherCache _cache = null!;
    private SettingsMgr<WeatherApiProperty>? _keyMgr;

    public WeatherCap()
    {
        InitializeComponent();
        PopupWindowType = null;
        InitLangRes();
    }

    private void InitLangRes()
    {
        string uri = $"/MyToolBar.Plugin.BasicPackage;component/LanguageRes/Caps/Lang{LocalCulture.Current switch
        {
            LocalCulture.Language.en_us => "En_US",
            LocalCulture.Language.zh_cn => "Zh_CN",
            _ => throw new Exception("Unsupported Language")
        }}.xaml";
        Resources.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri(uri, UriKind.Relative) });
    }

    public override void Uninstall()
    {
        base.Uninstall();
        GlobalService.GlobalTimer.Elapsed -= GlobalTimer_Elapsed;
    }

    private async Task LoadWeatherData()
    {
        if (!await _weatherService.IsNetworkAvailableAsync())
        {
            Weather_info.Text = "No Network";
            return;
        }

        _cache ??= await WeatherCache.LoadCache();

        if (_cache.IsEmpty || _cache.UsingLocatingAsDefault)
        {
            if (_cache.LocatingDate == null || DateTime.Now - _cache.LocatingDate >= TimeSpan.FromHours(6))
            {
                Weather_info.Text = FindResource("Tip_Locating").ToString();
                _cache.DefaultCity = await _weatherService.GetCityByPositionAsync();
                _cache.LocatingDate = DateTime.Now;
            }
        }

        if (_cache.DefaultCity != null)
        {
            Weather_info.Text = FindResource("Tip_Loading").ToString();
            if (await _cache.RequestCache(_cache.DefaultCity) is { } data)
            {
                var wdata = data.CurrentWeather;
                Weather_info.Text = $"{wdata.temp}℃  {wdata.status}";
                WeatherIcon.Text = _weatherService.GetWeatherIconChar(wdata.code);
                AirLevel.Background = data.CurrentAir is { } aqi
                    ? new SolidColorBrush(_weatherService.GetAirLevelColor(aqi.level))
                    : null;
            }
            else
            {
                Weather_info.Text = FindResource("Tip_Failed").ToString();
            }
        }
        else
        {
            Weather_info.Text = FindResource("Tip_Failed").ToString();
        }

        PopupWindowType = typeof(WeatherBox);
    }

    public override async void Install()
    {
        if (_keyMgr == null)
        {
            _keyMgr = new(_settingsAPIKey, WeatherCapPlugin._name);
            await _keyMgr.LoadAsync();
            _keyMgr.OnDataChanged += async delegate
            {
                await _keyMgr.LoadAsync();
                Dispatcher.Invoke(() => Install());
            };
        }

        if (string.IsNullOrEmpty(_keyMgr.Data.key))
        {
            Weather_info.Text = "No Key";
        }
        else
        {
            _keyMgr.Data.lang = LocalCulture.Current switch
            {
                LocalCulture.Language.en_us => "en",
                LocalCulture.Language.zh_cn => "zh",
                _ => "en"
            };
            _weatherService.SetProperty(_keyMgr.Data);
            await LoadWeatherData();
            GlobalService.GlobalTimer.Elapsed += GlobalTimer_Elapsed;
        }
    }

    private void GlobalTimer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
    {
        if (_cache.DefaultCity == null) return;
        if (_cache.DataCache.TryGetValue(_cache.DefaultCity.Id, out var cached) &&
            DateTime.Now - cached.UpdateTime >= TimeSpan.FromMinutes(10))
        {
            Dispatcher.Invoke(async () => await LoadWeatherData());
        }
    }

    protected override void RequestPopup()
    {
        if (string.IsNullOrEmpty(_keyMgr?.Data.key))
            return;
        base.RequestPopup();
    }

    protected override async void SetPopupProperty()
    {
        base.SetPopupProperty();
        if (PopupWindowInstance != null &&
            PopupWindowInstance.TryGetTarget(out var window) &&
            window is WeatherBox wb)
        {
            wb.ViewModel.Initialize(_cache, _weatherService);
            if (_cache.DefaultCity != null)
                await wb.ViewModel.LoadWeatherDataAsync(_cache.DefaultCity);
            wb.ViewModel.DefaultCityChanged += (_, city) =>
            {
                _cache.UsingLocatingAsDefault = _cache.DefaultCity?.Id == city.Id;
                _cache.DefaultCity = city;
                _cache.SaveCache();
                Dispatcher.Invoke(async () => await LoadWeatherData());
            };
        }
    }
}
