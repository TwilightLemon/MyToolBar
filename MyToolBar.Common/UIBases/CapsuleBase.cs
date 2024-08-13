using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace MyToolBar.Common.UIBases
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
        public virtual void Install() { }
        public virtual void Uninstall() { }
        private void CapsuleBase_Initialized(object? sender, EventArgs e)
        {
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
                    _ViewMask.Opacity = 1;
                };
                MouseLeave += delegate
                {
                    _ViewMask.Opacity = 0;
                };
                //插入到最底层
                _Container.Children.Insert(0, _ViewMask);
            }
        }
    }
}
