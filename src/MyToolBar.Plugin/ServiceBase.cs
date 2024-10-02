namespace MyToolBar.Plugin;

/// <summary>
/// PluginType.UserService实体基类
/// </summary>
public interface ServiceBase : IDisposable {
    bool IsRunning { get;}
    Task Start();
    Task Stop();

    event EventHandler<bool>? IsRunningChanged;
}