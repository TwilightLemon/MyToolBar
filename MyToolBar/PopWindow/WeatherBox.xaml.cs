﻿using MyToolBar.Func;
using MyToolBar.WinApi;
using System;
using System.Diagnostics;
using System.Windows;
using static MyToolBar.GlobalService;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Input;
using System.Windows.Documents;
using MyToolBar.Capsules;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace MyToolBar.PopWindow
{
    /// <summary>
    /// WeatherBox.xaml 的交互逻辑
    /// </summary>
    public partial class WeatherBox : Window
    {
        public WeatherBox()
        {
            InitializeComponent();
            this.Activate();
            this.Top = -1 * this.Height;
            this.Deactivated += WeatherBox_Deactivated;
            this.Loaded += WeatherBox_Loaded;
        }

        private void WeatherBox_Loaded(object sender, RoutedEventArgs e)
        {
            var da = new DoubleAnimation(-1 * this.Height, 30, TimeSpan.FromMilliseconds(300));
            da.EasingFunction = new CubicEase();
            BeginAnimation(TopProperty, da);
        }

        private void WeatherBox_Deactivated(object? sender, EventArgs e)
        {
            var da = new DoubleAnimation(this.Top,-1*this.Height,TimeSpan.FromMilliseconds(300));
            da.EasingFunction = new CubicEase();
            da.Completed += Da_Completed;
            BeginAnimation(TopProperty, da);
        }

        private void Da_Completed(object? sender, EventArgs e)
        {
            Close();
        }

        private WeatherCache cache = null;
        public async void LoadData(WeatherApi.City city,WeatherCache dat=null)
        {
            //Now Weather:
            cache ??= dat;
            var wdata = await dat.RequstCache(city);
            Now_Location.Tag = wdata.CurrentWeather.link;
            Now_Location.Text = wdata.City.area+" "+wdata.City.city;
            Now_Temp.Text = wdata.CurrentWeather.temp+"℃";
            Now_desc.Text = wdata.CurrentWeather.status;
            Now_icon.Background= new ImageBrush(new BitmapImage(new Uri(WeatherApi.GetIcon(wdata.CurrentWeather.code))));
            WindDir.Text = wdata.CurrentWeather.windDir;
            WindScale.Text = "Level "+wdata.CurrentWeather.windScale;
            Humidity.Text = wdata.CurrentWeather.humidity+"%";
            vis.Text = wdata.CurrentWeather.vis+" km";
            FeelsLike.Text = wdata.CurrentWeather.feel+"℃";
            UpdateTime.Text="Updated at "+wdata.UpdateTime.ToString("HH:mm");
            AQI_text.Text="AQI "+wdata.CurrentAir.aqi;
            AQI_text.ToolTip = wdata.CurrentAir.sug;
            AQI_Viewer.Background = new SolidColorBrush(WeatherApi.GetAirLevelColor(wdata.CurrentAir.level));
            
            Days.Children.Clear();
            int averageTemp = 0;
            var data= wdata.DailyForecast;
            var airData = wdata.DailyAirForecast;
            //找出temp_max和temp_min的最大值和最小值
            int max = int.MinValue, min =int.MaxValue;
            foreach (var item in data)
            {
                if (item.temp_max > max) max = item.temp_max;
                if (item.temp_min < min) min = item.temp_min;
            }
            for(int i=0;i<5;i++)
            {
                averageTemp += (data[i].temp_max+data[i].temp_min)/2;
                Days.Children.Add(new WeatherDayItem(data[i], airData[i]));
            }
            averageTemp /= 5;
            double offset_max = 50.0 / (max-averageTemp+1),
                offset_min=50.0/(averageTemp-min+1);
            DateTime date= DateTime.Now;
            foreach(WeatherDayItem item in Days.Children)
            {
                //0:230 100 ,100:180 50
                var margin = item.TempLine.Margin;
                margin.Left=230-(item.DailyData.temp_max-averageTemp)*offset_max;
                margin.Right = 50+ ( averageTemp- item.DailyData.temp_min) * offset_min;
                item.TempLine.Margin = margin;
                item.Day.Text = date.DayOfWeek.ToString()[..3];
                date=date.AddDays(1);
            }
            //加载Favorite
            LoadFavorList();
        }
        private async void LoadFavorList()
        {
            DefaultPosition.Children.Clear();
            var cur = new WeatherCityItem()
            {
                city = cache.DefaultCity,
                IsFavor = cache.FavorCities.Exists((c) => c.id == cache.DefaultCity.id)
            };
            cur.CitySelected += CityItem_CitySelected;
            cur.AddFavorCity += CityItem_AddFavorCity;
            cur.SetAsDefaultCity += CityItem_SetAsDefaultCity;
            DefaultPosition.Children.Add(cur);
            FavorCity.Children.Clear();
            foreach (var i in cache.FavorCities)
            {
                var item = new WeatherCityItem()
                {
                    city = i,
                    IsFavor = true
                };
                item.CitySelected += CityItem_CitySelected;
                item.AddFavorCity += CityItem_AddFavorCity;
                item.SetAsDefaultCity += CityItem_SetAsDefaultCity;
                FavorCity.Children.Add(item);
            }
        }

        public event EventHandler<WeatherApi.City> DefaultCityChanged;
        private void CityItem_SetAsDefaultCity(object? sender, WeatherApi.City e)
        {
            DefaultPosition.Children.Clear();
            var cur = new WeatherCityItem()
            {
                city = e,
                IsFavor = cache.FavorCities.Exists((c) => c.id == e.id)
            };
            cur.CitySelected += CityItem_CitySelected;
            cur.AddFavorCity += CityItem_AddFavorCity;
            cur.SetAsDefaultCity += CityItem_SetAsDefaultCity;
            DefaultPosition.Children.Add(cur);
            DefaultCityChanged?.Invoke(this, e);
        }

        private async void CityItem_AddFavorCity(object? sender, WeatherApi.City e)
        {
            var item = sender as WeatherCityItem;
            if (item.IsFavor)
            {
                item.IsFavor = false;
                cache.FavorCities.Remove(e);
            }
            else
            {
                item.IsFavor = true;
                cache.FavorCities.Add(e);
            }
            LoadFavorList();
            await cache.SaveCache();
        }

        private async void CityItem_CitySelected(object? sender, WeatherApi.City e)
        {
            LoadData(e,cache);
            (Resources["PageBack"] as Storyboard).Begin();
            await cache.SaveCache();
        }
      

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ToolWindowApi.SetToolWindow(this);
            WindowAccentCompositor wac = new(this,false, (c) =>
            {
                c.A = 255;
                Background = new SolidColorBrush(c);
            });
            wac.Color = DarkMode ?
            Color.FromArgb(180, 0, 0, 0) :
            Color.FromArgb(180, 255, 255, 255);
            wac.DarkMode = DarkMode;
            wac.IsEnabled = true;
        }

        private void Now_Location_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Process.Start("explorer",Now_Location.Tag.ToString());
        }

        private async void SearchCityBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var result = await WeatherApi.SearchCitiesAsync(SearchCityBox.Text);
                SearchCity_Result.Children.Clear();
                var t = new Thickness(0, 10, 0, 0);
                foreach (var city in result)
                {
                    var item = new WeatherCityItem()
                    {
                        city = city,
                        IsFavor= cache.FavorCities.Exists((c) => c.id == city.id),
                        Margin = t
                    };
                    item.CitySelected += CityItem_CitySelected;
                    item.AddFavorCity += CityItem_AddFavorCity;
                    item.SetAsDefaultCity += CityItem_SetAsDefaultCity;
                    SearchCity_Result.Children.Add(item);
                }
            }
        }

        private async void LocateBtn_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            SearchCity_Result.Children.Clear();
            var city = await WeatherApi.GetPositionByIpAsync();
            if(await city.VerifyCityIdAsync())
            {
                var item = new WeatherCityItem()
                {
                    city = city,
                    IsFavor = cache.FavorCities.Exists((c) => c.id == city.id),
                    Margin = new Thickness(0, 10, 0, 0)
            };
                item.CitySelected += CityItem_CitySelected;
                item.AddFavorCity += CityItem_AddFavorCity;
                item.SetAsDefaultCity += CityItem_SetAsDefaultCity;
                SearchCity_Result.Children.Add(item);
            }
        }
    }
}