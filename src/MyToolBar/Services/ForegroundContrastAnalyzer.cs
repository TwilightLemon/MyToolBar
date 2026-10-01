using System;
using System.Collections.Generic;

namespace MyToolBar.Services;

/// <summary>
/// 根据一组背景相对亮度，判断是否应使用黑色前景。
/// </summary>
internal static class ForegroundContrastAnalyzer
{
    private const double RepresentativePercentile = 0.40;
    private const double ContrastCrossoverLuminance = 0.179;
    private const double LuminanceHysteresis = 0.03;

    /// <summary>
    /// 将 sRGB 颜色转换为 WCAG 相对亮度。
    /// </summary>
    internal static double GetRelativeLuminance(byte red, byte green, byte blue)
    {
        static double SrgbToLinear(double value) =>
            value <= 0.03928
                ? value / 12.92
                : Math.Pow((value + 0.055) / 1.055, 2.4);

        return
            0.2126 * SrgbToLinear(red / 255.0) +
            0.7152 * SrgbToLinear(green / 255.0) +
            0.0722 * SrgbToLinear(blue / 255.0);
    }

    /// <summary>
    /// 使用偏暗侧的亮度分位数进行判断，并以当前结果形成滞回区。
    /// </summary>
    internal static bool UseBlackForeground(List<double> luminances, bool? currentUseBlackForeground)
    {
        if (luminances.Count == 0)
            throw new ArgumentException("At least one luminance sample is required.", nameof(luminances));

        luminances.Sort();
        int percentileIndex = (int)Math.Floor((luminances.Count - 1) * RepresentativePercentile);
        double representativeLuminance = luminances[percentileIndex];

        if (currentUseBlackForeground == null)
            return representativeLuminance >= ContrastCrossoverLuminance;

        return currentUseBlackForeground.Value
            ? representativeLuminance > ContrastCrossoverLuminance - LuminanceHysteresis
            : representativeLuminance >= ContrastCrossoverLuminance + LuminanceHysteresis;
    }
}
