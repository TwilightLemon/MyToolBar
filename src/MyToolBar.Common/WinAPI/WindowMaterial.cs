using EleCho.WpfSuite;
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Shell;
using static MyToolBar.Common.WinAPI.MaterialApis;

namespace MyToolBar.Common.WinAPI;

public class WindowMaterial : DependencyObject
{
    /// <summary>
    /// 调用API的类型
    /// </summary>
    private enum APIType
    {
        NONE, SYSTEMBACKDROP, COMPOSITION
    }
    /// <summary>
    /// 所附加的窗口
    /// </summary>
    private Window? AttachedWindow
    {
        get => _window;
        set
        {
            _window = value;
            if (value != null)
            {
                _hWnd = new WindowInteropHelper(_window).Handle;
                if (_hWnd == IntPtr.Zero)
                    //窗口句柄未创建
                    value.SourceInitialized += AttachedWindow_SourceInitialized;
                else InitWindow();
            }
        }
    }

    private void AttachedWindow_SourceInitialized(object? sender, EventArgs e)
    {
        InitWindow();
        _window!.SourceInitialized -= AttachedWindow_SourceInitialized;
    }

    /// <summary>
    /// 初始化时调用
    /// </summary>
    private void InitWindow()
    {
        _hWnd = new WindowInteropHelper(_window).Handle;
        if (WindowChromeEx != null)
        {
            WindowChrome.SetWindowChrome(_window, WindowChromeEx);
        }
        SetDarkMode(IsDarkMode);
        Apply();
    }

    private void Apply()
    {
        if (_window == null | _hWnd == IntPtr.Zero) return;

        bool enable = MaterialMode != MaterialType.None || UseWindowComposition;
        if (enable)
        {
            //操作系统判定，如果是window10 即使使用MaterialMode也调用CompositionAPI
            var osVersion = Environment.OSVersion.Version;
            var windows10_1809 = new Version(10, 0, 17763);
            var windows11 = new Version(10, 0, 22621);
            //强制使用或仅支持CompositionAPI的系统
            if (UseWindowComposition || (osVersion >= windows10_1809 && osVersion < windows11))
            {
                SetWindowProperty(true);
                SetWindowCompositon(true);
            }
            else
            {
                //先关闭CompositionAPI 如果开启
                if (CurrentAPI == APIType.COMPOSITION)
                    SetWindowCompositon(false);
                SetWindowProperty(false);
                SetBackDropType(MaterialMode);
            }
        }
        else
        {
            if (CurrentAPI == APIType.COMPOSITION)
                SetWindowCompositon(false);
            else if (CurrentAPI == APIType.SYSTEMBACKDROP)
                SetBackDropType(MaterialMode);
        }
    }

    #region Window 附加属性
    public static WindowMaterial GetMaterial(Window obj)
    {
        return (WindowMaterial)obj.GetValue(MaterialProperty);
    }

    public static void SetMaterial(Window obj, WindowMaterial value)
    {
        obj.SetValue(MaterialProperty, value);
    }

    public static readonly DependencyProperty MaterialProperty =
        DependencyProperty.RegisterAttached("Material",
            typeof(WindowMaterial), typeof(WindowMaterial),
            new PropertyMetadata(null, OnMaterialChanged));

