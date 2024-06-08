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

namespace MyToolBar.Services
{
    /// <summary>
    /// [主线任务] 全局异常捕获写日志、加载应用缓存目录、加载主窗口
    /// </summary>
    internal class ApplicationService : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger _logger;

        public ApplicationService(
            IServiceProvider serviceProvider,
            ILogger<ApplicationService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            //异常捕获+写日志
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            App.Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;

            //加载AppData目录
            Settings.LoadPath();
            //注册Hyperlink的跳转事件
            EventManager.RegisterClassHandler(typeof(Hyperlink), Hyperlink.RequestNavigateEvent, new RequestNavigateEventHandler((sender, e) =>
            {
                Process.Start("explorer.exe", e.Uri.AbsoluteUri);
                e.Handled = true;
            }));
            //加载插件包管理器
            _serviceProvider.GetRequiredService<ManagedPackageService>().Load();

            var mainWindow = _serviceProvider.GetRequiredService<AppBarWindow>();
            mainWindow.Show();

            return Task.CompletedTask;
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
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is not Exception exception)
                return;

            _logger.LogError(new EventId(-1), exception, exception.Message);
        }
    }
}
