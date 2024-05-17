using MyToolBar.WinApi;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace MyToolBar.PenPackages
{
    /// <summary>
    /// DrawboardWindow.xaml 的交互逻辑
    /// </summary>
    public partial class DrawboardWindow : Window
    {
        public DrawboardWindow()
        {
            InitializeComponent();
            this.Loaded += DrawboardWindow_Loaded;
            this.PreviewStylusInRange += DrawboardWindow_PreviewStylusInRange;
            this.PreviewStylusOutOfRange += DrawboardWindow_PreviewStylusOutOfRange;
            this.PreviewMouseDoubleClick += DrawboardWindow_PreviewMouseDoubleClick;
        }
        private bool _isDrawingMode = false;
        private void DrawboardWindow_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ink.IsEnabled =_isDrawingMode= false;
            SolidColorBrush bg = new SolidColorBrush();
            Background = bg;
            var ca = new ColorAnimation(Color.FromArgb(20, 0, 0, 0), Color.FromArgb(0, 0, 0, 0), TimeSpan.FromMilliseconds(400));
            ca.Completed += delegate
            {
                Background = null;
                if (_CurrentPen != null)
                {
                    _CurrentPen.BorderThickness = new Thickness(0);
                    _CurrentPen = null;
                }
            };
            bg.BeginAnimation(SolidColorBrush.ColorProperty, ca);
        }

        private void DrawboardWindow_PreviewStylusOutOfRange(object sender, StylusEventArgs e)
        {
            ink.IsEnabled = false;
        }

        private void DrawboardWindow_PreviewStylusInRange(object sender, StylusEventArgs e)
        {
            if (e.StylusDevice.TabletDevice.Type == TabletDeviceType.Stylus)
            {
                ink.IsEnabled=_isDrawingMode = true;
            }
        }

        private void DrawboardWindow_Loaded(object sender, RoutedEventArgs e)
        {
            ToolWindowApi.SetToolWindow(this);
            Topmost = true;
            this.Width = SystemParameters.WorkArea.Width;
            this.Height = SystemParameters.WorkArea.Height;
            this.Left = 0;
            this.Top = 0;

            SolidColorBrush seletedColor = new SolidColorBrush(GlobalService.DarkMode ? Colors.White : Colors.Black);
            foreach (Border item in PenColors.Children)
            {
                item.MouseUp += PenColor_MouseUp;
                item.BorderBrush = seletedColor;
            }
            PenColor_MouseUp(PenColors.Children[GlobalService.DarkMode ? 1 : 0], null);
        }
        private Border _CurrentPen = null;
        private void PenColor_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Border b = (Border)sender;
            if (_CurrentPen != null)
            {
                _CurrentPen.BorderThickness = new Thickness(0);
            }
            b.BorderThickness = new Thickness(2);

            _CurrentPen = b;
            if (!_isDrawingMode)
            {
                SolidColorBrush bg = new SolidColorBrush();
                Background = bg;
                bg.BeginAnimation(SolidColorBrush.ColorProperty, new ColorAnimation(Color.FromArgb(0, 0, 0, 0), Color.FromArgb(20, 0, 0, 0), TimeSpan.FromMilliseconds(400)));
            }
            else {
                Background = new SolidColorBrush(Color.FromArgb(20,0,0,0));
            }
            ink.IsEnabled = _isDrawingMode = true;
            ink.DefaultDrawingAttributes.Color = ((SolidColorBrush)_CurrentPen.Background).Color;
        }

        private void Border_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Close();
        }
    }
}
