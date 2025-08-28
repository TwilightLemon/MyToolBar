using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using MyToolBar.Common;
using MyToolBar.Common.WinAPI;

namespace MyToolBar.Services
{
    /// <summary>
    /// 提供主题资源切换服务 ThemeMode为全局主题模式，AppBarFontColor为AppBar字体颜色
    /// AppBar字体颜色在沉浸模式下需要根据背景切换
    /// </summary>
    public class UIResourceService
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

        public void UpdateDwmColor()
        {
            App.Current.Resources["SystemThemeColor"] = new SolidColorBrush(SystemThemeAPI.GetSystemAccentColor(GlobalService.IsDarkMode));
            GlobalService.NotifyThemeColorChanged();
        }

        /// <summary>
        /// 单独为AppBar配置前景色
        /// </summary>
        public void SetAppBarFontColor(bool left,bool center,bool right)
        {
            void set(string pos,bool isDark)
            {
                App.Current.Resources["AppBarForeground"+pos] = new SolidColorBrush(
                    isDark ? Color.FromRgb(0x0E, 0x0E, 0x0E) : Color.FromRgb(0xFE, 0xFE, 0xFE)
                    );
            }
            set("Left", left);
            set("Center", center);
            set("Right", right);
        }
        public void SetAppBarFontColor(bool isDark)
        {
            void set(string pos)
            {
                App.Current.Resources["AppBarForeground" + pos] = new SolidColorBrush(
                    isDark ? Color.FromRgb(0x0E, 0x0E, 0x0E) : Color.FromRgb(0xFE, 0xFE, 0xFE)
                    );
            }
            set("Left");
            set("Center");
            set("Right");
        }

        /// <summary>
        /// 设置主程序的语言资源
        /// </summary>
        /// <param name="lang"></param>
        public void SetLanguage(LocalCulture.Language lang)
        {
            string uri = $"/LanguageRes/Lang{
                lang switch { 
                    LocalCulture.Language.en_us=> "En_US",
                    LocalCulture.Language.zh_cn=> "Zh_CN",
                    _=>throw new Exception("Unsupported Language")
                }
                }.xaml";
            // 移除当前语言资源字典（如果存在）
            var oldDict = App.Current.Resources.MergedDictionaries.FirstOrDefault(d => d.Source != null && d.Source.OriginalString.Contains("LanguageRes/Lang"));
            if (oldDict != null) {
                App.Current.Resources.MergedDictionaries.Remove(oldDict);
            }
            // 添加新的语言资源字典
            App.Current.Resources.MergedDictionaries.Add(new ResourceDictionary() { Source = new Uri(uri, UriKind.Relative) });
        }

        public static void SetAppFontFamilly(string? font)
        {
            if (string.IsNullOrEmpty(font)) return;

            if(App.Current.Resources.MergedDictionaries.FirstOrDefault(d => d.Source.OriginalString.Contains("Styles/UITemplate.xaml")) is { } dic)
            {
                dic["AppDefaultFontFamilly"] = new FontFamily(font);
            }
        }
    }
}
