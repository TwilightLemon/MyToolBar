namespace MyToolBar.Plugin;

/// <summary>
/// PluginType.UserService实体基类
/// </summary>
public interface ServiceBase : IDisposable {
    /// <summary>
    /// 服务是否正在运行
    /// </summary>
    bool IsRunning { get;}
    Task Start();
    Task Stop();

    event EventHandler<bool>? IsRunningChanged;

    /// <summary>
    /// 服务强制停止时发生（内部错误或者用户请求退出）
    /// </summary>
    event EventHandler? OnForceStop;
}