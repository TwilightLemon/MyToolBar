using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MyToolBar.Services
{
    public class ThemeResourceService
    {
        public void SetThemeMode(bool isDark)
        {
            GlobalService.IsDarkMode = isDark;
            string uri = isDark ? "Styles/ThemeColor.xaml" : "Styles/ThemeColor_Light.xaml";
            // 移除当前主题资源字典（如果存在）
            var oldDict = App.Current.Resources.MergedDictionaries.FirstOrDefault(d => d.Source != null && (d.Source.OriginalString.Contains("ThemeColor.xaml") || d.Source.OriginalString.Contains("ThemeColor_Light.xaml")));

            if (oldDict != null)
            {
                App.Current.Resources.MergedDictionaries.Remove(oldDict);
            }

            // 添加新的主题资源字典
            App.Current.Resources.MergedDictionaries.Add(new ResourceDictionary() { Source = new Uri(uri, UriKind.Relative) });
        }
    }
}
