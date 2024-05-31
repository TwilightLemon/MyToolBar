using Microsoft.Win32;
using System.Collections.Generic;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MyToolBar.Common
{
    /// <summary>
    /// 为整个AppDom提供常量集
    /// </summary>
    public static class GlobalService
    {
        /// <summary>
        /// 不知道为什么Transparent无法点击，所以用了一个很小的透明度
        /// </summary>
        public static readonly Brush CapsuleBackground= new SolidColorBrush(Color.FromArgb(1, 0, 0, 0));
        /// <summary>
        /// 指示前台窗口状态，用于适配OuterControl的高亮UI，0为正常，1为最大化
        /// </summary>
        public static  int CurrentWindowStyle = 0;
        public static bool IsDarkMode { get; set; } = true;
        /// <summary>
        /// 公共Timer
        /// </summary>

        public static System.Timers.Timer GlobalTimer = null;

        public static Brush OuterControlNormalDarkModeForeColor= new SolidColorBrush(Color.FromArgb(250, 3, 3, 3));
        public static double GetPopupWindowLeft(FrameworkElement capsule,Window popupWindow)
        {
            //获取相对于MainWindow的坐标
            double rel=capsule.TransformToAncestor(Application.Current.MainWindow).Transform(new Point(0, 0)).X;
            //计算PopupWindow的左边界
            double left=rel+capsule.ActualWidth/2 - popupWindow.Width/2;
            //防止超出屏幕
            double max=SystemParameters.WorkArea.Width- popupWindow.Width;
            if(left < 0) left = 0;
            else if(left > max) left = max;
            return left;
        }
    }
}
