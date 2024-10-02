using System.Windows;
using System.Windows.Input;
using Microsoft.Xaml.Behaviors;

namespace MyToolBar.Common.Behaviors
{
    public class WindowDragMoveBehavior : Behavior<Window>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.MouseLeftButtonDown += AssociatedObject_MouseLeftButtonDown;
        }

        private void AssociatedObject_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Window w)
            {
                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    w.DragMove();
                }
            }
        }
        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.MouseLeftButtonDown -= AssociatedObject_MouseLeftButtonDown;
        }
    }
}
