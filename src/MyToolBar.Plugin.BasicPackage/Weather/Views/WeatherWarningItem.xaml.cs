using System.Windows;
using System.Windows.Media;
using MyToolBar.Common.UIBases;
using MyToolBar.Plugin.BasicPackage.Weather.Models;
using MyToolBar.Plugin.BasicPackage.Weather.Services;

namespace MyToolBar.Plugin.BasicPackage.Weather.Views;

public partial class WeatherWarningItem : ItemBase
{
    public WeatherWarningItem()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
        Click += (_, _) =>
        {
            DetalPart.Visibility = DetalPart.Visibility == Visibility.Visible
                ? Visibility.Collapsed : Visibility.Visible;
        };
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue is Warning warning)
        {
            LevelColor.Fill = new SolidColorBrush(WeatherApi.GetWarningLevelColor(warning.level));
            SeverityTb.Text = warning.severity;
            TypeTb.Text = warning.typeName;
            DetalTb.Text = warning.text;
            PublisherTb.Text = warning.sender;
        }
    }
}
