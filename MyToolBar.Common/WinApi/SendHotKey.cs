using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MyToolBar.Common.WinApi
{
    public class SendHotKey
    {

        [DllImport("user32.dll")]
        public static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, uint dwExtraInfo);

        public const byte VK_LWIN = 0x5B;
        public const byte VK_TAB = 0x09,
            VK_SHIFT=0x10;
        public const byte VK_SNAPSHOT = 0x2C;

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
        public static void ScreenShot()
        {
            // 按下Win键
            keybd_event(VK_LWIN, 0, 0, 0);
            // 按下Shift键
            keybd_event(VK_SHIFT, 0, 0, 0);
            // 按下S键
            keybd_event(0x53, 0, 0, 0);
            // 释放S键
            keybd_event(0x53, 0, 2, 0);
            // 释放Shift键
            keybd_event(VK_SHIFT, 0, 2, 0);
            // 释放Win键
            keybd_event(VK_LWIN, 0, 2, 0);
        }
    }
}
