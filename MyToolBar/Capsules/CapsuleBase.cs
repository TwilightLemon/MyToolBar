using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace MyToolBar.Capsules
{
    /// <summary>
    /// 为Capsule提供基类
    /// </summary>
    public class CapsuleBase : UserControl
    {
        public CapsuleBase()
        {
            Initialized += CapsuleBase_Initialized;
        }
        protected Grid? _Container;
        protected Border? _ViewMask;
        private void CapsuleBase_Initialized(object? sender, EventArgs e)
        {
            Background = GlobalService.CapsuleBackground;
            if (Content is Grid g)
            {
                _Container = g;
                _ViewMask = new Border()
                {
                    CornerRadius = new CornerRadius(12),
                    Opacity = 0
                };
                _ViewMask.SetResourceReference(BackgroundProperty, "MaskColor");
                MouseEnter += delegate
                {
                    _ViewMask.BeginAnimation(OpacityProperty, new DoubleAnimation(0.2, 1, TimeSpan.FromMilliseconds(300)));
                };
                MouseLeave += delegate
                {
                    _ViewMask.BeginAnimation(OpacityProperty, new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(300)));
                };
                //插入到最底层
                _Container.Children.Insert(0, _ViewMask);
            }
        }
    }
}
