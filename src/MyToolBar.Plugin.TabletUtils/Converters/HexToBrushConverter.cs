using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace MyToolBar.Plugin.TabletUtils.Converters;

/// <summary>
/// 将十六进制颜色字符串 (#RRGGBB) 转换为 SolidColorBrush
/// </summary>
public class HexToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var hex = value as string;
        if (string.IsNullOrEmpty(hex))
            return new SolidColorBrush(Color.FromRgb(128, 128, 128));

        try
        {
            hex = hex.TrimStart('#');
            byte r = System.Convert.ToByte(hex[..2], 16);
            byte g = System.Convert.ToByte(hex[2..4], 16);
            byte b = System.Convert.ToByte(hex[4..6], 16);
            return new SolidColorBrush(Color.FromRgb(r, g, b));
        }
        catch
        {
            return new SolidColorBrush(Color.FromRgb(128, 128, 128));
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