    private static void OnMaterialChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is Window w && e.NewValue is WindowMaterial m)
        {
            m.AttachedWindow = w;
        }
    }
    #endregion
    #region WindowMaterial 依赖属性
    /// <summary>
    /// 是否启用暗色模式
    /// </summary>
    public bool IsDarkMode
    {
        get { return (bool)GetValue(IsDarkModeProperty); }
        set { SetValue(IsDarkModeProperty, value); }
    }

    public static readonly DependencyProperty IsDarkModeProperty =
        DependencyProperty.Register("IsDarkMode",
            typeof(bool), typeof(WindowMaterial),
            new PropertyMetadata(false, OnIsDarkModeChanged));
    private static void OnIsDarkModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is WindowMaterial w)
        {
            w.SetDarkMode((bool)e.NewValue);
        }
    }

    /// <summary>
    /// 指定窗口的材质类型
    /// </summary>
    public MaterialType MaterialMode
    {
        get { return (MaterialType)GetValue(MaterialModeProperty); }
        set { SetValue(MaterialModeProperty, value); }
    }

    public static readonly DependencyProperty MaterialModeProperty =
        DependencyProperty.Register("MaterialMode",
            typeof(MaterialType), typeof(WindowMaterial),
            new PropertyMetadata(MaterialType.None, OnMaterialModeChanged));
    private static void OnMaterialModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is WindowMaterial m)
        {
            m.Apply();
        }
    }

    /// <summary>
    /// 另需指定的WindowChrome
    /// </summary>
    public WindowChrome WindowChromeEx
    {
        get { return (WindowChrome)GetValue(WindowChromeExProperty); }
        set { SetValue(WindowChromeExProperty, value); }
    }

    public static readonly DependencyProperty WindowChromeExProperty =
        DependencyProperty.Register("WindowChromeEx",
            typeof(WindowChrome), typeof(WindowMaterial),
            new PropertyMetadata(null, OnWindowChromeExChanged));

    private static void OnWindowChromeExChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is WindowMaterial { } m && e.NewValue is WindowChrome { } wc && m._window != null)
        {
            //如果WindowChrome直接附加在窗口上会覆盖掉我们设置的GlassFrameThickness
            //故这里的设计是将WindowChrome附加在WindowMaterial上进行管理
            WindowChrome.SetWindowChrome(m._window, wc);
            m.Apply();
        }
    }

    public bool UseWindowComposition
    {
        get { return (bool)GetValue(UseWindowCompositionProperty); }
        set { SetValue(UseWindowCompositionProperty, value); }
    }

    public static readonly DependencyProperty UseWindowCompositionProperty =
        DependencyProperty.Register("UseWindowComposition",
            typeof(bool), typeof(WindowMaterial),
            new PropertyMetadata(false, OnUseWindowCompositionChanged));

    private static void OnUseWindowCompositionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is WindowMaterial m)
        {
            m.Apply();
        }
    }

    public Color CompositonColor
    {
        get { return (Color)GetValue(CompositonColorProperty); }
        set { SetValue(CompositonColorProperty, value); }
    }

    public static readonly DependencyProperty CompositonColorProperty =
        DependencyProperty.Register("CompositonColor",
            typeof(Color), typeof(WindowMaterial),
            new PropertyMetadata(Color.FromArgb(180, 0, 0, 0), OnCompositionColorChanged));
    private static void OnCompositionColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is WindowMaterial m)
        {
            m.SetCompositionColor((Color)e.NewValue);
            m.Apply();
        }
    }
    #endregion

    private IntPtr _hWnd = IntPtr.Zero;
    private Window? _window = null;
    private APIType CurrentAPI = APIType.NONE;
    private int _blurColor;
    private void SetCompositionColor(Color value)
    {
        _blurColor = value.ToHexColor();
    }
    private void SetDarkMode(bool isDarkMode)
    {
        if (_hWnd == IntPtr.Zero) return;
        MaterialApis.SetDarkMode(_hWnd, isDarkMode);
    }
    private void SetBackDropType(MaterialType blurMode)
    {
        if (_hWnd == IntPtr.Zero) return;
        MaterialApis.SetBackDropType(_hWnd, blurMode);
        CurrentAPI = blurMode == MaterialType.None ? APIType.NONE : APIType.SYSTEMBACKDROP;
    }
    private void SetWindowCompositon(bool enable)
    {
        if (_hWnd == IntPtr.Zero) return;
        MaterialApis.SetWindowComposition(_hWnd, enable, _blurColor);
        CurrentAPI = enable ? APIType.COMPOSITION : APIType.NONE;
    }
    private void SetWindowProperty(bool isLagcy = false)
    {
        if (_hWnd == IntPtr.Zero) return;
        var hwndSource = (HwndSource)PresentationSource.FromVisual(_window);
        int margin = isLagcy ? 1 : -1;
        MaterialApis.SetWindowProperties(hwndSource, margin);
    }
}

/// <summary>
/// 材质类型
/// </summary>
public enum MaterialType
{
    None = 1,
    Acrylic = 3,
    Mica = 2,
    MicaAlt = 4
}

