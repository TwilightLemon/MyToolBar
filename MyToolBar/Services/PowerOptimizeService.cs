﻿using Microsoft.Extensions.Hosting;
using Microsoft.Win32;
using MyToolBar.Common;
using Windows.System.Power;
using MyToolBar.Common.WinAPI;
using System.Threading;
using System.Threading.Tasks;
using System;
using System.Diagnostics;

namespace MyToolBar.Services
{
    public class PowerOptimizeService:IHostedService
    {
        private IntPtr _standbyAPIHandle;
        public event Action<bool>? OnEnergySaverStatusChanged;
        public Task StartAsync(CancellationToken cancellationToken)
        {
            GlobalService.IsPowerModeOn=PowerManager.EnergySaverStatus== EnergySaverStatus.On;
            PowerManager.EnergySaverStatusChanged += PowerManager_EnergySaverStatusChanged;
            SystemEvents.PowerModeChanged += SystemEvents_PowerModeChanged;
            ModernStandbyPowerAPI.RegisterNotification((IntPtr context, int type, IntPtr setting) =>
            {
                if (type == ModernStandbyPowerAPI.PBT_APMSUSPEND)
                {
                    //进入低功耗状态时停止全局Timer
                    GlobalService.GlobalTimer.Stop();
                }else if (type == ModernStandbyPowerAPI.PBT_APMRESUMESUSPEND)
                {
                    //手动唤醒时启动全局Timer
                    GlobalService.GlobalTimer.Start();
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
            PowerManager.EnergySaverStatusChanged -= PowerManager_EnergySaverStatusChanged;
            ModernStandbyPowerAPI.UnregisterNotification(_standbyAPIHandle);
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
