
using System.Timers;
using System.Windows;
using System.Windows.Media;
/*
 *Basic info of Plugins
 *Assembly->Package->Plugs
 *One Package for single assembly, including several plugs.
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
    /// 全局自定义基于Window的服务
    /// </summary>
    WindowService
}
public interface IPackage
{
    /// <summary>
    /// 包名称
    /// </summary>
    string PackageName { get; }
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
    /// 插件名称
    /// </summary>
    string Name { get; }
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
    UIElement GetMainElement();
}
