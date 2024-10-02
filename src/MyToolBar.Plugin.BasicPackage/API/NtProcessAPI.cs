using System.Diagnostics;
using System.Runtime.InteropServices;

namespace MyToolBar.Plugin.BasicPackage.API
{
    internal static class NtProcessAPI
    {
        [DllImport("kernel32.dll")]
        private static extern bool SetProcessWorkingSetSize(IntPtr processHandle, int min, int max);

        [DllImport("ntdll.dll")]
        private static extern uint NtSuspendProcess([In] IntPtr processHandle);

        [DllImport("ntdll.dll")]
        private static extern uint NtResumeProcess([In] IntPtr processHandle);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr OpenProcess(
            uint desiredAccess,
            bool inheritHandle,
            int processId);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CloseHandle([In] IntPtr handle);

        public static void Freeze(this Process process)
        {
            IntPtr hProc = IntPtr.Zero;
            hProc = OpenProcess(0x800, false, process.Id);
            if (hProc != IntPtr.Zero)
            {
                NtSuspendProcess(hProc);
                SetProcessWorkingSetSize(process.Handle, -1, -1);
                CloseHandle(hProc);
            }
        }

        public static void Unfreeze(this Process process)
        {
            IntPtr hProc = IntPtr.Zero;
            hProc = OpenProcess(0x800, false, process.Id);
            if (hProc != IntPtr.Zero)
            {
                NtResumeProcess(hProc);
                CloseHandle(hProc);
            }
        }

        public static bool IsResponding(this Process process)
        {
            bool isResponding = false;
            try
            {
                var task = Task.Run(() => {
                    process.Refresh();
                    isResponding = process.Responding; 
                });
                if (!task.Wait(TimeSpan.FromSeconds(1))) 
                {
                    // 等待1秒，如果进程在这个时间内没有响应，就认为它被挂起了
                    isResponding = false;
                }
            }
            catch (AggregateException ae)
            {
                isResponding = false;
            }
            return isResponding;
        }
    }
}
