using System.Text.Json.Nodes;
using System.Web;
using System.Windows.Media;
using MyToolBar.Common.Func;
using MyToolBar.Plugin.BasicPackage.Weather.Models;

/*
 Weather Api
 see as : https://dev.qweather.com/docs/
 */

namespace MyToolBar.Plugin.BasicPackage.Weather.Services;

internal static class WeatherApi
{
    public static void SetProperty(WeatherApiProperty p)
    {
        key = p.key;
        lang = p.lang;
        host = p.host;
    }

    private static string? key;
    private static string lang = "en";
    private static string? host;

    public static async Task<bool> IsNetworkConnecting()
    {
        return await HttpHelper.Test("https://dev.qweather.com/");
    }

    private static async Task<(double x, double y)?> GetPosition()
    {
        try
        {
            var locator = new Windows.Devices.Geolocation.Geolocator();
            var location = await locator.GetGeopositionAsync();
            var position = location.Coordinate.Point.Position;
            return (position.Latitude, position.Longitude);
        }
        catch
        {
            return null;
        }
    }

    public static async Task<City?> GetCityByPositionAsync()
    {
        if (await GetPosition() is (double x, double y))
        {
            string url = $"https://geoapi.qweather.com/v2/city/lookup?location={Math.Round(y, 2)},{Math.Round(x, 2)}&key={key}&lang={lang}";
            var result = await CityLookUpAsync(url);
            if (result.Count > 0)
                return result.First();
        }
        return null;
    }

    public static async Task<bool> VerifyCityIdAsync(this City city)
    {
        string c1 = city.CityName, c2 = city.Area;
        if (string.IsNullOrEmpty(city.Area))
        {
            c1 = city.Province; c2 = city.CityName;
        }

        string url = $"https://geoapi.qweather.com/v2/city/lookup?location={HttpUtility.UrlEncode(c2)}&adm={HttpUtility.UrlEncode(c1)}&key={key}&lang={lang}";
        var result = await CityLookUpAsync(url);
        if (result.Count > 0)
        {
            city = result.First();
            return true;
        }
        return false;
    }

    public static async Task<List<City>> SearchCitiesAsync(string keyword)
    {
        string url = $"https://geoapi.qweather.com/v2/city/lookup?location={HttpUtility.UrlEncode(keyword)}&key={key}&lang={lang}";
        return await CityLookUpAsync(url);
    }

    private static async Task<List<City>> CityLookUpAsync(string url)
    {
        string data = await HttpHelper.Get(url);
        if (string.IsNullOrEmpty(data)) return [];
        var obj = JsonNode.Parse(data);
        var list = new List<City>();
        if (obj != null && obj["code"]?.ToString() == "200")
        {
            JsonArray cities = obj["location"]!.AsArray();
            foreach (var ci in cities)
            {
                list.Add(new City(
                    ci!["adm1"]!.ToString(),
                    ci["adm2"]!.ToString(),
                    ci["name"]!.ToString(),
                    ci["id"]!.ToString()));
            }
        }
        return list;
    }

    // ===== Extension methods for City (used by WeatherCache and WeatherService) =====

    public static async Task<WeatherNow?> GetCurrentWeatherExt(this City city)
    {
        string url = $"https://{host}/v7/weather/now?location={city.Id}&key={key}&lang={lang}";
        string data = await HttpHelper.Get(url);
        if (string.IsNullOrEmpty(data)) return new();
        var obj = JsonNode.Parse(data);
        if (obj != null && obj["code"]?.ToString() == "200")
        {
            var now = obj["now"]!;
            return new WeatherNow(
                now["text"]!.ToString(), obj["fxLink"]!.ToString(),
                now["feelsLike"]!.ToString(), now["windDir"]!.ToString(),
                now["windScale"]!.ToString(), now["humidity"]!.ToString(),
                now["vis"]!.ToString(), int.Parse(now["icon"]!.ToString()),
                int.Parse(now["temp"]!.ToString()));
        }
        return null;
    }

