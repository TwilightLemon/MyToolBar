using MyToolBar.Common.UIBases;
using MyToolBar.Plugin.BasicPackage.API;
using MyToolBar.Plugin.BasicPackage.Capsules;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MyToolBar.Plugin.BasicPackage.PopupWindows
{
    /// <summary>
    /// WeatherCityPreview.xaml 的交互逻辑
    /// </summary>
    public partial class WeatherCityPreview : ItemBase
    {
        public event EventHandler<WeatherApi.City> AddFavorCity;
        public event EventHandler<WeatherApi.City> CitySelected;
        public event EventHandler<WeatherApi.City> SetAsDefaultCity;

        private readonly WeatherData _data;
        private readonly WeatherApi.City _city;
        public WeatherCityPreview( WeatherApi.City City,WeatherData Data)
        {
            InitializeComponent();
            _city = City;
            _data = Data;
            LoadData();
            base.Click += WeatherCityPreview_Click;
            MouseRightButtonUp += WeatherCityPreview_MouseRightButtonUp;
        }

        private bool _IsFavor = false;
        public bool IsFavor
        {
            get => _IsFavor;
            set
            {
                _IsFavor = value;
                if (value)
                    Favor_icon.SetResourceReference(Shape.FillProperty, "ForeColor");
                else Favor_icon.Fill = null;
            }
        }

        private void WeatherCityPreview_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            SetAsDefaultCity?.Invoke(this, _city);
            e.Handled = true;
        }

        private void WeatherCityPreview_Click(object sender, RoutedEventArgs e)
        {
            CitySelected?.Invoke(this, _city);
        }

        private void LoadData()
        {
            AreaName.Text = _city.Area;
            CityName.Text = _city.CityName+ "  " + _city.Province;
            InfoTb.Text=_data.CurrentWeather.temp + "°C   "+_data.CurrentWeather.status;
            Img.Background = new ImageBrush(new BitmapImage(new Uri(WeatherApi.GetIcon(_data.CurrentWeather.code))));
        }

        private void AddFavorBtn_Click(object sender, RoutedEventArgs e)
        {
            AddFavorCity?.Invoke(this,_city);
            IsFavor = !IsFavor;
        }
    }
}
