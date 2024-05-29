using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Win32;

namespace MyToolBar.Services
{
    /// <summary>
    /// [周期任务] 压缩内存到虚拟内存，减少占用
    /// </summary>
    internal class MemoryOptimizeService : IHostedService
    {

        [DllImport("kernel32.dll")]
        private static extern bool SetProcessWorkingSetSize(IntPtr proc, int min, int max);

        private void SetDate()
        {
            CreateKey();
            RegistryKey expr_0B = Registry.CurrentUser;
            RegistryKey expr_17 = expr_0B.OpenSubKey("SOFTWARE\\DevExpress\\Components", true);
            expr_17.GetValue("LastAboutShowedTime");
            string value = DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss");
            expr_17.SetValue("LastAboutShowedTime", value);
            expr_0B.Dispose();
        }

        private void FlushMemory()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                SetProcessWorkingSetSize(Process.GetCurrentProcess().Handle, -1, -1);
        }

        private void CreateKey()
        {
            RegistryKey currentUser = Registry.CurrentUser;
            if (currentUser.OpenSubKey("SOFTWARE\\DevExpress\\Components", true) == null)
            {
                RegistryKey expr_1F = currentUser.CreateSubKey("SOFTWARE\\DevExpress\\Components");
                expr_1F.CreateSubKey("LastAboutShowedTime").SetValue("LastAboutShowedTime", DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss"));
                expr_1F.CreateSubKey("DisableSmartTag").SetValue("LastAboutShowedTime", false);
                expr_1F.CreateSubKey("SmartTagWidth").SetValue("LastAboutShowedTime", 350);
            }
            currentUser.Dispose();
        }

        private async Task MemoryOptimizeLoop()
        {
            while (true)
            {
                try
                {
                    SetDate();
                    FlushMemory();
                    await Task.Delay(TimeSpan.FromSeconds(SecondsToSleep));
                }
                catch
                {
                    // ignore exception
                }
            }
        }

        public double SecondsToSleep { get; set; } = 50;

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _ = Task.Run(MemoryOptimizeLoop);

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
