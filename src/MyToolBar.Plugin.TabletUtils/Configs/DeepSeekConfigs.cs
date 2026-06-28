using DeepSeek.Core;
using DeepSeek.Core.Models;
using System.Text.Json.Serialization;
using MyToolBar.Plugin;

namespace MyToolBar.Plugin.TabletUtils.Configs;

[SettingsConfig(DisplayName = "$DeepSeekSettings", ResourceManagerType = typeof(Package))]
public class DeepSeekConfig
{
    [SettingsField(DisplayName = "$ApiKey", Description = "$ApiKeyDesc",
                   HelpUrl = "https://platform.deepseek.com/api_keys",
                   IsPassword = true, IsRequired = true, Order = 0)]
    public string APIKey { get; set; } = string.Empty;

    [SettingsField(DisplayName = "$Model", Order = 1)]
    public string Model { get; set; } = DeepSeekModels.ReasonerModel;
}