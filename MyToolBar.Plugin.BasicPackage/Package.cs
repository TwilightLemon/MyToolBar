using MyToolBar.Plugin.BasicPackage.Capsules;
using MyToolBar.Plugin.BasicPackage.OuterControls;
using static MyToolBar.Plugin.BasicPackage.Package;
using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Windows;

namespace MyToolBar.Plugin.BasicPackage
{
    public class Package:IPackage
    {
        public static ResourceManager _rm 
            = new("MyToolBar.Plugin.BasicPackage.LanguageRes.PluginLang",
                Assembly.GetExecutingAssembly());
        internal static CultureInfo? _cultureInfo { get => CultureInfo.DefaultThreadCurrentCulture; }
        public string Description { get => _rm.GetString("PackageDesc", _cultureInfo); }
        public string PackageName { get; set; } = "MyToolBar.Plugin.BasicPackage";
        public Version Version { get; set; } = new Version(1,0,0,0);
        public List<IPlugin> Plugins { get; set; } = [ 
        new WeatherCapPlugin(),
        new LemonAppMusicOutControlPlugin(),
        new DemoClockOutControlPlugin(),
        new HardwareMonitorCapPlugin()
        ];
    }

    public class WeatherCapPlugin:IPlugin
    {
        public IPackage? AcPackage { get; set; }
        internal static readonly string _name = "WeatherCap";
        public string Name { get; } = _name;
        public string Description { get => _rm.GetString("WeatherCapPluginDesc", _cultureInfo); }
        public List<string> SettingsSignKeys { get; } = [WeatherCap._settingsAPIKey];
        public PluginType Type { get; } = PluginType.Capsule;
        public UIElement GetMainElement()
        {
            return new WeatherCap();
        }
    }
    public class HardwareMonitorCapPlugin : IPlugin
    {

       public IPackage? AcPackage { get; set; }
        internal static readonly string _name = "HardwareMonitorCap";
        public string Name { get; } = _name;
        public string Description { get => _rm.GetString("HardwareMonitorCapPluginDesc", _cultureInfo); }
        public List<string>? SettingsSignKeys { get; } = null;
        public PluginType Type { get; } = PluginType.Capsule;
        public UIElement GetMainElement()
        {
            return new HardwareMonitorCap();
        }
    }
    public class LemonAppMusicOutControlPlugin : IPlugin
    {
        public IPackage? AcPackage { get; set; }
        public string Name { get; } = "Media OutControl";
        public string Description { get => _rm.GetString("LemonAppMusicOutControlPluginDesc", _cultureInfo); }
        public List<string>? SettingsSignKeys { get; } = null;
        public PluginType Type { get; } = PluginType.OuterControl;
        public UIElement GetMainElement()
        {
            return new LemonAppMusic();
        }
    }
    public class DemoClockOutControlPlugin : IPlugin
    {
        public static string _name="Demo Clock";
        public IPackage? AcPackage { get; set; }
        public string Name { get; } = _name;
        public string Description { get => _rm.GetString("DemoClockOutControlPluginDesc", _cultureInfo); }
        public List<string>? SettingsSignKeys { get; } = [
            DemoClock.SettingSign
            ];
        public PluginType Type { get; } = PluginType.OuterControl;
        public UIElement GetMainElement()
        {
            return new DemoClock();
        }
    }

}
