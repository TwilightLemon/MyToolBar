using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace MyToolBar.Common.WinAPI
{
    /// <summary>
    /// 提供窗口事件的 WinEventHook，用于监听前台窗口切换、窗口移动/显示/隐藏等
    /// 以便 AppBar 实时响应下方环境变化
    /// </summary>
    public static class WindowEventHook
    {
        public delegate void WinEventDelegate(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime);

        [DllImport("user32.dll")]
        static extern IntPtr SetWinEventHook(uint eventMin, uint eventMax, IntPtr hmodWinEventProc, WinEventDelegate lpfnWinEventProc, uint idProcess, uint idThread, uint dwFlags);

        [DllImport("user32.dll")]
        static extern bool UnhookWinEvent(IntPtr hWinEventHook);

        // 我们关心的窗口事件
        private const uint EVENT_SYSTEM_FOREGROUND         = 0x0003;
        private const uint EVENT_OBJECT_LOCATIONCHANGE      = 0x800B;
        private const uint EVENT_OBJECT_SHOW                = 0x8002;
        private const uint EVENT_OBJECT_HIDE                = 0x8003;
        private const uint EVENT_SYSTEM_MINIMIZESTART       = 0x0016;
        private const uint EVENT_SYSTEM_MINIMIZEEND         = 0x0017;
        private const uint WINEVENT_OUTOFCONTEXT            = 0;

        // 目标事件集，用于回调中快速过滤
        private static readonly HashSet<uint> TargetEvents = new()
        {
            EVENT_SYSTEM_FOREGROUND,
            EVENT_OBJECT_LOCATIONCHANGE,
            EVENT_OBJECT_SHOW,
            EVENT_OBJECT_HIDE,
            EVENT_SYSTEM_MINIMIZESTART,
            EVENT_SYSTEM_MINIMIZEEND,
        };

        // 事件范围覆盖我们关心的所有事件
        private const uint EVENT_MIN = 0x0003;  // EVENT_SYSTEM_FOREGROUND
        private const uint EVENT_MAX = 0x800B;  // EVENT_OBJECT_LOCATIONCHANGE

        private static readonly Dictionary<IntPtr, GCHandle> _delegates = new();

        /// <summary>
        /// 注册窗口事件 Hook，监听前台窗口、窗口移动、显示/隐藏、最小化/还原等。
        /// 回调中会自动过滤掉非目标事件和非窗口对象。
        /// </summary>
        public static IntPtr Register(WinEventDelegate handler)
        {
            // 包装一层以过滤事件类型和对象类型
            WinEventDelegate filterHandler = (hWinEventHook, eventType, hwnd, idObject, idChild, dwEventThread, dwmsEventTime) =>
            {
                // 只响应窗口对象 (OBJID_WINDOW = 0) 且是我们关心的事件类型
                if (idObject == 0 && TargetEvents.Contains(eventType) && hwnd != IntPtr.Zero)
                {
                    handler(hWinEventHook, eventType, hwnd, idObject, idChild, dwEventThread, dwmsEventTime);
                }
            };

            var gcHandle = GCHandle.Alloc(filterHandler);
            IntPtr hook = SetWinEventHook(EVENT_MIN, EVENT_MAX, IntPtr.Zero, filterHandler, 0, 0, WINEVENT_OUTOFCONTEXT);
            _delegates[hook] = gcHandle;
            return hook;
        }

        /// <summary>
        /// 注销窗口事件 Hook
        /// </summary>
        public static void Unregister(IntPtr hook)
        {
            UnhookWinEvent(hook);
            if (_delegates.TryGetValue(hook, out var gcHandle))
            {
                gcHandle.Free();
                _delegates.Remove(hook);
            }
        }
    }
}
