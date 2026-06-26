using System.Windows.Media;
using MyToolBar.Plugin.BasicPackage.Weather.Models;

namespace MyToolBar.Plugin.BasicPackage.Weather.Services;

public interface IWeatherService
{
    void SetProperty(WeatherApiProperty p);
    Task<bool> IsNetworkAvailableAsync();
    Task<City?> GetCityByPositionAsync();
    Task<List<City>> SearchCitiesAsync(string keyword);

    Task<WeatherNow?> GetCurrentWeatherAsync(City city);
    Task<List<WeatherDay>> GetForecastAsync(City city);
    Task<AirData?> GetCurrentAQIAsync(City city);
    Task<List<AirData>> GetAirForecastAsync(City city);
    Task<List<Warning>> GetWarningsAsync(City city);

    /// <summary>
    /// 将天气code转换为qweather-icons字体字符
    /// </summary>
    string GetWeatherIconChar(int code);

    Color GetAirLevelColor(int level);
    Color GetWarningLevelColor(string level);
}
