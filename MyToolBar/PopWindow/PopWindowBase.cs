using MyToolBar.WinApi;
using System;
using System.Windows;
using static MyToolBar.GlobalService;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shell;

namespace MyToolBar.PopWindow
{
    public class PopWindowBase:Window
    {
        public PopWindowBase()
        {
            //Set basic style for popwindow
            SetResourceReference(BackgroundProperty,"MaskColor");
            SetResourceReference(ForegroundProperty, "ForeColor");
            WindowStyle = WindowStyle.None;
            ShowInTaskbar = false;
            Topmost = true;
            WindowChrome.SetWindowChrome(this, new WindowChrome()
            {
                GlassFrameThickness = new Thickness(1),
                CaptionHeight = 1
            });

            //behavior
            Activate();
            Top = -1 * Height;
            Deactivated += PopWindowBase_Deactivated;
            Loaded += PopWindowBase_Loaded;
        }

        private void PopWindowBase_Loaded(object sender, RoutedEventArgs e)
        {
            ToolWindowApi.SetToolWindow(this);
            WindowAccentCompositor wac = new(this, false, (c) =>
            {
                c.A = 255;
                Background = new SolidColorBrush(c);
            });
            wac.Color = DarkMode ?
            Color.FromArgb(180, 0, 0, 0) :
            Color.FromArgb(180, 255, 255, 255);
            wac.DarkMode = DarkMode;
            wac.IsEnabled = true;

            var da = new DoubleAnimation(-1 * this.Height, 30, TimeSpan.FromMilliseconds(300));
            da.EasingFunction = new CubicEase();
            BeginAnimation(TopProperty, da);
        }

        private void PopWindowBase_Deactivated(object? sender, EventArgs e)
        {
            var da = new DoubleAnimation(this.Top, -1 * this.Height, TimeSpan.FromMilliseconds(300));
            da.EasingFunction = new CubicEase();
            da.Completed += Da_Completed;
            BeginAnimation(TopProperty, da);
        }
        private void Da_Completed(object? sender, EventArgs e)
        {
            Close();
        }
    }
}
