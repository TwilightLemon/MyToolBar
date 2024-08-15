using Microsoft.Extensions.Hosting;
using MyToolBar.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MyToolBar.Services
{
    /// <summary>
    /// 管理MyToolBar主程序的配置服务
    /// </summary>
    public class AppSettingsService:IHostedService
    {
        private static readonly string _settingsSign = "AppSettings",
                                                    _packageName = typeof(AppSettingsService).FullName;
        private readonly SettingsMgr<AppSettings> settingsMgr=new(_settingsSign,_packageName);
        private bool _isLoaded = false;
        public AppSettings Settings { get => settingsMgr.Data; }
        public async Task WaitForLoading()
        {
            while (!_isLoaded) await Task.Delay(10);
        }
        
        public Task StartAsync(CancellationToken cancellationToken)
        {
            _= settingsMgr.Load();
            _isLoaded = true;
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _ = settingsMgr.Save();
            return Task.CompletedTask;
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
        public string Language { get; set; } = "en-us";


    }
}
