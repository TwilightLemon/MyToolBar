﻿using MyToolBar.Common;
using System;
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
            bool success=await settingsMgr.Load();
            Loaded?.Invoke(success);
        }

        public Task Save()
        {
            return settingsMgr.Save();
        }
    }

    public class AppSettings
    {
        /// <summary>
        /// 总是使用"沉浸模式"
        /// </summary>
        public bool AlwaysUseImmerseMode {  get; set; }=false;

        /// <summary>
        /// 开机自启动
        /// </summary>
        public bool AutoRunAtStartup { get; set; }=false;

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
