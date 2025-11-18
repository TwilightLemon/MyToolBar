using System.Windows;
/*
 *Basic info of Plugins
 *Assembly->Package->Plugins
 *One Package for single assembly, including several plug-ins.
 */
namespace MyToolBar.Plugin;
public enum PluginType
{
    /// <summary>
    /// 外部控制OuterControl
    /// </summary>
    OuterControl,
    /// <summary>
    /// 胶囊Capsule
    /// </summary>
    Capsule,
    /// <summary>
    /// 托管用户服务
    /// </summary>
    UserService
}
public interface IPackage
{
    /// <summary>
    /// 包名称
    /// </summary>
    string PackageName { get; }
    /// <summary>
    /// 名称（用于显示）
    /// </summary>
    string DisplayName { get; }
    /// <summary>
    /// 描述
    /// </summary>
    string Description { get; }
    /// <summary>
    /// 版本
    /// </summary>
    Version Version { get; }
    /// <summary>
    /// 插件
    /// </summary>
    List<IPlugin> Plugins { get; }
}
public interface IPlugin
{
    /// <summary>
    /// 插件名称(id)
    /// </summary>
    string Name { get; }
    /// <summary>
    /// 显示名称
    /// </summary>
    string DisplayName { get; }
    /// <summary>
    /// 描述
    /// </summary>
    string Description { get; }
    /// <summary>
    /// 托管设置的SignKeys
    /// </summary>
    List<string>? SettingsSignKeys { get; }
    /// <summary>
    /// 插件类型
    /// </summary>
    PluginType Type { get; }
    /// <summary>
    /// 获取插件UIElement
    /// </summary>
    /// <returns></returns>
    virtual UIElement? GetMainElement() => null;
    /// <summary>
    /// 获取服务实体
    /// </summary>
    /// <returns></returns>
    virtual IUserService? GetServiceHost() => null;
}
