using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using MyToolBar.Common.UIBases;
using MyToolBar.Plugin.BasicPackage.Weather.Models;
using MyToolBar.Plugin.BasicPackage.Weather.ViewModels;

namespace MyToolBar.Plugin.BasicPackage.Weather.Views;

public partial class WeatherCityPreview : ItemBase
{
    public event EventHandler<City>? AddFavorCity;
    public event EventHandler<City>? CitySelected;
    public event EventHandler<City>? SetAsDefaultCity;

    public WeatherCityPreview()
    {
        InitializeComponent();
        Click += (_, _) =>
        {
            if (DataContext is CityItemViewModel vm)
                CitySelected?.Invoke(this, vm.City);
        };
        MouseRightButtonUp += (_, e) =>
        {
            if (DataContext is CityItemViewModel vm)
            {
                SetAsDefaultCity?.Invoke(this, vm.City);
                e.Handled = true;
            }
        };
        DataContextChanged += (_, _) =>
        {
            if (DataContext is CityItemViewModel vm)
                UpdateFavorIcon(vm.IsFavor);
        };
    }

    private void UpdateFavorIcon(bool isFavor)
    {
        if (isFavor)
            Favor_icon.SetResourceReference(Shape.FillProperty, "ForeColor");
        else
            Favor_icon.Fill = null;
    }

    private void AddFavorBtn_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is CityItemViewModel vm)
        {
            AddFavorCity?.Invoke(this, vm.City);
            vm.IsFavor = !vm.IsFavor;
            UpdateFavorIcon(vm.IsFavor);
        }
    }
}
