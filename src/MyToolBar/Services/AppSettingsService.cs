using MyToolBar.Common;
using System;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MyToolBar.Services
{
    /// <summary>
    /// 管理MyToolBar主程序的配置服务
    /// </summary>
    public class AppSettingsService
    {
        private static readonly string _settingsSign = "AppSettings",
                                                    _packageName = typeof(AppSettingsService).FullName;
        private readonly SettingsMgr<AppSettings> settingsMgr=new(_settingsSign,_packageName);
        public AppSettings Settings { get => settingsMgr.Data??new AppSettings(); }
        public event Action<bool>? Loaded;
        
        public async void Load()
        {
            bool success=await settingsMgr.LoadAsync();
            Loaded?.Invoke(success);
        }

        public Task Save()
        {
            return settingsMgr.SaveAsync();
        }
    }

    public class AppSettings
    {
        /// <summary>
        /// 全局字体
        /// </summary>
        public string? FontFamily { get; set; }

        /// <summary>
        /// 窗口模式
        /// </summary>
        public enum WindowMode
        {
            /// <summary>嵌入模式（贴边）</summary>
            Embedded,
            /// <summary>悬浮模式（圆角悬浮）</summary>
            Floating
        }

        /// <summary>
        /// 背景模式
        /// </summary>
        public enum BackgroundMode
        {
            /// <summary>透明背景</summary>
            Transparent,
            /// <summary>模糊背景（默认）</summary>
            Acrylic
        }

        /// <summary>
        /// 沉浸模式
        /// </summary>
        public enum ImmerseMode
        {
            /// <summary>关闭沉浸模式</summary>
            Off,
            /// <summary>自动：有最大化窗口时启用沉浸，否则使用所选背景</summary>
            Auto,
            /// <summary>始终开启沉浸模式</summary>
            Always
        }

        /// <summary>
        /// 假岛
        /// </summary>
        public bool EnableIsland { get; set; } = false;
        public event Action? OnEnableIslandChanged;
        public void SetEnableIsland(bool enable)
        {
            EnableIsland = enable;
            OnEnableIslandChanged?.Invoke();
        }

        /// <summary>
        /// 当前窗口模式
        /// </summary>
        public WindowMode CurrentWindowMode { get; set; } = WindowMode.Embedded;
        public event Action? OnWindowModeChanged;
        public void SetWindowMode(WindowMode mode)
        {
            CurrentWindowMode = mode;
            OnWindowModeChanged?.Invoke();
        }

        /// <summary>
        /// 当前背景模式
        /// </summary>
        public BackgroundMode CurrentBackgroundMode { get; set; } = BackgroundMode.Acrylic;
        public event Action? OnBackgroundModeChanged;
        public void SetBackgroundMode(BackgroundMode mode)
        {
            CurrentBackgroundMode = mode;
            OnBackgroundModeChanged?.Invoke();
        }

        /// <summary>
        /// 沉浸模式：关闭/自动/始终开启
        /// </summary>
        public ImmerseMode CurrentImmerseMode { get; set; } = ImmerseMode.Auto;
        public event Action? OnImmerseModeChanged;
        public void SetImmerseMode(ImmerseMode mode)
        {
            CurrentImmerseMode = mode;
            OnImmerseModeChanged?.Invoke();
        }

        /// <summary>
        /// 开机自启动
        /// </summary>
        public bool AutoRunAtStartup { get; set; }=false;

        public enum MenuIcon
        {
            Round,Apple,WindowsXp,Gatito
        }
        /// <summary>
        /// 主菜单图标
        /// </summary>
        public MenuIcon MainMenuIcon { get; set; } = MenuIcon.Round;

        public event Action? OnMainMenuIconChanged;
        public void SetMainMenuIcon(MenuIcon icon)
        {
            MainMenuIcon = icon;
            OnMainMenuIconChanged?.Invoke();
        }

        /// <summary>
        /// App全局语言
        /// </summary>
        public int Language { get; set; } = 1;

        /// <summary>
        /// 窗口最大化时显示窗口控制按钮（最小化/最大化/关闭）
        /// </summary>
        public bool EnableWindowControl { get; set; } = false;
        public event Action? OnEnableWindowControlChanged;
        public void SetEnableWindowControl(bool enable)
        {
            EnableWindowControl = enable;
            OnEnableWindowControlChanged?.Invoke();
        }

        public record ProxyConf(string Address, string Port, string UserName, string Pwd);
        public enum ProxyMode
        {
            None,
            Global,
            Custom
        }
        /// <summary>
        /// 代理模式
        /// </summary>
        public ProxyMode UserProxyMode { get; set; } = ProxyMode.None;

        /// <summary>
        /// 代理配置
        /// </summary>
        public ProxyConf? Proxy { get; set; } = null;

        /// <summary>
        /// 目标显示器的设备名称（如 \\.\DISPLAY1）。
        /// 为 null 或空字符串时使用默认行为（主显示器/最近显示器）。
        /// </summary>
        public string? TargetMonitorDeviceName { get; set; }

        public event Action? OnTargetMonitorChanged;
        public void SetTargetMonitor(string? deviceName)
        {
            TargetMonitorDeviceName = deviceName;
            OnTargetMonitorChanged?.Invoke();
        }

        /// <summary>
        /// 悬浮模式下水平方向距离屏幕边缘的边距（单位：WPF 逻辑像素）
        /// </summary>
        public double FloatingMarginHorizontal { get; set; } = 8;

        /// <summary>
        /// 悬浮模式下垂直方向距离屏幕边缘的边距（单位：WPF 逻辑像素）
        /// </summary>
        public double FloatingMarginVertical { get; set; } = 4;

        /// <summary>
        /// AppBar 预留高度，控制 AppBarCreator.ReservedHeight（单位：WPF 逻辑像素，默认 32）
        /// </summary>
        public double AppBarHeight { get; set; } = 32;

        public event Action? OnFloatingMarginChanged;
        public void SetFloatingMargin(double horizontal, double vertical)
        {
            FloatingMarginHorizontal = horizontal;
            FloatingMarginVertical = vertical;
            OnFloatingMarginChanged?.Invoke();
        }

        public event Action? OnAppBarHeightChanged;
        public void SetAppBarHeight(double height)
        {
            AppBarHeight = height;
            OnAppBarHeightChanged?.Invoke();
        }

        /// <summary>
        /// 是否启用高光渲染效果（仅在字体为亮色区域可用，省电模式下强制关闭）
        /// </summary>
        public bool EnableHighlight { get; set; } = false;
        public event Action? OnEnableHighlightChanged;
        public void SetEnableHighlight(bool enable)
        {
            EnableHighlight = enable;
            OnEnableHighlightChanged?.Invoke();
        }

    }
}
