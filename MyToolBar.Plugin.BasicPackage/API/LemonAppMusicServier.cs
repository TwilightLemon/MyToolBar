using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MyToolBar.Plugin.BasicPackage.API;
internal class LemonAppMusicServier
{
    private TcpListener _listener;
    private CancellationTokenSource _cancellationTokenSource;
    private bool _isRunning = false;

    public LemonAppMusicServier()
    {
        _listener = new TcpListener(IPAddress.Loopback,12587);
    }
    public async Task StartAsync(Action<string> onMsgReceived)
    {
        if (_isRunning) return;
        _isRunning = true;
        _listener.Start();
        _cancellationTokenSource = new();

        while (!_cancellationTokenSource.Token.IsCancellationRequested)
        {
            using var client = await _listener.AcceptTcpClientAsync();
            if (client != null) 
            {
                using var stream = client.GetStream();
                using var reader=new StreamReader(stream,Encoding.UTF8);
                while (!_cancellationTokenSource.Token.IsCancellationRequested)
                {
                    try
                    {
                        var data = await reader.ReadLineAsync();
                        if (data == null)
                        {
                            break;
                        }
                        onMsgReceived(data);
                    }
                    catch
                    {
                        break;
                    }
                }
            }
        }
    }
    public void Stop()
    {
        _cancellationTokenSource.Cancel();
        _listener.Stop();
        _isRunning = false;
    }
}
