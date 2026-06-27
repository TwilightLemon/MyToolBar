namespace MyToolBar.ViewModels;

/// <summary>
/// ComboBox / Tile 显示器选项的辅助模型
/// </summary>
public class MonitorDisplayItem
{
    /// <summary>
    /// 设备名称(如 \\.\DISPLAY1)
    /// </summary>
    public string? DeviceName { get; set; }

    /// <summary>
    /// 显示器宽度（物理像素）
    /// </summary>
    public int MonitorWidth { get; set; }

    /// <summary>
    /// 显示器高度（物理像素）
    /// </summary>
    public int MonitorHeight { get; set; }

    /// <summary>
    /// 是否为主显示器
    /// </summary>
    public bool IsPrimary { get; set; }

    /// <summary>
    /// 分辨率文本（如 "2560×1440"）
    /// </summary>
    public string ResolutionText => $"{MonitorWidth}×{MonitorHeight}";
}
