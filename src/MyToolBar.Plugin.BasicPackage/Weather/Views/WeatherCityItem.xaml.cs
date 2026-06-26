using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using MyToolBar.Common.UIBases;
using MyToolBar.Plugin.BasicPackage.Weather.Models;
using MyToolBar.Plugin.BasicPackage.Weather.ViewModels;

namespace MyToolBar.Plugin.BasicPackage.Weather.Views;

public partial class WeatherCityItem : ItemBase
{
    public event EventHandler<City>? AddFavorCity;
    public event EventHandler<City>? CitySelected;
    public event EventHandler<City>? SetAsDefaultCity;

    private bool _isMousePressed = false;

    public WeatherCityItem()
    {
        InitializeComponent();
        base.EnableClickEvent = false;
        DataContextChanged += OnDataContextChanged;

        MouseLeftButtonDown += (_, _) => _isMousePressed = true;

        CityName.MouseLeftButtonUp += (_, _) =>
        {
            if (_isMousePressed && DataContext is CityItemViewModel vm)
                CitySelected?.Invoke(this, vm.City);
            _isMousePressed = false;
        };

        _ViewMask.MouseLeftButtonUp += (_, _) =>
        {
            if (_isMousePressed && DataContext is CityItemViewModel vm)
                CitySelected?.Invoke(this, vm.City);
            _isMousePressed = false;
        };

        AddFavorBtn.MouseLeftButtonUp += (_, _) =>
        {
            if (DataContext is CityItemViewModel vm)
            {
                AddFavorCity?.Invoke(this, vm.City);
                vm.IsFavor = !vm.IsFavor;
            }
        };

        MouseRightButtonUp += (_, _) =>
        {
            if (DataContext is CityItemViewModel vm)
                SetAsDefaultCity?.Invoke(this, vm.City);
        };
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue is CityItemViewModel vm)
        {
            UpdateFavorIcon(vm.IsFavor);
            vm.PropertyChanged += (_, args) =>
            {
                if (args.PropertyName == nameof(CityItemViewModel.IsFavor))
                    UpdateFavorIcon(vm.IsFavor);
            };
        }
    }

    private void UpdateFavorIcon(bool isFavor)
    {
        if (isFavor)
            Favor_icon.SetResourceReference(Shape.FillProperty, "ForeColor");
        else
            Favor_icon.Fill = null;
    }
}