    public static async Task<AirData?> GetCurrentAQIExt(this City city)
    {
        string url = $"https://{host}/airquality/v1/now/{city.Id}?key={key}&lang={lang}";
        string data = await HttpHelper.Get(url);
        if (string.IsNullOrEmpty(data)) return null;
        var obj = JsonNode.Parse(data);
        if (obj != null && obj["code"]?.ToString() == "200")
        {
            var aqi = obj["aqi"]![0]!;
            return new AirData(
                int.Parse(aqi["value"]!.ToString()),
                int.Parse(aqi["level"]!.ToString()),
                aqi["category"]!.ToString(),
                aqi["health"]!["effect"]!.ToString());
        }
        return null;
    }

    public static async Task<List<AirData>> GetAirForecastExt(this City city)
    {
        string url = $"https://{host}/v7/air/5d?location={city.Id}&key={key}&lang={lang}";
        string data = await HttpHelper.Get(url);
        if (string.IsNullOrEmpty(data)) return [];
        var obj = JsonNode.Parse(data);
        if (obj != null && obj["code"]?.ToString() == "200")
        {
            List<AirData> list = [];
            var daily = obj["daily"]!.AsArray();
            foreach (var i in daily)
            {
                list.Add(new AirData(
                    int.Parse(i!["aqi"]!.ToString()),
                    int.Parse(i["level"]!.ToString()),
                    i["category"]!.ToString(),
                    default));
            }
            return list;
        }
        return [];
    }

    public static async Task<List<WeatherDay>> GetForecastExt(this City city)
    {
        string url = $"https://{host}/v7/weather/7d?location={city.Id}&key={key}&lang={lang}";
        string data = await HttpHelper.Get(url);
        if (string.IsNullOrEmpty(data)) return [];
        var obj = JsonNode.Parse(data);
        if (obj != null && obj["code"]?.ToString() == "200")
        {
            List<WeatherDay> list = [];
            var daily = obj["daily"]!.AsArray();
            foreach (var i in daily)
            {
                list.Add(new WeatherDay(
                    i!["textDay"]!.ToString(), i["textNight"]!.ToString(),
                    int.Parse(i["iconDay"]!.ToString()), int.Parse(i["iconNight"]!.ToString()),
                    int.Parse(i["tempMax"]!.ToString()), int.Parse(i["tempMin"]!.ToString())));
            }
            return list;
        }
        return [];
    }

    public static async Task<List<Warning>> GetWarningExt(this City city)
    {
        string url = $"https://{host}/v7/warning/now?location={city.Id}&lang={lang}&key={key}";
        string data = await HttpHelper.Get(url);
        if (string.IsNullOrEmpty(data)) return [];
        var obj = JsonNode.Parse(data);
        var list = new List<Warning>();
        if (obj != null && obj["code"]?.ToString() == "200")
        {
            var warnings = obj["warning"]!.AsArray();
            foreach (var i in warnings)
            {
                var w = new Warning(
                    i!["sender"]!.ToString(), i["severityColor"]!.ToString(),
                    i["level"]!.ToString(), i["typeName"]!.ToString(),
                    i["text"]!.ToString(), DateTime.Parse(i["endTime"]!.ToString()));
                list.Add(w);
            }
        }
        return list;
    }

    // ===== Color helpers =====

    public static Color GetAirLevelColor(int level) => level switch
    {
        1 => Color.FromArgb(255, 0, 228, 0),
        2 => Color.FromArgb(255, 255, 255, 0),
        3 => Color.FromArgb(255, 255, 126, 0),
        4 => Color.FromArgb(255, 255, 0, 0),
        5 => Color.FromArgb(255, 153, 0, 76),
        6 => Color.FromArgb(255, 126, 0, 35),
        _ => Colors.Transparent
    };

    public static Color GetWarningLevelColor(string level) => level switch
    {
        "White" => Color.FromArgb(255, 255, 255, 255),
        "Blue" => Color.FromArgb(255, 59, 151, 221),
        "Green" => Color.FromArgb(255, 107, 239, 92),
        "Yellow" => Color.FromArgb(255, 242, 190, 62),
        "Orange" => Color.FromArgb(255, 255, 126, 0),
        "Red" => Color.FromArgb(255, 254, 94, 94),
        "Black" => Colors.Black,
        _ => Colors.Transparent
    };
}
