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
            Double temperature = 0;     
            // Query the MSAcpi_ThermalZoneTemperature API
            // Note: run your app or Visual Studio (while programming) or you will get "Access Denied"
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(@"root\WMI", "SELECT * FROM MSAcpi_ThermalZoneTemperature");

            foreach (ManagementObject obj in searcher.Get())
            {
                temperature = Convert.ToDouble(obj["CurrentTemperature"].ToString());
                // Convert the value to celsius degrees
                temperature = (temperature - 2732) / 10.0;
            }

            // Print the values e.g:

            // 29.8
            return temperature;
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
