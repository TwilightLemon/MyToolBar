
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
    /// λ��ToolBar�м������OuterControl
    /// </summary>
    OuterControl,
    /// <summary>
    /// λ��ToolBar�Ҳ��Capsule
    /// </summary>
    Capsule,
    /// <summary>
    /// ȫ���Զ������
    /// </summary>
    UserService
}
public interface IPackage
{
    /// <summary>
    /// ������(��ΪΨһ��ʶ��)
    /// </summary>
    string PackageName { get; }
    /// <summary>
    /// ��ʾ�İ�����
    /// </summary>
    string DisplayName { get; }
    /// <summary>
    /// ����
    /// </summary>
    string Description { get; }
    /// <summary>
    /// ���汾
    /// </summary>
    Version Version { get; }
    /// <summary>
    /// �����Ĳ��
    /// </summary>
    List<IPlugin> Plugins { get; }
}
public interface IPlugin
{
    /// <summary>
    /// ����İ� ������������������
    /// </summary>
    IPackage? AcPackage { get; set; }
    /// <summary>
    /// �������(��ΪΨһ��ʶ��)
    /// </summary>
    string Name { get; }
    /// <summary>
    /// ��ʾ�Ĳ������
    /// </summary>
    string DisplayName { get; }
    /// <summary>
    /// �������
    /// </summary>
    string Description { get; }
    /// <summary>
    /// �йܵ�����SignKeys
    /// </summary>
    List<string>? SettingsSignKeys { get; }
    /// <summary>
    /// �������
    /// </summary>
    PluginType Type { get; }
    /// <summary>
    /// ���������ṩ����ҪUIElement
    /// </summary>
    /// <returns></returns>
    virtual UIElement? GetMainElement() => null;
    virtual ServiceBase? GetServiceHost() => null;
}
