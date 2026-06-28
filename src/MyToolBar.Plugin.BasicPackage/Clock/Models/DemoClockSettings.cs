using MyToolBar.Plugin;

namespace MyToolBar.Plugin.BasicPackage.Clock.Models;

[SettingsConfig(DisplayName = "$DemoClockSettings", ResourceManagerType = typeof(Package))]
public class DemoClockSettings
{
    [SettingsField(DisplayName = "$FormatStr", Description = "$FormatStrDesc",
                   Placeholder = "MM-dd tt h:mm dddd", Order = 0)]
    public string FormatStr { get; set; } = "MM-dd tt h:mm dddd";

    [SettingsField(DisplayName = "$Sign", Description = "$SignDesc", Order = 1)]
    public string Sign { get; set; } = "  ❤  Have a nice day.";
}
