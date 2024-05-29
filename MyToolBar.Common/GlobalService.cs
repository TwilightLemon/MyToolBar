using Microsoft.Win32;
using System.Collections.Generic;
using System.Timers;
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
        public static bool IsDarkMode { get; set; } = true;

        public static System.Timers.Timer GlobalTimer = null;

        public static Brush OuterControlNormalDarkModeForeColor= new SolidColorBrush(Color.FromArgb(250, 3, 3, 3));
    }
}
