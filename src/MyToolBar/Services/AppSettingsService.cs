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
        /// 启用新样式
        /// </summary>
        public bool EnableNewStyle { get; set; } = false;
        public event Action? OnEnableNewStyleChanged;
        public void SetEnableNewStyle(bool enable)
        {
            EnableNewStyle = enable;
            OnEnableNewStyleChanged?.Invoke();
        }
        /// <summary>
        /// 返回桌面时使用"沉浸模式"
        /// </summary>
        public bool AlwaysUseImmerseMode {  get; set; }=false;

        /// <summary>
        /// 使用"沉浸模式"
        /// </summary>
        public bool UseImmerseMode { get; set; } = true;

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
