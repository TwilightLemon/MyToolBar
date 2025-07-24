using MyToolBar.Common.WinAPI;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace MyToolBar.Common.UIBases;

public static class FluentTooltip
{
    public static bool GetUseFluentStyle(DependencyObject obj)
    {
        return (bool)obj.GetValue(UseFluentStyleProperty);
    }

    public static void SetUseFluentStyle(DependencyObject obj, bool value)
    {
        obj.SetValue(UseFluentStyleProperty, value);
    }

    public static readonly DependencyProperty UseFluentStyleProperty =
        DependencyProperty.RegisterAttached("UseFluentStyle",
            typeof(bool), typeof(FluentTooltip),
            new PropertyMetadata(false,OnUseFluentStyleChanged));
    public static void OnUseFluentStyleChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue != e.OldValue)
        {
            if(obj is ToolTip tip)
            {
                if ((bool)e.NewValue)
                {
                    tip.Opened += Popup_Opened;
                }
                else
                {
                    tip.Opened -= Popup_Opened;
                }
            }else if(obj is ContextMenu menu)
            {
                if ((bool)e.NewValue)
                {
                    menu.Opened += Popup_Opened;
                }
                else
                {
                    menu.Opened -= Popup_Opened;
                }
            }
        }
    }

    private static void Popup_Opened(object sender, RoutedEventArgs e)
    {
        if(sender is ToolTip tip&& tip.Background is SolidColorBrush cb)
        {
            var hwnd = tip.GetNativeWindowHwnd();
            FluentPopupFunc.SetPopupWindowMaterial(hwnd, cb.Color,MaterialApis.WindowCorner.RoundSmall);
        }else if (sender is ContextMenu menu && menu.Background is SolidColorBrush color)
        {
            var hwnd = menu.GetNativeWindowHwnd();
            FluentPopupFunc.SetPopupWindowMaterial(hwnd, color.Color, MaterialApis.WindowCorner.RoundSmall);
        }
    }
}

public class FluentPopup:Popup
{
    public enum ExPopupAnimation
    {
        None,
        SlideUp,
        SlideDown
    }
    private DoubleAnimation? _slideAni;
    static FluentPopup()
    {
        //对IsOpenProperty添加PropertyChangedCallback
        IsOpenProperty.OverrideMetadata(typeof(FluentPopup), new FrameworkPropertyMetadata(false, OnIsOpenChanged));
    }
    public FluentPopup()
    {
        Opened += FluentPopup_Opened;
        Closed += FluentPopup_Closed;
    }
    #region


    public bool FollowWindowMoving
    {
        get { return (bool)GetValue(FollowWindowMovingProperty); }
        set { SetValue(FollowWindowMovingProperty, value); }
    }

    // Using a DependencyProperty as the backing store for FollowWindowMoving.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty FollowWindowMovingProperty =
        DependencyProperty.Register("FollowWindowMoving", typeof(bool), typeof(FluentPopup), new PropertyMetadata(false, OnFollowWindowMovingChanged));
    private static void OnFollowWindowMovingChanged(DependencyObject o,DependencyPropertyChangedEventArgs e)
    {
        if(o is FluentPopup popup&& Window.GetWindow(popup) is { } window)
        {
            if(e.NewValue is true)
            {
                window.LocationChanged += popup.AttachedWindow_LocationChanged;
                window.SizeChanged += popup.AttachedWindow_SizeChanged;
            }
        }
    }


    public MaterialApis.WindowCorner WindowCorner
    {
        get { return (MaterialApis.WindowCorner)GetValue(WindowCornerProperty); }
        set { SetValue(WindowCornerProperty, value); }
    }

    public static readonly DependencyProperty WindowCornerProperty =
        DependencyProperty.Register("WindowCorner", 
            typeof(MaterialApis.WindowCorner), typeof(FluentPopup),
            new PropertyMetadata(MaterialApis.WindowCorner.Round));

