using CommunityToolkit.Mvvm.ComponentModel;
using MyToolBar.Plugin.BasicPackage.Weather.Models;

namespace MyToolBar.Plugin.BasicPackage.Weather.ViewModels;

public partial class CityItemViewModel : ObservableObject
{
    public City City { get; }

    [ObservableProperty] private bool _isFavor;
    [ObservableProperty] private string _displayName;
    [ObservableProperty] private string? _tempInfo;
    [ObservableProperty] private string? _iconChar;

    public CityItemViewModel(City city, bool isFavor = false)
    {
        City = city;
        _isFavor = isFavor;
        _displayName = $"{city.Area} {city.CityName} {city.Province}";
    }

    /// <summary>
    /// Create a CityItemViewModel with weather preview data
    /// </summary>
    public static CityItemViewModel WithPreview(City city, WeatherData? data, string iconChar, bool isFavor = false)
    {
        var vm = new CityItemViewModel(city, isFavor)
        {
            _displayName = $"{city.Area}\n{city.CityName}  {city.Province}"
        };
        if (data != null)
        {
            vm.TempInfo = $"{data.CurrentWeather.temp}°C   {data.CurrentWeather.status}";
            vm.IconChar = iconChar;
        }
        return vm;
    }
}
