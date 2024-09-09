using Microsoft.Extensions.Hosting;
using Microsoft.Win32;
using MyToolBar.Common;
using Windows.System.Power;
using MyToolBar.Common.WinAPI;
using System.Threading;
using System.Threading.Tasks;
using System;
using System.Diagnostics;
using System.Windows;

namespace MyToolBar.Services
{
    /// <summary>
    /// 电源优化服务 用于处理电源模式变化和低功耗状态 适配Modern Standby
    /// </summary>
    public class PowerOptimizeService:IHostedService
    {
        private IntPtr _standbyAPIHandle;
        public event Action<bool>? OnEnergySaverStatusChanged;
        public event Action<bool>? OnStandbyStateChanged;
        public Task StartAsync(CancellationToken cancellationToken)
        {
            GlobalService.IsEnergySaverModeOn=PowerManager.EnergySaverStatus== EnergySaverStatus.On;
           Application.Current.Dispatcher.Invoke(() =>
            {
                PowerManager.EnergySaverStatusChanged += PowerManager_EnergySaverStatusChanged;
                SystemEvents.PowerModeChanged += SystemEvents_PowerModeChanged;
            });
            ModernStandbyPowerAPI.RegisterNotification((IntPtr context, int type, IntPtr setting) =>
            {
                if (type == ModernStandbyPowerAPI.PBT_APMSUSPEND)
                {
                    //进入低功耗状态时停止全局Timer
                    GlobalService.GlobalTimer.Stop();
                    OnStandbyStateChanged?.Invoke(true);
                }
                else if (type == ModernStandbyPowerAPI.PBT_APMRESUMESUSPEND)
                {
                    //手动唤醒时启动全局Timer
                    GlobalService.GlobalTimer.Start();
                    OnStandbyStateChanged?.Invoke(false);
                }
                return 0;
            }, ref _standbyAPIHandle);
            return Task.CompletedTask;
        }

        private void PowerManager_EnergySaverStatusChanged(object? sender, object e)
        {
            OnEnergySaverStatusChanged?.Invoke(PowerManager.EnergySaverStatus==EnergySaverStatus.On);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            ModernStandbyPowerAPI.UnregisterNotification(_standbyAPIHandle);
            App.Current.Dispatcher.InvokeAsync(() =>
            {
                PowerManager.EnergySaverStatusChanged -= PowerManager_EnergySaverStatusChanged;
                SystemEvents.PowerModeChanged -= SystemEvents_PowerModeChanged;
            });
            return Task.CompletedTask;
        }

        private void SystemEvents_PowerModeChanged(object sender, PowerModeChangedEventArgs e)
        {
            if (e.Mode == PowerModes.Suspend)
            {
                //休眠时停止全局Timer
                GlobalService.GlobalTimer.Stop();
                OnStandbyStateChanged?.Invoke(true);
            }
            else if (e.Mode == PowerModes.Resume)
            {
                //唤醒时启动全局Timer
                GlobalService.GlobalTimer.Start();
                OnStandbyStateChanged?.Invoke(false);
            }
        }
    }
}
