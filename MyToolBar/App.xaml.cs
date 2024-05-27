using Microsoft.Extensions.DependencyInjection;
using MyToolBar.Func;
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
        private static IServiceProvider _serviceProvider = BuildServiceProvider();

        static IServiceProvider BuildServiceProvider()
        {
            var services = new ServiceCollection();

            services.AddSingleton<MainWindow>();
            services.AddSingleton<SettingsWindow>();
            services.AddSingleton<SettingsViewModel>();

            // settings pages
            services.AddSingleton<CapsulesSettingsPage>();
            services.AddSingleton<OuterControlSettingsPage>();
            services.AddSingleton<ComponentsSettingsPage>();
            services.AddSingleton<AboutPage>();

            return services.BuildServiceProvider();
        }

        public static IServiceProvider ServiceProvider => _serviceProvider;

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

        public MemoryFlush cracker = new();

        protected override void OnStartup(StartupEventArgs e)
        {
            cracker.Cracker();
            base.OnStartup(e);

            var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }


        #region Theme Dark/Light Mode
        public void SetThemeMode(bool isDark)
        {
            GlobalService.IsDarkMode = isDark;
            string uri = isDark ? "Styles/ThemeColor.xaml" : "Styles/ThemeColor_Light.xaml";
            // 移除当前主题资源字典（如果存在）
            var oldDict = Resources.MergedDictionaries.FirstOrDefault(d => d.Source != null && (d.Source.OriginalString.Contains("ThemeColor.xaml") || d.Source.OriginalString.Contains("ThemeColor_Light.xaml")));
            if (oldDict != null)
            {
                Resources.MergedDictionaries.Remove(oldDict);
            }
            // 添加新的主题资源字典
            Resources.MergedDictionaries.Add(new ResourceDictionary() { Source = new Uri(uri, UriKind.Relative) });
        }

        public T? GetResource<T>(string resourceName) where T : class
        {
            return Resources[resourceName] as T;
        }
        #endregion
    }
}
