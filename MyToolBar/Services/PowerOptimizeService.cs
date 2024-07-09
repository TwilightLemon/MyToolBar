using Microsoft.Extensions.Hosting;
using Microsoft.Win32;
using MyToolBar.Common;
using Windows.System.Power;
using System.Threading;
using System.Threading.Tasks;
using System;

namespace MyToolBar.Services
{
    public class PowerOptimizeService:IHostedService
    {
        public event Action<bool>? OnEnergySaverStatusChanged;
        public Task StartAsync(CancellationToken cancellationToken)
        {
            PowerManager.EnergySaverStatusChanged += PowerManager_EnergySaverStatusChanged;
            SystemEvents.PowerModeChanged += SystemEvents_PowerModeChanged;
            return Task.CompletedTask;
        }

        private void PowerManager_EnergySaverStatusChanged(object? sender, object e)
        {
            OnEnergySaverStatusChanged?.Invoke(PowerManager.EnergySaverStatus==EnergySaverStatus.On);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            PowerManager.EnergySaverStatusChanged -= PowerManager_EnergySaverStatusChanged;
            SystemEvents.PowerModeChanged -= SystemEvents_PowerModeChanged;
            return Task.CompletedTask;
        }

        private void SystemEvents_PowerModeChanged(object sender, PowerModeChangedEventArgs e)
        {
            if (e.Mode == PowerModes.Suspend)
            {
                //休眠时停止全局Timer
                GlobalService.GlobalTimer.Stop();
            }
            else if (e.Mode == PowerModes.Resume)
            {
                //唤醒时启动全局Timer
                GlobalService.GlobalTimer.Start();
            }
        }
    }
}
