using System.Text.Json.Serialization;
using MyToolBar.Common;
using MyToolBar.Plugin.BasicPackage.Weather.Services;

namespace MyToolBar.Plugin.BasicPackage.Weather.Models;

public record WeatherData(
    DateTime UpdateTime,
    City City,
    AirData? CurrentAir,
    List<Warning> Warnings,
    WeatherNow CurrentWeather,
    List<WeatherDay> DailyForecast,
    List<AirData> DailyAirForecast);

/// <summary>
/// 为天气数据提供缓存、请求和保存
/// </summary>
public class WeatherCache
{
    private const string SettingSign = "WeatherCache";

    /// <summary>
    /// 对应城市ID的天气缓存数据
    /// </summary>
    public Dictionary<string, WeatherData> DataCache { get; set; } = [];

    /// <summary>
    /// Capsule上显示的默认城市
    /// </summary>
    public City? DefaultCity { get; set; }

    /// <summary>
    /// 收藏的城市列表
    /// </summary>
    public List<City> FavorCities { get; set; } = [];

    /// <summary>
    /// 上一次更新定位的时间
    /// </summary>
    [JsonIgnore]
    public DateTime? LocatingDate { get; set; }

    public bool UsingLocatingAsDefault { get; set; } = true;

    [JsonIgnore]
    public bool IsEmpty => DataCache.Count == 0;

    public async Task<WeatherData?> RequestCache(City city)
    {
        if (city?.Id == null) return null;

        if (DataCache.TryGetValue(city.Id, out var cached) &&
            DateTime.Now - cached.UpdateTime <= TimeSpan.FromMinutes(10))
        {
            return cached;
        }

        var data = new WeatherData(
            DateTime.Now,
            city,
            await city.GetCurrentAQIExt(),
            await city.GetWarningExt(),
            await city.GetCurrentWeatherExt(),
            await city.GetForecastExt(),
            await city.GetAirForecastExt());

        DataCache[city.Id] = data;
        SaveCache();
        return data;
    }

    public async void SaveCache()
    {
        await Settings.SaveAsync(this, SettingSign, Settings.sType.Cache);
    }

    public static async Task<WeatherCache> LoadCache()
    {
        var wc = await Settings.LoadAsync<WeatherCache>(SettingSign, Settings.sType.Cache);
        return wc ?? new WeatherCache();
    }
}
