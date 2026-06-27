using System.Runtime.InteropServices;

namespace MyToolBar.Common.WinAPI;

/// <summary>
/// 提供多显示器枚举和查询功能
/// </summary>
public static class MonitorAPI
{
    private delegate bool MonitorEnumProc(IntPtr hMonitor, IntPtr hdcMonitor, ref ScreenAPI.RECT lprcMonitor, IntPtr dwData);

    [DllImport("user32.dll")]
    private static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr lprcClip, MonitorEnumProc lpfnEnum, IntPtr dwData);

    /// <summary>
    /// 显示器信息
    /// </summary>
    public class MonitorDisplayInfo
    {
        /// <summary>
        /// 设备名称（如 \\.\DISPLAY1），可用作稳定标识符
        /// </summary>
        public string DeviceName { get; set; } = "";

        /// <summary>
        /// 显示器完整区域（物理像素，虚拟屏幕坐标）
        /// </summary>
        public ScreenAPI.RECT MonitorBounds { get; set; }

        /// <summary>
        /// 工作区域（排除任务栏，物理像素，虚拟屏幕坐标）
        /// </summary>
        public ScreenAPI.RECT WorkAreaBounds { get; set; }

        /// <summary>
        /// 是否为主显示器
        /// </summary>
        public bool IsPrimary { get; set; }

        /// <summary>
        /// DPI 缩放比例 X（dpiX / 96）
        /// </summary>
        public double DpiScaleX { get; set; } = 1.0;

        /// <summary>
        /// DPI 缩放比例 Y（dpiY / 96）
        /// </summary>
        public double DpiScaleY { get; set; } = 1.0;
    }

    /// <summary>
    /// 枚举所有已连接的显示器
    /// </summary>
    public static List<MonitorDisplayInfo> EnumerateMonitors()
    {
        var monitors = new List<MonitorDisplayInfo>();
        GCHandle handle = GCHandle.Alloc(monitors);
        try
        {
            EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, MonitorEnumCallback, GCHandle.ToIntPtr(handle));
        }
        finally
        {
            handle.Free();
        }
        return monitors;
    }

    private static bool MonitorEnumCallback(IntPtr hMonitor, IntPtr hdcMonitor, ref ScreenAPI.RECT lprcMonitor, IntPtr dwData)
    {
        var handle = GCHandle.FromIntPtr(dwData);
        if (handle.Target is List<MonitorDisplayInfo> monitors)
        {
            var info = ScreenAPI.GetMonitorInfoEx(hMonitor);
            if (info != null)
            {
                var (dpiX, dpiY) = ScreenAPI.GetDPI(hMonitor);
                monitors.Add(new MonitorDisplayInfo
                {
                    DeviceName = new string(info.szDevice).TrimEnd('\0'),
                    MonitorBounds = info.rcMonitor,
                    WorkAreaBounds = info.rcWork,
                    IsPrimary = (info.dwFlags & 1) != 0, // MONITORINFOF_PRIMARY
                    DpiScaleX = dpiX,
                    DpiScaleY = dpiY
                });
            }
        }
        return true; // 继续枚举
    }
}
