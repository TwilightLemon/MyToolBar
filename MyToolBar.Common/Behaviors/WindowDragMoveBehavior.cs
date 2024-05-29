using System.Windows;
using Microsoft.Xaml.Behaviors;

namespace MyToolBar.Common.Behaviors
{
    public class WindowDragMoveBehavior : Behavior<Window>
    {
        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.MouseLeftButtonDown += AssociatedObject_MouseDown;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            AssociatedObject.MouseLeftButtonDown -= AssociatedObject_MouseDown;
        }

        private void AssociatedObject_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender is not Window window)
                return;

            window.DragMove();
        }
    }
}
