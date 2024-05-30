using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MyToolBar.Common;
using MyToolBar.Views.Windows;

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
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            App.Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;

            Settings.LoadPath();
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
