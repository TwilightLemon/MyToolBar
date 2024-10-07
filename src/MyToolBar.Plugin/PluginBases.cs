
using System.Timers;
using System.Windows;
using System.Windows.Media;
/*
 *Basic info of Plugins
 *Assembly->Package->Plugs
 *One Package for single assembly, including several plug-ins.
 */
namespace MyToolBar.Plugin;
public enum PluginType
{
    /// <summary>
    /// 位于ToolBar中间区域的OuterControl
    /// </summary>
    OuterControl,
    /// <summary>
    /// 位于ToolBar右侧的Capsule
    /// </summary>
    Capsule,
    /// <summary>
    /// 全局自定义服务
    /// </summary>
    UserService
}
public interface IPackage
{
    /// <summary>
    /// 包名称(作为唯一标识符)
    /// </summary>
    string PackageName { get; }
    /// <summary>
    /// 显示的包名称
    /// </summary>
    string DisplayName { get; }
    /// <summary>
    /// 描述
    /// </summary>
    string Description { get; }
    /// <summary>
    /// 包版本
    /// </summary>
    Version Version { get; }
    /// <summary>
    /// 包含的插件
    /// </summary>
    List<IPlugin> Plugins { get; }
}
public interface IPlugin
{
    /// <summary>
    /// 所属的包 该属性由主程序设置
    /// </summary>
    IPackage? AcPackage { get; set; }
    /// <summary>
    /// 插件名称(作为唯一标识符)
    /// </summary>
    string Name { get; }
    /// <summary>
    /// 显示的插件名称
    /// </summary>
    string DisplayName { get; }
    /// <summary>
    /// 插件描述
    /// </summary>
    string Description { get; }
    /// <summary>
    /// 托管的设置SignKeys
    /// </summary>
    List<string>? SettingsSignKeys { get; }
    /// <summary>
    /// 插件类型
    /// </summary>
    PluginType Type { get; }
    /// <summary>
    /// 向主程序提供的主要UIElement
    /// </summary>
    /// <returns></returns>
    virtual UIElement? GetMainElement() => null;
    virtual ServiceBase? GetServiceHost() => null;
}
