using MyToolBar.Common.WinAPI;
using System.Diagnostics.Tracing;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace MyToolBar.Plugin.BasicPackage.API;

internal class LemonAppMusicServer
{
    private readonly TcpListener _listener;
    private CancellationTokenSource? _cancellationTokenSource;
    private bool _isRunning = false;

    public event Action<string>? OnMsgReceived;
    public event Action? OnClientExited;

    public LemonAppMusicServer()
    {
        _listener = new TcpListener(IPAddress.Loopback,12587);
    }

    private const string INTERACT_MTB_SYNC = "INTERACT_MTB_SYNC";
    public static void CallExistingInstance()
    {
        var hwnd = MsgHelper.FindWindow(null, "Lemon App");
        if (hwnd != IntPtr.Zero)
        {
            MsgHelper.SendMsg(INTERACT_MTB_SYNC, hwnd.ToInt32());
        }
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
        _cancellationTokenSource?.Cancel();
        _listener.Stop();
        _isRunning = false;
    }
}
