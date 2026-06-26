using System.Text.Json.Serialization;

namespace MyToolBar.Plugin.BasicPackage.Weather.Models;

/// <summary>
/// Settings Data for Api config
/// </summary>
public class WeatherApiProperty
{
    public string? key { get; set; }
    [JsonIgnore]
    public string? lang { get; set; } = "en";
    /// <summary>
    /// The default host will be expired at 2026-1-1
    /// see as: https://blog.qweather.com/announce/public-api-domain-change-to-api-host/
    /// </summary>
    public string? host { get; set; } = "devapi.qweather.com";
    public string? personalHost { get; set; } = null;
}

public record City(string Province, string CityName, string Area, string Id);

public record WeatherNow(
    string status,
    string link,
    string feel,
    string windDir,
    string windScale,
    string humidity,
    string vis,
    int code,
    int temp)
{
    public WeatherNow() : this("", "", "", "", "", "", "", 0, 0) { }
}

public record WeatherDay(
    string status_day,
    string status_night,
    int code_day,
    int code_night,
    int temp_max,
    int temp_min);

public record AirData(int aqi, int level, string desc, string sug);

public record Warning(
    string sender,
    string level,
    string severity,
    string typeName,
    string text,
    DateTime endTime);