public static class MaterialApis
{
    public static int ToHexColor(this Color value)
    {
        return value.R << 0 | value.G << 8 | value.B << 16 | value.A << 24;
    }
    public static void SetWindowProperties(HwndSource hwndSource, int margin)
    {
        if (hwndSource == null) return;
        hwndSource.CompositionTarget.BackgroundColor = Colors.Transparent;
        var margins = new Margins()
        {
            LeftWidth = margin,
            TopHeight = margin,
            RightWidth = margin,
            BottomHeight = margin
        };

        DwmExtendFrameIntoClientArea(hwndSource.Handle, ref margins);
    }


    [DllImport("DWMAPI")]
    public static extern nint DwmExtendFrameIntoClientArea(nint hwnd, ref Margins margins);

    [StructLayout(LayoutKind.Sequential)]
    public struct Margins
    {
        public int LeftWidth;
        public int RightWidth;
        public int TopHeight;
        public int BottomHeight;
    }


    [DllImport("user32.dll")]
    private static extern int SetWindowCompositionAttribute(IntPtr hwnd, ref WindowCompositionAttributeData data);

    private enum AccentState
    {
        ACCENT_DISABLED = 0,
        ACCENT_ENABLE_GRADIENT = 1,
        ACCENT_ENABLE_TRANSPARENTGRADIENT = 2,
        ACCENT_ENABLE_BLURBEHIND = 3,
        ACCENT_ENABLE_ACRYLICBLURBEHIND = 4,
        ACCENT_INVALID_STATE = 5,
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct AccentPolicy
    {
        public AccentState AccentState;
        public int AccentFlags;
        public int GradientColor;
        public int AnimationId;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct WindowCompositionAttributeData
    {
        public WindowCompositionAttribute Attribute;
        public IntPtr Data;
        public int SizeOfData;
    }

    private enum WindowCompositionAttribute
    {
        WCA_ACCENT_POLICY = 19,
    }

    [DllImport("dwmapi.dll")]
    static extern int DwmSetWindowAttribute(IntPtr hwnd, DWMWINDOWATTRIBUTE dwAttribute, ref int pvAttribute, int cbAttribute);

    public static int SetWindowAttribute(IntPtr hwnd, DWMWINDOWATTRIBUTE attribute, int parameter)
        => DwmSetWindowAttribute(hwnd, attribute, ref parameter, Marshal.SizeOf<int>());

    [Flags]
    public enum DWMWINDOWATTRIBUTE
    {
        DWMWA_USE_IMMERSIVE_DARK_MODE = 20,
        DWMWA_SYSTEMBACKDROP_TYPE = 38,
        WINDOW_CORNER_PREFERENCE = 33
    }
    public enum WindowCorner
    {
        Default = 0,
        DoNotRound = 1,
        Round = 2,
        RoundSmall = 3
    }
    public static void SetWindowCorner(IntPtr handle, WindowCorner corner)
    {
        SetWindowAttribute(handle, DWMWINDOWATTRIBUTE.WINDOW_CORNER_PREFERENCE, (int)corner);
    }

    public static void SetWindowComposition(IntPtr handle, bool enable, int? hexColor = null)
    {
        var accent = new AccentPolicy();
        if (!enable)
        {
            accent.AccentState = AccentState.ACCENT_DISABLED;
        }
        else
        {
            accent.AccentState = AccentState.ACCENT_ENABLE_ACRYLICBLURBEHIND;
            accent.GradientColor = hexColor ?? 0x00000000;
        }
        var data = new WindowCompositionAttributeData
        {
            Attribute = WindowCompositionAttribute.WCA_ACCENT_POLICY,
            SizeOfData = Marshal.SizeOf<AccentPolicy>(),
            Data = Marshal.AllocHGlobal(Marshal.SizeOf<AccentPolicy>())
        };
        Marshal.StructureToPtr(accent, data.Data, false);
        SetWindowCompositionAttribute(handle, ref data);
        Marshal.FreeHGlobal(data.Data);
    }
    public static void SetBackDropType(IntPtr handle, MaterialType mode)
    {
        SetWindowAttribute(
            handle,
            DWMWINDOWATTRIBUTE.DWMWA_SYSTEMBACKDROP_TYPE,
            (int)mode);
    }

    public static void SetDarkMode(IntPtr handle, bool isDarkMode)
    {
        SetWindowAttribute(
                    handle,
                    DWMWINDOWATTRIBUTE.DWMWA_USE_IMMERSIVE_DARK_MODE,
                    isDarkMode ? 1 : 0);
    }
}
