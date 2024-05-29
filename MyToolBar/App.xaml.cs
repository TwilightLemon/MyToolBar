using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MyToolBar.Services;
using MyToolBar.ViewModels;
using MyToolBar.Views.Pages.Settings;
using MyToolBar.Views.Windows;
using NLog.Extensions.Logging;
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
                services.AddSingleton<AppBarViewModel>();
                services.AddTransient<SettingsWindow>();
                services.AddTransient<SettingsViewModel>();

                // settings pages
                services.AddTransient<CapsulesSettingsPage>();
                services.AddTransient<OuterControlSettingsPage>();
                services.AddTransient<ComponentsSettingsPage>();
                services.AddTransient<AboutPage>();

                // function services
                services.AddSingleton<PluginService>();
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
