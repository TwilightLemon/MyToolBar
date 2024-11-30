using System.ComponentModel;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using Windows.Security.Cryptography.Certificates;

namespace MyToolBar.Common.WinAPI;
public static class AppBarCreator
{
    public static readonly DependencyProperty AppBarProperty =
        DependencyProperty.RegisterAttached(
            "AppBar",
            typeof(AppBar),
            typeof(AppBarCreator),
            new PropertyMetadata(null, OnAppBarChanged));
    private static void OnAppBarChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is Window window && e.NewValue is AppBar appBar)
        {
            appBar.AttachedWindow = window;
        }
    }
    public static void SetAppBar(Window element, AppBar value)
    {
        if (value == null) return;
        element.SetValue(AppBarProperty, value);
    }

    public static AppBar GetAppBar(Window element)
    {
        return (AppBar)element.GetValue(AppBarProperty);
    }
}

public class AppBar : DependencyObject
{
    /// <summary>
    /// 附加到的窗口
    /// </summary>
    public Window AttachedWindow
    {
        get => _window;
        set
        {
            if (value == null) return;
            _window = value;
            _window.Closing += _window_Closing;
            _window.LocationChanged += _window_LocationChanged;
            _window.DpiChanged += _window_DpiChanged;
            //获取窗口句柄hWnd
            var handle = new WindowInteropHelper(value).Handle;
            if (handle == IntPtr.Zero)
            {
                //Win32窗口未创建
                _window.SourceInitialized += _window_SourceInitialized;
            }
            else
            {
                _hWnd = handle;
                CheckPending();
            }
        }
    }

    private void _window_DpiChanged(object sender, DpiChangedEventArgs e)
    {
        UpdateDpiSettings();
    }

    public void UpdateDpiSettings()
    {
        //Update AppBar Position when dpi changed.
        if (Location != AppBarLocation.None && Location != AppBarLocation.RegisterOnly)
        {
            var temp = Location;
            Location = AppBarLocation.None;
            Location = temp;
        }
    }

    private void _window_LocationChanged(object? sender, EventArgs e)
    {
        Debug.WriteLine(_window.Title + " LocationChanged: Top: " + _window.Top + "  Left: " + _window.Left);
    }

    private void _window_Closing(object? sender, CancelEventArgs e)
    {
        _window.Closing -= _window_Closing;
        if (Location != AppBarLocation.None)
            DisableAppBar();
    }

    /// <summary>
    /// 检查是否需要应用之前的Location更改
    /// </summary>
    private void CheckPending()
    {
        //创建AppBar时提前触发的LocationChanged
        if (_locationChangePending)
        {
            _locationChangePending = false;
            LoadAppBar(Location);
        }
    }
    /// <summary>
    /// 载入AppBar
    /// </summary>
    /// <param name="e"></param>
    private void LoadAppBar(AppBarLocation e, AppBarLocation? previous = null)
    {

        if (e != AppBarLocation.None)
        {
            if (e == AppBarLocation.RegisterOnly)
            {
                //仅注册AppBarMsg
                //如果之前注册过有效的AppBar则先注销，以还原位置
                if (previous.HasValue && previous.Value != AppBarLocation.RegisterOnly)
                {
                    if (previous.Value != AppBarLocation.None)
                    {
                        //由生效的AppBar转为RegisterOnly，还原为普通窗口再注册空AppBar
                        DisableAppBar();
                    }
                    RegisterAppBarMsg();
                }
                else
                {
                    //之前未注册过AppBar，直接注册
                    RegisterAppBarMsg();
                }
            }
            else
            {
                if (previous.HasValue && previous.Value != AppBarLocation.None)
                {
                    //之前为RegisterOnly才备份窗口信息
                    if (previous.Value == AppBarLocation.RegisterOnly)
                    {
                        BackupWindowInfo();
                    }
                    SetAppBarPosition(_originalSize);
                    ForceWindowStyles();
                }
                else
                    EnableAppBar();
            }
        }
        else
        {
            DisableAppBar();
        }
    }
    private void _window_SourceInitialized(object? sender, EventArgs e)
    {
        _window.SourceInitialized -= _window_SourceInitialized;
        _hWnd = new WindowInteropHelper(_window).Handle;
        CheckPending();
    }

