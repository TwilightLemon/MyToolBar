using MyToolBar.Common.WinAPI;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace MyToolBar.Common
{
    /// <summary>
    /// 为整个AppDom提供常量集
    /// </summary>
    public static class GlobalService
    {
        /// <summary>
        /// 有窗口进入全屏时是否隐藏AppBar
        /// </summary>
        public static bool EnableHideWhenFullScreen { get; set; } = true;
        private static bool _isDarkMode = true;
        public static event Action<bool>? OnIsDarkModeChanged;
        /// <summary>
        /// 全局主题模式
        /// </summary>
        public static bool IsDarkMode
        {
            get => _isDarkMode;
            set
            {
                _isDarkMode = value;
                OnIsDarkModeChanged?.Invoke(value);
            }
        }
        /// <summary>
        /// 公共Timer
        /// </summary>
        public static System.Timers.Timer GlobalTimer = null;

        public static bool IsEnergySaverModeOn { get; private set; } = false;
        public static event Action<bool>? OnEnergySaverModeChanged;
        public static void SetEnergySaverMode(bool isOn)
        {
            if (isOn == IsEnergySaverModeOn) return;
            IsEnergySaverModeOn = isOn;
            OnEnergySaverModeChanged?.Invoke(isOn);
        }

        /// <summary>
        /// 获取PopupWindow相对于capsule弹出时的Left值
        /// </summary>
        /// <param name="capsule"></param>
        /// <param name="popupWindow"></param>
        /// <returns></returns>
        public static double GetPopupWindowLeft(FrameworkElement capsule,Window popupWindow)
        {
            //获取相对于MainWindow的坐标
            double rel=capsule.TransformToAncestor(Application.Current.MainWindow).Transform(new Point(0, 0)).X;
            //计算PopupWindow的左边界
            double left=rel+capsule.ActualWidth/2 - popupWindow.Width/2;
            //防止超出屏幕
            IntPtr hwnd = new WindowInteropHelper(popupWindow).Handle;
            double max=ScreenAPI.GetScreenArea(hwnd).Width- popupWindow.Width;
            if(left < 0) left = 0;
            else if(left > max) left = max;
            return left;
        }
    }
}
