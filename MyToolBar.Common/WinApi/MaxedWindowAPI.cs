using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MyToolBar.Common.WinApi
{
    public class MaxedWindowAPI(Action<bool> OnWindowFound)
    {
        public void Find()
        {
            EnumWindows(EnumWindowsCallback, IntPtr.Zero);
            OnWindowFound?.Invoke(false);
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
            bool maxed= IsWindowMaximized(hWnd)&&IsWindowVisible(hWnd)&&hWnd.IsZoomedWindow();
            var title = hWnd.GetWindowTitle();
            var className = hWnd.GetWindowClass();
           // Debug.WriteLineIf(maxed,title+" \\ "+className);
            if (maxed)
            {
                OnWindowFound(true);
                OnWindowFound = null;
                return false; // 如果找到了最大化窗口，可以停止遍历
            }
            return true;
        }
    }
}
