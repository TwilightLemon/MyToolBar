using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MyToolBar.Func;
using MyToolBar.Services;
using MyToolBar.ViewModels;
using MyToolBar.Views.Pages;
using MyToolBar.Views.Pages.Settings;
using MyToolBar.WinApi;
using NLog.Extensions.Hosting;
using NLog.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace MyToolBar
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static IHost Host { get; } = new HostBuilder()
            .ConfigureServices(services =>
            {
                // host
                services.AddHostedService<ApplicationService>();
                services.AddHostedService<MemoryOptimizeService>();

                // windows
                services.AddSingleton<AppBarWindow>();
                services.AddSingleton<SettingsWindow>();
                services.AddSingleton<SettingsViewModel>();

                // settings pages
                services.AddSingleton<CapsulesSettingsPage>();
                services.AddSingleton<OuterControlSettingsPage>();
                services.AddSingleton<ComponentsSettingsPage>();
                services.AddSingleton<AboutPage>();

                // function services
                services.AddSingleton<AppSettingsService>();
                services.AddSingleton<ThemeResourceService>();

                // logging
                services.AddLogging(builder =>
                {
                    builder.AddNLog();
                });
            })
            .Build();

        public App()
        {
            InitializeComponent();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            Host.Start();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);

            _ = Host.StopAsync();
        }
    }
}
