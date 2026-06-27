using System.Runtime.InteropServices;

namespace MyToolBar.Common.WinAPI
{
    /// <summary>
    /// 遍历窗口查找是否存在最大化窗口（仅限与参考窗口在同一显示器的窗口）
    /// </summary>
    public class MaxedWindowAPI
    {
        private Action<bool>? _onWindowFound;
        private readonly IntPtr _referenceHwnd;

        /// <param name="onWindowFound">找到最大化窗口时的回调</param>
        /// <param name="referenceHwnd">参考窗口句柄，仅检测与该窗口在同一显示器的最大化窗口</param>
        public MaxedWindowAPI(Action<bool> onWindowFound, IntPtr referenceHwnd)
        {
            _onWindowFound = onWindowFound;
            _referenceHwnd = referenceHwnd;
        }

        public void Find()
        {
            EnumWindows(EnumWindowsCallback, IntPtr.Zero);
            _onWindowFound?.Invoke(false);
        }

        [DllImport("user32.dll")]
        private static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);
        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern bool GetWindowPlacement(IntPtr hWnd, ref WINDOWPLACEMENT lpwndpl);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool IsWindowVisible(IntPtr hWnd);

        private struct WINDOWPLACEMENT
        {
            public int length;
            public int flags;
            public int showCmd;
            public Point minPosition;
            public Point maxPosition;
            public Rectangle normalPosition;
        }

        private struct Point
        {
            public int x;
            public int y;
        }

        private struct Rectangle
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        static bool IsWindowMaximized(IntPtr hWnd)
        {
            WINDOWPLACEMENT wp = new();
            wp.length = Marshal.SizeOf(wp);
            GetWindowPlacement(hWnd, ref wp);
            return wp.showCmd == 3; // 3 表示最大化
        }

        private bool EnumWindowsCallback(IntPtr hWnd, IntPtr lParam)
        {
            bool maxed = IsWindowMaximized(hWnd) && IsWindowVisible(hWnd) && hWnd.IsZoomedWindow();
            if (maxed)
            {
                // 检查该窗口是否与参考窗口在同一显示器上
                IntPtr windowMonitor = ScreenAPI.GetHmonitorForHwnd(hWnd);
                IntPtr refMonitor = ScreenAPI.GetHmonitorForHwnd(_referenceHwnd);
                if (windowMonitor == refMonitor)
                {
                    _onWindowFound?.Invoke(true);
                    _onWindowFound = null;
                    return false; // 找到了，停止遍历
                }
            }
            return true;
        }
    }
}
