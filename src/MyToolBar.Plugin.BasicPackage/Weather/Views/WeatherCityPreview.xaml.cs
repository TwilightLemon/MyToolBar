using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using MyToolBar.Common.UIBases;
using MyToolBar.Plugin.BasicPackage.Weather.ViewModels;

namespace MyToolBar.Plugin.BasicPackage.Weather.Views;

/// <summary>
/// 城市预览卡片 — 通过 ItemBase.Command / ItemBase.CommandParameter 与父级 WeatherBoxViewModel 交互
/// </summary>
public partial class WeatherCityPreview : ItemBase
{
    private CityItemViewModel? _vm;

    public WeatherCityPreview()
    {
        InitializeComponent();

        DataContextChanged += (_, _) =>
        {
            if (_vm != null)
                _vm.PropertyChanged -= OnVmPropertyChanged;

            if (DataContext is CityItemViewModel newVm)
            {
                _vm = newVm;
                _vm.PropertyChanged += OnVmPropertyChanged;
                UpdateFavorIcon(_vm.IsFavor);
            }
        };

        // 右键 → 设为默认城市
        MouseRightButtonUp += (_, e) =>
        {
            if (DataContext is CityItemViewModel vm)
            {
                var wb = Window.GetWindow(this) as WeatherBox;
                wb?.ViewModel.SetDefaultCityCommand.Execute(vm);
            }
            e.Handled = true;
        };
    }

    private void OnVmPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(CityItemViewModel.IsFavor) && _vm != null)
            UpdateFavorIcon(_vm.IsFavor);
    }

    private void UpdateFavorIcon(bool isFavor)
    {
        if (isFavor)
            FavorIcon.SetResourceReference(Shape.FillProperty, "ForeColor");
        else
            FavorIcon.Fill = null;
    }
}
