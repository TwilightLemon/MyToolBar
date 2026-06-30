using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace MyToolBar.Plugin.TabletUtils.Converters;

/// <summary>
/// bool ↔ Visibility 转换器
/// </summary>
public class BoolToVisibilityConverter : IValueConverter
{
    /// <summary>
    /// true 时返回 Visible，false 返回 Collapsed（可通过参数 Invert 反转）
    /// </summary>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool boolValue = value is true;
        bool invert = parameter is string s && s.Equals("Invert", StringComparison.OrdinalIgnoreCase);

        if (invert) boolValue = !boolValue;
        return boolValue ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
