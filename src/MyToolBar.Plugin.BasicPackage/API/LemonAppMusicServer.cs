using System.Diagnostics.Tracing;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace MyToolBar.Plugin.BasicPackage.API;
//弃用
internal class LemonAppMusicServer
{
    private TcpListener _listener;
    private CancellationTokenSource _cancellationTokenSource;
    private Mutex _mutex;
    private bool _isRunning = false;

    public event Action<string>? OnMsgReceived;
    public event Action? OnClientExited;

    public LemonAppMusicServer()
    {
        _listener = new TcpListener(IPAddress.Loopback,12587);
        InitMutex();
    }
    private void InitMutex()
    {
        _mutex = new Mutex(false, "MyToolBar.Plugin.BasicPackage//LemonAppMusicServier",out _);
    }
    public async Task StartAsync()
    {
        if (_isRunning) return;
        _isRunning = true;
        _listener.Start();
        _cancellationTokenSource = new();

        while (!_cancellationTokenSource.Token.IsCancellationRequested)
        {
            try
            {
                using var client = await _listener.AcceptTcpClientAsync();
                if (client != null)
                {
                    using var stream = client.GetStream();
                    using var reader = new StreamReader(stream, Encoding.UTF8);
                    while (!_cancellationTokenSource.Token.IsCancellationRequested)
                    {
                        try
                        {
                            var data = await reader.ReadLineAsync();
                            if (data == null)
                            {
                                break;
                            }
                            OnMsgReceived?.Invoke(data);
                        }
                        catch
                        {
                            break;
                        }
                    }
                }
            }
            catch
            {
                break;
            }
            finally
            {
                OnClientExited?.Invoke();
            }
        }
    }
    public void Stop()
    {
        _cancellationTokenSource.Cancel();
        _mutex.Dispose();
        _listener.Stop();
        _isRunning = false;
    }
}
