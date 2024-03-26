using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MyToolBar.WinApi
{
    internal class SendHotKey
    {

        [DllImport("user32.dll")]
        public static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, uint dwExtraInfo);

        public const byte VK_LWIN = 0x5B;
        public const byte VK_TAB = 0x09;

        public static void ShowTaskView()
        {
            // 按下Win键
            keybd_event(VK_LWIN, 0, 0, 0);
            // 按下Tab键
            keybd_event(VK_TAB, 0, 0, 0);
            // 释放Tab键
            keybd_event(VK_TAB, 0, 2, 0);
            // 释放Win键
            keybd_event(VK_LWIN, 0, 2, 0);
        }
    }
}
