using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MyToolBar.Common.WinAPI
{
    /// <summary>
    /// 提供实时监测活动窗口Hook，及获取窗口相关信息的API
    /// </summary>
    public static class ActiveWindow
    {
        public delegate void WinEventDelegate(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime);
        [DllImport("user32.dll")]
        static extern IntPtr SetWinEventHook(uint eventMin, uint eventMax, IntPtr hmodWinEventProc, WinEventDelegate lpfnWinEventProc, uint idProcess, uint idThread, uint dwFlags);
        [DllImport("user32.dll")]
        static extern bool UnhookWinEvent(IntPtr hWinEventHook);

        private const uint EVENT_SYSTEM_FOREGROUND = 0x0003,
                                    EVENT_OBJECT_NAMECHANGE = 0x800C;
        private const uint WINEVENT_OUTOFCONTEXT = 0;

        static readonly Dictionary<IntPtr,GCHandle> winEventDelegates = [];
        public static IntPtr RegisterActiveWindowHook(WinEventDelegate handler)
        {
            var gcHandle = GCHandle.Alloc(handler);
            IntPtr hook=SetWinEventHook(EVENT_OBJECT_NAMECHANGE, EVENT_OBJECT_NAMECHANGE, IntPtr.Zero, handler, 0, 0, WINEVENT_OUTOFCONTEXT);
            winEventDelegates[hook]=gcHandle;
            return hook;
        }
        public static void UnregisterActiveWindowHook(IntPtr hook)
        {
            UnhookWinEvent(hook);
            if(winEventDelegates.TryGetValue(hook, out var handler))
            {
                handler.Free();
                winEventDelegates.Remove(hook);
            }
        }

        [DllImport("user32.dll", SetLastError = true)]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

        /// <summary>
        /// 获取指定窗口hWnd所属的进程
        /// </summary>
        /// <param name="hWnd">窗口句柄</param>
        /// <returns></returns>
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

        /// <summary>
        /// 获取窗口的类名
        /// </summary>
        /// <param name="hWnd"></param>
        /// <returns></returns>
        public static string GetWindowClass(this IntPtr hWnd)
        {
            var sb= new StringBuilder(60);
            GetClassName(hWnd, sb, sb.Capacity);
            return sb.ToString();
        }
        /// <summary>
        /// 获取窗口的标题
        /// </summary>
        /// <param name="hWnd"></param>
        /// <returns></returns>
        public static string GetWindowTitle(this IntPtr hWnd)
        {
            int length = GetWindowTextLength(hWnd);
            StringBuilder sb = new(length + 1);
            GetWindowText(hWnd, sb, sb.Capacity);
            return sb.ToString();
        }
        /// <summary>
        /// 仅获取当前活动窗口的标题
        /// </summary>
        /// <returns></returns>
        public static string GetActiveWindowTitle() {
            IntPtr hWnd = GetForegroundWindow();
            int length = GetWindowTextLength(hWnd);
            StringBuilder sb = new(length + 1);
            GetWindowText(hWnd, sb, sb.Capacity);
            return sb.ToString();
        }
    }
}
