using MyToolBar.Plugin;

namespace MyToolBar.Plugin.BasicPackage.Clock.Models;

[SettingsConfig(DisplayName = "$ClockCapSettings", ResourceManagerType = typeof(Package))]
public class ClockCapSettings
{
    [SettingsField(DisplayName = "$FormatStr", Description = "$FormatStrDesc",
                   Placeholder = "MM-dd tt h:mm dddd", Order = 0)]
    public string FormatStr { get; set; } = "MM-dd tt h:mm dddd";
}
