using MyToolBar.Plugin.BasicPackage.Capsules;
using MyToolBar.Plugin.BasicPackage.OuterControls;
using System.Windows;

namespace MyToolBar.Plugin.BasicPackage
{
    public class Package:IPackage
    {
        public string Description { get; set; } = "MyToolBar官方基础插件包";
        public string PackageName { get; set; } = "MyToolBar.Plugin.BasicPackage";
        public Version Version { get; set; } = new Version(1,0,0,0);
        public List<IPlugin> Plugins { get; set; } = new List<IPlugin>() { 
        new WeatherCapPlugin(),
        new LemonAppMusicOutControlPlugin(),
        new DemoClockOutControlPlugin(),
        new HardwareMonitorCapPlugin()
        };
    }

    public class WeatherCapPlugin:IPlugin
    {
        public IPackage? AcPackage { get; set; }
        internal static readonly string _name = "WeatherCap";
        public string Name { get; } = _name;
        public string Description { get;} = "天气小组件";
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
        public string Description { get;} = "硬件监控小组件";
        public List<string> SettingsSignKeys { get; } = null;
        public PluginType Type { get; } = PluginType.Capsule;
        public UIElement GetMainElement()
        {
            return new HardwareMonitorCap();
        }
    }
    public class LemonAppMusicOutControlPlugin : IPlugin
    {
        public IPackage? AcPackage { get; set; }
        public string Name { get; } = "LemonAppMusicOutControl";
        public string Description { get; } = "与Lemon App联动显示歌词与控制";
        public List<string>? SettingsSignKeys { get; } = null;
        public PluginType Type { get; } = PluginType.OuterControl;
        public UIElement GetMainElement()
        {
            return new LemonAppMusic();
        }
    }
    public class DemoClockOutControlPlugin : IPlugin
    {
        public static string _name="DemoClock";
        public IPackage? AcPackage { get; set; }
        public string Name { get; } = _name;
        public string Description { get; } = "时钟";
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
