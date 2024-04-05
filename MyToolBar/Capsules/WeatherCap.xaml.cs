﻿using MyToolBar.Func;
using MyToolBar.PopWindow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using static MyToolBar.GlobalService;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace MyToolBar.Capsules
{
    public class WeatherCache
    {
        public Dictionary<string, WeatherData> DataCache = new();
        public WeatherApi.City DefaultCity = new();
        public List<WeatherApi.City> FavorCities = new();
        public bool UsingIpAsDefault = true;
        private static string SettingSign="WeatherCache";
        public bool isEmpty => DataCache.Count == 0;
        public async Task<WeatherData> RequstCache(WeatherApi.City city)
        {
            if (DataCache.ContainsKey(city.id))
            {
                if (DateTime.Now - DataCache[city.id].UpdateTime <= TimeSpan.FromMinutes(10))
                {
                    //cache is valid
                    return DataCache[city.id];
                }
            }
            var data = new WeatherData();
            data.City = city;
            data.CurrentWeather = await city.GetCurrentWeather();
            data.DailyAirForecast = await city.GetAirForecastAsync();
            data.CurrentAir = await city.GetCurrentAQIAsync();
            data.DailyForecast = await city.GetForecastAsync();
            data.UpdateTime = DateTime.Now;
            DataCache[city.id] = data;
            await SaveCache();
            return data;
        }
        public Task SaveCache()
        {
            return Settings.Save(this, SettingSign);
        }
        public static async Task<WeatherCache> LoadCache()
        {
            var wc=await Settings.Load<WeatherCache>(SettingSign);
            return wc??new WeatherCache();
        }
    }
    /// <summary>
    /// WeatherCap.xaml 的交互逻辑
    /// </summary>
    public partial class WeatherCap : UserControl
    {
        public WeatherCap()
        {
            InitializeComponent();
        }
        private WeatherCache cache = null;

        private async Task LoadWeatherData()
        {
                cache ??=await WeatherCache.LoadCache();

            if (cache.isEmpty|| cache.UsingIpAsDefault)
                cache.DefaultCity = await WeatherApi.GetPositionByIpAsync();
            
            if (cache.DefaultCity.id!=null||await cache.DefaultCity.VerifyCityIdAsync())
            {
                var data=await cache.RequstCache(cache.DefaultCity);
                var wdata = data.CurrentWeather;
                Weather_info.Text = wdata.temp + "℃  " + wdata.status;
                Weather_Icon.Background = new ImageBrush(new BitmapImage(new Uri(WeatherApi.GetIcon(wdata.code))));
                var aqi = data.CurrentAir;
                AirLevel.Background = new SolidColorBrush(WeatherApi.GetAirLevelColor(aqi.level));
                data.UpdateTime = DateTime.Now;
            }
        }
        public async void LoadData()
        {
            await LoadWeatherData();
            GlobalTimer.Elapsed += GlobalTimer_Elapsed;
        }

        private void GlobalTimer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            //update weather data every t minutes
            if (DateTime.Now - cache.DataCache[cache.DefaultCity.id].UpdateTime >= TimeSpan.FromMinutes(10))
            {
                Dispatcher.Invoke(async () => await LoadWeatherData());
            }
        }

        private void UserControl_MouseEnter(object sender, MouseEventArgs e)
        {
            ViewerMask.BeginAnimation(OpacityProperty, new DoubleAnimation(0.2, 1, TimeSpan.FromMilliseconds(300)));
        }

        private void UserControl_MouseLeave(object sender, MouseEventArgs e)
        {
            ViewerMask.BeginAnimation(OpacityProperty, new DoubleAnimation(1,0, TimeSpan.FromMilliseconds(300)));
        }
        private bool BoxShowed = false;
        private async void ShowWeatherBox()
        {
            if(BoxShowed) return;
            if (cache.DataCache[cache.DefaultCity.id].CurrentWeather == null)
                await LoadWeatherData();
            var wb = new WeatherBox();
            wb.Closed += delegate { BoxShowed = false; };
            wb.LoadData(cache.DefaultCity,cache);
            wb.DefaultCityChanged += Wb_DefaultCityChanged;
            wb.Left = SystemParameters.WorkArea.Width - wb.Width;
            wb.Show();
            BoxShowed = true;
        }

        private async void Wb_DefaultCityChanged(object? sender, WeatherApi.City e)
        {
            cache.DefaultCity = e;
            cache.UsingIpAsDefault = false;
            await cache.SaveCache();
            await LoadWeatherData();
        }

        private void UserControl_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ShowWeatherBox();
        }

        private void UserControl_TouchEnter(object sender, TouchEventArgs e)
        {
            ShowWeatherBox();
        }
    }
}