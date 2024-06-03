using System.Windows.Shapes;
using MyToolBar.Common.UIBases;
using MyToolBar.Plugin.BasicPackage.API;

namespace MyToolBar.Plugin.BasicPackage.PopupWindows
{
    /// <summary>
    /// WeatherCityItem.xaml 的交互逻辑
    /// </summary>
    public partial class WeatherCityItem : ItemBase
    {
        private WeatherApi.City _city;
        public WeatherApi.City city
        {
            get => _city;
            set
            {
                _city = value;
                CityName.Text = _city.Area + " " + _city.CityName + " " + _city.Province;
            }
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

        private bool IsMousePressed= false;
        public WeatherCityItem()
        {
            InitializeComponent();
            EnableClickEvent = false;
            MouseLeftButtonDown += delegate
            {
                IsMousePressed = true;
            };
            CityName.MouseLeftButtonUp += delegate
            {
                if (IsMousePressed)
                    CitySelected?.Invoke(this, city);
                IsMousePressed= false;
            };
            _ViewMask.MouseLeftButtonUp += delegate
            {
                if (IsMousePressed)
                    CitySelected?.Invoke(this, city);
                IsMousePressed = false;
            };
            AddFavorBtn.MouseLeftButtonUp += delegate
            {
                AddFavorCity?.Invoke(this, city);
            };
            //触摸长按 or 鼠标右击
            MouseRightButtonUp += delegate
            {
                SetAsDefaultCity?.Invoke(this, city);
            };
        }
        public event EventHandler<WeatherApi.City> AddFavorCity;
        public event EventHandler<WeatherApi.City> CitySelected;
        public event EventHandler<WeatherApi.City> SetAsDefaultCity;
    }
}
