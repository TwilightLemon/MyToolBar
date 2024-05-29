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
    /// 为可交互Item提供基类 ViewMask 基本视觉样式
    /// </summary>
    public class ItemBase:UserControl
    {
        public ItemBase()
        {
            Initialized += ItemBase_Initialized;
        }
        protected Grid? _Container;
        protected Border? _ViewMask;
        private void ItemBase_Initialized(object? sender, EventArgs e)
        {
            if(Content is Grid g)
            {
                _Container = g;
                _ViewMask = new Border()
                {
                    CornerRadius=new CornerRadius(15),
                    Opacity=0
                };
                _ViewMask.SetResourceReference(BackgroundProperty, "MaskColor");
                MouseEnter += delegate {
                    _ViewMask.BeginAnimation(OpacityProperty, new DoubleAnimation(0, 0.5, TimeSpan.FromMilliseconds(300)));
                };
                MouseLeave += delegate
                {
                    _ViewMask.BeginAnimation(OpacityProperty, new DoubleAnimation(0.5, 0, TimeSpan.FromMilliseconds(300)));
                };
                //插入到最底层
                _Container.Children.Insert(0, _ViewMask);
            }
        }
    }
}
