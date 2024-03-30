using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Management;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;

/*
 * Get hardware information
 * using System.Management & OpenHardwareMonitorLib
 */

namespace MyToolBar.Func;
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
    public static void Load()
    {
        counters = new PerformanceCounter("Processor Information", "% Processor Utility", "_Total");
        counters.NextValue();
    }
    public static string GetCPUUsedPercent()
    {
        return Math.Round(counters.NextValue()) + "%";
    }
}
internal class MemoryInfo
{
    public static double GetUsedPercent()
    {
        MEMORY_INFO mi = new();
        mi.dwLength = (uint)Marshal.SizeOf(mi);
        GlobalMemoryStatusEx(ref mi);
        return 100 - ((double)mi.ullAvailPhys / (double)mi.ullTotalPhys) * 100;
    }
    #region 获得内存信息API
    [DllImport("kernel32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool GlobalMemoryStatusEx(ref MEMORY_INFO mi);

    //定义内存的信息结构
    [StructLayout(LayoutKind.Sequential)]
    public struct MEMORY_INFO
    {
        public uint dwLength; //当前结构体大小
        public uint dwMemoryLoad; //当前内存使用率
        public ulong ullTotalPhys; //总计物理内存大小
        public ulong ullAvailPhys; //可用物理内存大小
        public ulong ullTotalPageFile; //总计交换文件大小
        public ulong ullAvailPageFile; //总计交换文件大小
        public ulong ullTotalVirtual; //总计虚拟内存大小
        public ulong ullAvailVirtual; //可用虚拟内存大小
        public ulong ullAvailExtendedVirtual; //保留 这个值始终为0
    }
    #endregion
}
internal class NetworkInfo
{
    public NetworkInfo()
    {
        List<PerformanceCounter> pcs = new();
        List<PerformanceCounter> pcs2 = new();
        string[] names = getAdapter();
        foreach (string name in names)
        {
            try
            {
                PerformanceCounter pc = new("Network Interface", "Bytes Received/sec", name.Replace('(', '[').Replace(')', ']'), ".");
                PerformanceCounter pc2 = new("Network Interface", "Bytes Sent/sec", name.Replace('(', '[').Replace(')', ']'), ".");
                pc.NextValue();
                pcs.Add(pc);
                pcs2.Add(pc2);
            }
            catch
            {
                continue;
            }

        }
        pcss = new List<PerformanceCounter>[2];
        pcss[0] = pcs;
        pcss[1] = pcs2;
    }
    List<PerformanceCounter>[] pcss;
    public string[] GetNetworkspeed()
    {
        List<PerformanceCounter> pcs = pcss[0];
        List<PerformanceCounter> pcs2 = pcss[1];
        long recv = 0;
        long sent = 0;
        foreach (PerformanceCounter pc in pcs)
        {
            recv += Convert.ToInt32(pc.NextValue());
        }
        foreach (PerformanceCounter pc in pcs2)
        {
            sent += Convert.ToInt32(pc.NextValue());
        }
        return new string[] { FormatSize(recv), FormatSize(sent) };

    }
    private static string FormatSize(double size)
    {
        double d = (double)size;
        int i = 0;
        while ((d > 1024) && (i < 5))
        {
            d /= 1024;
            i++;
        }
        string[] unit = { "B", "KB", "MB", "GB", "TB" };
        return (string.Format("{0} {1}", Math.Round(d, 2), unit[i]));
    }
    public string[] getAdapter()
    {
        NetworkInterface[] adapters = NetworkInterface.GetAllNetworkInterfaces();
        string[] name = new string[adapters.Length];
        int index = 0;
        foreach (NetworkInterface ni in adapters)
        {
            name[index] = ni.Description;
            index++;
        }
        return name;
    }
}

