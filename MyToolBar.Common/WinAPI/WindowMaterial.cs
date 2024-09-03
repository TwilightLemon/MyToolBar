using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace MyToolBar.Common.WinAPI;
public class WindowMaterial : DependencyObject
{
    /// <summary>
    /// 指示内部使用的API类型
    /// </summary>
    internal enum APIType
    {
        NONE, SYSTEMBACKDROP, COMPOSITION
    }
    public Window? AttachedWindow
    {
        get => _window;
        set
        {
            _window = value;
            if (value != null)
            {
                _hWnd = new WindowInteropHelper(_window).Handle;
                if (_hWnd == IntPtr.Zero)
                    value.SourceInitialized += AttachedWindow_SourceInitialized;
                else InitWindow();
            }
        }
    }
    private void InitWindow()
    {
        _hWnd = new WindowInteropHelper(_window).Handle;
        SetDarkMode(IsDarkMode);
        Apply();
    }

    private void Apply()
    {
        bool enable = _window != null && (MaterialMode != MaterialMode.None || UseWindowComposition);
        if (enable)
        {
            //操作系统判定，如果是window10 即使使用MaterialMode也调用CompositionAPI
            var osVersion = Environment.OSVersion.Version;
            var windows10_1809 = new Version(10, 0, 17763);
            var windows11 = new Version(10, 0, 22621);
            if (UseWindowComposition || (osVersion >= windows10_1809 && osVersion < windows11))
            {
                SetWindowProperty(true);
                SetWindowCompositon(true);
            }
            else
            {
                SetWindowProperty(false);
                SetBackDropType(MaterialMode);
            }
        }
    }

    private void AttachedWindow_SourceInitialized(object? sender, EventArgs e)
    {
        InitWindow();
    }

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

    public static void OnMaterialChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is Window w && e.NewValue is WindowMaterial m)
        {
            m.AttachedWindow = w;
        }
    }

    public bool IsDarkMode
    {
        get { return (bool)GetValue(IsDarkModeProperty); }
        set { SetValue(IsDarkModeProperty, value); }
    }

    public static readonly DependencyProperty IsDarkModeProperty =
        DependencyProperty.Register("IsDarkMode",
            typeof(bool), typeof(WindowMaterial),
            new PropertyMetadata(false, OnIsDarkModeChanged));
    public static void OnIsDarkModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is WindowMaterial w)
        {
            w.SetDarkMode((bool)e.NewValue);
        }
    }


    public MaterialMode MaterialMode
    {
        get { return (MaterialMode)GetValue(MaterialModeProperty); }
        set { SetValue(MaterialModeProperty, value); }
    }

    public static readonly DependencyProperty MaterialModeProperty =
        DependencyProperty.Register("MaterialMode",
            typeof(MaterialMode), typeof(WindowMaterial),
            new PropertyMetadata(MaterialMode.None, OnMaterialModeChanged));
    public static void OnMaterialModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is WindowMaterial m)
        {
            if (m.CurrentAPI != APIType.COMPOSITION)
            {
                if (m.CurrentAPI != APIType.SYSTEMBACKDROP)
                    m.SetWindowProperty(false);
                m.SetBackDropType((MaterialMode)e.NewValue);
            }
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

    public static void OnUseWindowCompositionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is WindowMaterial m)
        {
            if ((bool)e.NewValue)
            {
                if (m.CurrentAPI != APIType.COMPOSITION)
                {
                    m.SetWindowProperty(true);
                    m.SetWindowCompositon(true);
                }
            }
            else
            {
                m.SetWindowCompositon(false);
                if (m.MaterialMode != MaterialMode.None)
                {
                    m.SetWindowProperty(false);
                    m.SetBackDropType(m.MaterialMode);
                }
            }
        }
    }


    private Color _compositionColor;
    private int _blurColor;


    public Color CompositonColor
    {
        get { return (Color)GetValue(CompositonColorProperty); }
        set { SetValue(CompositonColorProperty, value); }
    }

    public static readonly DependencyProperty CompositonColorProperty =
        DependencyProperty.Register("CompositonColor", 
            typeof(Color), typeof(WindowMaterial),
            new PropertyMetadata(Color.FromArgb(180,0,0,0),OnCompositionColorChanged));
    public static void OnCompositionColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is WindowMaterial m)
        {
            m.SetCompositionColor((Color)e.NewValue);
            if(m.CurrentAPI==APIType.COMPOSITION)
                m.SetWindowCompositon(true);
        }
    }

    private IntPtr _hWnd = IntPtr.Zero;
    private Window? _window = null;
    internal APIType CurrentAPI = APIType.NONE;
    public void SetCompositionColor(Color value)
    {
        _compositionColor = value;
        _blurColor =
       // 组装红色分量。
       value.R << 0 |
       // 组装绿色分量。
       value.G << 8 |
       // 组装蓝色分量。
       value.B << 16 |
       // 组装透明分量。
       value.A << 24;
    }
    public void SetDarkMode(bool isDarkMode)
    {
        if (_hWnd == IntPtr.Zero) return;
        MaterialApis.SetDarkMode(_hWnd, isDarkMode);
    }
    public void SetBackDropType(MaterialMode blurMode)
    {
        if (_hWnd == IntPtr.Zero) return;
        MaterialApis.SetBackDropType(_hWnd, blurMode);
        CurrentAPI = blurMode == MaterialMode.None ? APIType.NONE : APIType.SYSTEMBACKDROP;
    }
    public void SetWindowCompositon(bool enable)
    {
        if (_hWnd == IntPtr.Zero) return;
        MaterialApis.SetWindowComposition(_hWnd, enable, _blurColor);
        CurrentAPI = enable ? APIType.COMPOSITION : APIType.NONE;
    }
    public void SetWindowProperty(bool isLagcy = false)
    {
        if (_hWnd == IntPtr.Zero) return;
        var hwndSource = (HwndSource)PresentationSource.FromVisual(_window);
        hwndSource.CompositionTarget.BackgroundColor = Colors.Transparent;

        int margin = isLagcy ? 0 : -1;
        // 设置边框
        var margins = new MaterialApis.Margins()
        {
            LeftWidth = margin,
            TopHeight = margin,
            RightWidth = margin,
            BottomHeight = margin
        };

        MaterialApis.DwmExtendFrameIntoClientArea(hwndSource.Handle, ref margins);
    }
}
public enum MaterialMode
{
    None = 1,
    Acrylic = 3,
    Mica = 2,
    Tabbed = 4
}
internal static class MaterialApis
{

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

    [DllImport("user32.dll")]
    private static extern bool GetWindowCompositionAttribute(IntPtr hwnd, ref WindowCompositionAttributeData data);

    private enum AccentState
    {
        /// <summary>
        /// 完全禁用 DWM 的叠加特效。
        /// </summary>
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
        DWMWA_SYSTEMBACKDROP_TYPE = 38
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
    public static void SetBackDropType(IntPtr handle, MaterialMode mode)
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
