using MyToolBar.Plugin.BasicPackage.Capsules;
using System.Windows;

namespace MyToolBar.Plugin.BasicPackage
{
    public class Package:IPackage
    {
        public string PackageName { get; set; } = "MyToolBar.Plugin.BasicPackage";
        public Version Version { get; set; } = new Version(1,0,0,0);
        public List<IPlugin> Plugins { get; set; } = new List<IPlugin>() { 
        new WeatherCapPlugin(),
        new LemonAppMusicOutControlPlugin()
        };
    }

    public class WeatherCapPlugin:IPlugin
    {
        internal static readonly string _name = "WeatherCap";
        public string Name { get; } = _name;
        public string Description { get;} = "";
        public List<string> SettingsSignKeys { get; } = [WeatherCap._settingsAPIKey];
        public PluginType Type { get; } = PluginType.Capsule;
        public UIElement GetMainElement()
        {
            return new WeatherCap();
        }
    }
    public class LemonAppMusicOutControlPlugin : IPlugin
    {
        public string Name { get; } = "LemonAppMusicOutControl";
        public string Description { get; } = "";
        public List<string>? SettingsSignKeys { get; } = null;
        public PluginType Type { get; } = PluginType.OuterControl;
        public UIElement GetMainElement()
        {
            return null;
        }
    }

}
