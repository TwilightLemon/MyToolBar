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
/*
 * 删除了SystemEvents提供的OnPowerModeChanged事件，似乎会导致UI线程假死并且没有任何异常抛出..?
 * 本应用支持的Win10以上系统都提供了Mondern Standy API，故仅用此
 */

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
            GlobalService.SetEnergySaverMode(PowerManager.EnergySaverStatus == EnergySaverStatus.On);
            PowerManager.EnergySaverStatusChanged += PowerManager_EnergySaverStatusChanged;
            bool success = ModernStandbyPowerAPI.RegisterNotification(OnModernStandByStateChanged, ref _standbyAPIHandle);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            ModernStandbyPowerAPI.UnregisterNotification(_standbyAPIHandle);
            PowerManager.EnergySaverStatusChanged -= PowerManager_EnergySaverStatusChanged;
            return Task.CompletedTask;
        }

        private void PowerManager_EnergySaverStatusChanged(object? sender, object e)
        {
            OnEnergySaverStatusChanged?.Invoke(PowerManager.EnergySaverStatus==EnergySaverStatus.On);
        }

        private int OnModernStandByStateChanged(IntPtr context, int type, IntPtr setting)
        {
            Debug.WriteLine("Modern Standby State Changed: " + type);
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
        }
    }
}
