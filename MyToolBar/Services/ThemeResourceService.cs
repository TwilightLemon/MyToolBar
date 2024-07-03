using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using MyToolBar.Common;

namespace MyToolBar.Services
{
    /// <summary>
    /// 提供主题资源切换服务 ThemeMode为全局主题模式，AppBarFontColor为AppBar字体颜色
    /// AppBar字体颜色在沉浸模式下需要根据背景切换
    /// </summary>
    public class ThemeResourceService
    {
        /// <summary>
        /// 全局主题模式 Dark为深色，Light为浅色
        /// </summary>
        /// <param name="IsDark">是否为深色模式</param>
        public void SetThemeMode(bool IsDark)
        {
            GlobalService.IsDarkMode = IsDark;
            string uri ="pack://application:,,,/MyToolBar.Common;component/Styles/"+( IsDark ? "ThemeColor.xaml" : "ThemeColor_Light.xaml");
            // 移除当前主题资源字典（如果存在）
            var oldDict = App.Current.Resources.MergedDictionaries.FirstOrDefault(d => d.Source != null && (d.Source.OriginalString.Contains("ThemeColor.xaml") || d.Source.OriginalString.Contains("ThemeColor_Light.xaml")));
            if (oldDict != null)
            {
                App.Current.Resources.MergedDictionaries.Remove(oldDict);
            }

            // 添加新的主题资源字典
            App.Current.Resources.MergedDictionaries.Add(new ResourceDictionary() { Source = new Uri(uri, UriKind.Absolute) });
        }

        public void SetAppBarFontColor(bool IsDark)
        {
            App.Current.Resources["AppBarFontColor"] = new SolidColorBrush(
                IsDark?Color.FromRgb(0x0E, 0x0E, 0x0E) : Color.FromRgb(0xFE, 0xFE, 0xFE)
                );
        }
    }
}
