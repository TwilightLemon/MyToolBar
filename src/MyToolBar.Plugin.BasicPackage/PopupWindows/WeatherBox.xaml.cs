﻿using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Input;
using MyToolBar.Common.UIBases;
using MyToolBar.Plugin.BasicPackage.Capsules;
using MyToolBar.Plugin.BasicPackage.API;
using MyToolBar.Common;

namespace MyToolBar.Plugin.BasicPackage.PopupWindows
{
    /// <summary>
    /// WeatherBox.xaml 的交互逻辑
    /// </summary>
    public partial class WeatherBox : PopupWindowBase
    {
        public WeatherBox()
        {
            InitializeComponent();
            LocalCulture.OnLanguageChanged += LocalCulture_OnLanguageChanged;
            LocalCulture_OnLanguageChanged(null, LocalCulture.Current);
            this.StylusDown += WeatherBox_StylusDown;
            this.StylusSystemGesture += WeatherBox_StylusSystemGesture;
            Closing += delegate { 
                LocalCulture.OnLanguageChanged -= LocalCulture_OnLanguageChanged;
            };
        }

        private void LocalCulture_OnLanguageChanged(object? sender, LocalCulture.Language e)
        {
            string uri = $"/MyToolBar.Plugin.BasicPackage;component/LanguageRes/WeatherBox/Lang{e switch
            {
                LocalCulture.Language.en_us => "En_US",
                LocalCulture.Language.zh_cn => "Zh_CN",
                _ => throw new Exception("Unsupported Language")
            }}.xaml";
            //删除旧资源
            var old = Resources.MergedDictionaries.FirstOrDefault(p => p.Source != null && p.Source.OriginalString.Contains("LanguageRes/WeatherBox/Lang"));
            if (old != null)
                Resources.MergedDictionaries.Remove(old);
            //添加新资源
            Resources.MergedDictionaries.Add(new ResourceDictionary() { Source = new Uri(uri, UriKind.Relative) });
        }

        private Point _touchStart;
        private bool _AtSearchPage = false;
        private void WeatherBox_StylusDown(object sender, StylusDownEventArgs e)
        {
            _touchStart = e.GetPosition(this);
        }
        private void WeatherBox_StylusSystemGesture(object sender, StylusSystemGestureEventArgs e)
        {
            Point end=e.GetPosition(this);
            //向右滑动->打开SearchPage
            if (Math.Abs(end.Y - _touchStart.Y) > 5) return;
            if (end.X - _touchStart.X > 5&&!_AtSearchPage)
            {
                _AtSearchPage = true;
                (Resources["PageToSearch"] as Storyboard).Begin();
            }else if(_touchStart.X-end.X>5&&_AtSearchPage)
            {
                _AtSearchPage = false;
                (Resources["PageBack"] as Storyboard).Begin();
            }
        }


