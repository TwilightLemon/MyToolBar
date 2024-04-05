using MyToolBar.Func;
using MyToolBar.WinApi;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
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
        public App()
        {
            mutex=new Mutex(false, "MyToolBar",out bool firstInstant);
            if(!firstInstant)
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
            Settings.LoadPath();
        }
        private void WriteLog(Exception e)
        {
            if (e == null) return;
            string log= $"[{DateTime.Now}]\n{e.Source}\n {e.Message}\n{e.StackTrace}\n{e.Source}";
            File.AppendAllText(Path.Combine(Settings.MainPath, "Error.log"), log);
        }
        public MemoryFlush cracker = new();
        protected override void OnStartup(StartupEventArgs e)
        {
            cracker.Cracker();
            base.OnStartup(e);
        }
    }
}
