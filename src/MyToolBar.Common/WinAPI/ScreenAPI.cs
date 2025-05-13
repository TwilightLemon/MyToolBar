using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Interop;

namespace MyToolBar.Common.WinAPI
{
    public static class ScreenAPI
    {
        public static Bitmap CaptureScreenArea(int x , int y, int width, int height)
        {
            try
            {
                if (width == 0 || height == 0) return null;
                Bitmap bmp = new Bitmap(width, height);
                using Graphics g = Graphics.FromImage(bmp);
                g.CopyFromScreen(x, y, 0, 0, new Size(width, height));
                return bmp;
            }
            catch
            {
                return null;
            }
        }

        public static Size GetScreenArea(IntPtr hwnd)
        {
            IntPtr hmonitor = GetHmonitorForHwnd(hwnd);
            var screen = GetMonitorSize(hmonitor);
            var (dpiX, dpiY) = GetDPI(hmonitor);
            return new Size((int)(screen.Width / dpiX), (int)(screen.Height /dpiY));
        }

        public static IntPtr GetHmonitorForHwnd(IntPtr hwnd)
        {
            var hmonitor = MonitorFromWindow(hwnd, MonitorDefaultTo.MONITOR_DEFAULTTONEAREST);
            return hmonitor;
        }

        public static (double X,double Y) GetDPI(IntPtr hmonitor)
        {
            GetDpiForMonitor(hmonitor, DpiType.Effective, out uint dpiX, out uint dpiY);
            return (X:(double)dpiX/96, Y:(double)dpiY/96);
        }

        public static Size GetMonitorSize(IntPtr hmonitor)
        {
            MONITORINFOEX info = new MONITORINFOEX();
            if (GetMonitorInfo(new HandleRef(null, hmonitor), info))
            {
                return new Size(info.rcMonitor.right - info.rcMonitor.left, info.rcMonitor.bottom - info.rcMonitor.top);
            }
            return new Size(0, 0);
        }

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern IntPtr MonitorFromWindow(IntPtr hwnd, MonitorDefaultTo dwFlags);
        [DllImport("Shcore.dll")]
        private static extern IntPtr GetDpiForMonitor([In] IntPtr hmonitor, [In] DpiType dpiType, [Out] out uint dpiX, [Out] out uint dpiY);

        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        private static extern bool GetMonitorInfo(HandleRef hmonitor, [In, Out] MONITORINFOEX info);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto, Pack = 4)]
        public class MONITORINFOEX
        {
            public int cbSize = Marshal.SizeOf(typeof(MONITORINFOEX));
            public RECT rcMonitor = new RECT();
            public RECT rcWork = new RECT();
            public int dwFlags = 0;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public char[] szDevice = new char[32];
        }


        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }
        internal enum MonitorDefaultTo
        {
            MONITOR_DEFAULTTONULL,
            MONITOR_DEFAULTTOPRIMARY,
            MONITOR_DEFAULTTONEAREST,
        }
        public enum DpiType
        {
            Effective = 0,
            Angular = 1,
            Raw = 2,
        }
    }
}