    /// <summary>
    /// 当有窗口进入或退出全屏时触发 bool参数为true时表示全屏状态
    /// </summary>
    public event EventHandler<bool>? OnFullScreenStateChanged;
    /// <summary>
    /// 期望将AppBar停靠到的位置
    /// </summary>
    public AppBarLocation Location
    {
        get { return (AppBarLocation)GetValue(LocationProperty); }
        set { SetValue(LocationProperty, value); }
    }

    public static readonly DependencyProperty LocationProperty =
        DependencyProperty.Register(
            "Location",
            typeof(AppBarLocation), typeof(AppBar),
            new PropertyMetadata(AppBarLocation.None, OnLocationChanged));

    private bool _locationChangePending = false;
    private static void OnLocationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (DesignerProperties.GetIsInDesignMode(d))
            return;
        if (d is not AppBar appBar) return;
        if (appBar.AttachedWindow == null)
        {
            appBar._locationChangePending = true;
            return;
        }
        appBar.LoadAppBar((AppBarLocation)e.NewValue, (AppBarLocation)e.OldValue);
    }

    private int _callbackId = 0;
    private bool _isRegistered = false;
    private Window _window = null;
    private IntPtr _hWnd;
    private WindowStyle _originalStyle;
    private Point _originalPosition;
    private Size _originalSize = Size.Empty;
    private ResizeMode _originalResizeMode;
    private bool _originalTopmost;
    public Rect? DockedSize { get; set; } = null;

    public const int WM_USER = 0x0400;
    public const int WM_REFLECT = WM_USER + 0x1C00;
    public const int WM_DISPLAYCHANGE = 0x007E;

    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam,
                                    IntPtr lParam, ref bool handled)
    {
        if (msg == _callbackId)
        {
            Debug.WriteLine(_window.Title + " AppBarMsg(" + _callbackId + "): " + wParam.ToInt32() + " LParam: " + lParam.ToInt32());
            switch (wParam.ToInt32())
            {
                case (int)Interop.AppBarNotify.ABN_POSCHANGED:
                    Debug.WriteLine("AppBarNotify.ABN_POSCHANGED ! " + _window.Title);
                    if (Location != AppBarLocation.RegisterOnly)
                        SetAppBarPosition(Size.Empty);
                    handled = true;
                    break;
                case (int)Interop.AppBarNotify.ABN_FULLSCREENAPP:
                    OnFullScreenStateChanged?.Invoke(this, lParam.ToInt32() == 1);
                    handled = true;
                    break;
            }
        }else if(msg==WM_DISPLAYCHANGE)
        {
            Debug.WriteLine("WM_DISPLAYCHANGE ! " + _window.Title);
            UpdateDpiSettings();
            handled = true;
        }
        return IntPtr.Zero;
    }

    public void BackupWindowInfo()
    {
        _callbackId = 0;
        DockedSize = null;
        _originalStyle = _window.WindowStyle;
        _originalSize = new Size(_window.ActualWidth, _window.ActualHeight);
        _originalPosition = new Point(_window.Left, _window.Top);
        _originalResizeMode = _window.ResizeMode;
        _originalTopmost = _window.Topmost;
    }
    public void RestoreWindowInfo()
    {
        if (_originalSize != Size.Empty)
        {
            _window.WindowStyle = _originalStyle;
            _window.ResizeMode = _originalResizeMode;
            _window.Topmost = _originalTopmost;
            _window.Left = _originalPosition.X;
            _window.Top = _originalPosition.Y;
            _window.Width = _originalSize.Width;
            _window.Height = _originalSize.Height;
        }
    }
    public void ForceWindowStyles()
    {
        _window.WindowStyle = WindowStyle.None;
        _window.ResizeMode = ResizeMode.NoResize;
        _window.Topmost = true;
    }

    public void RegisterAppBarMsg()
    {
        var data = new Interop.APPBARDATA();
        data.cbSize = Marshal.SizeOf(data);
        data.hWnd = _hWnd;

        _isRegistered = true;
        _callbackId = Interop.RegisterWindowMessage(Guid.NewGuid().ToString());
        data.uCallbackMessage = _callbackId;
        var success = Interop.SHAppBarMessage((int)Interop.AppBarMsg.ABM_NEW, ref data);
        var source = HwndSource.FromHwnd(_hWnd);
        Debug.WriteLineIf(source == null, "HwndSource is null!");
        source?.AddHook(WndProc);
        Debug.WriteLine(_window.Title + " RegisterAppBarMsg: " + _callbackId);
    }
    public void EnableAppBar()
    {
        if (!_isRegistered)
        {
            //备份窗口信息并设置窗口样式
            BackupWindowInfo();
            //注册成为AppBar窗口
            RegisterAppBarMsg();
            ForceWindowStyles();
        }
        //成为AppBar窗口之后(或已经是)只需要注册并移动窗口位置即可
        SetAppBarPosition(_originalSize);
    }
    public void SetAppBarPosition(Size WindowSize)
    {
        var data = new Interop.APPBARDATA();
        data.cbSize = Marshal.SizeOf(data);
        data.hWnd = _hWnd;
        data.uEdge = (int)Location;
        data.uCallbackMessage = _callbackId;
        Debug.WriteLine("\r\nWindow: " + _window.Title);

        (double dpix,double dpiy)=ScreenAPI.GetDPI(_hWnd);
        Debug.WriteLine($"DPIX:{dpix}  DPIY:{dpiy}");
        //窗口在屏幕的实际大小
        if (WindowSize == Size.Empty)
            WindowSize = new Size(_window.ActualWidth, _window.ActualHeight);
        var actualSize =(X: WindowSize.Width*dpix, Y: WindowSize.Height*dpiy);
        //屏幕的真实像素
        var workArea = (X: SystemParameters.WorkArea.Width * dpix, Y: SystemParameters.WorkArea.Height * dpiy);
        Debug.WriteLine("WorkArea Width: {0}, Height: {1}", workArea.X, workArea.Y);

        if (Location is AppBarLocation.Left or AppBarLocation.Right)
        {
            data.rc.top = 0;
            data.rc.bottom = (int)workArea.Y;
            if (Location == AppBarLocation.Left)
            {
                data.rc.left = 0;
                data.rc.right = (int)Math.Round(actualSize.X);
            }
            else
            {
                data.rc.right = (int)workArea.X;
                data.rc.left = (int)workArea.X - (int)Math.Round(actualSize.X);
            }
        }
        else
        {
            data.rc.left = 0;
            data.rc.right = (int)workArea.X;
            if (Location == AppBarLocation.Top)
            {
                data.rc.top = 0;
                data.rc.bottom = (int)Math.Round(actualSize.Y);
            }
            else
            {
                data.rc.bottom = (int)workArea.Y;
                data.rc.top = (int)workArea.Y - (int)Math.Round(actualSize.Y);
            }
        }
        //以上生成的是四周都没有其他AppBar时的理想位置
        //系统将自动调整位置以适应其他AppBar
        Debug.WriteLine("Before QueryPos: Left: {0}, Top: {1}, Right: {2}, Bottom: {3}", data.rc.left, data.rc.top, data.rc.right, data.rc.bottom);
        Interop.SHAppBarMessage((int)Interop.AppBarMsg.ABM_QUERYPOS, ref data);
        Debug.WriteLine("After QueryPos: Left: {0}, Top: {1}, Right: {2}, Bottom: {3}", data.rc.left, data.rc.top, data.rc.right, data.rc.bottom);
        //自定义对齐方式，确保Height和Width不会小于0
        if (data.rc.bottom - data.rc.top < 0)
        {
            if (Location == AppBarLocation.Top)
                data.rc.bottom = data.rc.top + (int)Math.Round(actualSize.Y);//上对齐
            else if (Location == AppBarLocation.Bottom)
                data.rc.top = data.rc.bottom - (int)Math.Round(actualSize.Y);//下对齐
        }
        if (data.rc.right - data.rc.left < 0)
        {
            if (Location == AppBarLocation.Left)
                data.rc.right = data.rc.left + (int)Math.Round(actualSize.X);//左对齐
            else if (Location == AppBarLocation.Right)
                data.rc.left = data.rc.right - (int)Math.Round(actualSize.X);//右对齐
        }
        //调整完毕，设置为最终位置
        Interop.SHAppBarMessage((int)Interop.AppBarMsg.ABM_SETPOS, ref data);
        //应用到窗口
        var location = new Point(data.rc.left/dpix, data.rc.top/dpiy);
        var dimension = new Size((double)(data.rc.right  - data.rc.left )/ dpix, (double)(data.rc.bottom - data.rc.top) / dpiy);
        var rect = new Rect(location, dimension);
        DockedSize = rect;

        _window.Dispatcher.Invoke(DispatcherPriority.ApplicationIdle, () => {
            _window.Left = rect.Left;
            _window.Top = rect.Top;
            _window.Width = rect.Width;
            _window.Height = rect.Height;
        });

        Debug.WriteLine("Set {0} Left: {1} ,Top: {2}, Width: {3}, Height: {4}", _window.Title, _window.Left, _window.Top, _window.Width, _window.Height);
    }
    public void DisableAppBar()
    {
        if (_isRegistered)
        {
            _isRegistered = false;
            var data = new Interop.APPBARDATA();
            data.cbSize = Marshal.SizeOf(data);
            data.hWnd = _hWnd;
            data.uCallbackMessage = _callbackId;
            Interop.SHAppBarMessage((int)Interop.AppBarMsg.ABM_REMOVE, ref data);
            _isRegistered = false;
            RestoreWindowInfo();
            Debug.WriteLine(_window.Title + " DisableAppBar");
        }
    }
}

