using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace MyToolBar.Converters
{
    /// <summary>
    /// 将枚举值与参数比较，返回是否相等，用于 RadioButton 的 IsChecked 绑定
    /// </summary>
    [ValueConversion(typeof(Enum), typeof(bool))]
    public class EnumEqualsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null) return false;
            return value.Equals(parameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b && b && parameter != null)
                return parameter;
            return Binding.DoNothing;
        }
    }
}
