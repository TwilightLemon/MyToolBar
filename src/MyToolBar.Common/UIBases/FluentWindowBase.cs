using System.Windows;
using EleCho.WpfSuite;
using MyToolBar.Common.Behaviors;
using Microsoft.Xaml.Behaviors;
using System.Windows.Shell;
using MyToolBar.Common.WinAPI;
using wsButton = EleCho.WpfSuite.Controls.Button;
using System.ComponentModel;

namespace MyToolBar.Common.UIBases
{
    /// <summary>
    /// 提供含有标题栏的FluentWindow样式基类
    /// </summary>
    public class FluentWindowBase : Window
    {
        private  wsButton CloseBtn, MaxmizeBtn, MinimizeBtn;
        private readonly BehaviorCollection _behaviors;
        private readonly BlurWindowBehavior _blurBehavior;
        private readonly WindowChrome _windowChrome;

        private readonly int _captionHeight = 48;
        private readonly Thickness _resizeBorderThickness = new(12);

        public FluentWindowBase()
        {
            Style = (Style)FindResource("FluentWindowStyle");

            WindowOption.SetCorner(this, WindowCorner.Round);
            WindowLongAPI.SetDwmAnimation(this, true);

            _behaviors = Interaction.GetBehaviors(this);
            _behaviors.Add(new WindowDragMoveBehavior());
            _windowChrome = new()
            {
                CaptionHeight=_captionHeight,
                ResizeBorderThickness=_resizeBorderThickness
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

            //接管ResizeMode属性
            ApplyResizeMode();
            DependencyPropertyDescriptor.FromProperty(ResizeModeProperty, typeof(FluentWindowBase))
                .AddValueChanged(this, ResizeModeChanged);
        }
        private void ApplyResizeMode()
        {
            bool allShown= ResizeMode != ResizeMode.NoResize;
            MinimizeBtn.Visibility = MaxmizeBtn.Visibility = allShown ? Visibility.Visible : Visibility.Collapsed;
            MaxmizeBtn.IsEnabled = ResizeMode!= ResizeMode.CanMinimize;
            MaxmizeBtn.SetResourceReference(ForegroundProperty, MaxmizeBtn.IsEnabled ? "ForeColor" : "FocusMaskColor");

            _windowChrome.ResizeBorderThickness =
                (ResizeMode != ResizeMode.CanResize && ResizeMode != ResizeMode.CanResizeWithGrip)
                ? default : _resizeBorderThickness;
            _blurBehavior.WindowChromeEx = _windowChrome;
        }

        private void ResizeModeChanged(object? sender, EventArgs e)
        {
            ApplyResizeMode();
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