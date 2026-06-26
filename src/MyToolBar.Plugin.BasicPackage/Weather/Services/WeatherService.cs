using System.Windows.Media;
using MyToolBar.Plugin.BasicPackage.Weather.Models;

namespace MyToolBar.Plugin.BasicPackage.Weather.Services;

public class WeatherService : IWeatherService
{
    public void SetProperty(WeatherApiProperty p) => WeatherApi.SetProperty(p);

    public Task<bool> IsNetworkAvailableAsync() => WeatherApi.IsNetworkConnecting();

    public Task<City?> GetCityByPositionAsync() => WeatherApi.GetCityByPositionAsync();

    public Task<List<City>> SearchCitiesAsync(string keyword) => WeatherApi.SearchCitiesAsync(keyword);

    public Task<WeatherNow?> GetCurrentWeatherAsync(City city) => WeatherApi.GetCurrentWeatherExt(city);

    public Task<List<WeatherDay>> GetForecastAsync(City city) => WeatherApi.GetForecastExt(city);

    public Task<AirData?> GetCurrentAQIAsync(City city) => WeatherApi.GetCurrentAQIExt(city);

    public Task<List<AirData>> GetAirForecastAsync(City city) => WeatherApi.GetAirForecastExt(city);

    public Task<List<Warning>> GetWarningsAsync(City city) => WeatherApi.GetWarningExt(city);

    /// <summary>
    /// 将天气code转换为qweather-icons字体字符
    /// 映射规则: code -> Unicode codepoint 0xF{code}
    /// 例如: 301 -> U+F301, 1010 -> U+F1010
    /// </summary>
    public string GetWeatherIconChar(int code)
    {
        int codepoint = Convert.ToInt32($"F{code}", 16);
        return char.ConvertFromUtf32(codepoint);
    }

    public Color GetAirLevelColor(int level) => WeatherApi.GetAirLevelColor(level);

    public Color GetWarningLevelColor(string level) => WeatherApi.GetWarningLevelColor(level);
}
