using MyToolBar.Common;
using MyToolBar.Services;
using System;
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
                switch (proxyMode)
                {
                    case AppSettings.ProxyMode.None:
                        return App.Current.FindResource("AS_Proxy_None");
                    case AppSettings.ProxyMode.Global:
                        return App.Current.FindResource("AS_Proxy_Global");
                    case AppSettings.ProxyMode.Custom:
                        return App.Current.FindResource("AS_Proxy_Custom");
                }
            }else if(value is LocalCulture.Language language)
            {
                switch (language)
                {
                    case LocalCulture.Language.en_us:
                        return App.Current.FindResource("AS_Language_English");
                    case LocalCulture.Language.zh_cn:
                        return App.Current.FindResource("AS_Language_Chinese");
                }
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}
