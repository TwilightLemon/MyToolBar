using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MyToolBar.Func
{
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
        public  string[] GetNetworkspeed()
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
}
