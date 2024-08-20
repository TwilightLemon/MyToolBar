using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;

namespace MyToolBar.Common.WinAPI
{
    public static class SystemThemeAPI
    {
        public static bool GetIsLightTheme()
        {
            using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
            if (key != null)
            {
                var value = key.GetValue("AppsUseLightTheme");
                if (value != null && value is int)
                {
                    return (int)value > 0;
                }
            }

            return true; // 默认为浅色模式
        }
        public const int WM_SETTINGCHANGE = 0x001A;
        public const int WM_SYSCOLORCHANGE = 0x0015;
        public const int WM_USER = 0x0400;
        public const int WM_REFLECT = WM_USER + 0x1C00;
        public const int WM_THEMECHANGED = 0x031A;
        static Action? _onThemeChanged = null;
        public static void RegesterOnThemeChanged(Window window,Action onThemeChanged)
        {
            var source = HwndSource.FromHwnd(new WindowInteropHelper(window).Handle);
            source?.AddHook(WndProc);
            _onThemeChanged = onThemeChanged;
        }

        public static IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg ==26)
            {
                _onThemeChanged?.Invoke();
                handled = true;
            }
            return IntPtr.Zero;
        }
    }
}
