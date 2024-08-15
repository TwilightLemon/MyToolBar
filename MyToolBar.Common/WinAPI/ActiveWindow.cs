using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MyToolBar.Common.WinAPI
{
    public static class ActiveWindow
    {
        public delegate void WinEventDelegate(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime);
        [DllImport("user32.dll")]
        static extern IntPtr SetWinEventHook(uint eventMin, uint eventMax, IntPtr hmodWinEventProc, WinEventDelegate lpfnWinEventProc, uint idProcess, uint idThread, uint dwFlags);
        [DllImport("user32.dll")]
        static extern bool UnhookWinEvent(IntPtr hWinEventHook);

        private const uint EVENT_SYSTEM_FOREGROUND = 3,
                                    EVENT_OBJECT_NAMECHANGE = 0x800C;
        private const uint WINEVENT_OUTOFCONTEXT = 0;

        public static IntPtr RegisterActiveWindowHook(WinEventDelegate handler)
        {
            GCHandle.Alloc(handler);
            return SetWinEventHook(EVENT_OBJECT_NAMECHANGE, EVENT_OBJECT_NAMECHANGE, IntPtr.Zero, handler, 0, 0, WINEVENT_OUTOFCONTEXT);
        }
        public static void UnregisterActiveWindowHook(IntPtr hook)
        {
            UnhookWinEvent(hook);
        }

        [DllImport("user32.dll", SetLastError = true)]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

        public static Process? GetWindowProcess(this IntPtr hWnd)
        {
            GetWindowThreadProcessId(hWnd, out uint processId);
            try
            {
                return Process.GetProcessById((int)processId);
            }
            catch (ArgumentException)
            {
                // 指定的进程ID不存在
                return null;
            }
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll")]
        static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport("User32.dll", CharSet = CharSet.Auto)]
         static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int nMaxCount);
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        public static string GetWindowClass(this IntPtr hWnd)
        {
            var sb= new StringBuilder(60);
            GetClassName(hWnd, sb, sb.Capacity);
            return sb.ToString();
        }
        public static string GetWindowTitle(this IntPtr hWnd)
        {
            int length = GetWindowTextLength(hWnd);
            StringBuilder sb = new(length + 1);
            GetWindowText(hWnd, sb, sb.Capacity);
            return sb.ToString();
        }
        public static string GetActiveWindowTitle() {
            IntPtr hWnd = GetForegroundWindow();
            int length = GetWindowTextLength(hWnd);
            StringBuilder sb = new(length + 1);
            GetWindowText(hWnd, sb, sb.Capacity);
            return sb.ToString();
        }
    }
}
