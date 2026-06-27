using MyToolBar.Common.WinAPI;
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace MyToolBar.Views.Items
{
    /// <summary>
    /// Window control panel with Minimize, Maximize/Restore, and Close buttons.
    /// Operates on a target window handle set by the host before showing.
    /// </summary>
    public partial class WindowControlPanel : UserControl
    {
        // WinAPI for window operations
        private const int SW_MINIMIZE = 6;
        private const int SW_RESTORE = 9;
        private const int SW_MAXIMIZE = 3;
        private const int WM_CLOSE = 0x0010;

        [DllImport("user32.dll")]
        private static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        private static extern IntPtr PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool IsWindow(IntPtr hWnd);

        /// <summary>
        /// 目标窗口句柄，在面板显示前由宿主设置，避免点击时前台窗口切换为 AppBarWindow 自身
        /// </summary>
        private IntPtr _targetWindowHandle;

        public WindowControlPanel()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 设置目标窗口并刷新按钮图标状态
        /// </summary>
        public void SetTarget(IntPtr hWnd, bool isMaximized)
        {
            _targetWindowHandle = hWnd;
            RefreshState(isMaximized);
        }

        /// <summary>
        /// 清除目标窗口句柄
        /// </summary>
        public void ClearTarget()
        {
            _targetWindowHandle = IntPtr.Zero;
        }

        /// <summary>
        /// 根据最大化状态刷新 Maximize/Restore 按钮图标
        /// </summary>
        private void RefreshState(bool isMaximized)
        {
            MaxRestoreIcon.Text = isMaximized ? "\uE923" : "\uE922";

            MaxRestoreButton.ToolTip = isMaximized
                ? FindResource("WindowControl_Restore")
                : FindResource("WindowControl_Maximize");
        }

        private void MinimizeButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_targetWindowHandle != IntPtr.Zero && IsWindow(_targetWindowHandle))
                ShowWindowAsync(_targetWindowHandle, SW_MINIMIZE);
        }

        private void MaxRestoreButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_targetWindowHandle == IntPtr.Zero || !IsWindow(_targetWindowHandle))
                return;

            if (_targetWindowHandle.IsZoomedWindow())
                ShowWindowAsync(_targetWindowHandle, SW_RESTORE);
            else
                ShowWindowAsync(_targetWindowHandle, SW_MAXIMIZE);
        }

        private void CloseButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_targetWindowHandle != IntPtr.Zero && IsWindow(_targetWindowHandle))
                PostMessage(_targetWindowHandle, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
        }

        private void Button_MouseEnter(object sender, MouseEventArgs e)
        {
            if (sender is Border border)
            {
                border.Background = new SolidColorBrush(
                    border == CloseButton
                        ? Color.FromArgb(0x40, 0xFF, 0x45, 0x45)   // red tint for close
                        : Color.FromArgb(0x20, 0xFF, 0xFF, 0xFF));  // light tint for others
            }
        }

        private void Button_MouseLeave(object sender, MouseEventArgs e)
        {
            if (sender is Border border)
            {
                border.Background = new SolidColorBrush(Color.FromArgb(0x01, 0x00, 0x00, 0x00));
            }
        }
    }
}
