using MyToolBar.Plugin.TabletUtils.Services;
using System.Globalization;
using System.Reflection;
using System.Resources;
using static MyToolBar.Plugin.TabletUtils.Package;

namespace MyToolBar.Plugin.TabletUtils;
public class Package : IPackage
{
    public static ResourceManager _rm
    = new("MyToolBar.Plugin.TabletUtils.LanguageRes.PluginLang",
        Assembly.GetExecutingAssembly());
    internal static CultureInfo? _cultureInfo { get => CultureInfo.DefaultThreadCurrentCulture; }

    public string PackageName => "MyToolBar.Plugin.TabletUtils";
    public string DisplayName => _rm.GetString("PackageDisplayName", _cultureInfo) ?? "";

    public string Description => _rm.GetString("PackageDesc", _cultureInfo) ?? "";

    public Version Version => new Version(1,0,0,0);

    public List<IPlugin> Plugins { get; set; } = [
        new PenMenuPlugin(),
        new SideBarPlugin()
        ];
}

public class PenMenuPlugin : IPlugin
{
    public IPackage? AcPackage { get; set; }

    public string Name { get; }= "PenMenu";
    public string DisplayName => _rm.GetString("PenMenuDisplayName", _cultureInfo) ?? "";

    public string Description => _rm.GetString("PenMenuDesc", _cultureInfo) ?? "";

    public List<string>? SettingsSignKeys => null;

    public PluginType Type => PluginType.UserService;

    public ServiceBase GetServiceHost()
    {
        return new PenMenuService();
    }
}

public class SideBarPlugin : IPlugin
{
    public IPackage? AcPackage { get; set; }

    public string Name { get; } = "SideBar";
    public string DisplayName => _rm.GetString("SideBarDisplayName", _cultureInfo) ?? "";

    public string Description => _rm.GetString("SideBarDesc", _cultureInfo) ?? "";

    public List<string>? SettingsSignKeys => null;

    public PluginType Type => PluginType.UserService;

    public ServiceBase GetServiceHost()
    {
        return new SideBarService();
    }
}