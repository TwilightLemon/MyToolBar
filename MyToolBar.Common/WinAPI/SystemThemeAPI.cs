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
        public const int WM_DWMCOLORIZATIONCOLORCHANGED = 0x0320;
        static Action? _onThemeChanged = null;
        static Action? _onSystemColorChanged = null;
        public static void RegesterOnThemeChanged(Window window,Action onThemeChanged,Action onSystemColorChanged)
        {
            _onThemeChanged = onThemeChanged;
            _onSystemColorChanged = onSystemColorChanged;
            var source = HwndSource.FromHwnd(new WindowInteropHelper(window).Handle);
            source?.AddHook(WndProc);
        }

        public static IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_SETTINGCHANGE)
            {
                _onThemeChanged?.Invoke();
                handled = true;
            }else if(msg== WM_DWMCOLORIZATIONCOLORCHANGED)
            {
                _onSystemColorChanged?.Invoke();
                handled = true;
            }
            return IntPtr.Zero;
        }
    }
}
