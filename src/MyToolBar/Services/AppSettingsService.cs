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

    }
}
