using OpenHardwareMonitor.Hardware;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace MyToolBar.Func
{
    internal class CPUInfo
    {
        public static double GetCPUTemperature()
        {
            string str = "";
            ManagementObjectSearcher vManagementObjectSearcher = new(@"root\WMI", @"select * from MSAcpi_ThermalZoneTemperature");
            foreach (ManagementObject managementObject in vManagementObjectSearcher.Get().Cast<ManagementObject>())
            {
                str += managementObject.Properties["CurrentTemperature"].Value.ToString();
                if (str.Length > 3) break;
            }
            return Math.Round((float.Parse(str) ) / 100);
        }
        static PerformanceCounter counters;
        public static void Load() {
            counters = new PerformanceCounter("Processor Information", "% Processor Utility", "_Total");
            counters.NextValue();
        }
        public static string GetCPUUsedPercent()
        {
            return Math.Round(counters.NextValue())+"%";
        }
    }

}
