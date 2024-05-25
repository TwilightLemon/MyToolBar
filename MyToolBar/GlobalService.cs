using Microsoft.Win32;
using System.Timers;
using System.Windows.Media;

namespace MyToolBar
{
    internal static class GlobalService
    {
        public static bool DarkMode = true;
        public static Timer GlobalTimer = null;
        public static Brush OutterControlNormalDarkModeForeColor= new SolidColorBrush(Color.FromArgb(250, 3, 3, 3));
    }
}
