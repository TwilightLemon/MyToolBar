namespace MyToolBar.Plugin.BasicPackage.Weather.Services;

/// <summary>
/// 和风天气图标字体映射工具
/// 字体文件: qweather-icons.ttf
/// 映射规则: 天气code -> Unicode codepoint 0xF{code}
/// 例如: code 301 -> U+F301, code 1010 -> U+F1010
/// </summary>
public static class WeatherIconHelper
{
    /// <summary>
    /// 将天气code转换为qweather-icons字体字符
    /// </summary>
    public static string GetIconChar(int code)
    {
        int codepoint = Convert.ToInt32($"F{code}", 16);
        return char.ConvertFromUtf32(codepoint);
    }
}
