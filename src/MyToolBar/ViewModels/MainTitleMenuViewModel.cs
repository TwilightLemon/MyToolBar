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
                // 窗口已存在（HWND 已就绪）：直接激活并定位
                PositionWindowOnAppBarMonitor(_settingsWindow);
                if (_settingsWindow.WindowState == WindowState.Minimized)
                    _settingsWindow.WindowState = WindowState.Normal;
                _settingsWindow.Activate();
                return;
            }

            _settingsWindow = App.Host.Services.GetRequiredService<SettingsWindow>();
            _settingsWindow.Closing += delegate {
                _settingsWindow = null;
            };

            // 新窗口：在 SourceInitialized 后定位，确保 HWND 已创建
            _settingsWindow.SourceInitialized += OnSettingsWindowSourceInitialized;
            _settingsWindow.Show();
        }

        private static void OnSettingsWindowSourceInitialized(object? sender, EventArgs e)
        {
            if (sender is not Window window) return;
            window.SourceInitialized -= OnSettingsWindowSourceInitialized;
            PositionWindowOnAppBarMonitor(window);
        }

        /// <summary>
        /// 使用 SetWindowPos 将窗口直接定位到 AppBarWindow 所在显示器的 work area 中央（物理像素，无 DPI 问题）
        /// </summary>
        private static void PositionWindowOnAppBarMonitor(Window window)
        {
            var mainWindow = Application.Current.MainWindow;
            if (mainWindow == null) return;

            var mainHwnd = new WindowInteropHelper(mainWindow).Handle;
            var targetMonitor = ScreenAPI.GetHmonitorForHwnd(mainHwnd);

            var windowHwnd = new WindowInteropHelper(window).Handle;
            ScreenAPI.CenterWindowOnMonitor(windowHwnd, targetMonitor, window.Width, window.Height);
        }
        private void MenuItem_Exit()
        {
            Application.Current.Shutdown();
        }
    }
}
