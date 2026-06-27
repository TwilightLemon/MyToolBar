using System;
using System.Collections.Generic;
namespace MyToolBar.Plugin.BasicPackage.Weather.Services;
public static class QiCodeMapper
{
    // 定义所有有效的 code 区间（按顺序）
    private static readonly List<(int Start, int End)> _ranges = new()
    {
        (100, 104),
        (150, 153),
        (300, 318),
        (350, 351),
        (399, 410),
        (456, 457),
        (499, 504),
        (507, 515),
        (800, 807),
        (900, 901),
        (999, 999)
    };

    /// <summary>
    /// 根据 code 获取对应的索引位置（从 0 开始）
    /// </summary>
    private static int GetIndex(int code)
    {
        int total = 0;
        foreach (var (start, end) in _ranges)
        {
            if (code >= start && code <= end)
            {
                return total + (code - start);
            }
            total += end - start + 1;
        }
        throw new ArgumentOutOfRangeException(nameof(code), $"无效的 qi-code：{code}");
    }

    /// <summary>
    /// 获取普通版本字符（对应 .qi-xxx::before）
    /// </summary>
    public static char GetQiChar(int code)
    {
        int index = GetIndex(code);
        return (char)(0xF101 + index);
    }

    /// <summary>
    /// 获取 Fill 版本字符（对应 .qi-xxx-fill::before）
    /// </summary>
    public static char GetQiFillChar(int code)
    {
        int index = GetIndex(code);

        // 800~807 没有 Fill 版本
        if (index >= 59 && index <= 66)
        {
            throw new ArgumentOutOfRangeException(nameof(code), 
                $"code {code}（索引 {index}）不存在对应的 -fill 版本");
        }

        int fillIndex;
        if (index < 59)
        {
            // 前 59 个（100~515）直接对应
            fillIndex = index;
        }
        else
        {
            // 900、901、999（索引 67~69）需要跳过 800~807 这 8 个缺失项
            fillIndex = index - 8;
        }

        return (char)(0xF1CC + fillIndex);
    }
}