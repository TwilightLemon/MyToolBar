using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MyToolBar.Func
{
    internal class MsgHelper
    {
        static Socket socket;
        public delegate void msg(string data);
        public event msg MsgReceived;
        public void Start() {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Bind(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 3230));
            socket.Listen(100);
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
            }
            catch
            {
                socket.Close();
                socket.Dispose();
                Start();
                return;
            }
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
    }
}
