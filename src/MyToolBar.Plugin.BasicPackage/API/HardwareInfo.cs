﻿using System.Diagnostics;
using System.Management;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;

/*
 * Get hardware information
 * using System.Management & OpenHardwareMonitorLib
 * 
 * TODO: fix mem overflow caused by ManagementObjectSearcher
 * 
 */

namespace MyToolBar.Plugin.BasicPackage.API;

internal class CPUInfo
{
    public double GetCPUTemperature()
    {
        try
        {
            Double temperature = 0;
            // Query the MSAcpi_ThermalZoneTemperature API
            using ManagementObjectSearcher searcher = new (@"root\WMI", "SELECT * FROM MSAcpi_ThermalZoneTemperature");

            foreach (ManagementObject obj in searcher.Get())
            {
                temperature = Convert.ToDouble(obj["CurrentTemperature"].ToString());
                // Convert the value to celsius degrees
                temperature = (temperature - 2732) / 10.0;
                obj.Dispose();
            }
            return temperature;
        }
        catch
        {
            return double.NaN;
        }
    }

    protected PerformanceCounter counters;
    public static CPUInfo Create()
    {
        var ci=new CPUInfo();
        ci.counters = new PerformanceCounter("Processor Information", "% Processor Utility", "_Total");
        ci.counters.NextValue();
        return ci;
    }

    public void Dispose()
    {
        counters.Dispose();
    }
    public string GetCPUUsedPercent()
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
    private List<PerformanceCounter>[] _performanceCounters;

    public static async Task<NetworkInfo> Create()
    {
        var ni = new NetworkInfo();
        //无法直接判断实例是否有效，用try catch 但是严重影响性能
        //所以丢到线程里去初始化，不要影响UI加载
        await Task.Run(() =>
        {
            List<PerformanceCounter> pcs = new();
            List<PerformanceCounter> pcs2 = new();
            string[] names = GetAdapter();
            foreach (string name in names)
            {
                PerformanceCounter? pc=null, pc2=null;
                try
                {
                    pc = new("Network Interface", "Bytes Received/sec", name.Replace('(', '[').Replace(')', ']'), ".");
                    pc2 = new("Network Interface", "Bytes Sent/sec", name.Replace('(', '[').Replace(')', ']'), ".");
                    pc.NextValue();
                    pcs.Add(pc);
                    pcs2.Add(pc2);
                }
                catch
                {
                    pc?.Dispose();
                    pc2?.Dispose();
                    continue;
                }

            }
            ni._performanceCounters = new List<PerformanceCounter>[2];
            ni._performanceCounters[0] = pcs;
            ni._performanceCounters[1] = pcs2;
        });
        return ni;
    }
    public void Dispose()
    {
        // Dispose performance counters
        foreach (List<PerformanceCounter> pcs in _performanceCounters)
        {
            foreach (PerformanceCounter pc in pcs)
            {
                pc.Dispose();
            }
        }
    }

    public string[] GetNetworkSpeed()
    {
        List<PerformanceCounter> receivedPerformanceCounter = _performanceCounters[0];
        List<PerformanceCounter> pcs2 = _performanceCounters[1];
        long received = 0;
        long sent = 0;

        foreach (PerformanceCounter pc in receivedPerformanceCounter)
        {
            received += Convert.ToInt32(pc.NextValue());
        }
        foreach (PerformanceCounter pc in pcs2)
        {
            sent += Convert.ToInt32(pc.NextValue());
        }

        return [FormatSize(received), FormatSize(sent)];
    }

    public static string FormatSize(double size)
    {
        double finalSize = (double)size;

        int unitIndex = 0;
        while ((finalSize > 1024) && (unitIndex < 5))
        {
            finalSize /= 1024;
            unitIndex++;
        }
        string[] unit = { "B", "KB", "MB", "GB", "TB" };

        return $"{Math.Round(finalSize, 2)} {unit[unitIndex]}";
    }

    public static string[] GetAdapter()
    {
        NetworkInterface[] adapters = NetworkInterface.GetAllNetworkInterfaces();
        List<string> names = new List<string>();
        foreach (NetworkInterface ni in adapters)
        {
            // 检查网络接口的状态
            if (ni.OperationalStatus == OperationalStatus.Up)
            {
                names.Add(ni.Description);
            }
        }
        return names.ToArray();
    }
}

