using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using System.Windows.Media;

/*
 Weather Api
 see as : https://dev.qweather.com/docs/
 */

namespace MyToolBar.Plugin.BasicPackage.API
{
    public static class WeatherApi
    {
        public class Property
        {
            public string? key { get; set; }
            public string? lang { get; set; } = "en";
            public string? host { get; set; } = "devapi.qweather.com";
        }
        public class City
        {
            public string? Province { get; set; }
            public string? CityName { get; set; }
            public string? Area { get; set; }
            public string? Id { get; set; }
        }
        public class WeatherNow
        {
            public string status { get; set; }
            public string link{ get; set; }
            public string feel{ get; set; }
            public string windDir { get; set; }
            public string windScale{ get; set; }
            public string humidity { get; set; }
            public string vis { get; set; }
            public int code { get; set; }
            public int temp{ get; set; }
            //...more
        }
        public class WeatherDay
        {
            public string status_day { get; set; }
            public string status_night { get; set; }
            public int code_day { get; set; }
            public int code_night { get; set; }
            public int temp_max { get; set; }
            public int temp_min{ get; set; }
        }
        public class AirData
        {
            public int aqi { get; set; }
            public int level { get; set; }
            public string desc { get; set; }
            public string sug { get; set; }
        }

        public static void SetProperty(Property p)
        {
            key = p.key;
            lang = p.lang;
            host = p.host;
        }
        private static string key,
            lang = "en",
            host;
        public static async Task<City?> GetPositionByIpAsync()
        {
            string data = await HttpHelper.Get("https://www.useragentinfo.com", false);
            string str= HttpHelper.FindByAB(data, "位置信息</th>", "</td>")+ "</td>";
            Match m = Regex.Match(str, "<td class=\"col-auto fw-light\">(.*?) (.*?) (.*?) (.*?)</td>");
            if (m.Success)
            {
                Debug.WriteLine(m.Groups[0].Value);
                return new City()
                {
                    Province = m.Groups[2].Value,
                    CityName = m.Groups[3].Value,
                    Area = m.Groups[4].Value
                };
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
            string data=await HttpHelper.Get(url);
            var obj= JsonNode.Parse(data);
            if (obj != null && obj["code"].ToString() == "200")
            {
                JsonArray cities = obj["location"].AsArray();
                if (cities.Count() > 0)
                {
                    var ci = cities[0];
                    city.Area = ci["name"].ToString();
                    city.CityName = ci["adm2"].ToString();
                    city.Province = ci["adm1"].ToString();
                    city.Id = ci["id"].ToString();
                    return true;
                }
            }
            return false;
        }
        public static async Task<List<City>> SearchCitiesAsync(string keyword)
        {
            string url = $"https://geoapi.qweather.com/v2/city/lookup?location={HttpUtility.UrlEncode(keyword)}&key={key}&lang={lang}";
            string data = await HttpHelper.Get(url);
            var obj = JsonNode.Parse(data);
            var list=new List<City>();
            if (obj != null && obj["code"].ToString() == "200")
            {
                JsonArray cities = obj["location"].AsArray();
                foreach (var ci in cities)
                {
                    list.Add(new City()
                    {
                        Area = ci["name"].ToString(),
                        CityName = ci["adm2"].ToString(),
                        Province = ci["adm1"].ToString(),
                        Id = ci["id"].ToString()
                    });
                }
            }
            return list;
        }

        public static async Task<WeatherNow> GetCurrentWeather(this City city)
        {
            string url = $"https://{host}/v7/weather/now?location={city.Id}&key={key}&lang={lang}";
            string data= await HttpHelper.Get(url);
            var obj= JsonNode.Parse(data);
            if (obj != null & obj["code"].ToString() == "200")
            {
                var now = obj["now"];
                return new WeatherNow()
                {
                    temp = int.Parse(now["temp"].ToString()),
                    code = int.Parse(now["icon"].ToString()),
                    status = now["text"].ToString(),
                    link = obj["fxLink"].ToString(),
                    feel = now["feelsLike"].ToString(),
                    humidity = now["humidity"].ToString(),
                    windDir= now["windDir"].ToString(),
                    windScale= now["windScale"].ToString(),
                    vis= now["vis"].ToString()
                };
            }
            return null;
        }
        public static async Task<AirData> GetCurrentAQIAsync(this City city)
        {
            string url = $"https://{host}/airquality/v1/now/{city.Id}?key={key}&lang={lang}";
            string data = await HttpHelper.Get(url);
            var obj = JsonNode.Parse(data);
            if (obj != null & obj["code"].ToString() == "200")
            {
                var aqi = obj["aqi"][0];
                return new AirData()
                {
                    aqi = int.Parse(aqi["value"].ToString()),
                    level = int.Parse(aqi["level"].ToString()),
                    desc = aqi["category"].ToString(),
                    sug = aqi["health"]["effect"].ToString()
                };
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
            var obj = JsonNode.Parse(data);
            if (obj != null & obj["code"].ToString() == "200")
            {
                List<AirData> list = new();
                var daily = obj["daily"].AsArray();
                foreach(var i in daily)
                {
                    list.Add(new AirData()
                    {
                        aqi = int.Parse(i["aqi"].ToString()),
                        level = int.Parse(i["level"].ToString()),
                        desc = i["category"].ToString()
                    });
                }
                return list;
            }
            return null;
        }
        public static Color GetAirLevelColor(int level) => level switch
        {
            1=>Color.FromArgb(255,0,228,0),
            2=>Color.FromArgb(255,255,255,0),
            3=>Color.FromArgb(255,255,126,0),
            4=>Color.FromArgb(255,255,0,0),
            5=>Color.FromArgb(255,153,0,76),
            6=>Color.FromArgb(255,126,0,35)
        };
        public static async Task<List<WeatherDay>> GetForecastAsync(this City city)
        {
            string url = $"https://{host}/v7/weather/7d?location={city.Id}&key={key}&lang={lang}";
            string data = await HttpHelper.Get(url);
            var obj = JsonNode.Parse(data);
            if (obj != null & obj["code"].ToString() == "200")
            {
                List<WeatherDay> list = new();
                var daily = obj["daily"].AsArray();
                foreach (var i in daily)
                {
                    list.Add(new WeatherDay()
                    {
                        status_day = i["textDay"].ToString(),
                        status_night= i["textNight"].ToString(),
                        code_day= int.Parse(i["iconDay"].ToString()),
                        code_night= int.Parse(i["iconNight"].ToString()),
                        temp_max = int.Parse(i["tempMax"].ToString()),
                        temp_min = int.Parse(i["tempMin"].ToString()),
                    });
                }
                return list;
            }
            return null;
        }
    }
}
