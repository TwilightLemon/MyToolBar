using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Interop;
using System.Windows;

namespace MyToolBar.Common.WinAPI;

//TODO: 与ToolWindowAPI合并

public static class WindowLongAPI
{
    [Flags]
    public enum ExtendedWindowStyles
    {
        // ...
        WS_EX_TOOLWINDOW = 0x00000080,
        // ...
    }

    public enum GetWindowLongFields
    {
        // ...
        GWL_EXSTYLE = -20,
        GWL_STYLE = -16
        // ...
    }
    public static class WS
    {
        public static readonly long
        WS_BORDER = 0x00800000L,
        WS_CAPTION = 0x00C00000L,
        WS_CHILD = 0x40000000L,
        WS_CHILDWINDOW = 0x40000000L,
        WS_CLIPCHILDREN = 0x02000000L,
        WS_CLIPSIBLINGS = 0x04000000L,
        WS_DISABLED = 0x08000000L,
        WS_DLGFRAME = 0x00400000L,
        WS_GROUP = 0x00020000L,
        WS_HSCROLL = 0x00100000L,
        WS_ICONIC = 0x20000000L,
        WS_MAXIMIZE = 0x01000000L,
        WS_MAXIMIZEBOX = 0x00010000L,
        WS_MINIMIZE = 0x20000000L,
        WS_MINIMIZEBOX = 0x00020000L,
        WS_OVERLAPPED = 0x00000000L,
        WS_OVERLAPPEDWINDOW = WS_OVERLAPPED | WS_CAPTION | WS_SYSMENU | WS_THICKFRAME | WS_MINIMIZEBOX | WS_MAXIMIZEBOX,
        WS_POPUP = 0x80000000L,
        WS_POPUPWINDOW = WS_POPUP | WS_BORDER | WS_SYSMENU,
        WS_SIZEBOX = 0x00040000L,
        WS_SYSMENU = 0x00080000L,
        WS_TABSTOP = 0x00010000L,
        WS_THICKFRAME = 0x00040000L,
        WS_TILED = 0x00000000L,
        WS_TILEDWINDOW = WS_OVERLAPPED | WS_CAPTION | WS_SYSMENU | WS_THICKFRAME | WS_MINIMIZEBOX | WS_MAXIMIZEBOX,
        WS_VISIBLE = 0x10000000L,
        WS_VSCROLL = 0x00200000L;
    }

    [DllImport("user32.dll")]
    public static extern IntPtr GetWindowLong(IntPtr hWnd, int nIndex);

    public static IntPtr SetWindowLong(IntPtr hWnd, int nIndex, IntPtr dwNewLong)
    {
        int error = 0;
        IntPtr result = IntPtr.Zero;
        // Win32 SetWindowLong doesn't clear error on success
        SetLastError(0);

        if (IntPtr.Size == 4)
        {
            // use SetWindowLong
            Int32 tempResult = IntSetWindowLong(hWnd, nIndex, IntPtrToInt32(dwNewLong));
            error = Marshal.GetLastWin32Error();
            result = new IntPtr(tempResult);
        }
        else
        {
            // use SetWindowLongPtr
            result = IntSetWindowLongPtr(hWnd, nIndex, dwNewLong);
            error = Marshal.GetLastWin32Error();
        }

        if ((result == IntPtr.Zero) && (error != 0))
        {
            throw new System.ComponentModel.Win32Exception(error);
        }

        return result;
    }

    [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr", SetLastError = true)]
    private static extern IntPtr IntSetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

    [DllImport("user32.dll", EntryPoint = "SetWindowLong", SetLastError = true)]
    private static extern Int32 IntSetWindowLong(IntPtr hWnd, int nIndex, Int32 dwNewLong);

    private static int IntPtrToInt32(IntPtr intPtr)
    {
        return unchecked((int)intPtr.ToInt64());
    }

    [DllImport("kernel32.dll", EntryPoint = "SetLastError")]
    public static extern void SetLastError(int dwErrorCode);

    public static void SetToolWindow(Window win)
    {
        WindowInteropHelper wndHelper = new(win);
        int exStyle = (int)GetWindowLong(wndHelper.Handle, (int)GetWindowLongFields.GWL_EXSTYLE);
        exStyle |= (int)ExtendedWindowStyles.WS_EX_TOOLWINDOW;
        SetWindowLong(wndHelper.Handle, (int)GetWindowLongFields.GWL_EXSTYLE, (IntPtr)exStyle);
    }




    public static bool GetDwmAnimation(DependencyObject obj)
    {
        return (bool)obj.GetValue(DwmAnimationProperty);
    }

    public static void SetDwmAnimation(DependencyObject obj, bool value)
    {
        obj.SetValue(DwmAnimationProperty, value);
    }

    public static readonly DependencyProperty DwmAnimationProperty =
        DependencyProperty.RegisterAttached("DwmAnimation", 
            typeof(bool), typeof(WindowLongAPI),
            new PropertyMetadata(false,OnDwmAnimationChanged));

    private static void OnDwmAnimationChanged(DependencyObject o,DependencyPropertyChangedEventArgs e)
    {
        if(o is Window{ } w&&(bool)e.NewValue)
        {
            if (w.IsLoaded)
            {
                EnableDwnAnimation(w);
            }
            else
            {
                w.Loaded += Window_Loaded;
            }
        }
    }

    private static void Window_Loaded(object sender, RoutedEventArgs e)
    {
        if (sender is Window { } w){
            EnableDwnAnimation(w);
            w.Loaded -= Window_Loaded;
        }
    }



    //https://stackoverflow.com/questions/56175722/how-to-restore-the-default-show-close-animations-for-a-window-when-setting-windo
    public static void EnableDwnAnimation(Window w)
    {
        var myHWND = new WindowInteropHelper(w).Handle;
        IntPtr myStyle = new IntPtr(WS.WS_CAPTION);
        SetWindowLong(myHWND, (int)GetWindowLongFields.GWL_STYLE, myStyle);
    }

    [DllImport("user32.dll")]
    private static extern bool IsZoomed(IntPtr hWnd);
    public static bool IsZoomedWindow(this IntPtr intPtr)
    {
        return IsZoomed(intPtr);
    }
}
