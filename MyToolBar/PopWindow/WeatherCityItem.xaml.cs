using MyToolBar.Func;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MyToolBar.PopWindow
{
    /// <summary>
    /// WeatherCityItem.xaml 的交互逻辑
    /// </summary>
    public partial class WeatherCityItem : UserControl
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
        public WeatherCityItem()
        {
            InitializeComponent();
            MouseEnter += delegate {
                ViewMask.BeginAnimation(OpacityProperty, new DoubleAnimation(0.5, 1, TimeSpan.FromMilliseconds(300)));
            };
            MouseLeave += delegate
            {
                ViewMask.BeginAnimation(OpacityProperty, new DoubleAnimation(1, 0.5, TimeSpan.FromMilliseconds(300)));
            };
            CityName.MouseLeftButtonUp += delegate
            {
                CitySelected?.Invoke(this, city);
            };
            ViewMask.MouseLeftButtonUp += delegate
            {
                CitySelected?.Invoke(this, city);
            };
            AddFavorBtn.MouseLeftButtonUp += delegate
            {
                AddFavorCity?.Invoke(this, city);
            };
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