    public static void OnWindowCornerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if(d is FluentPopup popup)
        {
            popup.ApplyWindowCorner();
        }
    }

    private void AttachedWindow_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        FollowMove();
    }

    private void AttachedWindow_LocationChanged(object? sender, EventArgs e)
    {
        FollowMove();
    }
    private void FollowMove()
    {
        if (IsOpen)
        {
            var mi = typeof(Popup).GetMethod("UpdatePosition", BindingFlags.NonPublic | BindingFlags.Instance);
            mi?.Invoke(this, null);
        }
    }



    #endregion
    #region 启动动画控制
    private static void OnIsOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FluentPopup popup)
        {
            if ((bool)e.NewValue)
            {
                popup.BuildAnimation();
            }
        }
    }
    private void FluentPopup_Closed(object? sender, EventArgs e)
    {
        ResetAnimation();
    }

    public new bool IsOpen { get => base.IsOpen;
        set 
        {
            if (value)
            {
                BuildAnimation();
                base.IsOpen = value;
                // Run Animation in Opened Event
            }
            else
            {
                base.IsOpen = value;
                //closed event will reset animation
                //ResetAnimation();
            }
        }
    }
    public uint SlideAnimationOffset { get; set; } = 50;
    private void ResetAnimation()
    {
        if (ExtPopupAnimation is ExPopupAnimation.SlideUp or ExPopupAnimation.SlideDown)
        {
            BeginAnimation(VerticalOffsetProperty, null);
        }
    }
    public void BuildAnimation()
    {
        if (ExtPopupAnimation is ExPopupAnimation.SlideUp or ExPopupAnimation.SlideDown)
        {
            _slideAni = new DoubleAnimation(VerticalOffset+(ExtPopupAnimation == ExPopupAnimation.SlideUp ?
                SlideAnimationOffset : -SlideAnimationOffset),VerticalOffset, TimeSpan.FromMilliseconds(300))
            {
                EasingFunction = new CubicEase()
            };
        }
    }
    public void RunPopupAnimation()
    {
        if (_slideAni != null)
        {
            BeginAnimation(VerticalOffsetProperty, _slideAni);
        }
    }

    #endregion
    #region Fluent Style
    public SolidColorBrush Background
    {
        get { return (SolidColorBrush)GetValue(BackgroundProperty); }
        set { SetValue(BackgroundProperty, value); }
    }

    public static readonly DependencyProperty BackgroundProperty =
        DependencyProperty.Register("Background",
            typeof(SolidColorBrush), typeof(FluentPopup),
            new PropertyMetadata(Brushes.Transparent,OnBackgroundChanged));

    public static void OnBackgroundChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FluentPopup popup)
        {
            popup.ApplyFluentHwnd();
        }
    }

    public ExPopupAnimation ExtPopupAnimation
    {
        get { return (ExPopupAnimation)GetValue(ExtPopupAnimationProperty); }
        set { SetValue(ExtPopupAnimationProperty, value); }
    }

    public static readonly DependencyProperty ExtPopupAnimationProperty =
        DependencyProperty.Register("ExtPopupAnimation", typeof(ExPopupAnimation), typeof(FluentPopup),
            new PropertyMetadata(ExPopupAnimation.None));

    private nint _windowHandle= nint.Zero;
    private void FluentPopup_Opened(object? sender, EventArgs e)
    {
        _windowHandle = this.GetNativeWindowHwnd();
        ApplyFluentHwnd();
        Dispatcher.Invoke(RunPopupAnimation);
    }
    public void ApplyFluentHwnd()
    {
        FluentPopupFunc.SetPopupWindowMaterial(_windowHandle, Background.Color, WindowCorner);
    }
    public void ApplyWindowCorner()
    {
        MaterialApis.SetWindowCorner(_windowHandle, WindowCorner);
    }
    #endregion
}

internal static class FluentPopupFunc
{
    public const BindingFlags privateInstanceFlag = BindingFlags.NonPublic | BindingFlags.Instance;
    public static nint GetNativeWindowHwnd(this ToolTip tip)
    {
        var field=tip.GetType().GetField("_parentPopup", privateInstanceFlag);
        if (field != null)
        {
            if(field.GetValue(tip) is Popup{ } popup)
            {
                return popup.GetNativeWindowHwnd();
            }
        }
        return nint.Zero;
    }
    public static nint GetNativeWindowHwnd(this ContextMenu menu)
    {
        var field = menu.GetType().GetField("_parentPopup", privateInstanceFlag);
        if (field != null)
        {
            if (field.GetValue(menu) is Popup { } popup)
            {
                return popup.GetNativeWindowHwnd();
            }
        }
        return nint.Zero;
    }
    public static nint GetNativeWindowHwnd(this Popup popup)
    {
        var field = typeof(Popup).GetField("_secHelper", privateInstanceFlag);
        if (field != null)
        {
            if (field.GetValue(popup) is { } _secHelper)
            {
                if (_secHelper.GetType().GetProperty("Handle", privateInstanceFlag) is { } prop)
                {
                    if (prop.GetValue(_secHelper) is nint handle)
                    {
                        return handle;
                    }
                }
            }
        }
        return nint.Zero;
    }
    public static void SetPopupWindowMaterial(nint hwnd,Color compositionColor,
        MaterialApis.WindowCorner corner=MaterialApis.WindowCorner.Round)
    {
        if (hwnd != nint.Zero)
        {
            int hexColor = compositionColor.ToHexColor();
            var hwndSource = HwndSource.FromHwnd(hwnd);
            MaterialApis.SetWindowProperties(hwndSource, 1);
            MaterialApis.SetWindowComposition(hwnd, true, hexColor);
            MaterialApis.SetWindowCorner(hwnd, corner);
        }
    }
}