public enum AppBarLocation : int
{
    Left = 0,
    Top,
    Right,
    Bottom,
    None,
    RegisterOnly = 99
}

internal static class Interop
{
    #region Structures & Flags
    [StructLayout(LayoutKind.Sequential)]
    internal struct RECT
    {
        public int left;
        public int top;
        public int right;
        public int bottom;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct APPBARDATA
    {
        public int cbSize;
        public IntPtr hWnd;
        public int uCallbackMessage;
        public int uEdge;
        public RECT rc;
        public IntPtr lParam;
    }

    internal enum AppBarMsg : int
    {
        ABM_NEW = 0,
        ABM_REMOVE,
        ABM_QUERYPOS,
        ABM_SETPOS,
        ABM_GETSTATE,
        ABM_GETTASKBARPOS,
        ABM_ACTIVATE,
        ABM_GETAUTOHIDEBAR,
        ABM_SETAUTOHIDEBAR,
        ABM_WINDOWPOSCHANGED,
        ABM_SETSTATE
    }
    internal enum AppBarNotify : int
    {
        ABN_STATECHANGE = 0,
        ABN_POSCHANGED,
        ABN_FULLSCREENAPP,
        ABN_WINDOWARRANGE
    }
    #endregion

    #region Win32 API
    [DllImport("SHELL32", CallingConvention = CallingConvention.StdCall)]
    internal static extern uint SHAppBarMessage(int dwMessage, ref APPBARDATA pData);

    [DllImport("User32.dll", CharSet = CharSet.Auto)]
    internal static extern int RegisterWindowMessage(string msg);
    #endregion
}
