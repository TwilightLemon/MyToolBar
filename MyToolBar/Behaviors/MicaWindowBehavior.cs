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

        private WindowAccentCompositor? _windowAccentCompositor;

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

            if (AssociatedObject.IsLoaded)
            {
                InitializeBehavior();
            }
            else
            {
                AssociatedObject.Loaded += AssociatedObject_Loaded;
            }
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            // unregister event
            AssociatedObject.Loaded -= AssociatedObject_Loaded;

            // remove and disable wac
            if (s_allWindowsAccentCompositors.TryGetValue(AssociatedObject, out var acc))
            {
                s_allWindowsAccentCompositors.Remove(AssociatedObject);

                acc.IsEnabled = false;
            }
        }

        private void InitializeBehavior()
        {
            s_allWindowsAccentCompositors[AssociatedObject] 
                = _windowAccentCompositor 
                = CreateWindowAccentCompositor();
        }

        private WindowAccentCompositor CreateWindowAccentCompositor()
        {
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

            return wac;
        }

        private void AssociatedObject_Loaded(object sender, RoutedEventArgs e)
        {
            InitializeBehavior();
        }

        public WindowAccentCompositor WindowAccentCompositor => _windowAccentCompositor ?? throw new InvalidOperationException("Window is not loaded");
    }
}
