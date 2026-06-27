using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using MyToolBar.Common;
using MyToolBar.Common.WinAPI;
using MyToolBar.Views.Windows;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace MyToolBar.ViewModels
{
    public record struct MenuItem(string Title, Geometry? Icon, Action? Event);
    public partial class MainTitleMenuViewModel:ObservableObject
    {
        #region Constructor
        public MainTitleMenuViewModel()
        {
            LoadMenuItems();
        }

        void SetItem(string contResName, string? iconResName, Action? Event)
        {
            var item = new MenuItem
            {
                Title = (string)App.Current.FindResource($"MenuItem_{contResName}"),
                Icon = iconResName == null ? null : (Geometry)App.Current.FindResource($"Icon_{iconResName}"),
                Event = Event
            };
            MenuItems.Add(item);
        }
        public ObservableCollection<MenuItem> MenuItems { get; } = [];

        [ObservableProperty]
        private MenuItem? _selectedItem;
        partial void OnSelectedItemChanged(MenuItem? value)
        {
            if(value == null) return;
            value?.Event?.Invoke();
            MenuItems.Clear();
        }

        public double MenuHeight => MenuItems.Count * 50;

        private void LoadMenuItems()
        {
            SetItem("Settings", "Settings", MenuItem_Settings);
            SetItem("Exit", null, MenuItem_Exit);
        }
        #endregion

        static SettingsWindow? _settingsWindow;
        private void MenuItem_Settings()
        {
            if (_settingsWindow != null)
            {
                // 窗口已存在：激活并移动到 AppBarWindow 所在显示器中央
                PositionWindowOnAppBarMonitor(_settingsWindow);
                if (_settingsWindow.WindowState == WindowState.Minimized)
                    _settingsWindow.WindowState = WindowState.Normal;
                _settingsWindow.Activate();
                return;
            }

            _settingsWindow = App.Host.Services.GetRequiredService<SettingsWindow>();
            _settingsWindow.Owner = App.Current.MainWindow;
            _settingsWindow.Closing += delegate {
                _settingsWindow = null;
            };

            // 定位到 AppBarWindow 所在显示器中央
            PositionWindowOnAppBarMonitor(_settingsWindow);
            _settingsWindow.Show();
        }

        /// <summary>
        /// 将窗口定位到 AppBarWindow 所在显示器的工作区域中央
        /// </summary>
        private static void PositionWindowOnAppBarMonitor(Window window)
        {
            var mainWindow = Application.Current.MainWindow;
            if (mainWindow == null) return;

            var hwnd = new WindowInteropHelper(mainWindow).Handle;
            var hmonitor = ScreenAPI.GetHmonitorForHwnd(hwnd);
            var monitorInfo = ScreenAPI.GetMonitorInfoEx(hmonitor);
            if (monitorInfo == null) return;

            var (dpiX, dpiY) = ScreenAPI.GetDPI(hmonitor);

            double monitorLeft = monitorInfo.rcWork.left / dpiX;
            double monitorTop = monitorInfo.rcWork.top / dpiY;
            double monitorWidth = (monitorInfo.rcWork.right - monitorInfo.rcWork.left) / dpiX;
            double monitorHeight = (monitorInfo.rcWork.bottom - monitorInfo.rcWork.top) / dpiY;

            window.Left = monitorLeft + (monitorWidth - window.Width) / 2;
            window.Top = monitorTop + (monitorHeight - window.Height) / 2;
        }
        private void MenuItem_Exit()
        {
            Application.Current.Shutdown();
        }
    }
}
