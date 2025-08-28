using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Navigation;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MyToolBar.Common;
using MyToolBar.Views.Windows;
using System.Diagnostics;
using System.Net.Http;
using System.Net;
using Microsoft.Win32;

namespace MyToolBar.Services
{
    /// <summary>
    /// [主线任务] 全局异常捕获写日志、加载应用缓存目录、加载主窗口
    /// </summary>
    public class ApplicationService(
        IServiceProvider serviceProvider,
        ILogger<ApplicationService> logger,
        UIResourceService resourceService,
        AppSettingsService appSettingsService) : IHostedService
    {
        private readonly ILogger _logger = logger;

        public Task StartAsync(CancellationToken cancellationToken)
        {
            //异常捕获+写日志
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            App.Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
            //注册Hyperlink的跳转事件
            EventManager.RegisterClassHandler(typeof(Hyperlink), Hyperlink.RequestNavigateEvent, new RequestNavigateEventHandler((sender, e) =>
            {
                Process.Start("explorer.exe", e.Uri.AbsoluteUri);
                e.Handled = true;
            }));
            //override default style for Pages
            FrameworkElement.StyleProperty.OverrideMetadata(typeof(System.Windows.Controls.Page), new FrameworkPropertyMetadata
            {
                DefaultValue = App.Current.FindResource(typeof(System.Windows.Controls.Page))
            });
            //加载插件包管理器
            serviceProvider.GetRequiredService<ManagedPackageService>().Load();
            //加载配置
            appSettingsService.Loaded += delegate
            {
                if (appSettingsService.Settings == null) return;

                //设置字体
                UIResourceService.SetAppFontFamilly(appSettingsService.Settings.FontFamily);

                //设置Http代理
                UpdateDefaultProxy();

                //设置语言
                var lang = (LocalCulture.Language)appSettingsService.Settings.Language;
                LocalCulture.SetGlobalLanguage(lang, false);
                LocalCulture.OnLanguageChanged += LocalCulture_OnLanguageChanged;
                resourceService.SetLanguage(lang);

                //加载主窗口
                var mainWindow = serviceProvider.GetRequiredService<AppBarWindow>();
                mainWindow.Show();
                App.Current.MainWindow = mainWindow;
            };
            appSettingsService.Load();


            return Task.CompletedTask;
        }

        private void LocalCulture_OnLanguageChanged(object? sender, LocalCulture.Language e)
        {
            resourceService.SetLanguage(e);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            App.Current.Shutdown();

            return Task.CompletedTask;
        }

        private void TaskScheduler_UnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
        {
            _logger.LogError(new EventId(-1), e.Exception, e.Exception.Message);
        }

        private void Current_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            _logger.LogError(new EventId(-1), e.Exception, e.Exception.Message);
#if !DEBUG
            e.Handled = true;
#endif
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is not Exception exception)
                return;

            _logger.LogError(new EventId(-1), exception, exception.Message);
        }

        public void UpdateDefaultProxy()
        {
            var type = appSettingsService.Settings.UserProxyMode;
            switch (type)
            {
                case AppSettings.ProxyMode.None:
                    HttpClient.DefaultProxy = new WebProxy();
                    break;
                    case AppSettings.ProxyMode.Global:
                    HttpClient.DefaultProxy = WebRequest.GetSystemWebProxy();
                    break;
                case AppSettings.ProxyMode.Custom:
                    var proxy = new WebProxy();
                    var conf = appSettingsService.Settings.Proxy;
                    if (conf != null)
                    {
                        proxy.Address = new Uri($"http://{conf.Address}:{conf.Port}");
                        if (!string.IsNullOrEmpty(conf.UserName))
                        {
                            proxy.Credentials = new NetworkCredential(conf.UserName, conf.Pwd);
                        }
                    }
                    HttpClient.DefaultProxy = proxy;
                    break;
            }
            Debug.WriteLine(HttpClient.DefaultProxy);
        }

        public void SetAutoRunAtStartup(bool autoRun)
        {
            if (appSettingsService.Settings.AutoRunAtStartup == autoRun) return;
            appSettingsService.Settings.AutoRunAtStartup = autoRun;
            string exePath = $"\"{Process.GetCurrentProcess().MainModule.FileName}\"";
            using RegistryKey RKey = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run");
            if(autoRun)
                RKey.SetValue("MyToolBar", exePath);
            else
                RKey.DeleteValue("MyToolBar", false);
        }
    }
}
