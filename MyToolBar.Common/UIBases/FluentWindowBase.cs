using System.Windows;
using EleCho.WpfSuite.Controls;
using EleCho.WpfSuite;
using MyToolBar.Common.Behaviors;
using Microsoft.Xaml.Behaviors;
using System.Windows.Shell;
using MyToolBar.Common.WinAPI;
using System.Windows.Controls;
using wsButton = EleCho.WpfSuite.Controls.Button;


namespace MyToolBar.Common.UIBases
{
    public class FluentWindowBase : Window
    {
        private  wsButton CloseBtn, MaxmizeBtn, MinimizeBtn;
        private readonly BehaviorCollection _behaviors;
        private readonly BlurWindowBehavior _blurBehavior;
        private readonly WindowChrome _windowChrome;
        public FluentWindowBase()
        {
            Style = (Style)FindResource("FluentWindowStyle");
            //ResizeMode WindowStyle

            WindowOption.SetCorner(this, WindowCorner.Round);
            WindowLongAPI.SetDwmAnimation(this, true);

            _behaviors = Interaction.GetBehaviors(this);
            _behaviors.Add(new WindowDragMoveBehavior());
            _windowChrome = new()
            {
                ResizeBorderThickness=new Thickness(8)
            };
            _blurBehavior = new()
            {
                WindowChromeEx = _windowChrome
            };
            _behaviors.Add(_blurBehavior);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            CloseBtn = (wsButton)Template.FindName("CloseBtn", this);
            MaxmizeBtn = (wsButton)Template.FindName("MaxmizeBtn", this);
            MinimizeBtn = (wsButton)Template.FindName("MinimizeBtn", this);

            if (CloseBtn == null || MaxmizeBtn == null || MinimizeBtn == null)
                throw new NullReferenceException("!!!");

            CloseBtn.Click += CloseBtn_Click;
            MaxmizeBtn.Click += MaxmizeBtn_Click;
            MinimizeBtn.Click += MinimizeBtn_Click;

            if (ResizeMode == ResizeMode.NoResize)
            {
                MaxmizeBtn.Visibility = Visibility.Collapsed;
                MinimizeBtn.Visibility = Visibility.Collapsed;
            }
            else if (ResizeMode == ResizeMode.CanMinimize)
            {
                MaxmizeBtn.IsEnabled = false;
                MaxmizeBtn.SetResourceReference(ForegroundProperty, "FocusMaskColor");
                MaxmizeBtn.Click -= MaxmizeBtn_Click;
            }
            if (ResizeMode != ResizeMode.CanResize || ResizeMode != ResizeMode.CanResizeWithGrip)
            {
                _windowChrome.ResizeBorderThickness = new Thickness(0);
                _blurBehavior.WindowChromeEx = _windowChrome;
            }
        }


        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void MaxmizeBtn_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState == WindowState.Normal ? WindowState.Maximized : WindowState.Normal;
        }

        private void MinimizeBtn_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }
    }
}
