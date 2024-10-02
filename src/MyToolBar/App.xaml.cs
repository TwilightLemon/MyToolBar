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
                services.AddSingleton<ApplicationService>();
                services.AddHostedService(p=>p.GetRequiredService<ApplicationService>());
                services.AddHostedService<MemoryOptimizeService>();

                // windows
                services.AddSingleton<AppBarWindow>();
                services.AddTransient<SettingsWindow>();
                services.AddTransient<MainTitleMenu>();

                // view models
                services.AddTransient<SettingsViewModel>();
                services.AddTransient<MainTitleMenuViewModel>();

                // settings pages
                services.AddTransient<AppSettingsPage>();
                services.AddTransient<CapsulesSettingsPage>();
                services.AddTransient<OuterControlSettingsPage>();
                services.AddTransient<ComponentsSettingsPage>();
                services.AddTransient<ServicesSettingsPage>();
                services.AddTransient<AboutPage>();

                // function services
                services.AddSingleton<ManagedPackageService>();
                services.AddSingleton<PluginReactiveService>();
                services.AddSingleton<UIResourceService>();
                services.AddSingleton<PowerOptimizeService>();
                services.AddHostedService(provider => provider.GetRequiredService<PowerOptimizeService>());
                services.AddSingleton<AppSettingsService>();

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
