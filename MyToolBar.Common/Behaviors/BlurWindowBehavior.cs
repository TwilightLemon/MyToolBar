using System.Windows;
using System.Windows.Media;
using Microsoft.Win32;
using Microsoft.Xaml.Behaviors;
using MyToolBar.Common.WinAPI;

namespace MyToolBar.Common.Behaviors;
public class BlurWindowBehavior : Behavior<Window>
{
    static readonly Dictionary<Window, WindowMaterial> _allWindowMaterialManager = new();
    public bool IsToolWindow { get; set; } = false;

    private WindowMaterial _windowMaterial;

    public static void SetDarkMode(bool isDarkMode)
    {
        foreach (var wac in _allWindowMaterialManager.Values)
        {
            UpdateWindowBlurMode(wac, isDarkMode);
        }
    }

    private static void UpdateWindowBlurMode(WindowMaterial wac, bool isDarkMode, float opacity = 0)
    {
        wac.IsDarkMode = isDarkMode;
        wac.CompositonColor = isDarkMode ?
            Color.FromScRgb(opacity, 0, 0, 0) :
            Color.FromScRgb(opacity, 1, 1, 1);
    }

    protected override void OnAttached()
    {
        base.OnAttached();
        AssociatedObject.Closing += AssociatedObject_Closing;
        if (GlobalService.IsEnergySaverModeOn)
        {
            AssociatedObject.SetResourceReference(Window.BackgroundProperty, "BackgroundColor");
        }
        else
        {
            if (AssociatedObject.IsLoaded)
            {
                InitializeBehavior();
            }
            else
            {
                AssociatedObject.Loaded += AssociatedObject_Loaded;
            }
        }
    }

    private void AssociatedObject_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        //Detach this behavior:
        Interaction.GetBehaviors(AssociatedObject).Remove(this);
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();

        // unregister event
        AssociatedObject.Loaded -= AssociatedObject_Loaded;

        // remove and disable wac
        if (_allWindowMaterialManager.TryGetValue(AssociatedObject, out var acc))
        {
            _allWindowMaterialManager.Remove(AssociatedObject);
            acc.UseWindowComposition = false;
            acc.MaterialMode = MaterialMode.None;
        }
    }

    private void InitializeBehavior()
    {
        _allWindowMaterialManager[AssociatedObject]
            = _windowMaterial
            = CreateWindowAccentCompositor();
    }

    private WindowMaterial CreateWindowAccentCompositor()
    {
        var isDarkMode = !SystemThemeAPI.GetIsLightTheme();
        var wac = new WindowMaterial();
        wac.MaterialMode = Mode;
        wac.UseWindowComposition = IsToolWindow;
        UpdateWindowBlurMode(wac, isDarkMode);
        WindowMaterial.SetMaterial(AssociatedObject, wac);
        return wac;
    }

    private void AssociatedObject_Loaded(object sender, RoutedEventArgs e)
    {
        InitializeBehavior();
    }
    public WindowMaterial WindowMaterial => _windowMaterial ??
        throw new InvalidOperationException("Window is not loaded");



    public MaterialMode Mode
    {
        get { return (MaterialMode)GetValue(ModeProperty); }
        set { SetValue(ModeProperty, value); }
    }

    public static readonly DependencyProperty ModeProperty =
        DependencyProperty.Register("Mode", typeof(MaterialMode), typeof(BlurWindowBehavior),
            new PropertyMetadata(MaterialMode.Acrylic,OnModeChanged));
    public static void OnModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is BlurWindowBehavior behavior)
        {
            behavior.WindowMaterial.MaterialMode = (MaterialMode)e.NewValue;
        }
    }

}