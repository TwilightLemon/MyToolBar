using DeepSeek.Core;
using DeepSeek.Core.Models;
using System.Text.Json.Serialization;

namespace MyToolBar.Plugin.TabletUtils.Configs;

public class DeepSeekConfig
{
    public string APIKey { get; set; } = string.Empty;
    public string Model { get; set; } = DeepSeekModels.ReasonerModel;
}