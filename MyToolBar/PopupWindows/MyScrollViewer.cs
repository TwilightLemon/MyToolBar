using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace MyToolBar.PopupWindows
{
    public static class ScrollViewerBehavior
    {
        public static readonly DependencyProperty VerticalOffsetProperty = DependencyProperty.RegisterAttached("VerticalOffset", typeof(double), typeof(ScrollViewerBehavior), new UIPropertyMetadata(0.0, OnVerticalOffsetChanged));
        public static void SetVerticalOffset(FrameworkElement target, double value) => target.SetValue(VerticalOffsetProperty, value);
        public static double GetVerticalOffset(FrameworkElement target) => (double)target.GetValue(VerticalOffsetProperty);
        private static void OnVerticalOffsetChanged(DependencyObject target, DependencyPropertyChangedEventArgs e) => (target as ScrollViewer)?.ScrollToVerticalOffset((double)e.NewValue);
    }

    public class MyScrollViewer : ScrollViewer
    {
        public MyScrollViewer()
        {
            this.FocusVisualStyle = null;
            this.PreviewMouseUp += MyScrollView_PreviewMouseUp;
            this.PanningMode = PanningMode.VerticalOnly;
            this.SnapsToDevicePixels = true;
        }

        private void MyScrollView_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            //响应鼠标操作:  手动滑动滚动条的时候更新位置
            LastLocation = VerticalOffset;
        }

        public double LastLocation = 0;
        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
                double newOffset = LastLocation - e.Delta;
                ScrollToVerticalOffset(LastLocation);
                if (newOffset < 0)
                    newOffset = 0;
                if (newOffset > ScrollableHeight)
                    newOffset = ScrollableHeight;
                AnimateScroll(newOffset);
                LastLocation = newOffset;
                e.Handled = true;
        }

        private void AnimateScroll(double ToValue)
        {
            BeginAnimation(ScrollViewerBehavior.VerticalOffsetProperty, null);
            DoubleAnimation Animation = new DoubleAnimation();
            Animation.EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseOut };
            Animation.From = VerticalOffset;
            Animation.To = ToValue;
            Animation.Duration = TimeSpan.FromMilliseconds(300);
            //Timeline.SetDesiredFrameRate(Animation, 40);
            BeginAnimation(ScrollViewerBehavior.VerticalOffsetProperty, Animation);
        }
    }
}
