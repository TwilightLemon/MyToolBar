﻿using System.Windows;
using System.Windows.Media;
using System.Windows.Shell;
using Microsoft.Win32;
using MyToolBar;
using Microsoft.Xaml.Behaviors;
using MyToolBar.Common.WinAPI;

namespace MyToolBar.Common.Behaviors;

/// <summary>
/// 提供窗口模糊效果和统一的暗亮色模式管理行为
/// </summary>
public class BlurWindowBehavior : Behavior<Window>
{
    static readonly Dictionary<Window, WindowMaterial> _allWindowMaterialManager = [];
    static BlurWindowBehavior()
    {
        GlobalService.OnEnergySaverModeChanged += GlobalService_OnEnergySaverModeChanged;
    }

    private static void GlobalService_OnEnergySaverModeChanged(bool isOn)
    {
        foreach(var pair in _allWindowMaterialManager)
        {
            //对于UseWindowCaompositonAPI的窗口有自己的颜色管理
            if(pair.Value is { } material&&pair.Key is { }window&&!material.UseWindowComposition)
            window.SetResourceReference(Window.BackgroundProperty,
                isOn?"BackgroundColor": "WindowBackgroundColor");
        }
    }

    /// <summary>
    /// 是否为ToolWindow
    /// </summary>
    public bool IsToolWindow { get; set; } = false;

    private WindowChrome? _windowChrome = null;
    public WindowChrome? WindowChromeEx
    {
        get => _windowChrome;
        set
        {
            if (_windowChrome != value)
            {
                _windowChrome = value;
                UpdateWindowChromeEx();
            }
        }
    }

    private WindowMaterial _windowMaterial;

    /// <summary>
    /// 更新所有窗口的暗亮色模式，此方法由主程序调用
    /// </summary>
    /// <param name="isDarkMode"></param>
    public static void SetDarkMode(bool isDarkMode)
    {
        foreach (var wac in _allWindowMaterialManager.Values)
        {
            UpdateWindowBlurMode(wac, isDarkMode);
        }
    }

    private void UpdateWindowChromeEx()
    {
        if (_windowMaterial != null && WindowChromeEx != null)
            _windowMaterial.WindowChromeEx = WindowChromeEx;
    }

    private static void UpdateWindowBlurMode(WindowMaterial wac, bool isDarkMode, float opacity = 0.2f)
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
        if (AssociatedObject.IsLoaded)
        {
            InitializeBehavior();
        }
        else
        {
            AssociatedObject.Loaded += AssociatedObject_Loaded;
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
            acc.MaterialMode = MaterialType.None;
        }
    }

    private void InitializeBehavior()
    {
        if(GlobalService.IsEnergySaverModeOn)
        {
            AssociatedObject.SetResourceReference(Window.BackgroundProperty, "BackgroundColor");
        }

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
        if (WindowChromeEx != null)
            wac.WindowChromeEx = WindowChromeEx;
        UpdateWindowBlurMode(wac, isDarkMode);
        WindowMaterial.SetMaterial(AssociatedObject, wac);
        return wac;
    }

    private void AssociatedObject_Loaded(object sender, RoutedEventArgs e)
    {
        InitializeBehavior();
    }
    public WindowMaterial WindowMaterial => _windowMaterial;



    public MaterialType Mode
    {
        get { return (MaterialType)GetValue(ModeProperty); }
        set { SetValue(ModeProperty, value); }
    }

    public static readonly DependencyProperty ModeProperty =
        DependencyProperty.Register("Mode", typeof(MaterialType), typeof(BlurWindowBehavior),
            new PropertyMetadata(MaterialType.Acrylic,OnModeChanged));
    public static void OnModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is BlurWindowBehavior{ } behavior&& behavior.WindowMaterial!=null)
        {
            behavior.WindowMaterial.MaterialMode = (MaterialType)e.NewValue;
        }
    }

}