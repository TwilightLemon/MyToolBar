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
        private static Mutex mutex;

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
            })
            .Build();

        public static App? CurrentApp => Current as App;
        public App()
        {
            mutex = new Mutex(false, Assembly.GetExecutingAssembly().GetName().Name, out bool firstInstant);
#if DEBUG
            if (!firstInstant)
            {
                Environment.Exit(0);
            }
            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
            {
                try
                {
                    //坐以待毙吧
                    WriteLog((Exception)e.ExceptionObject);
                }
                catch { }
            };

            Current.DispatcherUnhandledException += (sender, e) =>
            {
                e.Handled = true;
                WriteLog(e.Exception);
            };
            TaskScheduler.UnobservedTaskException += (sender, e) =>
            {
                e.SetObserved();
                WriteLog(e.Exception);
            };
#endif
            Settings.LoadPath();
        }

        private void WriteLog(Exception e)
        {
            if (e == null)
                return;
            string log= $"[{DateTime.Now}]\n{e.Source}\n {e.Message}\n{e.StackTrace}\n{e.Source}";
            File.AppendAllText(Path.Combine(Settings.MainPath, "Error.log"), log);
        }
    }
}
