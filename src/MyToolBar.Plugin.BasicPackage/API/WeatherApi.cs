using System.Diagnostics;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Web;
using MyToolBar.Common.Func;
using System.Windows.Media;
using System.Text.Json.Serialization;

//TODO： 处理空值 from HttpHelper.Get

/*
 Weather Api
 see as : https://dev.qweather.com/docs/
 */

namespace MyToolBar.Plugin.BasicPackage.API
{
    public static class WeatherApi
    {
        #region Data
        /// <summary>
        /// Settings Data for Api config
        /// </summary>
        public class Property
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
        #endregion

        public static void SetProperty(Property p)
        {
            key = p.key;
            lang = p.lang;
            host = p.host;
        }
        private static string key,
            lang = "en",
            host;

        public static async Task<bool> IsNetworkConnecting()
        {
            return await HttpHelper.Test("https://dev.qweather.com/");
        }

        public static async Task<(double x, double y)?> GetPosition()
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
            if (obj != null && obj["code"].ToString() == "200")
            {
                JsonArray cities = obj["location"].AsArray();
                foreach (var ci in cities)
                {
                    list.Add(new City(ci["adm1"].ToString(), ci["adm2"].ToString(), ci["name"].ToString(), ci["id"].ToString()));
                }
            }
            return list;
        }

        public static async Task<WeatherNow> GetCurrentWeather(this City city)
        {
            string url = $"https://{host}/v7/weather/now?location={city.Id}&key={key}&lang={lang}";
            string data = await HttpHelper.Get(url);
            if (string.IsNullOrEmpty(data)) return new();
            var obj = JsonNode.Parse(data);
            if (obj != null & obj["code"].ToString() == "200")
            {
                var now = obj["now"];
                return new WeatherNow(now["text"].ToString(), obj["fxLink"].ToString(), now["feelsLike"].ToString(), now["windDir"].ToString(), now["windScale"].ToString(), now["humidity"].ToString(), now["vis"].ToString(), int.Parse(now["icon"].ToString()), int.Parse(now["temp"].ToString()));
            }
            return null;
        }
        public static async Task<AirData?> GetCurrentAQIAsync(this City city)
        {
            string url = $"https://{host}/airquality/v1/now/{city.Id}?key={key}&lang={lang}";
            string data = await HttpHelper.Get(url);
            if (string.IsNullOrEmpty(data)) return null;
            var obj = JsonNode.Parse(data);
            if (obj != null & obj["code"].ToString() == "200")
            {
                var aqi = obj["aqi"][0];
                return new AirData(int.Parse(aqi["value"].ToString()), int.Parse(aqi["level"].ToString()), aqi["category"].ToString(), aqi["health"]["effect"].ToString());
            }
            return null;
        }
        public static string GetIcon(int code)
        {
            return $"https://a.hecdn.net/img/common/icon/202106d/{code}.png";
        }
        public static async Task<List<AirData>> GetAirForecastAsync(this City city)
        {
            string url = $"https://{host}/v7/air/5d?location={city.Id}&key={key}&lang={lang}";
            string data = await HttpHelper.Get(url);
            if (string.IsNullOrEmpty(data)) return new();
            var obj = JsonNode.Parse(data);
            if (obj != null & obj["code"].ToString() == "200")
            {
                List<AirData> list = new();
                var daily = obj["daily"].AsArray();
                foreach (var i in daily)
                {
                    list.Add(new AirData(int.Parse(i["aqi"].ToString()), int.Parse(i["level"].ToString()), i["category"].ToString(), default));
                }
                return list;
            }
            return null;
        }
        public static Color GetAirLevelColor(int level) => level switch
        {
            1 => Color.FromArgb(255, 0, 228, 0),
            2 => Color.FromArgb(255, 255, 255, 0),
            3 => Color.FromArgb(255, 255, 126, 0),
            4 => Color.FromArgb(255, 255, 0, 0),
            5 => Color.FromArgb(255, 153, 0, 76),
            6 => Color.FromArgb(255, 126, 0, 35),
            _=>Colors.Transparent
        };
        public static async Task<List<WeatherDay>> GetForecastAsync(this City city)
        {
            string url = $"https://{host}/v7/weather/7d?location={city.Id}&key={key}&lang={lang}";
            string data = await HttpHelper.Get(url);
            if (string.IsNullOrEmpty(data)) return new();
            var obj = JsonNode.Parse(data);
            if (obj != null & obj["code"].ToString() == "200")
            {
                List<WeatherDay> list = new();
                var daily = obj["daily"].AsArray();
                foreach (var i in daily)
                {
                    list.Add(new WeatherDay(i["textDay"].ToString(), i["textNight"].ToString(), int.Parse(i["iconDay"].ToString()), int.Parse(i["iconNight"].ToString()), int.Parse(i["tempMax"].ToString()), int.Parse(i["tempMin"].ToString())));
                }
                return list;
            }
            return null;
        }

        public static Color GetWarningLevelColor(string level)
            => level switch
            {
                "White" => Color.FromArgb(255, 255, 255, 255),
                "Blue" => Color.FromArgb(255, 59,151,221),
                "Green" => Color.FromArgb(255, 107,239,92),
                "Yellow"=>Color.FromArgb(255,242,190,62),
                "Orange" => Color.FromArgb(255, 255, 126, 0),
                "Red" => Color.FromArgb(255, 254, 94, 94),
                "Black"=>Colors.Black
            };
        public static async Task<List<Warning>> GetWarningAsync(this City city)
        {
            string url = $"https://{host}/v7/warning/now?location={city.Id}&lang={lang}&key={key}";
            string data=await HttpHelper.Get(url);
            if (string.IsNullOrEmpty(data)) return new();
            var obj = JsonNode.Parse(data);
            var list = new List<Warning>();
            if (obj != null & obj["code"].ToString() == "200")
            {
                var warning = obj["warning"].AsArray();
                foreach( var i in warning)
                {
                    var a = new Warning(i["sender"].ToString(), i["severityColor"].ToString(), i["level"].ToString(), i["typeName"].ToString(), i["text"].ToString(), DateTime.Parse(i["endTime"].ToString()));
                    list.Add(a);
                }
            }
            return list;
        }
    }
}

