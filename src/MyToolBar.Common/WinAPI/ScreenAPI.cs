using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Interop;

namespace MyToolBar.Common.WinAPI
{
    public static class ScreenAPI
    {
        [DllImport("user32.dll")]
        public static extern bool GetWindowRect(IntPtr hwnd, out RECT lpRect);

        /// <summary>
        /// 将物理像素坐标转换为 WPF 新窗口的初始逻辑坐标（以主显示器 DPI 为准）
        /// </summary>
        public static (double X, double Y) PhysicalToNewWindowCoords(double physicalX, double physicalY)
        {
            var primaryMonitor = GetPrimaryMonitor();
            var (dpiX, dpiY) = GetDPI(primaryMonitor);
            return (physicalX / dpiX, physicalY / dpiY);
        }

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

        /// <summary>
        /// 获取主显示器的 HMONITOR 句柄
        /// </summary>
        public static IntPtr GetPrimaryMonitor()
        {
            return MonitorFromWindow(IntPtr.Zero, MonitorDefaultTo.MONITOR_DEFAULTTOPRIMARY);
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

        /// <summary>
        /// 获取完整的 MONITORINFOEX 信息（包含设备名称、显示器区域和工作区域）
        /// </summary>
        public static MONITORINFOEX? GetMonitorInfoEx(IntPtr hmonitor)
        {
            MONITORINFOEX info = new MONITORINFOEX();
            if (GetMonitorInfo(new HandleRef(null, hmonitor), info))
            {
                return info;
            }
            return null;
        }

        /// <summary>
        /// 获取指定监视器的工作区域尺寸（物理像素）
        /// </summary>
        public static Size GetMonitorWorkArea(IntPtr hmonitor)
        {
            MONITORINFOEX info = new MONITORINFOEX();
            if (GetMonitorInfo(new HandleRef(null, hmonitor), info))
            {
                return new Size(info.rcWork.right - info.rcWork.left, info.rcWork.bottom - info.rcWork.top);
            }
            return new Size(0, 0);
        }

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern IntPtr MonitorFromWindow(IntPtr hwnd, MonitorDefaultTo dwFlags);
        [DllImport("Shcore.dll")]
        private static extern IntPtr GetDpiForMonitor([In] IntPtr hmonitor, [In] DpiType dpiType, [Out] out uint dpiX, [Out] out uint dpiY);

        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        internal static extern bool GetMonitorInfo(HandleRef hmonitor, [In, Out] MONITORINFOEX info);

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
