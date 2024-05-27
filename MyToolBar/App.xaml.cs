using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MyToolBar.Func;
using MyToolBar.Services;
using MyToolBar.ViewModels;
using MyToolBar.Views.Pages;
using MyToolBar.WinApi;
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
        static Mutex? _appMutex;

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
            })
            .Build();

        /// <summary>
        /// Application entry point
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            if (!IsApplicationAlreadyStarted())
            {
                return;
            }

            var app = new App();
            app.Run();
        }

        static bool IsApplicationAlreadyStarted()
        {
            _appMutex = new Mutex(false, Assembly.GetExecutingAssembly().GetName().Name, out bool firstInstant);

            return !firstInstant;
        }
    }
}
