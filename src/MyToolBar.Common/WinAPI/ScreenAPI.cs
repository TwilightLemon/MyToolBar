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

        public static (double,double) GetDPI(IntPtr hwnd)
        {
            var hmonitor = MonitorFromWindow(hwnd, MonitorDefaultTo.MONITOR_DEFAULTTONEAREST);
            GetDpiForMonitor(hmonitor, DpiType.Effective, out uint dpiX, out uint dpiY);
            return ((double)dpiX/96, (double)dpiY/96);
        }

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern IntPtr MonitorFromWindow(IntPtr hwnd, MonitorDefaultTo dwFlags);
        [DllImport("Shcore.dll")]
        private static extern IntPtr GetDpiForMonitor([In] IntPtr hmonitor, [In] DpiType dpiType, [Out] out uint dpiX, [Out] out uint dpiY);
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
