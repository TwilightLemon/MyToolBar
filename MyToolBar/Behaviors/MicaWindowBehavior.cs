using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Microsoft.Win32;
using Microsoft.Xaml.Behaviors;
using MyToolBar.WinApi;

namespace MyToolBar.Behaviors
{

    public class MicaWindowBehavior : Behavior<Window>
    {
        static readonly Dictionary<Window, WindowAccentCompositor> s_allWindowsAccentCompositors = new();

        static MicaWindowBehavior()
        {
            SystemEvents.UserPreferenceChanged += SystemEvents_UserPreferenceChanged;
        }

        private static void SystemEvents_UserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
        {
            var isDarkMode = !ToolWindowApi.GetIsLightTheme();

            foreach (var wac in s_allWindowsAccentCompositors.Values)
            {
                UpdateWindowBlurMode(wac, isDarkMode);
            }
        }

        private static void UpdateWindowBlurMode(WindowAccentCompositor wac, bool isDarkMode, byte opacity = 180)
        {
            wac.Color = isDarkMode ?
                Color.FromArgb(opacity, 0, 0, 0) :
                Color.FromArgb(opacity, 255, 255, 255);
            wac.DarkMode = isDarkMode;
        }

        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.Loaded += AssociatedObject_Loaded;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            if (s_allWindowsAccentCompositors.TryGetValue(AssociatedObject, out var acc))
            {
                s_allWindowsAccentCompositors.Remove(AssociatedObject);

                acc.IsEnabled = false;
            }
        }

        private void AssociatedObject_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is not Window window)
                return;

            WindowAccentCompositor wac = new(AssociatedObject, false, (c) =>
            {
                c.A = 255;
                AssociatedObject.Background = new SolidColorBrush(c);
            });

            wac.Color = GlobalService.IsDarkMode ?
                Color.FromArgb(180, 0, 0, 0) :
                Color.FromArgb(180, 255, 255, 255);

            wac.DarkMode = GlobalService.IsDarkMode;
            wac.IsEnabled = true;

            s_allWindowsAccentCompositors[window] = wac;
        }
    }
}
