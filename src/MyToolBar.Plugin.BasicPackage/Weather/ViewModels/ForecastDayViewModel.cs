using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows;

namespace MyToolBar.Plugin.BasicPackage.Weather.ViewModels;

public partial class ForecastDayViewModel : ObservableObject
{
    [ObservableProperty] private string _dayLabel = "";
    [ObservableProperty] private string _maxTemp = "";
    [ObservableProperty] private string _minTemp = "";
    [ObservableProperty] private string _aqiText = "";
    [ObservableProperty] private System.Windows.Media.Color _aqiColor;
    [ObservableProperty] private bool _hasAirData = true;
    [ObservableProperty] private string _iconChar = "";
    [ObservableProperty] private Thickness _tempLineMargin;
    [ObservableProperty] private string _dayStatus = "";
}
