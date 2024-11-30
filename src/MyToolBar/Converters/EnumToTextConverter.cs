using MyToolBar.Common;
using MyToolBar.Services;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Data;

namespace MyToolBar.Converters
{
    [ValueConversion(typeof(Enum), typeof(string))]
    public class EnumToTextConverter:IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is AppSettings.ProxyMode proxyMode)
            {
                return App.Current.FindResource($"AS_Proxy_{proxyMode}");
            }
            else if(value is LocalCulture.Language language)
            {
                return App.Current.FindResource($"AS_Language_{language switch
                {
                    LocalCulture.Language.en_us => "English",
                    LocalCulture.Language.zh_cn => "Chinese",
                    _ => throw new NotImplementedException()
                }}"
                );
            }else if(value is AppSettings.MenuIcon icon)
            {
                return App.Current.FindResource($"AS_MenuIcon_{icon}");
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}
