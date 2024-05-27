using Microsoft.Win32;
using MyToolBar.Func;
using System.Collections.Generic;
using System.Timers;
using System.Windows.Media;

namespace MyToolBar
{
    internal static class GlobalService
    {
        /// <summary>
        /// 不知道为什么Transparent无法点击，所以用了一个很小的透明度
        /// </summary>
        internal static readonly Brush CapsuleBackground= new SolidColorBrush(Color.FromArgb(1, 0, 0, 0));
        public static bool DarkMode = true;
        public static Timer GlobalTimer = null;
        public static Brush OutterControlNormalDarkModeForeColor= new SolidColorBrush(Color.FromArgb(250, 3, 3, 3));

        internal static List<string> ManagedSettingsKey = new List<string>();
    }
}
