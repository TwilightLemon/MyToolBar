using System;
using System.IO.Pipes;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;

namespace MyToolBar.Common.WinAPI;

public class MsgHelper
{
    public const int WM_COPYDATA = 0x004A;

    public struct COPYDATASTRUCT
    {
        public IntPtr dwData;
        public int cData;
        [MarshalAs(UnmanagedType.LPStr)]
        public string lpData;
    }

    [DllImport("User32.dll", EntryPoint = "FindWindow")]
    public extern static IntPtr FindWindow(string lpClassName, string lpWindowName);

    [DllImport("User32.dll")]
    public static extern int SendMessage(int hwnd, int msg, int wParam, ref COPYDATASTRUCT IParam);

    public static void SendMsg(String strSent, int WindowHandle )
    {
        int WINDOW_HANDLE = WindowHandle;
        if (WINDOW_HANDLE != 0)
        {
            byte[] arr = Encoding.Default.GetBytes(strSent);
            int len = arr.Length;
            COPYDATASTRUCT cdata;
            cdata.dwData = (IntPtr)100;
            cdata.lpData = strSent;
            cdata.cData = len + 1;
            SendMessage(WINDOW_HANDLE, WM_COPYDATA, 0, ref cdata);
        }
    }
}
