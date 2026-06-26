using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyToolBar.Plugin.BasicPackage.Weather.Models;
using MyToolBar.Plugin.BasicPackage.Weather.Services;

namespace MyToolBar.Plugin.BasicPackage.Weather.ViewModels;

public partial class WeatherBoxViewModel : ObservableObject
{
    private WeatherCache _cache = null!;
    private IWeatherService? _weatherService;

    // ===== Current Weather =====
    [ObservableProperty] private string _cityName = "";
    [ObservableProperty] private string _updateTime = "";
    [ObservableProperty] private string _currentTemp = "";
    [ObservableProperty] private string _weatherStatus = "";
    [ObservableProperty] private string _weatherIconChar = "";
    [ObservableProperty] private string _weatherLink = "";
    [ObservableProperty] private string _feelsLikeTemp = "";
    [ObservableProperty] private string _windScale = "";
    [ObservableProperty] private string _windDir = "";
    [ObservableProperty] private string _humidity = "";
    [ObservableProperty] private string _visibility = "";

    // ===== Air Quality =====
    [ObservableProperty] private string _aqiText = "";
    [ObservableProperty] private Color _aqiColor;
    [ObservableProperty] private bool _hasAirData = true;
    [ObservableProperty] private bool _hasWarnings;

    // ===== Page State =====
    [ObservableProperty] private string _searchText = "";
    [ObservableProperty] private bool _isSearchPage;
    [ObservableProperty] private bool _isLoading;

    // ===== Collections =====
    public ObservableCollection<ForecastDayViewModel> DailyForecast { get; } = [];
    public ObservableCollection<Warning> Warnings { get; } = [];
    public ObservableCollection<CityItemViewModel> SearchResults { get; } = [];
    public ObservableCollection<CityItemViewModel> FavorCities { get; } = [];

    [ObservableProperty] private CityItemViewModel? _defaultCityItem;

    // ===== Events =====
    public event EventHandler<City>? DefaultCityChanged;

    // ===== Initialization =====
    public void Initialize(WeatherCache cache, IWeatherService weatherService)
    {
        _cache = cache;
        _weatherService = weatherService;
    }

