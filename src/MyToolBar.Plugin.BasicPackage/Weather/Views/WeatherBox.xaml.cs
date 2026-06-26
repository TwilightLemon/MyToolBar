using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using MyToolBar.Common;
using MyToolBar.Common.UIBases;
using MyToolBar.Plugin.BasicPackage.Weather.ViewModels;

namespace MyToolBar.Plugin.BasicPackage.Weather.Views;

/// <summary>
/// WeatherBox.xaml 的交互逻辑 (MVVM)
/// </summary>
public partial class WeatherBox : PopupWindowBase
{
    public WeatherBoxViewModel ViewModel { get; } = new();

    public WeatherBox()
    {
        DataContext = ViewModel;
        InitializeComponent();

        LocalCulture.OnLanguageChanged += OnLanguageChanged;
        OnLanguageChanged(null, LocalCulture.Current);

        StylusDown += WeatherBox_StylusDown;
        StylusSystemGesture += WeatherBox_StylusSystemGesture;

        Closing += delegate
        {
            LocalCulture.OnLanguageChanged -= OnLanguageChanged;
        };

        // Sync ViewModel page state with storyboard animations
        ViewModel.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(WeatherBoxViewModel.IsSearchPage))
            {
                if (ViewModel.IsSearchPage)
                    (Resources["PageToSearch"] as Storyboard)?.Begin();
                else
                    (Resources["PageBack"] as Storyboard)?.Begin();
            }
        };
    }

    private void OnLanguageChanged(object? sender, LocalCulture.Language e)
    {
        string uri = $"/MyToolBar.Plugin.BasicPackage;component/Weather/Resources/Lang{e switch
        {
            LocalCulture.Language.en_us => "En_US",
            LocalCulture.Language.zh_cn => "Zh_CN",
            _ => throw new Exception("Unsupported Language")
        }}.xaml";

        var old = Resources.MergedDictionaries.FirstOrDefault(p =>
            p.Source != null && p.Source.OriginalString.Contains("Weather/Resources/Lang"));
        if (old != null)
            Resources.MergedDictionaries.Remove(old);

        Resources.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri(uri, UriKind.Relative) });
    }

    // ===== Touch swipe for page navigation =====
    private Point _touchStart;

    private void WeatherBox_StylusDown(object sender, StylusDownEventArgs e)
    {
        _touchStart = e.GetPosition(this);
    }

    private void WeatherBox_StylusSystemGesture(object sender, StylusSystemGestureEventArgs e)
    {
        Point end = e.GetPosition(this);
        if (Math.Abs(end.Y - _touchStart.Y) > 5) return;

        if (end.X - _touchStart.X > 5 && !ViewModel.IsSearchPage)
        {
            ViewModel.ShowSearchPageCommand.Execute(null);
        }
        else if (_touchStart.X - end.X > 5 && ViewModel.IsSearchPage)
        {
            ViewModel.ShowMainPageCommand.Execute(null);
        }
    }

    // ===== Event handlers that still need code-behind =====

    private void Now_Location_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (sender is FrameworkElement fe && fe.Tag is string link && !string.IsNullOrEmpty(link))
            Process.Start("explorer", link);
    }

    private async void SearchCityBox_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            await ViewModel.SearchCitiesCommand.ExecuteAsync(null);
        }
    }
}
