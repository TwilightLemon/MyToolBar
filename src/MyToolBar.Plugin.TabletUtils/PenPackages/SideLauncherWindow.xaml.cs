using MyToolBar.Common.WinAPI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace MyToolBar.Plugin.TabletUtils.PenPackages
{
    /// <summary>
    /// SideLauncherWindow.xaml 的交互逻辑
    /// </summary>
    public partial class SideLauncherWindow : Window
    {
        public SideLauncherWindow()
        {
            InitializeComponent();
            SourceInitialized += SideTaskViewWindow_SourceInitialized;
            Height = SystemParameters.WorkArea.Height - 30;

            TouchDown += SideTaskViewWindow_TouchDown;
            MouseEnter += SideTaskViewWindow_MouseEnter;
            MouseMove += SideLauncherWindow_MouseMove;
            MouseLeave += SideTaskViewWindow_MouseLeave;

            ClickBd.MouseLeave += ClickBd_MouseLeave;
            ClickBd.MouseDown += ClickBd_MouseDown;
        }

        private void SideTaskViewWindow_MouseLeave(object sender, MouseEventArgs e)
        {
            if (_shouldShowClickBd != null)
            {
                _shouldShowClickBd.Cancel();
                _shouldShowClickBd = null;
            }
        }

        void ShowSideWindow()
        {
            new SideWindow().Show();
        }

        void ShowClickBd()
        {
            var da = new DoubleAnimation(-12, 0, TimeSpan.FromMilliseconds(300));
            da.EasingFunction = new CircleEase();
            ClickBd.Visibility = Visibility.Visible;
            ClickBd.BeginAnimation(Canvas.LeftProperty, da);
        }
        void HideClickBd()
        {
            var da = new DoubleAnimation(0, -12, TimeSpan.FromMilliseconds(300));
            da.EasingFunction = new CircleEase();
            da.Completed += delegate {
                ClickBd.Visibility = Visibility.Collapsed;
            };
            ClickBd.BeginAnimation(Canvas.LeftProperty, da);
        }

        private void ClickBd_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ShowSideWindow();
            HideClickBd();
        }

        private void ClickBd_MouseLeave(object sender, MouseEventArgs e)
        {
            HideClickBd();
        }
        private CancellationTokenSource? _shouldShowClickBd = new();
        private const int MinMouseTargetWidth = 2;
        private Point currentMousePoint;
        private void SideLauncherWindow_MouseMove(object sender, MouseEventArgs e)
        {
            currentMousePoint = e.GetPosition(this);
        }
        private async void SideTaskViewWindow_MouseEnter(object sender, MouseEventArgs e)
        {
            if (e.StylusDevice?.TabletDevice?.Type == TabletDeviceType.Touch) return;
            _shouldShowClickBd = new();
            try
            {
                await Task.Delay(400, _shouldShowClickBd.Token);
            }
            catch { return; }
            double top = currentMousePoint.Y - ClickBd.Height / 2;
            Canvas.SetTop(ClickBd, top);
            ShowClickBd();
            _shouldShowClickBd = null;
        }

        private void SideTaskViewWindow_TouchDown(object? sender, TouchEventArgs e)
        {
            ShowSideWindow();
        }

        private void SideTaskViewWindow_SourceInitialized(object? sender, System.EventArgs e)
        {
            WindowLongAPI.SetToolWindow(this);
        }
    }
}