    // ===== Core: Load Weather Data =====
    public async Task LoadWeatherDataAsync(City city)
    {
        if (_weatherService == null) return;

        IsLoading = true;
        try
        {
            var data = await _cache.RequestCache(city);
            if (data == null) return;

            // Current weather
            CityName = $"{data.City.Area} {data.City.CityName}";
            WeatherLink = data.CurrentWeather.link;
            CurrentTemp = $"{data.CurrentWeather.temp}℃";
            WeatherStatus = data.CurrentWeather.status;
            WeatherIconChar = _weatherService.GetWeatherIconChar(data.CurrentWeather.code);

            WindDir = data.CurrentWeather.windDir;
            WindScale = data.CurrentWeather.windScale;
            Humidity = $"{data.CurrentWeather.humidity}%";
            Visibility = $"{data.CurrentWeather.vis} km";
            FeelsLikeTemp = $"{data.CurrentWeather.feel}℃";
            UpdateTime = data.UpdateTime.ToString("HH:mm");

            // Air quality
            if (data.CurrentAir is { } air)
            {
                HasAirData = true;
                AqiText = $"AQI {air.aqi}";
                AqiColor = _weatherService.GetAirLevelColor(air.level);
            }
            else
            {
                HasAirData = false;
            }

            // Warnings
            Warnings.Clear();
            foreach (var w in data.Warnings)
                Warnings.Add(w);
            HasWarnings = data.Warnings.Count > 0;

            // Daily forecast
            BuildDailyForecast(data.DailyForecast, data.DailyAirForecast);

            // Load favorites
            await LoadFavorCitiesAsync();
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void BuildDailyForecast(List<WeatherDay> days, List<AirData> airData)
    {
        DailyForecast.Clear();
        if (days.Count == 0) return;

        // Calculate temp range for bar visualization
        int max = int.MinValue, min = int.MaxValue, avgTemp = 0;
        foreach (var d in days)
        {
            if (d.temp_max > max) max = d.temp_max;
            if (d.temp_min < min) min = d.temp_min;
            avgTemp += (d.temp_max + d.temp_min) / 2;
        }
        avgTemp /= days.Count;

        double offsetMax = 25.0 / Math.Max(max - avgTemp, 1);
        double offsetMin = 25.0 / Math.Max(avgTemp - min, 1);

        DateTime date = DateTime.Now;
        for (int i = 0; i < days.Count; i++)
        {
            var d = days[i];
            var air = airData.Count > i ? airData[i] : null;

            var fvm = new ForecastDayViewModel
            {
                DayLabel = date.ToString("M/d"),
                MaxTemp = $"{d.temp_max}℃",
                MinTemp = $"{d.temp_min}℃",
                DayStatus = d.status_day,
                IconChar = _weatherService?.GetWeatherIconChar(d.code_day) ?? "",
                TempLineMargin = new Thickness(230 - (d.temp_max - avgTemp) * offsetMax, 0,
                                                 50 + (avgTemp - d.temp_min) * offsetMin, 0)
            };

            if (air != null)
            {
                fvm.HasAirData = true;
                fvm.AqiText = air.aqi.ToString();
                fvm.AqiColor = _weatherService?.GetAirLevelColor(air.level) ?? Colors.Transparent;
            }
            else
            {
                fvm.HasAirData = false;
            }

            DailyForecast.Add(fvm);
            date = date.AddDays(1);
        }
    }

    private async Task LoadFavorCitiesAsync()
    {
        if (_weatherService == null) return;

        // Default city
        if (_cache.DefaultCity != null)
        {
            DefaultCityItem = new CityItemViewModel(_cache.DefaultCity,
                _cache.FavorCities.Exists(c => c.Id == _cache.DefaultCity.Id));
        }

        // Favorite cities
        FavorCities.Clear();
        foreach (var city in _cache.FavorCities)
        {
            var data = await _cache.RequestCache(city);
            var iconChar = data != null ? _weatherService.GetWeatherIconChar(data.CurrentWeather.code) : "";
            var vm = CityItemViewModel.WithPreview(city, data, iconChar, true);
            FavorCities.Add(vm);
        }
    }

    // ===== Commands =====

    [RelayCommand]
    private async Task SearchCities()
    {
        if (_weatherService == null || string.IsNullOrWhiteSpace(SearchText)) return;

        var results = await _weatherService.SearchCitiesAsync(SearchText);
        SearchResults.Clear();
        foreach (var city in results)
        {
            var vm = new CityItemViewModel(city,
                _cache.FavorCities.Exists(c => c.Id == city.Id));
            SearchResults.Add(vm);
        }
    }

    [RelayCommand]
    private async Task LocateCity()
    {
        if (_weatherService == null) return;

        IsLoading = true;
        try
        {
            SearchResults.Clear();
            var city = await _weatherService.GetCityByPositionAsync();
            if (city != null)
            {
                var vm = new CityItemViewModel(city,
                    _cache.FavorCities.Exists(c => c.Id == city.Id));
                SearchResults.Add(vm);
            }
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void ShowSearchPage()
    {
        IsSearchPage = true;
    }

    [RelayCommand]
    private void ShowMainPage()
    {
        IsSearchPage = false;
    }

    [RelayCommand]
    private async Task SelectCity(CityItemViewModel? vm)
    {
        if (vm == null) return;
        await LoadWeatherDataAsync(vm.City);
        IsSearchPage = false;
        _cache.SaveCache();
    }

    [RelayCommand]
    private void ToggleFavorCity(CityItemViewModel? vm)
    {
        if (vm == null) return;

        if (vm.IsFavor)
        {
            _cache.FavorCities.Remove(vm.City);
            vm.IsFavor = false;
        }
        else
        {
            _cache.FavorCities.Add(vm.City);
            vm.IsFavor = true;
        }

        // Refresh lists
        _ = LoadFavorCitiesAsync();
        _cache.SaveCache();
    }

    [RelayCommand]
    private void SetDefaultCity(CityItemViewModel? vm)
    {
        if (vm == null) return;

        _cache.UsingLocatingAsDefault = vm.City.Id == _cache.DefaultCity?.Id;
        _cache.DefaultCity = vm.City;

        DefaultCityItem = new CityItemViewModel(vm.City,
            _cache.FavorCities.Exists(c => c.Id == vm.City.Id));

        _cache.SaveCache();
        DefaultCityChanged?.Invoke(this, vm.City);
    }
}