        private WeatherCache cache = null;
        public async Task LoadData(WeatherApi.City city,WeatherCache dat=null)
        {
            //Now Weather:
            cache ??= dat;
            var wdata = await dat.RequstCache(city);

            Now_Location.Tag = wdata.CurrentWeather.link;
            Now_Location.Text = wdata.City.Area+" "+wdata.City.CityName;
            Now_Temp.Text = wdata.CurrentWeather.temp+"℃";
            Now_desc.Text = wdata.CurrentWeather.status;
            Now_icon.Background= new ImageBrush(new BitmapImage(new Uri(WeatherApi.GetIcon(wdata.CurrentWeather.code))));

            WindDir.Text = wdata.CurrentWeather.windDir;
            WindScale.Text = wdata.CurrentWeather.windScale;
            Humidity.Text = wdata.CurrentWeather.humidity+"%";
            vis.Text = wdata.CurrentWeather.vis+" km";
            FeelsLike.Text = wdata.CurrentWeather.feel+"℃";
            UpdateTime.Text=wdata.UpdateTime.ToString("HH:mm");

            AQI_text.Text="AQI "+wdata.CurrentAir.aqi;
            AQI_text.ToolTip = wdata.CurrentAir.sug;
            AQI_Viewer.Background = new SolidColorBrush(WeatherApi.GetAirLevelColor(wdata.CurrentAir.level));

            Warnings.Children.Clear();
            foreach (var i in wdata.Warnings)
            {
                Warnings.Children.Add(new WeatherWarningItem(i));
            }
            WarningsBox.Visibility= wdata.Warnings.Count == 0?Visibility.Collapsed: Visibility.Visible;


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
            double offset_max = 25.0 / (max-averageTemp+1),
                offset_min=25.0/(averageTemp-min+1);
            DateTime date= DateTime.Now;
            foreach(WeatherDayItem item in Days.Children)
            {
                //0:230 100 ,100:180 50
                var margin = item.TempLine.Margin;
                margin.Left=230-(item.DailyData.temp_max-averageTemp)*offset_max;
                margin.Right = 50+ ( averageTemp- item.DailyData.temp_min) * offset_min;
                item.TempLine.Margin = margin;
                item.Day.Text = date.ToString("M/d");
                date=date.AddDays(1);
            }
            //加载Favorite
             LoadFavorListAsync();
        }
        private async void LoadFavorListAsync()
        {
            //Default City
            DefaultPosition.Children.Clear();
            var cur = new WeatherCityItem()
            {
                city = cache.DefaultCity,
                IsFavor = cache.FavorCities.Exists((c) => c.Id == cache.DefaultCity.Id)
            };
            cur.CitySelected += CityItem_CitySelected;
            cur.AddFavorCity += CityItem_AddFavorCity;
            cur.SetAsDefaultCity += CityItem_SetAsDefaultCity;
            DefaultPosition.Children.Add(cur);

            //Favorite Cities
            FavorCity.Children.Clear();
            foreach (var i in cache.FavorCities)
            {
                var item = new WeatherCityPreview(i,await cache.RequstCache(i))
                {
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
                city=e,
                IsFavor = cache.FavorCities.Exists((c) => c.Id == e.Id)
            };
            cur.CitySelected += CityItem_CitySelected;
            cur.AddFavorCity += CityItem_AddFavorCity;
            cur.SetAsDefaultCity += CityItem_SetAsDefaultCity;
            DefaultPosition.Children.Add(cur);
            DefaultCityChanged?.Invoke(this, e);
        }

        private void CityItem_AddFavorCity(object? sender, WeatherApi.City e)
        {
            bool isFavor = sender is WeatherCityPreview p ? p.IsFavor : (sender is WeatherCityItem i?i.IsFavor:false);
            if (isFavor)
            {
                cache.FavorCities.Remove(e);
            }
            else
            {
                cache.FavorCities.Add(e);
            }
            LoadFavorListAsync();
            cache.SaveCache();
        }

        private async void CityItem_CitySelected(object? sender, WeatherApi.City e)
        {
            SetLoadingStatus(true);
            await LoadData(e,cache);
            _AtSearchPage= false;
            (Resources["PageBack"] as Storyboard).Begin();
            cache.SaveCache();
            SetLoadingStatus(false);
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
                foreach (var city in result)
                {
                    var item = new WeatherCityItem()
                    {
                        city = city,
                        IsFavor= cache.FavorCities.Exists((c) => c.Id == city.Id)
                    };
                    item.CitySelected += CityItem_CitySelected;
                    item.AddFavorCity += CityItem_AddFavorCity;
                    item.SetAsDefaultCity += CityItem_SetAsDefaultCity;
                    SearchCity_Result.Children.Add(item);
                }
            }
        }
        private void FavorBtn_Click(object sender, RoutedEventArgs e)
        {
            _AtSearchPage = true;
            (Resources["PageToSearch"] as Storyboard).Begin();
        }

        private void BackBtn_Click(object sender, RoutedEventArgs e)
        {
            _AtSearchPage = false;
            (Resources["PageBack"] as Storyboard).Begin();
        }

        private async void LocateBtn_Click(object sender, RoutedEventArgs e)
        {
            SetLoadingStatus(true);
            try
            {
                SearchCity_Result.Children.Clear();
                var city = await WeatherApi.GetCityByPositionAsync();
                if (city != null)
                {
                    var item = new WeatherCityItem()
                    {
                        city = city,
                        IsFavor = cache.FavorCities.Exists((c) => c.Id == city.Id)
                    };
                    item.CitySelected += CityItem_CitySelected;
                    item.AddFavorCity += CityItem_AddFavorCity;
                    item.SetAsDefaultCity += CityItem_SetAsDefaultCity;
                    SearchCity_Result.Children.Add(item);
                }
            }
            finally
            {
                SetLoadingStatus(false);
            }
        }
    }
}
