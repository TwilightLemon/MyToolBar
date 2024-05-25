using MyToolBar.WinApi;
using System;
using System.Windows;
using static MyToolBar.GlobalService;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shell;
using System.Windows.Interop;
using System.Windows.Controls;

namespace MyToolBar.PopWindow
{
    public class PopWindowBase:Window
    {
        public PopWindowBase()
        {
            Top = 0;
            this.IsEnabled = false;
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
            Activate();
            Deactivated += PopWindowBase_Deactivated;
            this.Loaded += PopWindowBase_Loaded;
            this.ContentRendered += PopWindowBase_ContentRendered;
        }

        private void PopWindowBase_ContentRendered(object? sender, EventArgs e)
        {
            var da = new DoubleAnimation(0, 35, TimeSpan.FromMilliseconds(200));
            da.EasingFunction = new CubicEase();
            da.Completed += (s, e) => this.IsEnabled = true;
            BeginAnimation(TopProperty, da);
            this.ContentRendered -= PopWindowBase_ContentRendered;
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
        }

        private void PopWindowBase_Deactivated(object? sender, EventArgs e)
        {
            this.IsEnabled = false;
            var da = new DoubleAnimation(this.Top, -1 * this.Height, TimeSpan.FromMilliseconds(300));
            da.EasingFunction = new CubicEase();
            da.Completed += Da_Completed;
            BeginAnimation(TopProperty, da);
        }
        private void Da_Completed(object? sender, EventArgs e)
        {
            Close();
        }

        private LoadingIcon? _loadingIcon = null;
        public void SetLoadingStatus(bool isLoading)
        {
            if(Content is Grid container)
            {
                if (isLoading)
                {
                    _loadingIcon = new LoadingIcon();
                    container.Children.Add(_loadingIcon);
                }
                else if(_loadingIcon != null)
                {
                    container.Children.Remove(_loadingIcon);
                }
            }
        }
    }
}
