using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Microsoft.Xaml.Behaviors;

namespace MyToolBar.Common.Behaviors
{
    public class DraggableUIBehavior(UIElement parent, UIElement moveReg) : Behavior<UIElement>
    {
        private bool _isDragging = false;
        private Point _startPoint;
        private TranslateTransform _transform = new TranslateTransform();

        private readonly UIElement _parent = parent, _moveReg = moveReg;
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.RenderTransform = _transform;
            _moveReg.MouseLeftButtonDown += AssociatedObject_MouseLeftButtonDown;
            _moveReg.MouseMove += AssociatedObject_MouseMove;
            _moveReg.MouseLeftButtonUp += AssociatedObject_MouseLeftButtonUp;
        }
        protected override void OnDetaching()
        {
            base.OnDetaching();
            _moveReg.MouseLeftButtonDown -= AssociatedObject_MouseLeftButtonDown;
            _moveReg.MouseMove -= AssociatedObject_MouseMove;
            _moveReg.MouseLeftButtonUp -= AssociatedObject_MouseLeftButtonUp;
        }

        private void AssociatedObject_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            _isDragging = false;
            _moveReg.ReleaseMouseCapture();
        }

        private void AssociatedObject_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (_isDragging)
            {
                Point curPoint=e.GetPosition(_parent);
                _transform.X += curPoint.X - _startPoint.X;
                _transform.Y += curPoint.Y - _startPoint.Y;
                _startPoint=curPoint;
            }
        }

        private void AssociatedObject_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            _isDragging = true;
            _startPoint = e.GetPosition(_parent);
            _moveReg.CaptureMouse();
        }

    }
}
