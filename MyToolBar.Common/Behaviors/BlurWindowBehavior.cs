using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Microsoft.Win32;
using Microsoft.Xaml.Behaviors;
using MyToolBar.Common.WinAPI;

namespace MyToolBar.Common.Behaviors
{

    public class BlurWindowBehavior : Behavior<Window>
    {
        static readonly Dictionary<Window, WindowAccentCompositor> s_allWindowsAccentCompositors = new();
        public bool IsToolWindow { get; set; } = false;

        static BlurWindowBehavior()
        {
            SystemEvents.UserPreferenceChanged += SystemEvents_UserPreferenceChanged;
        }

        private WindowAccentCompositor? _windowAccentCompositor;

        private static void SystemEvents_UserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
        {
            var isDarkMode = !ToolWindowAPI.GetIsLightTheme();

            foreach (var wac in s_allWindowsAccentCompositors.Values)
            {
                UpdateWindowBlurMode(wac, isDarkMode);
            }
        }

        private static void UpdateWindowBlurMode(WindowAccentCompositor wac, bool isDarkMode, float opacity = .7f)
        {
            wac.DarkMode = isDarkMode;
            wac.Color = isDarkMode ?
                Color.FromScRgb(opacity, 0, 0, 0) :
                Color.FromScRgb(opacity, 1, 1, 1);
            wac.IsEnabled = true;
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.Closing += AssociatedObject_Closing;
            if (GlobalService.IsPowerModeOn)
            {
                AssociatedObject.SetResourceReference(Window.BackgroundProperty, "BackgroundColor");
            }
            else
            {
                if (AssociatedObject.IsLoaded)
                {
                    InitializeBehavior();
                }
                else
                {
                    AssociatedObject.Loaded += AssociatedObject_Loaded;
                }
            }
        }

        private void AssociatedObject_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            //Detach this behavior:
            Interaction.GetBehaviors(AssociatedObject).Remove(this);
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
            var isDarkMode = !ToolWindowAPI.GetIsLightTheme();
            var wac = new WindowAccentCompositor(
                AssociatedObject, IsToolWindow, (c) =>
                {
                    c.A = 255;
                    AssociatedObject.Background = new SolidColorBrush(c);
                });

            UpdateWindowBlurMode(wac, isDarkMode, Opacity);
            wac.IsEnabled = true;

            return wac;
        }

        private void AssociatedObject_Loaded(object sender, RoutedEventArgs e)
        {
            InitializeBehavior();
        }


        private static void OpacityChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not BlurWindowBehavior behavior ||
                e.NewValue is not float opacity)
                return;

            if (behavior._windowAccentCompositor is null)
                return;

            var color = behavior.WindowAccentCompositor.Color;
            color.ScA = opacity;

            behavior.WindowAccentCompositor.Color = color;
            behavior.WindowAccentCompositor.IsEnabled = true;
        }


        public float Opacity
        {
            get { return (float)GetValue(OpacityProperty); }
            set { SetValue(OpacityProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Opacity.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty OpacityProperty =
            DependencyProperty.Register(nameof(Opacity), typeof(float), typeof(BlurWindowBehavior), new PropertyMetadata(0.6f, OpacityChangedCallback));

        public WindowAccentCompositor WindowAccentCompositor => _windowAccentCompositor ?? throw new InvalidOperationException("Window is not loaded");
    }
}
