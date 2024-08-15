using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
/*
 该服务的设计初衷：
 WPF本身占用内存很高，整个项目其实长时间都处于一个静默的状态，
 不需要太多的资源，故将大部分内存压缩至虚拟内存
 */
namespace MyToolBar.Services
{
    /// <summary>
    /// [周期任务] 优化内存
    /// </summary>
    internal class MemoryOptimizeService : IHostedService
    {

        [DllImport("kernel32.dll")]
        private static extern bool SetProcessWorkingSetSize(IntPtr proc, int min, int max);

        private void FlushMemory()
        {
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true, true);
            GC.WaitForPendingFinalizers();
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                SetProcessWorkingSetSize(Process.GetCurrentProcess().Handle, -1, -1);
        }

        private async Task MemoryOptimizeLoop(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    FlushMemory();
                    await Task.Delay(TimeSpan.FromSeconds(SecondsToSleep), cancellationToken);
                }
                catch(TaskCanceledException)
                {
                    break;
                }
            }
        }

        public double SecondsToSleep { get; set; } = 120;

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _ = MemoryOptimizeLoop(cancellationToken);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
