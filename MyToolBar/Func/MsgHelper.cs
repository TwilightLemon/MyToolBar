using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MyToolBar.Func
{
    internal class MsgHelper
    {
        #region SocketMsg
        static Socket socket;
        public delegate void msg(string data);
        public event msg MsgReceived;
        public void Start()
        {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Bind(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 3230));
            socket.Listen(10);
            //接收客户端的 Socket请求
            socket.BeginAccept(OnAccept, socket);
        }
        private void OnAccept(IAsyncResult async)
        {
            var serverSocket = async.AsyncState as Socket;
            //获取到客户端的socket
            Socket clientSocket = null;
            try
            {
                clientSocket = serverSocket.EndAccept(async);
                var bytes = new byte[10000];
                //获取socket的内容
                var len = clientSocket.Receive(bytes);
                //将 bytes[] 转换 string
                var request = Encoding.UTF8.GetString(bytes, 0, len);
                MsgReceived(request);
                socket.Listen(100);
                //接收客户端的 Socket请求
                socket.BeginAccept(OnAccept, socket);
            }
            catch
            {
                socket.Close();
                socket.Dispose();
                Start();
                return;
            }
        }
        #endregion
        #region WinMsg
        public const int WM_COPYDATA = 0x004A;
        public const string SEND_LAST = "SEND_LAST",
            SEND_PAUSE = "SEND_PAUSE",
            SEND_NEXT = "SEND_NEXT";
        public static int ConnectedWindowHandle = 0;

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

        public static void SendMsg(String strSent, int WindowHandle)
        {
            if (WindowHandle == 0) return;
            byte[] arr = Encoding.Default.GetBytes(strSent);
            int len = arr.Length;
            COPYDATASTRUCT cdata;
            cdata.dwData = (IntPtr)100;
            cdata.lpData = strSent;
            cdata.cData = len + 1;
            SendMessage(WindowHandle, WM_COPYDATA, 0, ref cdata);
        }
        #endregion
    }
}
