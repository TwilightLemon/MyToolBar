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
        public static IHost Host { get; private set; }

        private void BuildHost()
        {
            var builder = new HostBuilder();
            Host=builder.ConfigureServices(services =>
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
                services.AddSingleton<ManagedPackageService>();
                services.AddSingleton<PluginReactiveService>();
                services.AddSingleton<ThemeResourceService>();
                services.AddSingleton<PowerOptimizeService>();
                services.AddHostedService(provider => provider.GetRequiredService<PowerOptimizeService>());

                // logging
                services.AddLogging(builder =>
                {
                    builder.AddNLog();
                });
            })
            .Build();
        }

        public App()
        {
            InitializeComponent();
            BuildHost();
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
